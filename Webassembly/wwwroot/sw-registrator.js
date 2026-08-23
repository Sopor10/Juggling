window.updateAvailable = new Promise((resolve, reject) => {
    if (!('serviceWorker' in navigator)) {
        const errorMessage = `This browser doesn't support service workers`;
        console.error(errorMessage);
        reject(errorMessage);
        return;
    }

    const baseHref = document.querySelector('base')?.href || `${window.location.origin}/`;
    const basePath = new URL(baseHref).pathname;
    // PR preview deploys share the production origin; keep them free of a root-scoped worker.
    // Localhost / embedded IDE browsers (Cursor Electron) must not keep a stale SW either —
    // cached WASM/boot assets there show up as a blank boot screen + blazor-error-ui.
    const host = window.location.hostname;
    const isLocalHost = host === 'localhost' || host === '127.0.0.1' || host === '[::1]';
    const isEmbeddedIdeBrowser = /Electron|Cursor\//i.test(navigator.userAgent || '');
    if (basePath.includes('/pr-preview/') || isLocalHost || isEmbeddedIdeBrowser) {
        navigator.serviceWorker.getRegistrations?.().then(regs => {
            regs.forEach(r => r.unregister());
        }).catch(() => {});
        resolve(false);
        return;
    }

    const workerUrl = new URL('service-worker.js', baseHref).href;
    navigator.serviceWorker.register(workerUrl, { updateViaCache: 'none' })
        .then(registration => {
            console.info(`Service worker registration successful (scope: ${registration.scope})`);

            setInterval(() => {
                registration.update();
            }, 30 * 1000);

            registration.onupdatefound = () => {
                const installingServiceWorker = registration.installing;
                installingServiceWorker.onstatechange = () => {
                    if (installingServiceWorker.state === 'installed') {
                        resolve(!!navigator.serviceWorker.controller);
                    }
                };
            };
        })
        .catch(error => {
            console.error('Service worker registration failed with error:', error);
            reject(error);
        });
});

window.registerForUpdateAvailableNotification = (caller, methodName) => {
    window.updateAvailable.then(isUpdateAvailable => {
        if (isUpdateAvailable) {
            caller.invokeMethodAsync(methodName).then();
        }
    });
};
