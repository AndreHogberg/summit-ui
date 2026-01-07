/**
 * SummitUI Calendar JavaScript Module
 * Handles keyboard navigation support for Calendar component.
 * 
 * Date conversion and locale formatting have been moved to C# using
 * System.Globalization (CalendarProvider and CalendarFormatter services).
 */

// Store handlers to allow proper cleanup
const elementHandlers = new Map();

/**
 * Keys that should have their default browser behavior prevented when pressed on calendar day buttons.
 * This prevents page scrolling and screen reader virtual cursor movement.
 */
const NAVIGATION_KEYS = new Set([
    'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight',
    'Home', 'End', 'PageUp', 'PageDown'
]);

/**
 * Initializes keyboard navigation support for calendar day buttons.
 * Uses capture phase to intercept keyboard events before screen readers can consume them.
 * The actual navigation logic is handled by Blazor's C# event handlers.
 * @param {HTMLElement} element - The calendar grid element.
 */
export function initializeCalendar(element) {
    if (!element || elementHandlers.has(element)) return;

    /**
     * Handle keydown in capture phase to prevent default browser behavior.
     * We only preventDefault, NOT stopPropagation - Blazor needs to receive the event.
     */
    function handleKeyDown(event) {
        // Only handle events on calendar day buttons
        const target = event.target;
        if (!target || !target.hasAttribute('data-summit-calendar-day')) return;

        const key = event.key;
        
        if (NAVIGATION_KEYS.has(key)) {
            // Prevent default browser behavior (scrolling)
            event.preventDefault();
            // DO NOT stopPropagation - Blazor's handlers need to receive this event
        }
        
        // Prevent default for Space and Enter to avoid double selection
        // Space: prevents page scroll
        // Enter: prevents native button activation (we handle it via Blazor's keydown handler)
        if (key === ' ' || key === 'Enter') {
            event.preventDefault();
        }
    }

    // Use capture phase (third argument = true) to run before bubbling phase handlers
    element.addEventListener('keydown', handleKeyDown, true);
    elementHandlers.set(element, { handleKeyDown });
}

// Track pending focus operations to avoid race conditions
let pendingFocusDate = null;
let pendingFocusCalendarRoot = null;
let pendingFocusFrame = null;

/**
 * Focuses a specific date button element.
 * Uses requestAnimationFrame to defer focus until after Blazor completes all DOM updates.
 * This prevents focus from being lost when navigating across month boundaries,
 * where the fire-and-forget UpdateMonthNameIfChangedAsync causes a second render
 * that may replace the focused element.
 * @param {HTMLElement} element - The calendar day button element to focus.
 */
export function focusDate(element) {
    if (!element) return;
    
    const dateAttr = element.getAttribute('data-date');
    
    // Find the calendar root this element belongs to (for scoped queries when element is recreated)
    const calendarRoot = element.closest('[data-summit-calendar-root]');
    
    // Store the date and calendar root we want to focus (not the element, which may be replaced)
    pendingFocusDate = dateAttr;
    pendingFocusCalendarRoot = calendarRoot;
    
    // Cancel any pending focus operation
    if (pendingFocusFrame) {
        cancelAnimationFrame(pendingFocusFrame);
    }
    
    // Defer focus to the next animation frame, after Blazor completes DOM updates
    pendingFocusFrame = requestAnimationFrame(() => {
        pendingFocusFrame = null;
        
        if (!pendingFocusDate) return;
        
        // Find the element by data-date attribute within the same calendar (it may have been recreated)
        // This ensures we focus the correct calendar when multiple calendars are on the page
        let targetElement;
        if (pendingFocusCalendarRoot) {
            targetElement = pendingFocusCalendarRoot.querySelector(`[data-summit-calendar-day][data-date="${pendingFocusDate}"]`);
        } else {
            targetElement = document.querySelector(`[data-summit-calendar-day][data-date="${pendingFocusDate}"]`);
        }
        
        if (!targetElement) {
            pendingFocusDate = null;
            pendingFocusCalendarRoot = null;
            return;
        }
        
        // Blur the currently focused element first to ensure clean focus transition
        const currentlyFocused = document.activeElement;
        if (currentlyFocused && currentlyFocused !== targetElement && currentlyFocused !== document.body) {
            currentlyFocused.blur();
        }
        
        targetElement.focus();
        
        pendingFocusDate = null;
        pendingFocusCalendarRoot = null;
    });
}

/**
 * Cleanup event listeners.
 * @param {HTMLElement} element - The calendar grid element.
 */
export function destroyCalendar(element) {
    if (!element) return;

    const handlers = elementHandlers.get(element);
    if (handlers) {
        element.removeEventListener('keydown', handlers.handleKeyDown, true);
        elementHandlers.delete(element);
    }
}
