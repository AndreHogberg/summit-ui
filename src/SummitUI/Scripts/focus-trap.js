/**
 * SummitUI Focus Trap Module
 * Reusable focus management for modals, dialogs, popovers, etc.
 */

// Store active focus traps
const focusTraps = new Map();

// Focusable element selectors
const FOCUSABLE_SELECTORS = [
    'a[href]:not([disabled]):not([tabindex="-1"])',
    'button:not([disabled]):not([tabindex="-1"])',
    'input:not([disabled]):not([tabindex="-1"])',
    'select:not([disabled]):not([tabindex="-1"])',
    'textarea:not([disabled]):not([tabindex="-1"])',
    '[tabindex]:not([tabindex="-1"]):not([disabled])',
    '[contenteditable="true"]:not([disabled])'
].join(', ');

/**
 * Get all focusable elements within a container
 * @param {HTMLElement} container
 * @returns {HTMLElement[]}
 */
function getFocusableElements(container) {
    const elements = Array.from(container.querySelectorAll(FOCUSABLE_SELECTORS));
    // Filter out elements that are not visible
    return elements.filter(el => {
        return el.offsetParent !== null &&
               getComputedStyle(el).visibility !== 'hidden';
    });
}

/**
 * Initialize a focus trap on a container element
 * @param {HTMLElement} containerEl - The element to trap focus within
 * @param {object} options - Configuration options
 * @param {boolean} options.autoFocus - Whether to auto-focus first element (default: true)
 * @param {boolean} options.returnFocus - Whether to return focus on deactivate (default: true)
 * @param {HTMLElement} options.initialFocus - Element to focus initially (optional)
 * @param {HTMLElement} options.returnFocusTo - Element to return focus to (optional)
 * @returns {string} Trap ID for later reference
 */
export function activate(containerEl, options = {}) {
    if (!containerEl) return null;

    const trapId = `trap-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

    const config = {
        autoFocus: options.autoFocus !== false,
        returnFocus: options.returnFocus !== false,
        initialFocus: options.initialFocus || null,
        returnFocusTo: options.returnFocusTo || document.activeElement
    };

    // Handle Tab key for focus trapping
    function handleKeyDown(event) {
        if (event.key !== 'Tab') return;

        const focusableElements = getFocusableElements(containerEl);
        if (focusableElements.length === 0) {
            event.preventDefault();
            return;
        }

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        if (event.shiftKey) {
            // Shift+Tab: going backwards
            if (document.activeElement === firstElement || !containerEl.contains(document.activeElement)) {
                event.preventDefault();
                lastElement.focus();
            }
        } else {
            // Tab: going forwards
            if (document.activeElement === lastElement || !containerEl.contains(document.activeElement)) {
                event.preventDefault();
                firstElement.focus();
            }
        }
    }

    // Prevent focus from leaving the container via click
    function handleFocusIn(event) {
        if (!containerEl.contains(event.target)) {
            event.preventDefault();
            event.stopPropagation();

            const focusableElements = getFocusableElements(containerEl);
            if (focusableElements.length > 0) {
                focusableElements[0].focus();
            } else {
                containerEl.focus();
            }
        }
    }

    // Add event listeners
    containerEl.addEventListener('keydown', handleKeyDown);
    document.addEventListener('focusin', handleFocusIn, true);

    // Store trap info for cleanup
    focusTraps.set(trapId, {
        container: containerEl,
        config,
        cleanup: () => {
            containerEl.removeEventListener('keydown', handleKeyDown);
            document.removeEventListener('focusin', handleFocusIn, true);
        }
    });

    // Auto-focus
    if (config.autoFocus) {
        // Use requestAnimationFrame to ensure DOM is ready
        requestAnimationFrame(() => {
            if (config.initialFocus && containerEl.contains(config.initialFocus)) {
                config.initialFocus.focus();
            } else {
                const focusableElements = getFocusableElements(containerEl);
                if (focusableElements.length > 0) {
                    focusableElements[0].focus();
                } else {
                    // Make container focusable and focus it
                    if (!containerEl.hasAttribute('tabindex')) {
                        containerEl.setAttribute('tabindex', '-1');
                    }
                    containerEl.focus();
                }
            }
        });
    }

    return trapId;
}

/**
 * Deactivate a focus trap
 * @param {string} trapId - The trap ID returned from activate()
 */
export function deactivate(trapId) {
    const trap = focusTraps.get(trapId);
    if (!trap) return;

    trap.cleanup();

    // Return focus if configured
    if (trap.config.returnFocus && trap.config.returnFocusTo) {
        requestAnimationFrame(() => {
            if (trap.config.returnFocusTo && typeof trap.config.returnFocusTo.focus === 'function') {
                trap.config.returnFocusTo.focus();
            }
        });
    }

    focusTraps.delete(trapId);
}

/**
 * Deactivate focus trap by container element
 * @param {HTMLElement} containerEl - The container element
 */
export function deactivateByContainer(containerEl) {
    for (const [trapId, trap] of focusTraps) {
        if (trap.container === containerEl) {
            deactivate(trapId);
            return;
        }
    }
}

/**
 * Focus the first focusable element in a container
 * @param {HTMLElement} containerEl
 */
export function focusFirst(containerEl) {
    if (!containerEl) return;

    const focusableElements = getFocusableElements(containerEl);
    if (focusableElements.length > 0) {
        focusableElements[0].focus();
    }
}

/**
 * Focus the last focusable element in a container
 * @param {HTMLElement} containerEl
 */
export function focusLast(containerEl) {
    if (!containerEl) return;

    const focusableElements = getFocusableElements(containerEl);
    if (focusableElements.length > 0) {
        focusableElements[focusableElements.length - 1].focus();
    }
}

/**
 * Focus a specific element
 * @param {HTMLElement} element
 */
export function focusElement(element) {
    element?.focus();
}

/**
 * Check if an element is focusable
 * @param {HTMLElement} element
 * @returns {boolean}
 */
export function isFocusable(element) {
    if (!element) return false;
    return element.matches(FOCUSABLE_SELECTORS) &&
           element.offsetParent !== null &&
           getComputedStyle(element).visibility !== 'hidden';
}

/**
 * Get the count of focusable elements in a container
 * @param {HTMLElement} containerEl
 * @returns {number}
 */
export function getFocusableCount(containerEl) {
    if (!containerEl) return 0;
    return getFocusableElements(containerEl).length;
}
