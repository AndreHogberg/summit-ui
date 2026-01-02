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
