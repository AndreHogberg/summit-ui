/**
 * SummitUI Calendar JavaScript Module
 * Handles locale detection, Intl API formatting, calendar systems, and keyboard navigation support.
 */

import {
    CalendarDate,
    GregorianCalendar,
    JapaneseCalendar,
    BuddhistCalendar,
    TaiwanCalendar,
    PersianCalendar,
    IndianCalendar,
    IslamicUmalquraCalendar,
    IslamicCivilCalendar,
    IslamicTabularCalendar,
    HebrewCalendar,
    CopticCalendar,
    EthiopicCalendar,
    EthiopicAmeteAlemCalendar,
    toCalendar,
    startOfMonth,
    endOfMonth
} from '@internationalized/date';

// Store handlers to allow proper cleanup
const elementHandlers = new Map();

/**
 * Maps C# CalendarSystem enum values to @internationalized/date calendar instances.
 * The enum values are: Gregorian=0, Japanese=1, Buddhist=2, Taiwan=3, Persian=4,
 * Indian=5, IslamicUmalqura=6, IslamicCivil=7, IslamicTabular=8, Hebrew=9,
 * Coptic=10, Ethiopic=11, EthiopicAmeteAlem=12
 */
const calendarMap = {
    0: () => new GregorianCalendar(),
    1: () => new JapaneseCalendar(),
    2: () => new BuddhistCalendar(),
    3: () => new TaiwanCalendar(),
    4: () => new PersianCalendar(),
    5: () => new IndianCalendar(),
    6: () => new IslamicUmalquraCalendar(),
    7: () => new IslamicCivilCalendar(),
    8: () => new IslamicTabularCalendar(),
    9: () => new HebrewCalendar(),
    10: () => new CopticCalendar(),
    11: () => new EthiopicCalendar(),
    12: () => new EthiopicAmeteAlemCalendar()
};

/**
 * Maps C# CalendarSystem enum values to Intl calendar identifiers.
 */
const calendarIdentifierMap = {
    0: 'gregory',
    1: 'japanese',
    2: 'buddhist',
    3: 'roc',
    4: 'persian',
    5: 'indian',
    6: 'islamic-umalqura',
    7: 'islamic-civil',
    8: 'islamic-tbla',
    9: 'hebrew',
    10: 'coptic',
    11: 'ethiopic',
    12: 'ethioaa'
};

/**
 * Gets a calendar instance for the given calendar system enum value.
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {Calendar} The calendar instance.
 */
function getCalendar(calendarSystem) {
    const factory = calendarMap[calendarSystem];
    return factory ? factory() : new GregorianCalendar();
}

/**
 * Gets the Intl calendar identifier for the given calendar system enum value.
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {string} The Intl calendar identifier.
 */
function getCalendarIdentifier(calendarSystem) {
    return calendarIdentifierMap[calendarSystem] || 'gregory';
}

/**
 * Converts a Gregorian date to a date in the specified calendar system.
 * @param {number} gregorianYear - The Gregorian year.
 * @param {number} gregorianMonth - The Gregorian month (1-12).
 * @param {number} gregorianDay - The Gregorian day.
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {{ year: number, month: number, day: number, era: string }} The date in the target calendar.
 */
export function convertFromGregorian(gregorianYear, gregorianMonth, gregorianDay, calendarSystem) {
    const gregorianDate = new CalendarDate(new GregorianCalendar(), gregorianYear, gregorianMonth, gregorianDay);
    const targetCalendar = getCalendar(calendarSystem);
    const targetDate = toCalendar(gregorianDate, targetCalendar);
    
    return {
        year: targetDate.year,
        month: targetDate.month,
        day: targetDate.day,
        era: targetDate.era || ''
    };
}

/**
 * Converts a date from the specified calendar system to Gregorian.
 * @param {number} year - The year in the source calendar.
 * @param {number} month - The month in the source calendar (1-based).
 * @param {number} day - The day in the source calendar.
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {{ year: number, month: number, day: number }} The Gregorian date.
 */
export function convertToGregorian(year, month, day, calendarSystem) {
    const sourceCalendar = getCalendar(calendarSystem);
    const sourceDate = new CalendarDate(sourceCalendar, year, month, day);
    const gregorianDate = toCalendar(sourceDate, new GregorianCalendar());
    
    return {
        year: gregorianDate.year,
        month: gregorianDate.month,
        day: gregorianDate.day
    };
}

/**
 * Gets calendar month information for rendering a month grid.
 * Returns information about the month in the specified calendar system,
 * with all dates also provided as Gregorian equivalents for binding.
 * @param {number} gregorianYear - The Gregorian year of the first day of the displayed month.
 * @param {number} gregorianMonth - The Gregorian month (1-12).
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {object} Month information including days in month, months in year, etc.
 */
export function getCalendarMonthInfo(gregorianYear, gregorianMonth, calendarSystem) {
    const gregorianDate = new CalendarDate(new GregorianCalendar(), gregorianYear, gregorianMonth, 1);
    const targetCalendar = getCalendar(calendarSystem);
    const targetDate = toCalendar(gregorianDate, targetCalendar);
    
    const calendar = targetDate.calendar;
    const daysInMonth = calendar.getDaysInMonth(targetDate);
    const monthsInYear = calendar.getMonthsInYear(targetDate);
    
    return {
        year: targetDate.year,
        month: targetDate.month,
        day: targetDate.day,
        era: targetDate.era || '',
        daysInMonth: daysInMonth,
        monthsInYear: monthsInYear
    };
}

/**
 * Gets the number of days in a month for a given calendar date.
 * @param {number} year - The year in the calendar system.
 * @param {number} month - The month in the calendar system (1-based).
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {number} The number of days in the month.
 */
export function getDaysInMonth(year, month, calendarSystem) {
    const calendar = getCalendar(calendarSystem);
    const date = new CalendarDate(calendar, year, month, 1);
    return calendar.getDaysInMonth(date);
}

/**
 * Gets the number of months in a year for a given calendar date.
 * This is important for calendars like Hebrew which can have 12 or 13 months.
 * @param {number} year - The year in the calendar system.
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {number} The number of months in the year.
 */
export function getMonthsInYear(year, calendarSystem) {
    const calendar = getCalendar(calendarSystem);
    // Use month 1 as a reference point
    const date = new CalendarDate(calendar, year, 1, 1);
    return calendar.getMonthsInYear(date);
}

/**
 * Batch converts multiple Gregorian dates to the specified calendar system.
 * More efficient than calling convertFromGregorian for each date individually.
 * @param {string} locale - The locale for formatting localized date strings.
 * @param {Array<[number, number, number]>} dates - Array of [year, month, day] arrays.
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {Array<{day: number, localizedDate: string}>} Array of conversion results.
 */
export function batchConvertFromGregorian(locale, dates, calendarSystem) {
    const results = [];
    const targetCalendar = getCalendar(calendarSystem);
    const calendarId = getCalendarIdentifier(calendarSystem);
    
    // Create the date formatter once for all dates
    const options = { 
        weekday: 'long', 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
    };
    if (calendarId !== 'gregory') {
        options.calendar = calendarId;
    }
    
    let formatter;
    try {
        formatter = new Intl.DateTimeFormat(locale, options);
    } catch {
        formatter = null;
    }
    
    for (const [year, month, day] of dates) {
        try {
            // Convert from Gregorian to target calendar
            const gregorianDate = new CalendarDate(new GregorianCalendar(), year, month, day);
            const targetDate = toCalendar(gregorianDate, targetCalendar);
            
            // Format the localized date string
            let localizedDate;
            if (formatter) {
                const jsDate = new Date(year, month - 1, day);
                localizedDate = formatter.format(jsDate);
            } else {
                localizedDate = `${month}/${day}/${year}`;
            }
            
            results.push({
                day: targetDate.day,
                localizedDate: localizedDate
            });
        } catch {
            // Fallback for any conversion errors
            results.push({
                day: day,
                localizedDate: `${month}/${day}/${year}`
            });
        }
    }
    
    return results;
}

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
 * @param {number} year - The Gregorian year.
 * @param {number} month - The Gregorian month (1-12).
 * @param {number} calendarSystem - The C# CalendarSystem enum value (optional, defaults to Gregorian).
 * @returns {string} The localized month name.
 */
export function getMonthName(locale, year, month, calendarSystem = 0) {
    try {
        const date = new Date(year, month - 1, 1);
        const options = { month: 'long' };
        const calendarId = getCalendarIdentifier(calendarSystem);
        if (calendarId !== 'gregory') {
            options.calendar = calendarId;
        }
        return new Intl.DateTimeFormat(locale, options).format(date);
    } catch {
        const months = ['January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'];
        return months[month - 1] || '';
    }
}

/**
 * Gets the localized month and year heading.
 * @param {string} locale - The locale to use.
 * @param {number} year - The Gregorian year.
 * @param {number} month - The Gregorian month (1-12).
 * @param {number} calendarSystem - The C# CalendarSystem enum value (optional, defaults to Gregorian).
 * @returns {string} The localized month and year string.
 */
export function getMonthYearHeading(locale, year, month, calendarSystem = 0) {
    try {
        const date = new Date(year, month - 1, 1);
        const options = { month: 'long', year: 'numeric' };
        const calendarId = getCalendarIdentifier(calendarSystem);
        if (calendarId !== 'gregory') {
            options.calendar = calendarId;
        }
        return new Intl.DateTimeFormat(locale, options).format(date);
    } catch {
        return `${getMonthName(locale, year, month, calendarSystem)} ${year}`;
    }
}

/**
 * Gets localized weekday names (both short and long forms).
 * Returns arrays starting from Sunday (index 0).
 * @param {string} locale - The locale to use.
 * @param {number} calendarSystem - The C# CalendarSystem enum value (optional, defaults to Gregorian).
 * @returns {{ short: string[], long: string[] }} Object containing short and long weekday names.
 */
export function getWeekdayNames(locale, calendarSystem = 0) {
    const fallbackShort = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    const fallbackLong = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    try {
        const shortNames = [];
        const longNames = [];
        
        // Start from a known Sunday (Jan 7, 2024 is a Sunday)
        const baseDate = new Date(2024, 0, 7);
        
        const calendarId = getCalendarIdentifier(calendarSystem);
        const shortOptions = { weekday: 'short' };
        const longOptions = { weekday: 'long' };
        
        if (calendarId !== 'gregory') {
            shortOptions.calendar = calendarId;
            longOptions.calendar = calendarId;
        }
        
        for (let i = 0; i < 7; i++) {
            const date = new Date(baseDate);
            date.setDate(baseDate.getDate() + i);
            
            shortNames.push(new Intl.DateTimeFormat(locale, shortOptions).format(date));
            longNames.push(new Intl.DateTimeFormat(locale, longOptions).format(date));
        }
        
        return { short: shortNames, long: longNames };
    } catch {
        return { short: fallbackShort, long: fallbackLong };
    }
}

/**
 * Gets the full localized date string for accessibility (aria-label).
 * @param {string} locale - The locale to use.
 * @param {number} year - The Gregorian year.
 * @param {number} month - The Gregorian month (1-12).
 * @param {number} day - The Gregorian day of month.
 * @param {number} calendarSystem - The C# CalendarSystem enum value (optional, defaults to Gregorian).
 * @returns {string} The full localized date string.
 */
export function getFullDateString(locale, year, month, day, calendarSystem = 0) {
    try {
        const date = new Date(year, month - 1, day);
        const options = { 
            weekday: 'long', 
            year: 'numeric', 
            month: 'long', 
            day: 'numeric' 
        };
        const calendarId = getCalendarIdentifier(calendarSystem);
        if (calendarId !== 'gregory') {
            options.calendar = calendarId;
        }
        return new Intl.DateTimeFormat(locale, options).format(date);
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
