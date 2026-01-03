// accordion.js - ES6 module for accordion animations
// Note: Keyboard navigation has been moved to Blazor for better cross-render-mode compatibility.
// Only DOM measurement and scroll prevention functionality remains here.

// Store trigger handlers to prevent default scroll behavior
const triggerHandlers = new Map();

/**
 * Register trigger element to prevent default scroll behavior on arrow keys.
 * Call this on first render of the trigger.
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function registerTrigger(triggerEl) {
    if (!triggerEl || triggerHandlers.has(triggerEl)) return;
    
    function handleKeyDown(event) {
        // Prevent default scroll behavior for navigation keys
        if (['ArrowDown', 'ArrowUp', 'ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) {
            event.preventDefault();
        }
    }
    
    triggerEl.addEventListener('keydown', handleKeyDown);
    triggerHandlers.set(triggerEl, handleKeyDown);
}

/**
 * Unregister trigger element keyboard handler.
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

/**
 * Set CSS variables for content height and width (for smooth animations).
 * This requires DOM measurement which cannot be done in pure Blazor.
 * @param {HTMLElement} contentElement
 */
export function setContentHeight(contentElement) {
    if (!contentElement) return;

    // Temporarily make visible to measure
    const wasHidden = contentElement.hidden;
    if (wasHidden) {
        contentElement.style.visibility = 'hidden';
        contentElement.style.position = 'absolute';
        contentElement.hidden = false;
    }

    const height = contentElement.scrollHeight;
    const width = contentElement.scrollWidth;

    contentElement.style.setProperty('--summit-accordion-content-height', `${height}px`);
    contentElement.style.setProperty('--summit-accordion-content-width', `${width}px`);

    // Restore hidden state
    if (wasHidden) {
        contentElement.hidden = true;
        contentElement.style.visibility = '';
        contentElement.style.position = '';
    }
}
