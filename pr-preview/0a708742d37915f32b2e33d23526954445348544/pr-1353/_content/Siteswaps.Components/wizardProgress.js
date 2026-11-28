export function focusElement(id) {
    requestAnimationFrame(() => {
        const element = document.getElementById(id);
        if (element instanceof HTMLElement) element.focus();
    });
}
