// Color palette manager for dynamically updating color palette attributes
export function setColorPalette(palette, theme) {
    // Set the color palette attribute on the document root
    document.documentElement.setAttribute('data-color-palette', palette);

    // Also ensure theme is set correctly for palette to work
    if (theme) {
        document.documentElement.setAttribute('data-theme', theme);
    }
}

export function getColorPalette() {
    // Get from localStorage, default to 'classic-red'
    return localStorage.getItem('colorPalette') || 'classic-red';
}

export function getCurrentTheme() {
    // Get current theme from document attribute
    return document.documentElement.getAttribute('data-theme') || 'light';
}
