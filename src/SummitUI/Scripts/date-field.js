/**
 * SummitUI DateField JavaScript Module
 * Handles keyboard navigation, numeric input, and segment interaction for DateField.
 */

import {
    CalendarDate,
    CalendarDateTime,
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
    toCalendar
} from '@internationalized/date';

// Store handlers to allow proper cleanup
const elementHandlers = new Map();

// Store typed digits for buffered input
const inputBuffers = new Map();

/**
 * Maps C# CalendarSystem enum values to @internationalized/date calendar instances.
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
 * Gets a calendar instance for the given calendar system enum value.
 * @param {number} calendarSystem - The C# CalendarSystem enum value.
 * @returns {Calendar} The calendar instance.
 */
function getCalendar(calendarSystem) {
    const factory = calendarMap[calendarSystem];
    return factory ? factory() : new GregorianCalendar();
}

/**
 * Gets the browser's current locale.
 * @returns {string} The browser locale (e.g., "en-US", "sv-SE").
 */
export function getBrowserLocale() {
    return navigator.language || 'en-US';
}

/**
 * Gets localized segment labels using Intl.DisplayNames.
 * Falls back to English if the API is not available or fails.
 * @param {string} locale - The locale to use (e.g., "en-US", "sv-SE").
 * @returns {Object} Object containing localized labels for each segment type.
 */
export function getSegmentLabels(locale) {
    const fallback = {
        year: 'Year',
        month: 'Month',
        day: 'Day',
        hour: 'Hour',
        minute: 'Minute',
        second: 'Second',
        dayPeriod: 'AM/PM'
    };

    if (typeof Intl.DisplayNames === 'undefined') {
        return fallback;
    }

    try {
        const dn = new Intl.DisplayNames(locale, { type: 'dateTimeField' });
        return {
            year: dn.of('year') ?? fallback.year,
            month: dn.of('month') ?? fallback.month,
            day: dn.of('day') ?? fallback.day,
            hour: dn.of('hour') ?? fallback.hour,
            minute: dn.of('minute') ?? fallback.minute,
            second: dn.of('second') ?? fallback.second,
            dayPeriod: dn.of('dayPeriod') ?? fallback.dayPeriod
        };
    } catch {
        return fallback;
    }
}

/**
 * Gets localized AM/PM designators using Intl.DateTimeFormat.
 * @param {string} locale - The locale to use (e.g., "en-US", "sv-SE").
 * @returns {Object} Object containing 'am' and 'pm' designator strings.
 */
export function getDayPeriodDesignators(locale) {
    const fallback = { am: 'AM', pm: 'PM' };
    
    try {
        const formatter = new Intl.DateTimeFormat(locale, {
            hour: 'numeric',
            hour12: true
        });
        
        // Format a time in AM (6:00 AM)
        const amDate = new Date(2000, 0, 1, 6, 0, 0);
        const amParts = formatter.formatToParts(amDate);
        const amPart = amParts.find(p => p.type === 'dayPeriod');
        
        // Format a time in PM (18:00 / 6:00 PM)
        const pmDate = new Date(2000, 0, 1, 18, 0, 0);
        const pmParts = formatter.formatToParts(pmDate);
        const pmPart = pmParts.find(p => p.type === 'dayPeriod');
        
        return {
            am: amPart?.value ?? fallback.am,
            pm: pmPart?.value ?? fallback.pm
        };
    } catch {
        return fallback;
    }
}

/**
 * Initializes interactions for a date field segment.
 * @param {HTMLElement} element - The segment element.
 * @param {DotNetObjectReference} dotNetHelper - Reference to C# component for callbacks.
 */
export function initializeSegment(element, dotNetHelper) {
    if (!element || elementHandlers.has(element)) return;

    // Get segment type for input handling
    const segmentType = element.getAttribute('data-segment');
    const isDayPeriod = segmentType === 'dayperiod';
    
    // Clear input buffer for this element
    inputBuffers.set(element, { value: '', timeout: null });

    function handleKeyDown(event) {
        const key = event.key;
        
        // Navigation keys
        if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(key)) {
            event.preventDefault();
            handleNavigation(element, key);
            return;
        }
        
        // Tab navigation - allow default behavior but clear buffer
        if (key === 'Tab') {
            clearInputBuffer(element);
            return;
        }
        
        // Backspace - clear the segment value
        if (key === 'Backspace' || key === 'Delete') {
            event.preventDefault();
            clearInputBuffer(element);
            dotNetHelper.invokeMethodAsync('ClearSegment');
            return;
        }
        
        // Numeric input (0-9)
        if (/^[0-9]$/.test(key) && !isDayPeriod) {
            event.preventDefault();
            handleNumericInput(element, key, dotNetHelper);
            return;
        }
        
        // AM/PM toggle for day period segments
        if (isDayPeriod && /^[aApP]$/i.test(key)) {
            event.preventDefault();
            if (/^[aA]$/.test(key)) {
                dotNetHelper.invokeMethodAsync('SetDayPeriod', 'AM');
            } else {
                dotNetHelper.invokeMethodAsync('SetDayPeriod', 'PM');
            }
            return;
        }
        
        // Prevent default for other character keys
        if (key.length === 1 && !event.ctrlKey && !event.metaKey) {
            event.preventDefault();
        }
    }
    
    function handleNavigation(el, key) {
        if (key === 'ArrowRight') {
            focusNextSegment(el);
        } else if (key === 'ArrowLeft') {
            focusPrevSegment(el);
        } else if (key === 'ArrowUp') {
            dotNetHelper.invokeMethodAsync('IncrementSegment');
        } else if (key === 'ArrowDown') {
            dotNetHelper.invokeMethodAsync('DecrementSegment');
        }
    }
    
    function handleNumericInput(el, digit, helper) {
        const buffer = inputBuffers.get(el);
        if (!buffer) return;
        
        // Clear any existing timeout
        if (buffer.timeout) {
            clearTimeout(buffer.timeout);
        }
        
        // Append digit to buffer
        buffer.value += digit;
        
        // Get the max length based on segment type
        const maxLength = getMaxInputLength(segmentType);
        
        // Parse the current buffered value
        const numValue = parseInt(buffer.value, 10);
        
        // Send the value to C#
        helper.invokeMethodAsync('SetSegmentValue', numValue);
        
        // If buffer is full or value is "complete", move to next segment
        if (buffer.value.length >= maxLength || isInputComplete(segmentType, buffer.value)) {
            clearInputBuffer(el);
            focusNextSegment(el);
        } else {
            // Set timeout to commit after delay
            buffer.timeout = setTimeout(() => {
                clearInputBuffer(el);
            }, 1000);
        }
    }
    
    function getMaxInputLength(type) {
        switch (type) {
            case 'year': return 4;
            case 'month':
            case 'day':
            case 'hour':
            case 'minute':
            case 'second': return 2;
            default: return 2;
        }
    }
    
    function isInputComplete(type, value) {
        const num = parseInt(value, 10);
        switch (type) {
            case 'month':
                // If first digit is > 1, complete immediately (e.g., "2" -> February)
                // Or if value > 12, we've exceeded max
                return value.length === 1 && num > 1 || value.length >= 2;
            case 'day':
                // If first digit is > 3, complete immediately
                return value.length === 1 && num > 3 || value.length >= 2;
            case 'hour':
                // For 24-hour: if first digit > 2, complete immediately
                // For 12-hour: if first digit > 1, complete immediately
                return value.length === 1 && num > 2 || value.length >= 2;
            case 'minute':
            case 'second':
                // If first digit > 5, complete immediately
                return value.length === 1 && num > 5 || value.length >= 2;
            case 'year':
                return value.length >= 4;
            default:
                return value.length >= 2;
        }
    }
    
    function clearInputBuffer(el) {
        const buffer = inputBuffers.get(el);
        if (buffer) {
            if (buffer.timeout) {
                clearTimeout(buffer.timeout);
            }
            buffer.value = '';
            buffer.timeout = null;
        }
    }

    function focusNextSegment(el) {
        let next = el.nextElementSibling;
        while (next && next.getAttribute('data-segment') === 'literal') {
            next = next.nextElementSibling;
        }
        if (next && next.hasAttribute('data-segment')) {
            next.focus();
        }
    }

    function focusPrevSegment(el) {
        let prev = el.previousElementSibling;
        while (prev && prev.getAttribute('data-segment') === 'literal') {
            prev = prev.previousElementSibling;
        }
        if (prev && prev.hasAttribute('data-segment')) {
            prev.focus();
        }
    }
    
    function handleFocus(event) {
        // Clear buffer on focus to start fresh input
        clearInputBuffer(element);
    }
    
    function handleBlur(event) {
        // Clear buffer on blur
        clearInputBuffer(element);
    }

    // Register all event listeners
    element.addEventListener('keydown', handleKeyDown);
    element.addEventListener('focus', handleFocus);
    element.addEventListener('blur', handleBlur);
    
    elementHandlers.set(element, { 
        handleKeyDown, 
        handleFocus, 
        handleBlur
    });
}

// ============================================================================
// Calendar System Support Functions
// ============================================================================

/**
 * Gets the locale's preferred date format pattern.
 * Uses Intl.DateTimeFormat formatToParts to detect the order of day, month, year.
 * @param {string} locale - The locale (e.g., "en-US", "de-DE")
 * @returns {{ dateFormat: string, dateSeparator: string }}
 */
export function getLocaleDateFormat(locale) {
    try {
        const formatter = new Intl.DateTimeFormat(locale, {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit'
        });
        
        // Use a reference date to get the parts
        const parts = formatter.formatToParts(new Date(2000, 0, 15));
        
        // Build format string based on part order
        let format = '';
        let separator = '/';
        
        for (const part of parts) {
            switch (part.type) {
                case 'year':
                    format += 'yyyy';
                    break;
                case 'month':
                    format += 'MM';
                    break;
                case 'day':
                    format += 'dd';
                    break;
                case 'literal':
                    if (format.length > 0 && format.length < 8) {
                        // This is a separator between date parts
                        format += part.value;
                        separator = part.value;
                    }
                    break;
            }
        }
        
        return { dateFormat: format || 'yyyy-MM-dd', dateSeparator: separator };
    } catch {
        return { dateFormat: 'yyyy-MM-dd', dateSeparator: '-' };
    }
}

/**
 * Converts a Gregorian date to the specified calendar system and returns full info.
 * @param {number} gregorianYear
 * @param {number} gregorianMonth (1-12)
 * @param {number} gregorianDay
 * @param {number} calendarSystem - CalendarSystem enum value
 * @returns {{ year: number, month: number, day: number, era: string, daysInMonth: number, monthsInYear: number }}
 */
export function getCalendarDateInfo(gregorianYear, gregorianMonth, gregorianDay, calendarSystem) {
    try {
        const gregorianDate = new CalendarDate(new GregorianCalendar(), gregorianYear, gregorianMonth, gregorianDay);
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
    } catch (error) {
        // Fallback to Gregorian
        return {
            year: gregorianYear,
            month: gregorianMonth,
            day: gregorianDay,
            era: '',
            daysInMonth: new Date(gregorianYear, gregorianMonth, 0).getDate(),
            monthsInYear: 12
        };
    }
}

/**
 * Gets min/max values for a segment in a calendar system.
 * @param {string} segmentType - "year" | "month" | "day" | "hour" | "minute"
 * @param {number} gregorianYear
 * @param {number} gregorianMonth (1-12)
 * @param {number} gregorianDay
 * @param {number} calendarSystem
 * @param {boolean} use12Hour - Whether using 12-hour clock
 * @returns {{ min: number, max: number }}
 */
export function getSegmentBounds(segmentType, gregorianYear, gregorianMonth, gregorianDay, calendarSystem, use12Hour) {
    try {
        const gregorianDate = new CalendarDate(new GregorianCalendar(), gregorianYear, gregorianMonth, gregorianDay);
        const targetCalendar = getCalendar(calendarSystem);
        const targetDate = toCalendar(gregorianDate, targetCalendar);
        
        const calendar = targetDate.calendar;
        
        switch (segmentType) {
            case 'year':
                return { min: 1, max: 9999 };
            case 'month':
                return { min: 1, max: calendar.getMonthsInYear(targetDate) };
            case 'day':
                return { min: 1, max: calendar.getDaysInMonth(targetDate) };
            case 'hour':
                return use12Hour ? { min: 1, max: 12 } : { min: 0, max: 23 };
            case 'minute':
                return { min: 0, max: 59 };
            default:
                return { min: 0, max: 0 };
        }
    } catch {
        // Fallback defaults
        switch (segmentType) {
            case 'year': return { min: 1, max: 9999 };
            case 'month': return { min: 1, max: 12 };
            case 'day': return { min: 1, max: 31 };
            case 'hour': return use12Hour ? { min: 1, max: 12 } : { min: 0, max: 23 };
            case 'minute': return { min: 0, max: 59 };
            default: return { min: 0, max: 0 };
        }
    }
}

/**
 * Adds a value to a date segment respecting calendar system.
 * Returns the new Gregorian date.
 * @param {number} gregorianYear
 * @param {number} gregorianMonth (1-12)
 * @param {number} gregorianDay
 * @param {string} segmentType - "year" | "month" | "day"
 * @param {number} amount - Amount to add (can be negative)
 * @param {number} calendarSystem
 * @returns {{ year: number, month: number, day: number }}
 */
export function addToDateSegment(gregorianYear, gregorianMonth, gregorianDay, segmentType, amount, calendarSystem) {
    try {
        const gregorianDate = new CalendarDate(new GregorianCalendar(), gregorianYear, gregorianMonth, gregorianDay);
        const targetCalendar = getCalendar(calendarSystem);
        let targetDate = toCalendar(gregorianDate, targetCalendar);
        
        // Add to the appropriate segment
        switch (segmentType) {
            case 'year':
                targetDate = targetDate.add({ years: amount });
                break;
            case 'month':
                targetDate = targetDate.add({ months: amount });
                break;
            case 'day':
                targetDate = targetDate.add({ days: amount });
                break;
        }
        
        // Convert back to Gregorian
        const resultGregorian = toCalendar(targetDate, new GregorianCalendar());
        
        return {
            year: resultGregorian.year,
            month: resultGregorian.month,
            day: resultGregorian.day
        };
    } catch {
        // Fallback to JavaScript Date arithmetic
        const date = new Date(gregorianYear, gregorianMonth - 1, gregorianDay);
        switch (segmentType) {
            case 'year':
                date.setFullYear(date.getFullYear() + amount);
                break;
            case 'month':
                date.setMonth(date.getMonth() + amount);
                break;
            case 'day':
                date.setDate(date.getDate() + amount);
                break;
        }
        return {
            year: date.getFullYear(),
            month: date.getMonth() + 1,
            day: date.getDate()
        };
    }
}

/**
 * Sets a specific segment value in the calendar system and returns the new Gregorian date.
 * @param {number} gregorianYear
 * @param {number} gregorianMonth (1-12)
 * @param {number} gregorianDay
 * @param {string} segmentType - "year" | "month" | "day"
 * @param {number} newValue - The new value in the calendar system
 * @param {number} calendarSystem
 * @returns {{ year: number, month: number, day: number }}
 */
export function setDateSegmentValue(gregorianYear, gregorianMonth, gregorianDay, segmentType, newValue, calendarSystem) {
    try {
        const gregorianDate = new CalendarDate(new GregorianCalendar(), gregorianYear, gregorianMonth, gregorianDay);
        const targetCalendar = getCalendar(calendarSystem);
        const targetDate = toCalendar(gregorianDate, targetCalendar);
        
        let newYear = targetDate.year;
        let newMonth = targetDate.month;
        let newDay = targetDate.day;
        
        switch (segmentType) {
            case 'year':
                newYear = newValue;
                break;
            case 'month':
                newMonth = newValue;
                break;
            case 'day':
                newDay = newValue;
                break;
        }
        
        // Clamp day to valid range for the new month/year
        const testDate = new CalendarDate(targetCalendar, newYear, newMonth, 1);
        const maxDay = targetCalendar.getDaysInMonth(testDate);
        newDay = Math.min(newDay, maxDay);
        
        // Create the new date in the target calendar
        const newTargetDate = new CalendarDate(targetCalendar, newYear, newMonth, newDay);
        
        // Convert back to Gregorian
        const resultGregorian = toCalendar(newTargetDate, new GregorianCalendar());
        
        return {
            year: resultGregorian.year,
            month: resultGregorian.month,
            day: resultGregorian.day
        };
    } catch {
        // Fallback - just return the input
        return {
            year: gregorianYear,
            month: gregorianMonth,
            day: gregorianDay
        };
    }
}

/**
 * Cleanup event listeners.
 * @param {HTMLElement} element - The segment element.
 */
export function destroySegment(element) {
    if (!element) return;
    
    const handlers = elementHandlers.get(element);
    if (handlers) {
        element.removeEventListener('keydown', handlers.handleKeyDown);
        element.removeEventListener('focus', handlers.handleFocus);
        element.removeEventListener('blur', handlers.handleBlur);
        elementHandlers.delete(element);
    }
    
    // Clean up input buffer
    const buffer = inputBuffers.get(element);
    if (buffer && buffer.timeout) {
        clearTimeout(buffer.timeout);
    }
    inputBuffers.delete(element);
}
