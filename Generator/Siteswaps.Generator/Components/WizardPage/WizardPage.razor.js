export function scrollIntoView(selector) {
  document.querySelector(selector)?.scrollIntoView({
    behavior: 'smooth',
    block: 'start',
  });
}

const historyHandlers = new WeakMap();
let previousActiveElement = null;

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
