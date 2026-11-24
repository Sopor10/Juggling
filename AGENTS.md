# UI guidance

- Keep instructional help text out of the default page layout. Reveal it only when the user activates a compact inline information icon placed beside the relevant heading or control. Do not give help its own row or a standalone visible button. Give the icon an accessible label and `aria-expanded` state, and keep the full help text available when expanded.

## Mobile touch and interaction semantics

- Treat phone use as a primary interaction target. Keep the complete user task usable at narrow phone widths; preserve keyboard and mouse operation as well. Verify the actual hit area and touch gesture, not only the visible icon or desktop layout. Existing E2E expectations include at least 40×40 CSS-pixel targets for wizard steppers/range controls (`WizardTouchTargetUxTests`) and 44×44 targets for causal-diagram throw handles (`CausalDiagramEditorTests`). Do not assume those sizes are met without checking the rendered control.
- Use native `<button>` elements for actions and `<a href>` for navigation. Give icon-only controls an accessible name, and keep state such as `aria-expanded` synchronized with the rendered UI. A clickable `<div>` or `<span>` is not made equivalent by adding only `tabindex` or `role="button"`.
- Custom SVG controls are allowed when their role, accessible name, focus behavior, and keyboard interaction match the task. For example, a directly manipulable editor target may use arrow keys to move or rotate, with those keys documented and tested. Do not assign `role="button"` to a structural table row to make the whole row clickable; keep table semantics and put a native action button in an appropriate cell.
- For layout or gesture changes, inspect a phone-width viewport and the relevant touch interaction. If the diff lacks enough viewport or handler context to establish usability, report the gap instead of assuming the mobile path works.
