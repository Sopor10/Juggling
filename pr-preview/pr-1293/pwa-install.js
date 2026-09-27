(function () {
    'use strict';

    const productionHost = 'siteswaps.passing.zone';

    function isLiveProduction() {
        if (window.location.hostname !== productionHost) {
            return false;
        }

        const baseHref = document.querySelector('base')?.getAttribute('href') || '';
        const path = `${window.location.pathname}${baseHref}`;
        return !path.includes('/pr-preview/');
    }

    function isInstalled() {
        return window.matchMedia('(display-mode: standalone)').matches
            || window.matchMedia('(display-mode: minimal-ui)').matches
            || window.matchMedia('(display-mode: window-controls-overlay)').matches
            || window.navigator.standalone === true;
    }

    const enabled = isLiveProduction() && !isInstalled();
    let deferredPrompt = null;
    let subscriber = null;

    function canPrompt() {
        return enabled && deferredPrompt !== null && !isInstalled();
    }

    function notify() {
        if (!subscriber) {
            return;
        }

        subscriber.invokeMethodAsync('OnInstallAvailabilityChanged', canPrompt());
    }

    if (enabled) {
        window.addEventListener('beforeinstallprompt', (event) => {
            event.preventDefault();
            deferredPrompt = event;
            notify();
        });

        window.addEventListener('appinstalled', () => {
            deferredPrompt = null;
            notify();
        });
    }

    window.pwaInstall = {
        canPrompt,
        subscribe(dotNetRef) {
            subscriber = dotNetRef;
            notify();
        },
        unsubscribe() {
            subscriber = null;
        },
        async prompt() {
            if (!deferredPrompt) {
                return 'unavailable';
            }

            const event = deferredPrompt;
            deferredPrompt = null;
            event.prompt();
            const choice = await event.userChoice;
            notify();
            return choice.outcome;
        }
    };
})();
