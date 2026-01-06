/**
 * SummitUI Calendar JavaScript Module
 * Handles locale detection, Intl API formatting, and keyboard navigation support.
 */

// Store handlers to allow proper cleanup
const elementHandlers = new Map();

/**
 * Gets the browser's current locale.
 * @returns {string} The browser locale (e.g., "en-US", "sv-SE").
 */
export function getBrowserLocale() {
    return navigator.language || 'en-US';
}

/**
 * Gets the first day of the week for a locale using Intl.Locale.getWeekInfo().
 * Falls back to Sunday (0) if the API is not available.
 * @param {string} locale - The locale to check (e.g., "en-US", "de-DE").
 * @returns {number} The first day of the week (0 = Sunday, 1 = Monday, etc.).
 */
export function getFirstDayOfWeek(locale) {
    try {
        const localeObj = new Intl.Locale(locale);
        // getWeekInfo() returns { firstDay: number, weekend: number[], minimalDays: number }
        // firstDay: 1 = Monday, 7 = Sunday (ISO 8601)
        const weekInfo = localeObj.getWeekInfo?.() || localeObj.weekInfo;
        if (weekInfo && typeof weekInfo.firstDay === 'number') {
            // Convert from ISO (1=Mon, 7=Sun) to JS (0=Sun, 1=Mon, ...)
            return weekInfo.firstDay === 7 ? 0 : weekInfo.firstDay;
        }
    } catch {
        // API not available or locale invalid
    }
    
    // Fallback: common defaults by locale region
    const region = locale.split('-')[1]?.toUpperCase() || '';
    const mondayFirst = ['GB', 'DE', 'FR', 'ES', 'IT', 'NL', 'BE', 'AT', 'CH', 'PL', 'SE', 'NO', 'DK', 'FI'];
    const saturdayFirst = ['AE', 'AF', 'BH', 'DJ', 'DZ', 'EG', 'IQ', 'IR', 'JO', 'KW', 'LY', 'OM', 'QA', 'SA', 'SD', 'SY'];
    const fridayFirst = ['MV'];
    
    if (mondayFirst.includes(region)) return 1;
    if (saturdayFirst.includes(region)) return 6;
    if (fridayFirst.includes(region)) return 5;
    
    return 0; // Default to Sunday
}

/**
 * Gets the localized month name.
 * @param {string} locale - The locale to use.
 * @param {number} year - The year.
 * @param {number} month - The month (1-12).
 * @returns {string} The localized month name.
 */
export function getMonthName(locale, year, month) {
    try {
        const date = new Date(year, month - 1, 1);
        return new Intl.DateTimeFormat(locale, { month: 'long' }).format(date);
    } catch {
        const months = ['January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'];
        return months[month - 1] || '';
    }
}

/**
 * Gets the localized month and year heading.
 * @param {string} locale - The locale to use.
 * @param {number} year - The year.
 * @param {number} month - The month (1-12).
 * @returns {string} The localized month and year string.
 */
export function getMonthYearHeading(locale, year, month) {
    try {
        const date = new Date(year, month - 1, 1);
        return new Intl.DateTimeFormat(locale, { month: 'long', year: 'numeric' }).format(date);
    } catch {
        return `${getMonthName(locale, year, month)} ${year}`;
    }
}

/**
 * Gets localized weekday names (both short and long forms).
 * Returns arrays starting from Sunday (index 0).
 * @param {string} locale - The locale to use.
 * @returns {{ short: string[], long: string[] }} Object containing short and long weekday names.
 */
export function getWeekdayNames(locale) {
    const fallbackShort = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    const fallbackLong = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    try {
        const shortNames = [];
        const longNames = [];
        
        // Start from a known Sunday (Jan 7, 2024 is a Sunday)
        const baseDate = new Date(2024, 0, 7);
        
        for (let i = 0; i < 7; i++) {
            const date = new Date(baseDate);
            date.setDate(baseDate.getDate() + i);
            
            shortNames.push(new Intl.DateTimeFormat(locale, { weekday: 'short' }).format(date));
            longNames.push(new Intl.DateTimeFormat(locale, { weekday: 'long' }).format(date));
        }
        
        return { short: shortNames, long: longNames };
    } catch {
        return { short: fallbackShort, long: fallbackLong };
    }
}

/**
 * Gets the full localized date string for accessibility (aria-label).
 * @param {string} locale - The locale to use.
 * @param {number} year - The year.
 * @param {number} month - The month (1-12).
 * @param {number} day - The day of month.
 * @returns {string} The full localized date string.
 */
export function getFullDateString(locale, year, month, day) {
    try {
        const date = new Date(year, month - 1, day);
        return new Intl.DateTimeFormat(locale, { 
            weekday: 'long', 
            year: 'numeric', 
            month: 'long', 
            day: 'numeric' 
        }).format(date);
    } catch {
        return `${month}/${day}/${year}`;
    }
}

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
