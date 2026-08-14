export function scrollIntoView(selector) {
  document.querySelector(selector)?.scrollIntoView({
    behavior: 'smooth',
    block: 'start',
  });
}

const historyHandlers = new WeakMap();
const focusTrapHandlers = new WeakMap();
let previousActiveElement = null;

const focusableSelector = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',');

export function activateFocusTrap(element) {
  if (!element || focusTrapHandlers.has(element)) {
    return;
  }

  const onKeyDown = (event) => {
    if (event.key !== 'Tab') {
      return;
    }

    const focusable = [...element.querySelectorAll(focusableSelector)].filter(
      (candidate) =>
        candidate instanceof HTMLElement &&
        candidate.getClientRects().length > 0,
    );

    if (focusable.length === 0) {
      event.preventDefault();
      element.focus();
      return;
    }

    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  };

  element.addEventListener('keydown', onKeyDown);
  focusTrapHandlers.set(element, onKeyDown);
}

export function deactivateFocusTrap(element) {
  const handler = focusTrapHandlers.get(element);
  if (handler) {
    element.removeEventListener('keydown', handler);
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
    if (!location.pathname.toLowerCase().includes('/wizard')) {
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
