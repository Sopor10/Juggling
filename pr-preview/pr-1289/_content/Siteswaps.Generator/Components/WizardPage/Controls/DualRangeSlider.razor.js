// Tap-to-jump for the Wizard dual range slider (native range inputs).

const cleanups = new WeakMap();

const THUMB_RADIUS = 20;

function pointerPercent(container, clientX, lowerBound, upperBound) {
  const rect = container.getBoundingClientRect();
  const usableWidth = rect.width - 2 * THUMB_RADIUS;
  const pct = usableWidth <= 0
    ? 0
    : (clientX - rect.left - THUMB_RADIUS) / usableWidth;
  const clamped = Math.min(1, Math.max(0, pct));
  // Inclusive: pct=1 must resolve to upperBound, not upperBound-1.
  const value = Math.round(lowerBound + clamped * (upperBound - lowerBound));
  return Math.min(upperBound, Math.max(lowerBound, value));
}

function relayoutRange(input) {
  if (input.getBoundingClientRect().width <= 0) {
    return;
  }

  const next = input.value;
  input.value = input.min;
  input.value = next;
}

export function refreshThumbs(minInput, maxInput) {
  relayoutRange(minInput);
  relayoutRange(maxInput);
}

export function initTrackTap(container, minInput, maxInput, lowerBound, upperBound) {
  const onPointerDown = (ev) => {
    if (ev.target === minInput || ev.target === maxInput) {
      return;
    }

    const value = pointerPercent(container, ev.clientX, lowerBound, upperBound);
    const minVal = parseInt(minInput.value, 10);
    const maxVal = parseInt(maxInput.value, 10);
    const target = Math.abs(value - minVal) <= Math.abs(value - maxVal) ? minInput : maxInput;
    target.value = String(value);
    target.dispatchEvent(new Event('input', { bubbles: true }));
  };

  const onResize = () => refreshThumbs(minInput, maxInput);
  const observer = new ResizeObserver(onResize);
  observer.observe(container);

  container.addEventListener('pointerdown', onPointerDown);
  cleanups.set(container, () => {
    container.removeEventListener('pointerdown', onPointerDown);
    observer.disconnect();
  });
  onResize();
}

export function dispose(container) {
  cleanups.get(container)?.();
  cleanups.delete(container);
}
