(function () {
    'use strict';

    const listeners = new Set();
    let registration = null;
    let updateAvailable = false;
    let reloadPending = false;

    function notifyListeners() {
        for (const listener of listeners) {
            listener.invokeMethodAsync('OnUpdateAvailable').catch(() => { });
        }
    }

    function setUpdateAvailable(value) {
        if (updateAvailable === value) {
            return;
        }

        updateAvailable = value;
        if (value) {
            notifyListeners();
        }
    }

    function isServiceWorkerSupported() {
        return 'serviceWorker' in navigator;
    }

    function shouldRegisterServiceWorker() {
        const baseHref = document.querySelector('base')?.href || `${window.location.origin}/`;
        const basePath = new URL(baseHref).pathname;
        const host = window.location.hostname;
        const isLocalHost = host === 'localhost' || host === '127.0.0.1' || host === '[::1]';
        const isEmbeddedIdeBrowser = /Electron|Cursor\//i.test(navigator.userAgent || '');
        return !basePath.includes('/pr-preview/') && !isLocalHost && !isEmbeddedIdeBrowser;
    }

    async function unregisterAll() {
        const regs = await navigator.serviceWorker.getRegistrations?.() ?? [];
        await Promise.all(regs.map(r => r.unregister()));
    }

    function getBaseHref() {
        const href = document.querySelector('base')?.href || `${window.location.origin}/`;
        return href.endsWith('/') ? href : `${href}/`;
    }

    function getAssetsManifestUrl() {
        const url = new URL('service-worker-assets.js', getBaseHref());
        url.searchParams.set('_', Date.now().toString());
        return url;
    }

    async function fetchBootFingerprint() {
        const response = await fetch(getAssetsManifestUrl(), { cache: 'no-store' });
        if (!response.ok) {
            throw new Error(`service-worker-assets.js fetch failed: ${response.status}`);
        }

        const text = await response.text();
        const versionMatch = text.match(/"version"\s*:\s*"([^"]+)"/);
        if (versionMatch) {
            return versionMatch[1];
        }

        throw new Error('service-worker-assets.js missing version fingerprint');
    }

    async function captureCurrentBootFingerprint() {
        const existing = sessionStorage.getItem('appBootFingerprint');
        if (existing) {
            return existing;
        }

        try {
            const fingerprint = await fetchBootFingerprint();
            sessionStorage.setItem('appBootFingerprint', fingerprint);
            return fingerprint;
        } catch {
            return null;
        }
    }

    function trackWorker(reg, worker) {
        if (!worker) {
            return;
        }

        worker.addEventListener('statechange', () => {
            if (worker.state === 'installed' && navigator.serviceWorker.controller) {
                setUpdateAvailable(true);
            }
        });
    }

    function handleRegistration(reg) {
        registration = reg;

        if (reg.waiting) {
            setUpdateAvailable(true);
        }

        trackWorker(reg, reg.installing);

        reg.addEventListener('updatefound', () => {
            trackWorker(reg, reg.installing);
        });
    }

    async function initServiceWorker() {
        if (!isServiceWorkerSupported() || !shouldRegisterServiceWorker()) {
            await unregisterAll().catch(() => { });
            return null;
        }

        const workerUrl = new URL('service-worker.js', getBaseHref()).href;
        const reg = await navigator.serviceWorker.register(workerUrl, { updateViaCache: 'none' });
        handleRegistration(reg);

        setInterval(() => {
            reg.update().catch(() => { });
        }, 60_000);

        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (reloadPending) {
                window.location.reload();
            }
        });

        await captureCurrentBootFingerprint();
        return reg;
    }

    async function getRegistration() {
        if (registration) {
            return registration;
        }

        return initServiceWorker();
    }

    async function clearAllCaches() {
        const cacheKeys = await caches.keys();
        await Promise.all(cacheKeys.map(key => caches.delete(key)));
    }

    window.appUpdate = {
        async checkForUpdates() {
            if (!shouldRegisterServiceWorker() || !isServiceWorkerSupported()) {
                return { status: 'Unsupported' };
            }

            const reg = await getRegistration();
            if (!reg) {
                return { status: 'Unsupported' };
            }

            await reg.update().catch(() => { });
            await new Promise(resolve => setTimeout(resolve, 750));

            if (reg.waiting) {
                setUpdateAvailable(true);
                return { status: 'UpdateAvailable' };
            }

            try {
                const currentFingerprint = sessionStorage.getItem('appBootFingerprint');
                const remoteFingerprint = await fetchBootFingerprint();

                if (currentFingerprint && remoteFingerprint !== currentFingerprint) {
                    setUpdateAvailable(true);
                    return { status: 'UpdateAvailable' };
                }

                return { status: 'UpToDate' };
            } catch (error) {
                return { status: 'CheckFailed', message: String(error) };
            }
        },

        async applyUpdate() {
            reloadPending = true;

            const reg = await getRegistration();
            if (reg?.waiting) {
                reg.waiting.postMessage({ type: 'SKIP_WAITING' });
                return;
            }

            await clearAllCaches();
            await unregisterAll().catch(() => { });
            sessionStorage.removeItem('appBootFingerprint');
            window.location.reload();
        },

        subscribe(dotNetRef) {
            listeners.add(dotNetRef);
            if (updateAvailable) {
                dotNetRef.invokeMethodAsync('OnUpdateAvailable').catch(() => { });
            }
        },

        unsubscribe(dotNetRef) {
            listeners.delete(dotNetRef);
        }
    };

    window.updateAvailable = getRegistration().then(reg => !!reg?.waiting);

    window.registerForUpdateAvailableNotification = (caller, methodName) => {
        const wrapper = {
            invokeMethodAsync: (name) => {
                if (name === methodName) {
                    return caller.invokeMethodAsync(methodName);
                }

                return Promise.resolve();
            }
        };

        window.appUpdate.subscribe(wrapper);
    };

    initServiceWorker();
})();
