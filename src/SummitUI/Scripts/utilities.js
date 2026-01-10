/**
 * SummitUI Core Utilities Module
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
 * Check if a specific element's direction is RTL (right-to-left).
 * This checks the computed style direction, which accounts for inherited dir attributes.
 * @param {string} elementId - The ID of the element to check.
 * @returns {boolean} True if the element is in RTL mode.
 */
export function isElementRtl(elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        return document.dir === 'rtl' || document.documentElement.dir === 'rtl';
    }
    return getComputedStyle(element).direction === 'rtl';
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
    
    element._summitCheckboxKeyHandler = (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
        }
    };
    element.addEventListener('keydown', element._summitCheckboxKeyHandler);
}

/**
 * Cleanup checkbox event handlers.
 * @param {HTMLElement} element - The checkbox button element.
 */
export function destroyCheckbox(element) {
    if (!element || !element._summitCheckboxKeyHandler) return;
    
    element.removeEventListener('keydown', element._summitCheckboxKeyHandler);
    delete element._summitCheckboxKeyHandler;
}

/**
 * Initialize a radio item element to prevent arrow keys from scrolling the page.
 * @param {HTMLElement} element - The radio item button element.
 */
export function initializeRadioItem(element) {
    if (!element) return;
    
    element._summitRadioItemKeyHandler = (e) => {
        // Prevent default scroll behavior for navigation keys
        if (['ArrowDown', 'ArrowUp', 'ArrowLeft', 'ArrowRight', ' '].includes(e.key)) {
            e.preventDefault();
        }
    };
    element.addEventListener('keydown', element._summitRadioItemKeyHandler);
}

/**
 * Cleanup radio item event handlers.
 * @param {HTMLElement} element - The radio item button element.
 */
export function destroyRadioItem(element) {
    if (!element || !element._summitRadioItemKeyHandler) return;
    
    element.removeEventListener('keydown', element._summitRadioItemKeyHandler);
    delete element._summitRadioItemKeyHandler;
}
