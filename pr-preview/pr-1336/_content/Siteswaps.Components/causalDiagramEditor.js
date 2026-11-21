let activeMove;

export function startDrag(svg, dotnet, id, cycle, pointerId) {
    stopDrag();
    const move = event => {
        if (event.pointerId !== pointerId) return;
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
        document.removeEventListener("pointermove", move);
        document.removeEventListener("pointerup", up, true);
        document.removeEventListener("pointercancel", up);
        activeMove = undefined;
    };
    document.addEventListener("pointermove", move, { passive: false });
    document.addEventListener("pointerup", up, true);
    document.addEventListener("pointercancel", up);
}

export function stopDrag() {
    if (activeMove) activeMove();
}

export function startEdgeDrag(svg, dotnet, edgeId, sourceCycle, pointerId) {
    stopDrag();
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
        if (event.pointerId === pointerId) stopDrag();
    };
    activeMove = () => {
        document.removeEventListener("pointerup", up, true);
        document.removeEventListener("pointercancel", cancel);
        activeMove = undefined;
    };
    document.addEventListener("pointerup", up, true);
    document.addEventListener("pointercancel", cancel);
}
