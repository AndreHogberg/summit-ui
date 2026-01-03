/**
 * SummitUI Dropdown Menu JavaScript Module
 * Minimal module for portal and trigger functionality only.
 * All other functionality (positioning, keyboard nav, etc.) is handled by Blazor + FloatingJsInterop.
 */

// Store portal containers
const portalContainers = new Map();

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
