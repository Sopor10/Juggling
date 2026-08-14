// Minimal pointer-drag interop for the Card-Stack dual range slider.
// Mirrors the drag mechanics of design-mockups/01-card-stack.html (makeDraggable),
// re-implemented from scratch for this isolated component.

const state = new WeakMap();

// Half the thumb's width/height (see DualRangeSlider.razor.css: 40px thumb,
// centered via margin-left/-top:-20px). The thumb's own center - not the
// container's raw left edge - is what "0%"/"100%" must map to, otherwise the
// value jumps the moment you grab a thumb that isn't already at dead center.
const THUMB_RADIUS = 20;

function pointerPercent(container, clientX) {
  const rect = container.getBoundingClientRect();
  const usableWidth = rect.width - 2 * THUMB_RADIUS;
  const pct = usableWidth <= 0
    ? 0
    : (clientX - rect.left - THUMB_RADIUS) / usableWidth;
  return Math.min(1, Math.max(0, pct));
}

export function init(container, thumbMin, thumbMax, dotnetHelper) {
  const cleanupFns = [];

  const makeDraggable = (thumb, isMin) => {
    const onPointerDown = (ev) => {
      // Handled directly by the thumb - stop it from also bubbling to the
      // container's tap-to-jump handler below.
      ev.stopPropagation();
      thumb.setPointerCapture(ev.pointerId);
      const onPointerMove = (moveEv) => {
        dotnetHelper.invokeMethodAsync('OnDrag', isMin, pointerPercent(container, moveEv.clientX));
      };
      const onPointerUp = () => {
        window.removeEventListener('pointermove', onPointerMove);
        window.removeEventListener('pointerup', onPointerUp);
      };
      window.addEventListener('pointermove', onPointerMove);
      window.addEventListener('pointerup', onPointerUp);
    };
    thumb.addEventListener('pointerdown', onPointerDown);
    cleanupFns.push(() => thumb.removeEventListener('pointerdown', onPointerDown));
  };

  makeDraggable(thumbMin, true);
  makeDraggable(thumbMax, false);

  // Tap-to-jump: tapping the track itself (not a thumb, which stops
  // propagation above) moves whichever thumb is nearest to the tap straight
  // to that position, instead of doing nothing.
  const onTrackPointerDown = (ev) => {
    const pct = pointerPercent(container, ev.clientX);
    const minLeft = parseFloat(thumbMin.style.left) / 100;
    const maxLeft = parseFloat(thumbMax.style.left) / 100;
    const isMin = Math.abs(pct - minLeft) <= Math.abs(pct - maxLeft);
    dotnetHelper.invokeMethodAsync('OnDrag', isMin, pct);
  };
  container.addEventListener('pointerdown', onTrackPointerDown);
  cleanupFns.push(() => container.removeEventListener('pointerdown', onTrackPointerDown));

  state.set(container, cleanupFns);
}

export function dispose(container) {
  const cleanupFns = state.get(container);
  if (cleanupFns) {
    cleanupFns.forEach((fn) => fn());
    state.delete(container);
  }
}
