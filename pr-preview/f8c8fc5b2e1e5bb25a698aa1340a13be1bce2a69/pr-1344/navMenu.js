let outsidePointerDown;

export function watchOutsideClicks(menu, toggleButton, dotnet) {
    stopWatchingOutsideClicks();
    outsidePointerDown = event => {
        if (menu?.contains(event.target) || toggleButton?.contains(event.target)) return;
        dotnet.invokeMethodAsync("CloseMenuFromOutside");
    };
    document.addEventListener("pointerdown", outsidePointerDown, true);
}

export function stopWatchingOutsideClicks() {
    if (!outsidePointerDown) return;
    document.removeEventListener("pointerdown", outsidePointerDown, true);
    outsidePointerDown = undefined;
}
