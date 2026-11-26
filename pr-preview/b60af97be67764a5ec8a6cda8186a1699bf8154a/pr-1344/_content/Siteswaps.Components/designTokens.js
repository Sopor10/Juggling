export function readDecisionTokens(names) {
    const styles = getComputedStyle(document.documentElement);
    return names.map((name) => styles.getPropertyValue(name).trim());
}
