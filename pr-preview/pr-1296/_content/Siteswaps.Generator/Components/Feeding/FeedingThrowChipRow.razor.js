export function captureChipPointer(row, beat, pointerId) {
  const chip = row?.querySelector?.(`button[data-chip-beat="${beat}"]`);
  if (!chip?.setPointerCapture) {
    return false;
  }

  chip.setPointerCapture(pointerId);
  return true;
}

export function releaseChipPointer(row, beat, pointerId) {
  const chip = row?.querySelector?.(`button[data-chip-beat="${beat}"]`);
  if (chip?.hasPointerCapture?.(pointerId)) {
    chip.releasePointerCapture(pointerId);
  }
}

export function findChipTarget(scope, clientX, clientY) {
  const hit = document.elementFromPoint(clientX, clientY);
  if (!hit || !scope?.contains?.(hit)) {
    return null;
  }

  const chip = hit.closest('button[data-chip-beat]');
  if (!chip) {
    return null;
  }

  const row = chip.closest('[data-row-person]');
  if (!row) {
    return null;
  }

  const person = Number.parseInt(row.dataset.rowPerson ?? '', 10);
  const beat = Number.parseInt(chip.dataset.chipBeat ?? '', 10);
  if (Number.isNaN(person) || Number.isNaN(beat)) {
    return null;
  }

  return { person, beat };
}
