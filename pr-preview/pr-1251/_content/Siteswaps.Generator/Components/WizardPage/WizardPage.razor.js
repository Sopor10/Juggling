export function scrollIntoView(selector) {
  document.querySelector(selector)?.scrollIntoView({
    behavior: 'smooth',
    block: 'start',
  });
}

const historyHandlers = new WeakMap();
const focusTrapHandlers = new WeakMap();
const touchSwipeHandlers = new WeakMap();
let lastSwipeElement = null;
let previousActiveElement = null;

const SWIPE_DIRECTION_THRESHOLD = 10;
const SWIPE_THRESHOLD = 50;
const SWIPE_DIRECTION_FACTOR = 1.2;
const SWIPE_ANGLE_FACTOR = 1.5;

function isSwipeBlockedTarget(target) {
  return (
    target instanceof Element &&
    !!target.closest('[data-wizard-no-swipe], input[type="range"]')
  );
}

export function initTouchSwipe(element, dotnetHelper) {
  if (!element) {
    return;
  }

  if (lastSwipeElement && lastSwipeElement !== element) {
    disposeTouchSwipe(lastSwipeElement);
  }

  if (touchSwipeHandlers.has(element)) {
    lastSwipeElement = element;
    return;
  }

  let startX = 0;
  let startY = 0;
  let horizontalSwipe = false;
  let tracking = false;

  const reset = () => {
    startX = 0;
    startY = 0;
    horizontalSwipe = false;
    tracking = false;
    element.classList.remove('wizard-swipe-active');
  };

  const onTouchStart = (event) => {
    if (event.touches.length !== 1 || isSwipeBlockedTarget(event.target)) {
      reset();
      return;
    }

    startX = event.touches[0].clientX;
    startY = event.touches[0].clientY;
    horizontalSwipe = false;
    tracking = true;
  };

  const onTouchMove = (event) => {
    if (!tracking || event.touches.length !== 1) {
      return;
    }

    const dx = event.touches[0].clientX - startX;
    const dy = event.touches[0].clientY - startY;
    if (
      !horizontalSwipe &&
      Math.abs(dx) > SWIPE_DIRECTION_THRESHOLD &&
      Math.abs(dx) > Math.abs(dy) * SWIPE_DIRECTION_FACTOR
    ) {
      horizontalSwipe = true;
      element.classList.add('wizard-swipe-active');
    }

    if (horizontalSwipe) {
      event.preventDefault();
    }
  };

  const onTouchEnd = (event) => {
    if (!tracking || !horizontalSwipe || event.changedTouches.length !== 1) {
      reset();
      return;
    }

    const touch = event.changedTouches[0];
    const dx = touch.clientX - startX;
    const dy = touch.clientY - startY;
    const isSwipe =
      Math.abs(dx) >= SWIPE_THRESHOLD &&
      Math.abs(dx) > Math.abs(dy) * SWIPE_ANGLE_FACTOR;

    reset();
    if (isSwipe) {
      dotnetHelper.invokeMethodAsync('OnTouchSwipe', dx < 0);
    }
  };

  const onTouchCancel = () => reset();

  element.addEventListener('touchstart', onTouchStart, { passive: true });
  element.addEventListener('touchmove', onTouchMove, { passive: false, capture: true });
  element.addEventListener('touchend', onTouchEnd, { passive: true });
  element.addEventListener('touchcancel', onTouchCancel, { passive: true });

  lastSwipeElement = element;
  touchSwipeHandlers.set(element, () => {
    element.removeEventListener('touchstart', onTouchStart);
    element.removeEventListener('touchmove', onTouchMove, true);
    element.removeEventListener('touchend', onTouchEnd);
    element.removeEventListener('touchcancel', onTouchCancel);
    reset();
  });
}

export function disposeTouchSwipe(element) {
  const cleanup = touchSwipeHandlers.get(element);
  if (cleanup) {
    cleanup();
    touchSwipeHandlers.delete(element);
  }
  if (lastSwipeElement === element) {
    lastSwipeElement = null;
  }
}

const focusableSelector = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',');

function getFocusableWithin(element) {
  return [...element.querySelectorAll(focusableSelector)].filter(
    (candidate) =>
      candidate instanceof HTMLElement &&
      candidate.getClientRects().length > 0,
  );
}

export function activateFocusTrap(element) {
  if (!element || focusTrapHandlers.has(element)) {
    return;
  }

  const onKeyDown = (event) => {
    if (event.key === 'Escape') {
      const backdrop = element.previousElementSibling;
      if (backdrop instanceof HTMLElement && backdrop.classList.contains('wizard-sheet-backdrop')) {
        backdrop.click();
      }
      return;
    }

    if (event.key !== 'Tab') {
      return;
    }

    const focusable = getFocusableWithin(element);

    if (focusable.length === 0) {
      event.preventDefault();
      element.focus();
      return;
    }

    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    const active = document.activeElement;

    if (!(active instanceof Node) || !element.contains(active)) {
      event.preventDefault();
      first.focus();
      return;
    }

    const activeIndex = focusable.indexOf(active);
    if (activeIndex === -1) {
      event.preventDefault();
      if (event.shiftKey) {
        last.focus();
      } else {
        first.focus();
      }
      return;
    }

    if (event.shiftKey && active === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && active === last) {
      event.preventDefault();
      first.focus();
    }
  };

  document.addEventListener('keydown', onKeyDown, true);
  focusTrapHandlers.set(element, onKeyDown);
}

export function deactivateFocusTrap(element) {
  const handler = focusTrapHandlers.get(element);
  if (handler) {
    document.removeEventListener('keydown', handler, true);
    focusTrapHandlers.delete(element);
  }
}

export function captureActiveElement() {
  previousActiveElement = document.activeElement;
}

export function restoreActiveElement() {
  if (previousActiveElement instanceof HTMLElement) {
    previousActiveElement.focus();
  }
  previousActiveElement = null;
}

export function focusElement(id) {
  document.getElementById(id)?.focus();
}

export function initHistory(dotnetHelper, step) {
  history.replaceState(
    { ...history.state, pzWizard: { step, isResults: false } },
    '',
  );

  const onPopState = (event) => {
    const path = location.pathname.replace(/\/+$/, '').toLowerCase();
    const isWizardPath =
      path === '' ||
      path === '/' ||
      path === '/wizard' ||
      path.endsWith('/wizard');
    if (!isWizardPath) {
      return;
    }

    const state = event.state?.pzWizard;
    dotnetHelper.invokeMethodAsync(
      'OnBrowserPopState',
      state?.step ?? 0,
      state?.isResults ?? false,
    );
  };

  window.addEventListener('popstate', onPopState);
  historyHandlers.set(dotnetHelper, onPopState);
}

export function pushEditorState(step) {
  history.pushState(
    { ...history.state, pzWizard: { step, isResults: false } },
    '',
  );
}

export function pushResultsState(step) {
  history.pushState(
    { ...history.state, pzWizard: { step, isResults: true } },
    '',
  );
}

export function replaceEditorState(step) {
  history.replaceState(
    { ...history.state, pzWizard: { step, isResults: false } },
    '',
  );
}

export function back() {
  history.back();
}

export function disposeHistory(dotnetHelper) {
  const handler = historyHandlers.get(dotnetHelper);
  if (handler) {
    window.removeEventListener('popstate', handler);
    historyHandlers.delete(dotnetHelper);
  }
}
