// Scroll/footer helpers for the Card Stack page.

export function measureFooterHeight(footerEl, pageEl) {
  if (!footerEl || !pageEl) {
    return;
  }

  const height = footerEl.getBoundingClientRect().height;
  pageEl.style.setProperty('--cs-footer-height', `${Math.ceil(height)}px`);
}

export function scrollIntoView(selector) {
  document.querySelector(selector)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

export function initFooterObserver(footerEl, pageEl) {
  if (!footerEl || !pageEl) {
    return null;
  }

  const update = () => measureFooterHeight(footerEl, pageEl);
  update();

  const ro = typeof ResizeObserver !== 'undefined'
    ? new ResizeObserver(update)
    : null;
  ro?.observe(footerEl);

  window.addEventListener('resize', update);

  return {
    dispose() {
      ro?.disconnect();
      window.removeEventListener('resize', update);
    },
  };
}

const historyHandlers = new WeakMap();

export function initHistory(dotnetHelper) {
  history.replaceState(
    { ...history.state, pzCardStack: { showResults: false } },
    '',
  );

  const onPopState = (event) => {
    if (!location.pathname.toLowerCase().includes('/cardstack')) {
      return;
    }

    dotnetHelper.invokeMethodAsync(
      'OnBrowserPopState',
      event.state?.pzCardStack?.showResults ?? false,
    );
  };

  window.addEventListener('popstate', onPopState);
  historyHandlers.set(dotnetHelper, onPopState);
}

export function pushResultsState() {
  const state = { ...history.state, pzCardStack: { showResults: true } };
  if (history.state?.pzCardStack?.showResults) {
    history.replaceState(state, '');
  } else {
    history.pushState(state, '');
  }
}

export function disposeHistory(dotnetHelper) {
  const handler = historyHandlers.get(dotnetHelper);
  if (handler) {
    window.removeEventListener('popstate', handler);
    historyHandlers.delete(dotnetHelper);
  }
}
