/**
 * SummitUI Dropdown Menu JavaScript Module
 * Minimal module for portal, trigger, and submenu hover functionality.
 * All other functionality (positioning, keyboard nav, etc.) is handled by Blazor + FloatingJsInterop.
 */

// Store portal containers
const portalContainers = new Map();

// Store sub trigger state
const subTriggerState = new Map();

/**
 * Create a portal container at the specified location
 * @param {string} containerId - Unique ID for the container
 * @param {string|null} targetSelector - CSS selector for parent (default: body)
 */
export function createPortal(containerId, targetSelector) {
    if (portalContainers.has(containerId)) return;

    const container = document.createElement('div');
    container.id = containerId;
    container.setAttribute('data-summit-dropdown-menu-portal', '');
    container.style.position = 'absolute';
    container.style.top = '0';
    container.style.left = '0';
    container.style.zIndex = '9999';

    const target = targetSelector
        ? document.querySelector(targetSelector)
        : document.body;

    if (target) {
        target.appendChild(container);
        portalContainers.set(containerId, container);
    }
}

/**
 * Destroy and remove a portal container
 * @param {string} containerId - ID of the container to remove
 */
export function destroyPortal(containerId) {
    const container = portalContainers.get(containerId);
    if (container) {
        container.remove();
        portalContainers.delete(containerId);
    }
}

/**
 * Initialize trigger to prevent default scroll on arrow keys
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function initializeTrigger(triggerEl) {
    if (!triggerEl) return;

    function handleKeyDown(event) {
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
        }
    }

    triggerEl.addEventListener('keydown', handleKeyDown);

    // Store cleanup function on element
    triggerEl._summitCleanup = () => {
        triggerEl.removeEventListener('keydown', handleKeyDown);
    };
}

/**
 * Cleanup trigger event listeners
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function destroyTrigger(triggerEl) {
    if (triggerEl?._summitCleanup) {
        triggerEl._summitCleanup();
        delete triggerEl._summitCleanup;
    }
}

/**
 * Initialize sub trigger for hover intent
 * @param {HTMLElement} triggerEl - Sub trigger element
 * @param {object} dotNetRef - .NET object reference for callbacks
 * @param {number} openDelay - Delay before opening in ms
 * @param {number} closeDelay - Delay before closing in ms
 */
export function initializeSubTrigger(triggerEl, dotNetRef, openDelay, closeDelay) {
    if (!triggerEl || !dotNetRef) return;

    const state = {
        openTimer: null,
        closeTimer: null,
        dotNetRef,
        openDelay,
        closeDelay
    };

    subTriggerState.set(triggerEl, state);

    // Store cleanup function on element
    triggerEl._summitSubTriggerCleanup = () => {
        clearTimeout(state.openTimer);
        clearTimeout(state.closeTimer);
        subTriggerState.delete(triggerEl);
    };
}

/**
 * Cleanup sub trigger event listeners and state
 * @param {HTMLElement} triggerEl - Sub trigger element
 */
export function destroySubTrigger(triggerEl) {
    if (triggerEl?._summitSubTriggerCleanup) {
        triggerEl._summitSubTriggerCleanup();
        delete triggerEl._summitSubTriggerCleanup;
    }
}

/**
 * Cancel the close timer for a sub trigger (called when entering content)
 * @param {HTMLElement} triggerEl - Sub trigger element
 */
export function cancelSubTriggerClose(triggerEl) {
    const state = subTriggerState.get(triggerEl);
    if (state) {
        clearTimeout(state.closeTimer);
        state.closeTimer = null;
    }
}
