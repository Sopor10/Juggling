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
    const params = new URLSearchParams(location.search);
    dotnetHelper.invokeMethodAsync(
      'OnBrowserPopState',
      state?.phase ?? 'Setup',
      params.get('state') ?? '',
    );
  };

  window.addEventListener('popstate', onPopState);
  historyHandlers.set(dotnetHelper, onPopState);
}

export function pushPhaseState(phase, url) {
  history.pushState({ ...history.state, pzFeeding: { phase } }, '');
  if (url) history.replaceState(history.state, '', url);
}

export function replacePhaseState(phase, url) {
  history.replaceState({ ...history.state, pzFeeding: { phase } }, '');
  if (url) history.replaceState(history.state, '', url);
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
