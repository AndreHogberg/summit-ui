/**
 * SummitUI Focus Trap Module
 * Reusable focus management for modals, dialogs, popovers, etc.
 * 
 * Uses a stack-based approach to handle multiple simultaneous focus traps.
 * Only the topmost (most recently activated) focus trap intercepts focusin events.
 * This prevents conflicts when multiple dialogs/modals are open (e.g., Dialog + AlertDialog).
 */

// Stack of active focus traps - the last one is the "active" one
// Format: [{ id, container, config, handleKeyDown }, ...]
const focusTrapStack = [];

// Store trap configs by ID for cleanup
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
 * Global focusin handler - only the topmost focus trap intercepts focus events.
 * This prevents multiple focus traps from fighting over focus.
 */
function handleGlobalFocusIn(event) {
    if (focusTrapStack.length === 0) return;

    // Get the topmost (active) focus trap
    const topTrap = focusTrapStack[focusTrapStack.length - 1];
    const containerEl = topTrap.container;

    // If focus is already inside the topmost trap's container, allow it
    if (containerEl.contains(event.target)) return;

    // Focus escaped the topmost trap - bring it back
    event.preventDefault();
    event.stopPropagation();

    const focusableElements = getFocusableElements(containerEl);
    if (focusableElements.length > 0) {
        focusableElements[0].focus({ preventScroll: true });
    } else {
        // Make container focusable and focus it
        if (!containerEl.hasAttribute('tabindex')) {
            containerEl.setAttribute('tabindex', '-1');
        }
        containerEl.focus({ preventScroll: true });
    }
}

/**
 * Initialize a focus trap on a container element
 * @param {HTMLElement} containerEl - The element to trap focus within
 * @param {object} options - Configuration options
 * @param {boolean} options.autoFocus - Whether to auto-focus first element (default: true)
 * @param {boolean} options.returnFocus - Whether to return focus on deactivate (default: true)
 * @param {HTMLElement} options.initialFocus - Element to focus initially (optional)
 * @param {string} options.initialFocusSelector - CSS selector for element to focus initially (optional)
 * @param {HTMLElement} options.returnFocusTo - Element to return focus to (optional)
 * @returns {string} Trap ID for later reference
 */
export function activate(containerEl, options = {}) {
    if (!containerEl) return null;

    const trapId = `trap-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

    // Resolve initial focus element from selector if provided
    let initialFocusEl = options.initialFocus || null;
    if (!initialFocusEl && options.initialFocusSelector) {
        initialFocusEl = containerEl.querySelector(options.initialFocusSelector);
    }

    const config = {
        autoFocus: options.autoFocus !== false,
        returnFocus: options.returnFocus !== false,
        initialFocus: initialFocusEl,
        returnFocusTo: options.returnFocusTo || document.activeElement
    };

    // Handle Tab key for focus trapping (still per-container for proper Tab cycling)
    function handleKeyDown(event) {
        if (event.key !== 'Tab') return;

        // Only handle Tab if this is the topmost trap
        const topTrap = focusTrapStack[focusTrapStack.length - 1];
        if (!topTrap || topTrap.id !== trapId) return;

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
                lastElement.focus({ preventScroll: true });
            }
        } else {
            // Tab: going forwards
            if (document.activeElement === lastElement || !containerEl.contains(document.activeElement)) {
                event.preventDefault();
                firstElement.focus({ preventScroll: true });
            }
        }
    }

    // Add keydown listener to the container
    containerEl.addEventListener('keydown', handleKeyDown);

    // Create trap entry
    const trapEntry = {
        id: trapId,
        container: containerEl,
        config,
        handleKeyDown
    };

    // Add to stack
    focusTrapStack.push(trapEntry);

    // Register the global focusin listener when first trap is added
    if (focusTrapStack.length === 1) {
        document.addEventListener('focusin', handleGlobalFocusIn, true);
    }

    // Store for cleanup
    focusTraps.set(trapId, trapEntry);

    // Auto-focus
    if (config.autoFocus) {
        // Use requestAnimationFrame to ensure DOM is ready
        requestAnimationFrame(() => {
            if (config.initialFocus && containerEl.contains(config.initialFocus)) {
                config.initialFocus.focus({ preventScroll: true });
            } else {
                const focusableElements = getFocusableElements(containerEl);
                if (focusableElements.length > 0) {
                    focusableElements[0].focus({ preventScroll: true });
                } else {
                    // Make container focusable and focus it
                    if (!containerEl.hasAttribute('tabindex')) {
                        containerEl.setAttribute('tabindex', '-1');
                    }
                    containerEl.focus({ preventScroll: true });
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

    // Remove keydown listener from container
    trap.container.removeEventListener('keydown', trap.handleKeyDown);

    // Remove from stack
    const stackIndex = focusTrapStack.findIndex(t => t.id === trapId);
    if (stackIndex !== -1) {
        focusTrapStack.splice(stackIndex, 1);
    }

    // Unregister global focusin listener when last trap is removed
    if (focusTrapStack.length === 0) {
        document.removeEventListener('focusin', handleGlobalFocusIn, true);
    }

    // Return focus if configured
    if (trap.config.returnFocus && trap.config.returnFocusTo) {
        requestAnimationFrame(() => {
            if (trap.config.returnFocusTo && typeof trap.config.returnFocusTo.focus === 'function') {
                trap.config.returnFocusTo.focus({ preventScroll: true });
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
        focusableElements[0].focus({ preventScroll: true });
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
        focusableElements[focusableElements.length - 1].focus({ preventScroll: true });
    }
}

/**
 * Focus a specific element
 * @param {HTMLElement} element
 */
export function focusElement(element) {
    element?.focus({ preventScroll: true });
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

/**
 * Get the current focus trap stack depth (useful for debugging)
 * @returns {number}
 */
export function getStackDepth() {
    return focusTrapStack.length;
}
