// Font size manager for dynamically updating CSS variables
export function setFontSizeMultiplier(multiplier) {
    document.documentElement.style.setProperty('--font-size-multiplier', multiplier);
}

export function getFontSizeMultiplier() {
    return getComputedStyle(document.documentElement).getPropertyValue('--font-size-multiplier');
}
