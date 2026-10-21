const historyHandlers = new WeakMap();

export function initHistory(dotnetHelper, phase) {
  history.replaceState(
    { ...history.state, pzFeeding: { phase } },
    '',
  );

  const onPopState = (event) => {
    const path = location.pathname.replace(/\/+$/, '').toLowerCase();
    if (!path.endsWith('/feeding')) {
      return;
    }

    const state = event.state?.pzFeeding;
    dotnetHelper.invokeMethodAsync(
      'OnBrowserPopState',
      state?.phase ?? 'Setup',
    );
  };

  window.addEventListener('popstate', onPopState);
  historyHandlers.set(dotnetHelper, onPopState);
}

export function pushPhaseState(phase) {
  history.pushState({ ...history.state, pzFeeding: { phase } }, '');
}

export function replacePhaseState(phase) {
  history.replaceState({ ...history.state, pzFeeding: { phase } }, '');
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
