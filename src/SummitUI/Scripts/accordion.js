// accordion.js - ES6 module for accordion animations
// Note: Keyboard navigation has been moved to Blazor for better cross-render-mode compatibility.
// Handles DOM measurement, scroll prevention, and animation-aware presence management.

// Store trigger handlers to prevent default scroll behavior
const triggerHandlers = new Map();

// Store pending animation watchers for cleanup
const animationWatchers = new Map();

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

/**
 * Wait for all animations on an element to complete, then invoke a callback.
 * If no animations are running, calls back immediately.
 * This enables animation-aware presence management (hide after animations finish).
 * @param {HTMLElement} element - The element to watch for animations
 * @param {Function} dotNetCallback - .NET object reference with invokeMethodAsync
 * @param {string} methodName - Name of the .NET method to call when animations complete
 */
export function waitForAnimationsComplete(element, dotNetCallback, methodName) {
    if (!element) {
        dotNetCallback.invokeMethodAsync(methodName);
        return;
    }

    // Cancel any existing watcher for this element
    cancelAnimationWatcher(element);

    // Check if getAnimations is supported
    if (typeof element.getAnimations !== 'function') {
        dotNetCallback.invokeMethodAsync(methodName);
        return;
    }

    // Use requestAnimationFrame to ensure we catch animations that just started
    const frameId = requestAnimationFrame(() => {
        const animations = element.getAnimations();

        if (animations.length === 0) {
            // No animations running, call back immediately
            animationWatchers.delete(element);
            dotNetCallback.invokeMethodAsync(methodName);
            return;
        }

        // Wait for all animations to complete
        Promise.allSettled(animations.map(a => a.finished))
            .then(() => {
                animationWatchers.delete(element);
                dotNetCallback.invokeMethodAsync(methodName);
            });
    });

    // Store for potential cleanup
    animationWatchers.set(element, { frameId });
}

/**
 * Cancel any pending animation watcher for an element.
 * Call this when the element is being disposed or state changes again.
 * @param {HTMLElement} element - The element to cancel watching
 */
export function cancelAnimationWatcher(element) {
    if (!element) return;
    
    const watcher = animationWatchers.get(element);
    if (watcher) {
        if (watcher.frameId) {
            cancelAnimationFrame(watcher.frameId);
        }
        animationWatchers.delete(element);
    }
}

/**
 * Set the hidden attribute on an element.
 * Used as a callback after animations complete.
 * @param {HTMLElement} element - The element to hide
 */
export function setHidden(element) {
    if (element) {
        element.hidden = true;
    }
}

/**
 * Remove the hidden attribute from an element.
 * @param {HTMLElement} element - The element to show
 */
export function removeHidden(element) {
    if (element) {
        element.hidden = false;
    }
}
