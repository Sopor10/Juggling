let activeMove;
let activePressCleanup;
const outsideCloseHandlers = new WeakMap();

export function bindOutsideClose(element) {
    if (!element || outsideCloseHandlers.has(element)) return;
    const pointerDown = event => {
        if (element.open && !element.contains(event.target)) element.open = false;
    };
    document.addEventListener("pointerdown", pointerDown, true);
    outsideCloseHandlers.set(element, pointerDown);
}

export function unbindOutsideClose(element) {
    const pointerDown = outsideCloseHandlers.get(element);
    if (!pointerDown) return;
    document.removeEventListener("pointerdown", pointerDown, true);
    outsideCloseHandlers.delete(element);
}

export function startJugglerHold(dotnet, jugglerId, pointerId, originX, originY) {
    stopJugglerHold();
    const target = [...document.querySelectorAll(".juggler-add-target")]
        .find(item => item.dataset.jugglerId === jugglerId);
    target?.classList.add("pressing");
    let timer;
    const cleanup = () => {
        clearTimeout(timer);
        target?.classList.remove("pressing");
        document.removeEventListener("pointermove", move);
        document.removeEventListener("pointerup", end, true);
        document.removeEventListener("pointercancel", end, true);
        document.removeEventListener("contextmenu", preventContextMenu, true);
        if (activePressCleanup === cleanup) activePressCleanup = undefined;
    };
    const move = event => {
        if (event.pointerId !== pointerId) return;
        if (Math.hypot(event.clientX - originX, event.clientY - originY) > 12) cleanup();
    };
    const end = event => {
        if (event.pointerId === pointerId) cleanup();
    };
    const preventContextMenu = event => {
        if (event.target.closest?.(".juggler-add-target")) event.preventDefault();
    };
    activePressCleanup = cleanup;
    document.addEventListener("pointermove", move, { passive: true });
    document.addEventListener("pointerup", end, true);
    document.addEventListener("pointercancel", end, true);
    document.addEventListener("contextmenu", preventContextMenu, true);
    timer = setTimeout(() => {
        cleanup();
        dotnet.invokeMethodAsync("AddThrowForJuggler", jugglerId);
    }, 550);
}

export function stopJugglerHold() {
    if (activePressCleanup) activePressCleanup();
}

export function startThrowInteraction(svg, dotnet, id, cycle, pointerId, originX, originY, editingAlready) {
    stopDrag();
    const target = [...svg.querySelectorAll(".throw-hit-target")]
        .find(item => item.dataset.throwId === id && item.dataset.cycle === String(cycle));
    let editing = editingAlready;
    let timer;
    if (!editing) target?.classList.add("pressing");
    const move = event => {
        if (event.pointerId !== pointerId) return;
        if (!editing) {
            if (Math.hypot(event.clientX - originX, event.clientY - originY) > 12) stopDrag();
            return;
        }
        const bounds = svg.getBoundingClientRect();
        if (bounds.width <= 0) return;
        const normalizedX = (event.clientX - bounds.left) / bounds.width;
        dotnet.invokeMethodAsync("MoveThrow", id, cycle, normalizedX);
        event.preventDefault();
    };
    const up = event => {
        if (event.pointerId === pointerId) stopDrag();
    };
    activeMove = () => {
        clearTimeout(timer);
        target?.classList.remove("pressing");
        document.removeEventListener("pointermove", move);
        document.removeEventListener("pointerup", up, true);
        document.removeEventListener("pointercancel", up);
        activeMove = undefined;
    };
    document.addEventListener("pointermove", move, { passive: false });
    document.addEventListener("pointerup", up, true);
    document.addEventListener("pointercancel", up);
    if (!editing) {
        timer = setTimeout(() => {
            editing = true;
            target?.classList.remove("pressing");
            const suppressReleaseClick = event => {
                document.removeEventListener("click", suppressReleaseClick, true);
                if (event.target !== target) return;
                event.preventDefault();
                event.stopImmediatePropagation();
            };
            document.addEventListener("click", suppressReleaseClick, true);
            setTimeout(() => document.removeEventListener("click", suppressReleaseClick, true), 1200);
            dotnet.invokeMethodAsync("EnableThrowEditing", id, cycle);
        }, 550);
    }
}

export function stopDrag() {
    if (activeMove) activeMove();
}

export function startEdgeDrag(svg, dotnet, edgeId, sourceCycle, pointerId) {
    stopDrag();
    const move = event => {
        if (event.pointerId !== pointerId) return;
        const bounds = svg.getBoundingClientRect();
        if (bounds.width <= 0 || bounds.height <= 0) return;
        dotnet.invokeMethodAsync(
            "PreviewEdgeDrop",
            edgeId,
            sourceCycle,
            (event.clientX - bounds.left) / bounds.width,
            (event.clientY - bounds.top) / bounds.height
        );
        event.preventDefault();
    };
    const up = event => {
        if (event.pointerId !== pointerId) return;
        const bounds = svg.getBoundingClientRect();
        if (bounds.width > 0 && bounds.height > 0) {
            const normalizedX = (event.clientX - bounds.left) / bounds.width;
            const normalizedY = (event.clientY - bounds.top) / bounds.height;
            dotnet.invokeMethodAsync("CompleteEdgeDrop", edgeId, sourceCycle, normalizedX, normalizedY);
        }
        stopDrag();
    };
    const cancel = event => {
        if (event.pointerId !== pointerId) return;
        dotnet.invokeMethodAsync("CancelEdgeDrop");
        stopDrag();
    };
    activeMove = () => {
        document.removeEventListener("pointermove", move);
        document.removeEventListener("pointerup", up, true);
        document.removeEventListener("pointercancel", cancel);
        activeMove = undefined;
    };
    document.addEventListener("pointermove", move, { passive: false });
    document.addEventListener("pointerup", up, true);
    document.addEventListener("pointercancel", cancel);
}
