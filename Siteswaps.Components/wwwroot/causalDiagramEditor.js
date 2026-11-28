let activeMove;
let activePressCleanup;
const outsideCloseHandlers = new WeakMap();
const touchDragHandlers = new WeakMap();
const diagramScrollCueHandlers = new WeakMap();
const orbitRadioHandlers = new WeakMap();
const savedEditorPresetsKey = "siteswaps.causal-diagram-editor.presets.v1";

export function getSavedEditorPresets() {
    try {
        const presets = JSON.parse(localStorage.getItem(savedEditorPresetsKey) || "[]");
        return Array.isArray(presets) ? presets.filter(isSavedEditorPreset) : [];
    } catch {
        return [];
    }
}

export function saveEditorPreset(preset) {
    if (!isSavedEditorPreset(preset)) throw new Error("Ungültige Diagrammvorlage.");
    const presets = getSavedEditorPresets().filter(item => item.id !== preset.id);
    presets.unshift(preset);
    localStorage.setItem(savedEditorPresetsKey, JSON.stringify(presets.slice(0, 30)));
    return getSavedEditorPresets();
}

export function deleteEditorPreset(id) {
    const presets = getSavedEditorPresets().filter(item => item.id !== id);
    localStorage.setItem(savedEditorPresetsKey, JSON.stringify(presets));
    return presets;
}

function isSavedEditorPreset(value) {
    return value && typeof value.id === "string" && typeof value.name === "string"
        && typeof value.stateToken === "string" && value.name.trim().length > 0;
}

function capturePointer(element, pointerId) {
    try {
        element.setPointerCapture(pointerId);
    } catch {
        // The pointer may already have ended while Blazor was dispatching pointerdown.
    }
}

function releasePointer(element, pointerId) {
    try {
        if (element.hasPointerCapture(pointerId)) element.releasePointerCapture(pointerId);
    } catch {
        // Pointer capture is already gone after pointerup or pointercancel.
    }
}

export function bindTouchDragTargets(svg) {
    if (!svg || touchDragHandlers.has(svg)) return;
    const activeTouchGuards = new Map();
    const keyDown = event => {
        const control = event.target.closest?.(
            ".throw-hit-target, .arrow-target-handle, .juggler-add-target, .throw-delete-action, .throw-menu-action, .throw-hand-menu [role=button]"
        );
        if (!control || !svg.contains(control)) return;
        const keys = ["Enter", " "];
        if (control.matches(".arrow-target-handle"))
            keys.push("ArrowLeft", "ArrowRight");
        else if (control.matches(".throw-hit-target"))
            keys.push("Escape", "ArrowLeft", "ArrowRight", "Delete", "Backspace");
        if (keys.includes(event.key)) event.preventDefault();
    };
    svg.addEventListener("keydown", keyDown, true);
    const pointerDown = event => {
        if (event.pointerType === "mouse" && !window.matchMedia("(pointer: coarse)").matches) return;
        const target = event.target.closest?.(".throw-hit-target, .arrow-target-handle, .juggler-add-target");
        if (!target || !svg.contains(target)) return;
        event.preventDefault();
        if (!target.matches(".arrow-target-handle")) return;

        // Start capture and suppress native scrolling synchronously, before
        // Blazor's pointerdown callback can round-trip through JS interop.
        const touchInput = event.pointerType !== "mouse" || window.matchMedia("(pointer: coarse)").matches;
        if (!touchInput || activeTouchGuards.has(event.pointerId)) return;
        capturePointer(svg, event.pointerId);
        const preventTouchScroll = touchEvent => touchEvent.preventDefault();
        const cleanup = release => {
            document.removeEventListener("touchmove", preventTouchScroll, true);
            document.removeEventListener("pointerup", end, true);
            document.removeEventListener("pointercancel", end, true);
            activeTouchGuards.delete(event.pointerId);
            if (release) releasePointer(svg, event.pointerId);
        };
        const end = endEvent => {
            if (endEvent.pointerId === event.pointerId) cleanup(false);
        };
        document.addEventListener("touchmove", preventTouchScroll, { capture: true, passive: false });
        document.addEventListener("pointerup", end, true);
        document.addEventListener("pointercancel", end, true);
        activeTouchGuards.set(event.pointerId, () => cleanup(true));
    };
    svg.addEventListener("pointerdown", pointerDown, { capture: true, passive: false });
    touchDragHandlers.set(svg, { pointerDown, keyDown, activeTouchGuards });
}

export function unbindTouchDragTargets(svg) {
    const binding = touchDragHandlers.get(svg);
    if (!binding) return;
    svg.removeEventListener("pointerdown", binding.pointerDown, true);
    svg.removeEventListener("keydown", binding.keyDown, true);
    for (const cleanup of binding.activeTouchGuards.values()) cleanup();
    touchDragHandlers.delete(svg);
}

export function bindDiagramScrollCue(svg) {
    const frame = svg?.closest(".diagram-scroll-frame");
    const scroller = frame?.querySelector(".diagram-scroll");
    if (!frame || !scroller || diagramScrollCueHandlers.has(svg)) return;
    const update = () => frame.classList.toggle("has-horizontal-overflow", scroller.scrollWidth > scroller.clientWidth + 1);
    const observer = new ResizeObserver(update);
    observer.observe(scroller);
    observer.observe(svg);
    scroller.addEventListener("scroll", update, { passive: true });
    update();
    diagramScrollCueHandlers.set(svg, { frame, scroller, observer, update });
}

export function unbindDiagramScrollCue(svg) {
    const binding = diagramScrollCueHandlers.get(svg);
    if (!binding) return;
    binding.observer.disconnect();
    binding.scroller.removeEventListener("scroll", binding.update);
    binding.frame.classList.remove("has-horizontal-overflow");
    diagramScrollCueHandlers.delete(svg);
}

export function bindOrbitRadioKeys(group) {
    if (!group || orbitRadioHandlers.has(group)) return;
    const keyDown = event => {
        const radios = [...group.querySelectorAll('[role="radio"]')];
        const currentIndex = radios.indexOf(event.target.closest?.('[role="radio"]'));
        if (currentIndex < 0) return;
        const nextIndex = event.key === "ArrowRight" || event.key === "ArrowDown"
            ? (currentIndex + 1) % radios.length
            : event.key === "ArrowLeft" || event.key === "ArrowUp"
                ? (currentIndex + radios.length - 1) % radios.length
                : event.key === "Home"
                    ? 0
                    : event.key === "End"
                        ? radios.length - 1
                        : -1;
        if (nextIndex < 0) return;
        event.preventDefault();
        radios[nextIndex].focus();
        radios[nextIndex].click();
    };
    group.addEventListener("keydown", keyDown);
    orbitRadioHandlers.set(group, keyDown);
}

export function unbindOrbitRadioKeys(group) {
    const keyDown = orbitRadioHandlers.get(group);
    if (!keyDown) return;
    group.removeEventListener("keydown", keyDown);
    orbitRadioHandlers.delete(group);
}

export function bindOutsideClose(element) {
    if (!element || outsideCloseHandlers.has(element)) return;
    const pointerDown = event => {
        if (element.open && !element.contains(event.target)) element.open = false;
    };
    document.addEventListener("pointerdown", pointerDown, true);
    outsideCloseHandlers.set(element, pointerDown);
}

export function closeDiagramOptionsOnMobile(element) {
    if (element && window.matchMedia("(max-width: 700px)").matches) element.open = false;
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

export function startThrowInteraction(svg, dotnet, id, cycle, pointerId, originX, originY, pointerType, editingAlready) {
    stopDrag();
    const touchInput = pointerType !== "mouse" || window.matchMedia("(pointer: coarse)").matches;
    // Blazor rerenders the SVG after each moved throw. Keep capture on this
    // stable ancestor so Chrome does not lose the touch stream with the node.
    if (touchInput) capturePointer(svg, pointerId);
    const target = [...svg.querySelectorAll(".throw-hit-target")]
        .find(item => item.dataset.throwId === id && item.dataset.cycle === String(cycle));
    let editing = editingAlready || touchInput;
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
        releasePointer(svg, pointerId);
        document.removeEventListener("pointermove", move);
        document.removeEventListener("pointerup", up, true);
        document.removeEventListener("pointercancel", up);
        activeMove = undefined;
    };
    document.addEventListener("pointermove", move, { passive: false });
    document.addEventListener("pointerup", up, true);
    document.addEventListener("pointercancel", up);
    if (touchInput && !editingAlready) {
        dotnet.invokeMethodAsync("EnableThrowEditing", id, cycle);
    } else if (!editing) {
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

export function startEdgeDrag(svg, dotnet, edgeId, sourceCycle, pointerId, pointerType) {
    stopDrag();
    const touchInput = pointerType !== "mouse" || window.matchMedia("(pointer: coarse)").matches;
    if (touchInput) capturePointer(svg, pointerId);
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
        releasePointer(svg, pointerId);
        document.removeEventListener("pointermove", move);
        document.removeEventListener("pointerup", up, true);
        document.removeEventListener("pointercancel", cancel);
        activeMove = undefined;
    };
    document.addEventListener("pointermove", move, { passive: false });
    document.addEventListener("pointerup", up, true);
    document.addEventListener("pointercancel", cancel);
}
