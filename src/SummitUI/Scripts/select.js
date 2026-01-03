/**
 * SummitUI Select JavaScript Module
 * Minimal module for trigger-related functionality only.
 * All other functionality (positioning, keyboard nav, etc.) is handled by Blazor + FloatingJsInterop.
 */

// Store trigger handlers separately (active before dropdown opens)
const triggerHandlers = new Map();

/**
 * Register trigger element to prevent default scroll behavior on arrow keys
 * Call this on first render of the trigger, before any dropdown interaction
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function registerTrigger(triggerEl) {
    if (!triggerEl || triggerHandlers.has(triggerEl)) return;
    
    function handleKeyDown(event) {
        // Prevent default scroll behavior for navigation keys
        if (['ArrowDown', 'ArrowUp', 'Home', 'End', ' ', 'Enter'].includes(event.key)) {
            event.preventDefault();
        }
    }
    
    triggerEl.addEventListener('keydown', handleKeyDown);
    triggerHandlers.set(triggerEl, handleKeyDown);
}

/**
 * Unregister trigger element keyboard handler
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function unregisterTrigger(triggerEl) {
    if (!triggerEl) return;
    
    const handler = triggerHandlers.get(triggerEl);
    if (handler) {
        triggerEl.removeEventListener('keydown', handler);
        triggerHandlers.delete(triggerEl);
    }
}
