// Theme management module for SummitUI Docs
// Supports: 'light', 'dark', 'system' (default)

const STORAGE_KEY = 'summitui-theme';

/**
 * Get the current theme preference from localStorage
 * @returns {'light' | 'dark' | 'system'}
 */
export function getTheme() {
    return localStorage.getItem(STORAGE_KEY) || 'system';
}

/**
 * Set the theme preference and apply it
 * @param {'light' | 'dark' | 'system'} theme
 */
export function setTheme(theme) {
    if (theme === 'system') {
        localStorage.removeItem(STORAGE_KEY);
    } else {
        localStorage.setItem(STORAGE_KEY, theme);
    }
    applyTheme(theme);
}

/**
 * Toggle between light and dark themes
 * If currently on system preference, will toggle to the opposite of the current effective theme
 * @returns {'light' | 'dark'} The new theme
 */
export function toggleTheme() {
    const current = getTheme();
    const isDark = current === 'dark' || 
        (current === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);
    
    const newTheme = isDark ? 'light' : 'dark';
    setTheme(newTheme);
    return newTheme;
}

/**
 * Check if dark mode is currently active
 * @returns {boolean}
 */
export function isDarkMode() {
    const theme = getTheme();
    return theme === 'dark' || 
        (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);
}

/**
 * Apply theme to the document
 * @param {'light' | 'dark' | 'system'} theme
 */
export function applyTheme(theme) {
    const root = document.documentElement;
    
    // Remove both classes first
    root.classList.remove('light', 'dark');
    
    // Add the appropriate class (only if user has explicit preference)
    if (theme !== 'system') {
        root.classList.add(theme);
    }
}

/**
 * Initialize theme on page load
 * This should be called as early as possible to prevent flash
 */
export function initTheme() {
    const theme = getTheme();
    applyTheme(theme);
    
    // Listen for system preference changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        const currentTheme = getTheme();
        if (currentTheme === 'system') {
            applyTheme('system');
        }
    });
    
    // Re-apply theme after Blazor enhanced navigation (in case DOM was modified)
    if (typeof Blazor !== 'undefined') {
        Blazor.addEventListener('enhancedload', () => {
            const theme = getTheme();
            applyTheme(theme);
        });
    }
}

// Auto-initialize when module loads
initTheme();
