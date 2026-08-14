// Shared demo helpers for the Passing Zone siteswap-generator mockups.
// These mockups are click-dummies: no real siteswap math, just believable
// live feedback so the interaction patterns can be judged.

// Mirrors Generator/Siteswaps.Generator/Components/State/GeneratorState.cs (record Throw).
// Named throws keep their name as the default label; unnamed heights fall back to the number,
// exactly like Throw.GetDisplayValue(showName) does in the real app.
const PZ_THROWS = [
  { height: 1, name: null },
  { height: 2, name: 'Zip' },
  { height: 3, name: null },
  { height: 4, name: 'Hold' },
  { height: 5, name: 'Zap' },
  { height: 6, name: 'Self' },
  { height: 7, name: 'Single' },
  { height: 8, name: 'Heff' },
  { height: 9, name: 'Double' },
  { height: 10, name: 'Triple S' },
  { height: 11, name: 'Triple' },
  { height: 12, name: 'Quad' },
];

/** Default selection for the demo: a typical passing range (Hold..Heff). */
const PZ_DEFAULT_THROW_HEIGHTS = [3, 4, 5, 6, 7, 8];

function pzThrowLabel(height, showNames) {
  const t = PZ_THROWS.find(x => x.height === height);
  if (showNames && t && t.name) return t.name;
  return String(height);
}

/**
 * Builds and manages a "Würfe" chip grid. Default label mode is names
 * (Heff, Zip, Self, ...) as requested; a small toggle lets the user switch
 * to plain numbers, mirroring the real Settings.ShowThrowNames option.
 */
function pzCreateThrowsController(gridEl, options) {
  const chipClass = options.chipClass || 'pz-chip py-2 text-sm';
  const activeHeights = new Set(options.defaultActive || PZ_DEFAULT_THROW_HEIGHTS);
  let showNames = options.showNames !== false;

  gridEl.setAttribute('role', 'group');
  gridEl.setAttribute('aria-label', pzCurrentLang === 'de' ? 'Erlaubte Würfe' : 'Allowed throws');

  function render() {
    gridEl.innerHTML = '';
    PZ_THROWS.forEach(t => {
      const chip = document.createElement('button');
      chip.type = 'button';
      chip.className = chipClass + (activeHeights.has(t.height) ? ' active' : '');
      chip.textContent = pzThrowLabel(t.height, showNames);
      chip.dataset.height = String(t.height);
      const longLabel = t.name ? `${t.name} (Höhe ${t.height})` : `Höhe ${t.height}`;
      chip.title = longLabel;
      chip.setAttribute('aria-pressed', String(activeHeights.has(t.height)));
      chip.setAttribute('aria-label', longLabel);
      chip.addEventListener('click', () => {
        if (activeHeights.has(t.height)) activeHeights.delete(t.height);
        else activeHeights.add(t.height);
        chip.classList.toggle('active');
        chip.setAttribute('aria-pressed', String(activeHeights.has(t.height)));
        if (options.onChange) options.onChange(activeHeights.size);
      });
      gridEl.appendChild(chip);
    });
  }

  render();

  return {
    toggleNames() {
      showNames = !showNames;
      render();
      return showNames;
    },
    selectAll() {
      PZ_THROWS.forEach(t => activeHeights.add(t.height));
      render();
      if (options.onChange) options.onChange(activeHeights.size);
    },
    count() {
      return activeHeights.size;
    },
    isShowingNames() {
      return showNames;
    },
  };
}

/** Deterministic pseudo-random match count so the UI feels alive but stable. */
function pzFakeMatchCount(state) {
  const { jugglers = 3, period = 5, clubsMin = 6, clubsMax = 6, throws = 6, filters = 0 } = state;
  let n = 40000;
  n = n / (jugglers * 1.6);
  n = n / (period * 0.9);
  n = n / (1 + (clubsMax - clubsMin) * 0.3 + clubsMin * 0.5);
  n = n * (throws / 6);
  n = n / (1 + filters * 1.8);
  n = Math.max(0, Math.round(n));
  return n;
}

const pzPrefersReducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function pzPulse(el, duration = 500) {
  if (!el || pzPrefersReducedMotion) return;
  el.classList.add('pz-pulse');
  clearTimeout(el._pzPulseTimer);
  el._pzPulseTimer = setTimeout(() => el.classList.remove('pz-pulse'), duration);
}

function pzAnimateNumber(el, target, duration = 260) {
  if (pzPrefersReducedMotion) {
    el.textContent = target.toLocaleString('de-DE');
    el.dataset.value = String(target);
    return;
  }
  const start = parseInt(el.dataset.value || '0', 10);
  const startTime = performance.now();
  function tick(now) {
    const p = Math.min(1, (now - startTime) / duration);
    const value = Math.round(start + (target - start) * (1 - Math.pow(1 - p, 3)));
    el.textContent = value.toLocaleString('de-DE');
    if (p < 1) requestAnimationFrame(tick);
    else el.dataset.value = String(target);
  }
  requestAnimationFrame(tick);
}

const PZ_SAMPLE_PATTERNS = [
  { name: '5-Count', clubs: 6, meta: 'Feed · 3 Jugglers' },
  { name: 'Every Other', clubs: 6, meta: 'Passing · 2 Jugglers' },
  { name: 'Custom 7531', clubs: 7, meta: 'Passing · 3 Jugglers' },
  { name: 'Tribute', clubs: 9, meta: 'Feed · 4 Jugglers' },
  { name: '408', clubs: 6, meta: 'Passing · 2 Jugglers' },
  { name: 'Ultimate', clubs: 6, meta: 'Passing · 3 Jugglers' },
];

function pzRenderResultCards(container, count) {
  const items = PZ_SAMPLE_PATTERNS.slice(0, Math.min(6, Math.max(2, Math.round(count / 40) || 2)));
  container.innerHTML = items.map(p => `
    <div class="pz-result-card p-3 flex flex-col justify-between h-24">
      <div class="pz-font-display text-base leading-tight">${p.name}</div>
      <div class="text-[11px] opacity-80 flex items-center justify-between">
        <span>${p.meta}</span>
        <span class="bg-white/15 rounded-full px-2 py-0.5">${p.clubs}c</span>
      </div>
    </div>
  `).join('');
}

/* =========================================================================
 * i18n – lightweight DE/EN translation layer.
 * The app itself (passing.zone) is English-first, but the primary audience
 * for these mockups is German-speaking, so both are first-class citizens.
 * Throw names (Heff, Zip, Self, ...) are juggling jargon and stay identical
 * in both languages – only UI copy is translated.
 * ========================================================================= */

const PZ_I18N = {
  'app.eyebrow': { de: 'SITESWAP GENERATOR', en: 'SITESWAP GENERATOR' },

  'section.jugglers': { de: 'Jongleure', en: 'Jugglers' },
  'section.period': { de: 'Periode', en: 'Period' },
  'section.clubs': { de: 'Anzahl Clubs', en: 'Number of clubs' },
  'section.throws': { de: 'Erlaubte Würfe', en: 'Allowed throws' },
  'section.filters': { de: 'Filter', en: 'Filters' },

  'period.hint': { de: 'Wie viele Würfe wiederholen sich?', en: 'How many throws repeat?' },
  'clubs.hint': { de: 'Bereich von–bis ist erlaubt', en: 'A min–max range is allowed' },

  'throws.all': { de: 'Alle', en: 'All' },
  'throws.names': { de: 'Namen', en: 'Names' },
  'throws.numbers': { de: 'Zahlen', en: 'Numbers' },

  'filters.optional': { de: 'optional', en: 'optional' },
  'filters.add': { de: '+ Filter hinzufügen', en: '+ Add filter' },
  'filters.empty': { de: 'Noch keine Filter – das Muster ist noch unbeschränkt.', en: 'No filters yet – the pattern is still unrestricted.' },
  'filters.andHint': { de: 'Filter in einer Gruppe müssen alle zutreffen · Gruppen reichen einzeln', en: 'All filters in a group must match · any single group is enough' },
  'filters.andConnector': { de: 'UND', en: 'AND' },
  'filters.and': { de: 'UND', en: 'AND' },
  'filters.or': { de: 'ODER', en: 'OR' },
  'filters.group': { de: 'Gruppe {n}', en: 'Group {n}' },
  'filters.connectorAndHint': { de: 'UND – zum Wechseln zu ODER antippen', en: 'AND – tap to switch to OR' },
  'filters.connectorOrHint': { de: 'ODER – zum Wechseln zu UND antippen', en: 'OR – tap to switch to AND' },
  'filters.addToGroup': { de: 'Filter zur Gruppe hinzufügen', en: 'Add filter to group' },
  'filters.edit': { de: 'Bearbeiten', en: 'Edit' },
  'filters.remove': { de: 'Entfernen', en: 'Remove' },

  'footer.matches': { de: 'Treffer', en: 'Matches' },
  'footer.generate': { de: 'Generieren →', en: 'Generate →' },
  'footer.generating': { de: 'Generiere…', en: 'Generating…' },
  'footer.estimatedMatches': { de: 'Voraussichtliche Treffer', en: 'Estimated matches' },

  'sheet.titleAdd': { de: 'Filter hinzufügen', en: 'Add filter' },
  'sheet.titleEdit': { de: 'Filter bearbeiten', en: 'Edit filter' },
  'sheet.saveAdd': { de: 'Filter übernehmen', en: 'Add filter' },
  'sheet.saveEdit': { de: 'Änderungen speichern', en: 'Save changes' },
  'sheet.delete': { de: 'Filter entfernen', en: 'Remove filter' },

  'tab.number': { de: 'Anzahl', en: 'Number' },
  'tab.pattern': { de: 'Muster', en: 'Pattern' },
  'tab.state': { de: 'Zustand', en: 'State' },

  'number.hint': { de: 'z. B. „genau 2× Heff“', en: 'e.g. "exactly 2× Heff"' },
  'number.comparisonLabel': { de: 'Vergleich', en: 'Comparison' },
  'number.amountLabel': { de: 'Anzahl', en: 'Amount' },
  'number.throwLabel': { de: 'Wurf', en: 'Throw' },
  'number.comparison.exact': { de: 'Genau', en: 'Exactly' },
  'number.comparison.max': { de: 'Maximal', en: 'At most' },
  'number.comparison.min': { de: 'Mindestens', en: 'At least' },

  'pattern.hint': { de: 'Tippe ein Feld an, dann einen Wurf darunter.', en: 'Tap a slot, then a throw below.' },
  'pattern.rotationLabel': { de: 'Bezug', en: 'Rotation' },
  'pattern.rotation.global': { de: 'Global (ganzer Zyklus)', en: 'Global (whole cycle)' },
  'pattern.rotation.local': { de: 'Lokal – Jongleur {letter}', en: 'Local – juggler {letter}' },
  'pattern.includeLabel': { de: 'Muster soll…', en: 'Pattern should…' },
  'pattern.include.yes': { de: 'enthalten sein', en: 'be included' },
  'pattern.include.no': { de: 'ausgeschlossen sein', en: 'be excluded' },
  'pattern.includesWord': { de: 'enthält', en: 'includes' },
  'pattern.excludesWord': { de: 'ohne', en: 'without' },
  'pattern.empty': { de: 'leer', en: 'empty' },

  'state.hint': { de: 'Welche zukünftigen Würfe sind belegt?', en: 'Which upcoming beats are occupied?' },
  'state.beat': { de: 'Beat {n}', en: 'Beat {n}' },

  'wizard.stepOf': { de: 'Schritt {n} / {total}', en: 'Step {n} / {total}' },
  'wizard.optional': { de: 'optional', en: 'optional' },
  'wizard.back': { de: 'Zurück', en: 'Back' },
  'wizard.next': { de: 'Weiter', en: 'Next' },
  'wizard.showAll': { de: 'Alles zeigen', en: 'Show all' },
  'wizard.swipeHint': { de: '← nach links/rechts wischen →', en: '← swipe left/right →' },
  'wizard.nextPreview': { de: 'Weiter: {n}', en: 'Next: {n}' },
  'wizard.title.basics': { de: 'Jongleure & Periode', en: 'Jugglers & period' },
  'wizard.subtitle.basics': { de: 'Wer wirft mit, und wie lang ist das Muster?', en: 'Who is throwing, and how long is the pattern?' },
  'wizard.title.clubsThrows': { de: 'Clubs & Würfe', en: 'Clubs & throws' },
  'wizard.subtitle.clubsThrows': { de: 'Wie viele Clubs, und welche Wurfhöhen dürfen vorkommen?', en: 'How many clubs, and which throw heights are allowed?' },
  'wizard.title.jugglers': { de: 'Wie viele Jongleure?', en: 'How many jugglers?' },
  'wizard.subtitle.jugglers': { de: 'Wähle, wie viele Personen mitwerfen.', en: 'Choose how many people are throwing.' },
  'wizard.title.period': { de: 'Wie lang ist die Periode?', en: 'How long is the period?' },
  'wizard.subtitle.period': { de: 'Nach wie vielen Würfen wiederholt sich das Muster?', en: 'After how many throws does the pattern repeat?' },
  'wizard.title.clubs': { de: 'Wie viele Clubs?', en: 'How many clubs?' },
  'wizard.subtitle.clubs': { de: 'Ein Bereich von–bis ist erlaubt.', en: 'A min–max range is allowed.' },
  'wizard.title.throws': { de: 'Welche Würfe erlauben?', en: 'Which throws to allow?' },
  'wizard.subtitle.throws': { de: 'Tippe die Wurfhöhen an, die vorkommen dürfen.', en: 'Tap the throw heights that are allowed to occur.' },
  'wizard.title.filters': { de: 'Noch Filter?', en: 'Any filters?' },
  'wizard.subtitle.filters': { de: 'Kannst du auch überspringen und direkt generieren.', en: 'You can also skip this and generate right away.' },
};

let pzCurrentLang = 'de';

function pzT(key, vars) {
  const entry = PZ_I18N[key];
  if (!entry) return key;
  let s = entry[pzCurrentLang] || entry.de || key;
  if (vars) {
    Object.keys(vars).forEach(k => { s = s.split('{' + k + '}').join(String(vars[k])); });
  }
  return s;
}

function pzApplyI18n(root) {
  (root || document).querySelectorAll('[data-i18n]').forEach(el => {
    el.textContent = pzT(el.getAttribute('data-i18n'));
  });
}

/**
 * Renders a small "DE | EN" segmented pill into `container` and keeps
 * `pzCurrentLang` in sync. Calls `onChange(lang)` after switching so callers
 * can re-render dynamic text (filter chips, echoes, ...) that isn't covered
 * by plain [data-i18n] elements.
 */
function pzCreateLangToggle(container, onChange) {
  container.innerHTML = '';
  const wrap = document.createElement('div');
  wrap.className = 'inline-flex rounded-full p-0.5 gap-0.5';
  wrap.style.background = 'rgba(255,255,255,0.16)';
  wrap.setAttribute('role', 'radiogroup');
  wrap.setAttribute('aria-label', 'Sprache / Language');

  const makeBtn = (label, lang) => {
    const b = document.createElement('button');
    b.textContent = label;
    b.dataset.lang = lang;
    b.type = 'button';
    b.setAttribute('role', 'radio');
    b.setAttribute('aria-label', lang === 'de' ? 'Deutsch' : 'English');
    b.className = 'px-2.5 py-1.5 rounded-full text-[11px] font-extrabold transition-colors min-h-[28px]';
    return b;
  };
  const deBtn = makeBtn('DE', 'de');
  const enBtn = makeBtn('EN', 'en');

  function render() {
    [deBtn, enBtn].forEach(b => {
      const active = b.dataset.lang === pzCurrentLang;
      b.style.background = active ? 'white' : 'transparent';
      b.style.color = active ? 'var(--pz-purple-800)' : 'rgba(255,255,255,0.85)';
      b.setAttribute('aria-checked', String(active));
    });
    document.documentElement.lang = pzCurrentLang;
  }

  [deBtn, enBtn].forEach(b => {
    b.addEventListener('click', () => {
      if (pzCurrentLang === b.dataset.lang) return;
      pzCurrentLang = b.dataset.lang;
      render();
      pzApplyI18n();
      if (onChange) onChange(pzCurrentLang);
    });
  });

  wrap.appendChild(deBtn);
  wrap.appendChild(enBtn);
  container.appendChild(wrap);
  render();
}

/* =========================================================================
 * Filter model + formatting – mirrors the three real filter types
 * (EasyNumberFilter, EasyPatternFilter, EasyStateFilter) but flattened to a
 * simple AND-list instead of the And/Or tree, which is far friendlier on
 * mobile while covering the same underlying filter kinds.
 * ========================================================================= */

let pzFilterIdCounter = 1;
function pzNextFilterId() { return pzFilterIdCounter++; }

function pzRotationLabel(rotation) {
  if (rotation === 'global') return pzT('pattern.rotation.global');
  const letter = String.fromCharCode(65 + rotation);
  return pzT('pattern.rotation.local', { letter });
}

function pzFormatFilter(filter) {
  if (filter.kind === 'number') {
    const cmp = pzT('number.comparison.' + filter.comparison);
    const throwLabel = pzThrowLabel(filter.height, true);
    return `${cmp} ${filter.amount}× ${throwLabel}`;
  }
  if (filter.kind === 'pattern') {
    const seq = filter.sequence.length
      ? filter.sequence.map(h => pzThrowLabel(h, true)).join(' ')
      : pzT('pattern.empty');
    const verb = filter.include ? pzT('pattern.includesWord') : pzT('pattern.excludesWord');
    const rotation = pzRotationLabel(filter.rotation);
    return pzCurrentLang === 'de'
      ? `Muster ${verb} „${seq}“ · ${rotation}`
      : `Pattern ${verb} "${seq}" · ${rotation}`;
  }
  if (filter.kind === 'state') {
    const beats = filter.active.map((v, i) => (v ? i + 1 : null)).filter(v => v !== null);
    const list = beats.length ? beats.join(', ') : (pzCurrentLang === 'de' ? 'keine' : 'none');
    return pzCurrentLang === 'de' ? `Zustand · Beats ${list} belegt` : `State · beats ${list} occupied`;
  }
  return '';
}

const PZ_STATE_MAX_HEIGHT = 9;

/**
 * Wires up the whole "Filter" feature: a flat AND-list of filter cards
 * (Number / Pattern / State – mirroring EasyNumberFilter, EasyPatternFilter,
 * EasyStateFilter from the real app) plus a bottom sheet with 3 tabs to
 * add/edit them. Expects a fixed set of element ids to exist in the page
 * (see 01-card-stack.html / 02-wizard.html for the markup).
 */
function pzInitFilterBuilder(options) {
  const opts = Object.assign({
    getPeriod: () => 5,
    getJugglers: () => 3,
    getAllowedThrows: () => PZ_THROWS.map(t => t.height),
    onChange: () => {},
  }, options);

  const listEl = document.getElementById('filterList');
  const addBtn = document.getElementById('addFilterBtn');
  const sheet = document.getElementById('filterSheet');
  const backdrop = document.getElementById('sheetBackdrop');
  const sheetTitleEl = document.getElementById('sheetTitle');
  const tabBtns = Array.from(document.querySelectorAll('#filterTabs [data-tab]'));
  const panels = Array.from(document.querySelectorAll('#filterSheet [data-panel]'));
  const saveBtn = document.getElementById('sheetSaveBtn');
  const deleteBtn = document.getElementById('sheetDeleteBtn');

  const numComparison = document.getElementById('numComparison');
  const numAmount = document.getElementById('numAmount');
  const numThrow = document.getElementById('numThrow');

  const patRotation = document.getElementById('patRotation');
  const patIncludeYes = document.getElementById('patIncludeYes');
  const patIncludeNo = document.getElementById('patIncludeNo');
  const patSlots = document.getElementById('patSlots');
  const patPalette = document.getElementById('patPalette');

  const stateGrid = document.getElementById('stateGrid');

  let filters = (opts.defaultFilters || []).map(f => Object.assign({ id: pzNextFilterId() }, f));
  // One connector per gap between adjacent filters ('and' | 'or'). Consecutive
  // 'and' runs form a group; groups are joined by 'or'. Any assignment of
  // per-gap connectors is automatically a valid "OR of AND-groups" (DNF) –
  // there is no invalid state to guard against, unlike a free-form tree.
  let connectors = new Array(Math.max(0, filters.length - 1)).fill('and');
  let editingId = null;
  let pendingInsertAfter = null;
  let activeKind = 'number';
  let patInclude = true;
  let patSequence = [];
  let patActiveSlot = 0;
  let stateActive = [];

  function currentFilterCount() { return filters.length; }

  function computeGroups() {
    if (filters.length === 0) return [];
    const groups = [[filters[0]]];
    for (let i = 1; i < filters.length; i++) {
      if (connectors[i - 1] === 'and') groups[groups.length - 1].push(filters[i]);
      else groups.push([filters[i]]);
    }
    return groups;
  }

  function removeFilterById(id) {
    const idx = filters.findIndex(f => f.id === id);
    if (idx === -1) return;
    filters.splice(idx, 1);
    if (connectors.length > 0) {
      const connIdx = Math.min(idx, connectors.length - 1);
      connectors.splice(connIdx, 1);
    }
    renderList();
    opts.onChange(currentFilterCount());
  }

  function renderList() {
    if (filters.length === 0) {
      listEl.innerHTML = `<p class="text-sm text-[color:var(--pz-purple-400)] font-semibold py-1" data-i18n="filters.empty">${pzT('filters.empty')}</p>`;
      return;
    }
    listEl.setAttribute('role', 'list');
    const groups = computeGroups();
    let idx = 0;
    let html = '';
    groups.forEach((group, gi) => {
      if (gi > 0) {
        const connIdx = idx - 1;
        html += `
        <div class="flex justify-center py-0.5">
          <button type="button" data-connector="${connIdx}" class="pz-pill text-xs font-extrabold min-h-[40px] px-5" style="background:var(--pz-orange);color:#fff" aria-label="${pzT('filters.connectorOrHint')}">${pzT('filters.or')}</button>
        </div>`;
      }
      const isGroup = group.length > 1;
      html += `<div ${isGroup ? `role="group" aria-label="${pzT('filters.group', { n: gi + 1 })}" class="rounded-2xl p-2 space-y-2" style="border:2px dashed var(--pz-purple-300);background:rgba(138,99,196,0.07)"` : ''}>`;
      if (isGroup) {
        html += `<div class="text-[10px] font-extrabold text-[color:var(--pz-purple-400)] uppercase tracking-wide px-1">${pzT('filters.group', { n: gi + 1 })}</div>`;
      }
      group.forEach((f, li) => {
        if (li > 0) {
          const connIdx = idx - 1;
          html += `
          <div class="flex justify-center">
            <button type="button" data-connector="${connIdx}" class="text-[10px] font-extrabold text-[color:var(--pz-purple-500)] tracking-wide px-3 py-2 min-h-[36px] rounded-full" style="background:var(--pz-purple-100)" aria-label="${pzT('filters.connectorAndHint')}">${pzT('filters.and')}</button>
          </div>`;
        }
        const desc = pzFormatFilter(f);
        html += `
        <div class="flex items-center gap-2 pz-card px-3 py-2.5" style="box-shadow:none;border:2px solid var(--pz-purple-100)" role="listitem">
          <div class="flex-1 text-sm font-bold text-[color:var(--pz-purple-700)]">${desc}</div>
          <button type="button" data-edit="${f.id}" class="w-11 h-11 rounded-full flex items-center justify-center text-[color:var(--pz-purple-500)] shrink-0" style="background:var(--pz-purple-100)" title="${pzT('filters.edit')}" aria-label="${pzT('filters.edit')}: ${desc}">✎</button>
          <button type="button" data-del="${f.id}" class="w-11 h-11 rounded-full flex items-center justify-center text-red-500 shrink-0" style="background:#fde3e3" title="${pzT('filters.remove')}" aria-label="${pzT('filters.remove')}: ${desc}">✕</button>
        </div>`;
        idx++;
      });
      html += `
        <button type="button" data-add-after="${idx - 1}" class="w-full text-center text-xs font-bold py-2 min-h-[40px] rounded-lg" style="border:1.5px dashed var(--pz-purple-300);color:var(--pz-purple-500)" aria-label="${pzT('filters.addToGroup')}">+ ${pzT('filters.addToGroup')}</button>`;
      html += `</div>`;
    });
    listEl.innerHTML = html;
  }

  listEl.addEventListener('click', e => {
    const editBtn = e.target.closest('[data-edit]');
    const delBtn = e.target.closest('[data-del]');
    const connBtn = e.target.closest('[data-connector]');
    const addAfterBtn = e.target.closest('[data-add-after]');
    if (editBtn) {
      const f = filters.find(x => x.id === parseInt(editBtn.dataset.edit, 10));
      if (f) openForEdit(f);
    } else if (delBtn) {
      removeFilterById(parseInt(delBtn.dataset.del, 10));
    } else if (addAfterBtn) {
      openForNew('number', parseInt(addAfterBtn.dataset.addAfter, 10));
    } else if (connBtn) {
      const connIdx = parseInt(connBtn.dataset.connector, 10);
      connectors[connIdx] = connectors[connIdx] === 'and' ? 'or' : 'and';
      renderList();
      opts.onChange(currentFilterCount());
    }
  });

  function selectTab(kind) {
    activeKind = kind;
    tabBtns.forEach(b => {
      const active = b.dataset.tab === kind;
      b.classList.toggle('active', active);
      b.setAttribute('aria-selected', String(active));
    });
    panels.forEach(p => p.classList.toggle('hidden', p.dataset.panel !== kind));
  }
  tabBtns.forEach(b => b.addEventListener('click', () => selectTab(b.dataset.tab)));

  function populateThrowSelect(selectEl) {
    const heights = opts.getAllowedThrows();
    selectEl.innerHTML = heights.map(h => `<option value="${h}">${pzThrowLabel(h, true)}</option>`).join('');
  }

  function populateRotationSelect() {
    const jugglers = opts.getJugglers();
    let html = `<option value="global">${pzT('pattern.rotation.global')}</option>`;
    for (let i = 0; i < jugglers; i++) {
      html += `<option value="${i}">${pzRotationLabel(i)}</option>`;
    }
    patRotation.innerHTML = html;
  }

  function patternLength() {
    if (patRotation.value === 'global') return opts.getPeriod();
    return Math.max(1, Math.round(opts.getPeriod() / opts.getJugglers()));
  }

  function renderPatSlots() {
    const len = patternLength();
    if (patSequence.length !== len) {
      const fallback = opts.getAllowedThrows()[0] ?? 5;
      patSequence = Array.from({ length: len }, (_, i) => patSequence[i] ?? fallback);
    }
    if (patActiveSlot >= len) patActiveSlot = 0;
    patSlots.setAttribute('role', 'group');
    patSlots.setAttribute('aria-label', pzCurrentLang === 'de' ? 'Muster-Sequenz, Feld antippen und dann Wurf unten wählen' : 'Pattern sequence, tap a slot then pick a throw below');
    patSlots.innerHTML = '';
    patSequence.forEach((h, i) => {
      const slot = document.createElement('button');
      slot.type = 'button';
      const isActive = i === patActiveSlot;
      slot.className = 'pz-chip flex items-center justify-center text-sm min-h-[44px]' + (isActive ? ' active' : '');
      slot.textContent = pzThrowLabel(h, true);
      const posLabel = pzCurrentLang === 'de' ? `Feld ${i + 1} von ${len}: ${pzThrowLabel(h, true)}` : `Slot ${i + 1} of ${len}: ${pzThrowLabel(h, true)}`;
      slot.setAttribute('aria-label', posLabel);
      slot.setAttribute('aria-pressed', String(isActive));
      slot.addEventListener('click', () => { patActiveSlot = i; renderPatSlots(); });
      patSlots.appendChild(slot);
    });
  }

  function renderPatPalette() {
    const heights = opts.getAllowedThrows();
    patPalette.setAttribute('role', 'group');
    patPalette.setAttribute('aria-label', pzCurrentLang === 'de' ? 'Wurf-Palette' : 'Throw palette');
    patPalette.innerHTML = '';
    heights.forEach(h => {
      const chip = document.createElement('button');
      chip.type = 'button';
      chip.className = 'pz-chip py-2.5 px-3 text-xs min-h-[44px] min-w-[44px]';
      chip.textContent = pzThrowLabel(h, true);
      chip.setAttribute('aria-label', pzThrowLabel(h, true));
      chip.addEventListener('click', () => {
        patSequence[patActiveSlot] = h;
        patActiveSlot = Math.min(patActiveSlot + 1, patSequence.length - 1);
        renderPatSlots();
      });
      patPalette.appendChild(chip);
    });
  }

  function setPatInclude(val) {
    patInclude = val;
    patIncludeYes.classList.toggle('active', patInclude);
    patIncludeNo.classList.toggle('active', !patInclude);
    patIncludeYes.setAttribute('aria-checked', String(patInclude));
    patIncludeNo.setAttribute('aria-checked', String(!patInclude));
  }
  patIncludeYes.addEventListener('click', () => setPatInclude(true));
  patIncludeNo.addEventListener('click', () => setPatInclude(false));
  patRotation.addEventListener('change', () => { patSequence = []; patActiveSlot = 0; renderPatSlots(); });

  function renderStateGrid() {
    if (stateActive.length !== PZ_STATE_MAX_HEIGHT) {
      const clubs = Math.round(opts.getPeriod() > 0 ? Math.min(6, PZ_STATE_MAX_HEIGHT) : 6);
      stateActive = Array.from({ length: PZ_STATE_MAX_HEIGHT }, (_, i) => stateActive[i] ?? (i < clubs));
    }
    stateGrid.setAttribute('role', 'group');
    stateGrid.setAttribute('aria-label', pzT('state.hint'));
    stateGrid.innerHTML = '';
    stateActive.forEach((active, i) => {
      const chip = document.createElement('button');
      chip.type = 'button';
      chip.className = 'pz-chip py-2 text-sm min-h-[44px]' + (active ? ' active' : '');
      chip.textContent = String(i + 1);
      const label = pzT('state.beat', { n: i + 1 });
      chip.title = label;
      chip.setAttribute('aria-label', label);
      chip.setAttribute('aria-pressed', String(active));
      chip.addEventListener('click', () => {
        stateActive[i] = !stateActive[i];
        chip.classList.toggle('active');
        chip.setAttribute('aria-pressed', String(stateActive[i]));
      });
      stateGrid.appendChild(chip);
    });
  }

  function openSheet() { sheet.classList.add('open'); backdrop.classList.add('open'); }
  function closeSheet() { sheet.classList.remove('open'); backdrop.classList.remove('open'); }

  function refreshSheetChrome() {
    sheetTitleEl.textContent = editingId === null ? pzT('sheet.titleAdd') : pzT('sheet.titleEdit');
    saveBtn.textContent = editingId === null ? pzT('sheet.saveAdd') : pzT('sheet.saveEdit');
    deleteBtn.classList.toggle('hidden', editingId === null);
    tabBtns.forEach(b => { b.textContent = pzT('tab.' + b.dataset.tab); });
    numComparison.querySelectorAll('option').forEach(o => { o.textContent = pzT('number.comparison.' + o.value); });
    populateRotationSelect();
    setPatInclude(patInclude);
    renderPatSlots();
  }

  function openForNew(kind, insertAfterIndex) {
    pendingInsertAfter = insertAfterIndex ?? null;
    editingId = null;
    populateThrowSelect(numThrow);
    populateRotationSelect();
    numComparison.value = 'exact';
    numAmount.value = 2;
    if (numThrow.options.length) numThrow.value = numThrow.options[0].value;
    patSequence = [];
    patActiveSlot = 0;
    setPatInclude(true);
    renderPatSlots();
    renderPatPalette();
    renderStateGrid();
    selectTab(kind || 'number');
    refreshSheetChrome();
    openSheet();
  }

  function openForEdit(f) {
    editingId = f.id;
    populateThrowSelect(numThrow);
    populateRotationSelect();
    if (f.kind === 'number') {
      numComparison.value = f.comparison;
      numAmount.value = f.amount;
      numThrow.value = String(f.height);
    } else if (f.kind === 'pattern') {
      patRotation.value = f.rotation === 'global' ? 'global' : String(f.rotation);
      patSequence = f.sequence.slice();
      patActiveSlot = 0;
      setPatInclude(f.include);
    } else if (f.kind === 'state') {
      stateActive = f.active.slice();
    }
    renderPatSlots();
    renderPatPalette();
    renderStateGrid();
    selectTab(f.kind);
    refreshSheetChrome();
    openSheet();
  }

  addBtn.addEventListener('click', () => openForNew('number'));
  backdrop.addEventListener('click', closeSheet);

  saveBtn.addEventListener('click', () => {
    let filter;
    if (activeKind === 'number') {
      filter = { kind: 'number', comparison: numComparison.value, amount: parseInt(numAmount.value, 10) || 0, height: parseInt(numThrow.value, 10) };
    } else if (activeKind === 'pattern') {
      filter = { kind: 'pattern', rotation: patRotation.value === 'global' ? 'global' : parseInt(patRotation.value, 10), include: patInclude, sequence: patSequence.slice() };
    } else {
      filter = { kind: 'state', active: stateActive.slice() };
    }
    if (editingId === null) {
      filter.id = pzNextFilterId();
      const insertIdx = pendingInsertAfter === null ? filters.length : pendingInsertAfter + 1;
      filters.splice(insertIdx, 0, filter);
      if (filters.length > 1) {
        connectors.splice(Math.max(0, insertIdx - 1), 0, 'and');
      }
      pendingInsertAfter = null;
    } else {
      filter.id = editingId;
      filters = filters.map(f => (f.id === editingId ? filter : f));
    }
    closeSheet();
    renderList();
    opts.onChange(currentFilterCount());
  });

  deleteBtn.addEventListener('click', () => {
    removeFilterById(editingId);
    closeSheet();
  });

  renderList();

  return {
    count: currentFilterCount,
    refreshTexts() {
      renderList();
      refreshSheetChrome();
    },
  };
}
