/**
 * SummitUI DateField JavaScript Module
 * Handles keyboard navigation, numeric input, and segment interaction for DateField.
 */

// Store handlers to allow proper cleanup
const elementHandlers = new Map();

// Store typed digits for buffered input
const inputBuffers = new Map();

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
