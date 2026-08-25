// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => {
    if (event.data?.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.br$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest.
    // Activation waits for user confirmation via SKIP_WAITING.
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    await self.clients.claim();

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

function shouldUseNetworkFirst(request, requestUrl) {
    if (request.mode === 'navigate') {
        return true;
    }

    const path = requestUrl.pathname;
    return path.endsWith('/index.html')
        || path.endsWith('index.html')
        || path.endsWith('service-worker-assets.js')
        || path.endsWith('dotnet.js')
        || path.includes('/_framework/');
}

async function networkFirst(cache, cacheKey, networkRequest) {
    try {
        const networkResponse = await fetch(networkRequest);
        if (networkResponse?.ok) {
            await cache.put(cacheKey, networkResponse.clone());
            return networkResponse;
        }
    } catch (error) {
        console.warn('Service worker: network-first fetch failed, using cache', error);
    }

    return cache.match(cacheKey);
}

async function onFetch(event) {
    // PR previews live under /pr-preview/ on the same origin. Never intercept them —
    // otherwise this root-scoped worker serves the production index.html for those URLs.
    const requestUrl = new URL(event.request.url);
    if (requestUrl.pathname.startsWith('/pr-preview/')) {
        return fetch(event.request);
    }

    if (event.request.method !== 'GET') {
        return fetch(event.request);
    }

    const shouldServeIndexHtml = event.request.mode === 'navigate'
        && !manifestUrlList.some(url => url === event.request.url);

    const cache = await caches.open(cacheName);
    const cacheKey = shouldServeIndexHtml ? 'index.html' : event.request;
    const networkRequest = shouldServeIndexHtml
        ? new Request(new URL('index.html', baseUrl).href, { cache: 'no-cache' })
        : event.request;

    if (shouldUseNetworkFirst(event.request, requestUrl) || shouldServeIndexHtml) {
        const networkFirstResponse = await networkFirst(cache, cacheKey, networkRequest);
        if (networkFirstResponse) {
            return networkFirstResponse;
        }
    } else {
        const cachedResponse = await cache.match(cacheKey);
        if (cachedResponse) {
            return cachedResponse;
        }
    }

    return fetch(event.request);
}
