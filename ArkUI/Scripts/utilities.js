/**
 * ArkUI Core Utilities Module
 * Minimal JavaScript functions for operations that cannot be done in pure Blazor.
 */

/**
 * Check if the document direction is RTL (right-to-left).
 * @returns {boolean} True if the document is in RTL mode.
 */
export function isRtl() {
    return document.dir === 'rtl' || document.documentElement.dir === 'rtl';
}

/**
 * Focus a specific element.
 * @param {HTMLElement} element - The element to focus.
 */
export function focusElement(element) {
    element?.focus();
}

/**
 * Focus an element by its ID.
 * @param {string} elementId - The ID of the element to focus.
 */
export function focusElementById(elementId) {
    const element = document.getElementById(elementId);
    element?.focus();
}

/**
 * Initialize a checkbox element to prevent Enter key from activating it.
 * Checkboxes should only respond to Space key per WAI-ARIA patterns.
 * @param {HTMLElement} element - The checkbox button element.
 */
export function initializeCheckbox(element) {
    if (!element) return;
    
    element._arkCheckboxKeyHandler = (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
        }
    };
    element.addEventListener('keydown', element._arkCheckboxKeyHandler);
}

/**
 * Cleanup checkbox event handlers.
 * @param {HTMLElement} element - The checkbox button element.
 */
export function destroyCheckbox(element) {
    if (!element || !element._arkCheckboxKeyHandler) return;
    
    element.removeEventListener('keydown', element._arkCheckboxKeyHandler);
    delete element._arkCheckboxKeyHandler;
}
