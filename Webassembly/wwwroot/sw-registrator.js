window.updateAvailable = new Promise((resolve, reject) => {
    if (!('serviceWorker' in navigator)) {
        const errorMessage = `This browser doesn't support service workers`;
        console.error(errorMessage);
        reject(errorMessage);
        return;
    }

    const baseHref = document.querySelector('base')?.href || `${window.location.origin}/`;
    // PR preview deploys share the production origin; keep them free of a root-scoped worker.
    if (new URL(baseHref).pathname.includes('/pr-preview/')) {
        resolve(false);
        return;
    }

    const workerUrl = new URL('service-worker.js', baseHref).href;
    navigator.serviceWorker.register(workerUrl)
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
