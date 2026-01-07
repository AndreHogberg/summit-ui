/**
 * SummitUI JavaScript Bundle
 * Single entry point that exports all component modules with prefixed names
 * to avoid conflicts and enable tree-shaking where possible.
 */

// Core Utilities exports
export {
    isRtl as utilities_isRtl,
    focusElement as utilities_focusElement,
    focusElementById as utilities_focusElementById,
    initializeCheckbox as utilities_initializeCheckbox,
    destroyCheckbox as utilities_destroyCheckbox
} from './utilities.js';

// Floating UI wrapper exports (positioning only)
export {
    initializeFloating as floating_initializeFloating,
    destroyFloating as floating_destroyFloating,
    updatePosition as floating_updatePosition,
    focusElement as floating_focusElement,
    focusFirstElement as floating_focusFirstElement,
    focusLastElement as floating_focusLastElement,
    scrollIntoView as floating_scrollIntoView,
    scrollItemIntoView as floating_scrollItemIntoView,
    clickElementById as floating_clickElementById,
    scrollElementIntoViewById as floating_scrollElementIntoViewById,
    focusElementById as floating_focusElementById,
    registerOutsideClick as floating_registerOutsideClick,
    unregisterOutsideClick as floating_unregisterOutsideClick,
    registerEscapeKey as floating_registerEscapeKey,
    unregisterEscapeKey as floating_unregisterEscapeKey,
    waitForAnimationsComplete as floating_waitForAnimationsComplete,
    cancelAnimationWatcher as floating_cancelAnimationWatcher
} from './floating.js';

// Accordion exports
// Note: Keyboard navigation moved to Blazor. Handles DOM measurement, scroll prevention, and animation-aware presence.
export {
    setContentHeight as accordion_setContentHeight,
    registerTrigger as accordion_registerTrigger,
    unregisterTrigger as accordion_unregisterTrigger,
    waitForAnimationsComplete as accordion_waitForAnimationsComplete,
    cancelAnimationWatcher as accordion_cancelAnimationWatcher,
    setHidden as accordion_setHidden,
    removeHidden as accordion_removeHidden
} from './accordion.js';

// Focus Trap exports
export {
    activate as focusTrap_activate,
    deactivate as focusTrap_deactivate,
    deactivateByContainer as focusTrap_deactivateByContainer,
    focusFirst as focusTrap_focusFirst,
    focusLast as focusTrap_focusLast,
    focusElement as focusTrap_focusElement,
    isFocusable as focusTrap_isFocusable,
    getFocusableCount as focusTrap_getFocusableCount
} from './focus-trap.js';

// Popover exports (legacy - will be deprecated)
export {
    initializePopover as popover_initializePopover,
    destroyPopover as popover_destroyPopover,
    updatePosition as popover_updatePosition,
    createPortal as popover_createPortal,
    destroyPortal as popover_destroyPortal,
    focusFirstElement as popover_focusFirstElement,
    focusElement as popover_focusElement
} from './popover.js';

// Select exports (trigger functionality only - keyboard nav handled by Blazor)
export {
    registerTrigger as select_registerTrigger,
    unregisterTrigger as select_unregisterTrigger
} from './select.js';

// DateField exports (segment interaction)
export {
    initializeSegment as dateField_initializeSegment,
    destroySegment as dateField_destroySegment,
    getBrowserLocale as dateField_getBrowserLocale,
    getSegmentLabels as dateField_getSegmentLabels,
    getDayPeriodDesignators as dateField_getDayPeriodDesignators,
    // Calendar system support
    getLocaleDateFormat as dateField_getLocaleDateFormat,
    getCalendarDateInfo as dateField_getCalendarDateInfo,
    getSegmentBounds as dateField_getSegmentBounds,
    addToDateSegment as dateField_addToDateSegment,
    setDateSegmentValue as dateField_setDateSegmentValue
} from './date-field.js';

// Dropdown Menu exports (portal and trigger functionality only - keyboard nav handled by Blazor)
export {
    createPortal as dropdownMenu_createPortal,
    destroyPortal as dropdownMenu_destroyPortal,
    initializeTrigger as dropdownMenu_initializeTrigger,
    destroyTrigger as dropdownMenu_destroyTrigger
} from './dropdown-menu.js';

// Dialog exports (scroll lock and portal management)
export {
    lockScroll as dialog_lockScroll,
    unlockScroll as dialog_unlockScroll,
    forceUnlockScroll as dialog_forceUnlockScroll,
    createPortal as dialog_createPortal,
    destroyPortal as dialog_destroyPortal,
    getScrollLockCount as dialog_getScrollLockCount
} from './dialog.js';

// Calendar exports (locale detection, Intl API, calendar systems, keyboard navigation)
export {
    getBrowserLocale as calendar_getBrowserLocale,
    getFirstDayOfWeek as calendar_getFirstDayOfWeek,
    getMonthName as calendar_getMonthName,
    getMonthYearHeading as calendar_getMonthYearHeading,
    getWeekdayNames as calendar_getWeekdayNames,
    getFullDateString as calendar_getFullDateString,
    initializeCalendar as calendar_initializeCalendar,
    focusDate as calendar_focusDate,
    destroyCalendar as calendar_destroyCalendar,
    // Calendar system functions
    convertFromGregorian as calendar_convertFromGregorian,
    convertToGregorian as calendar_convertToGregorian,
    getCalendarMonthInfo as calendar_getCalendarMonthInfo,
    getDaysInMonth as calendar_getDaysInMonth,
    getMonthsInYear as calendar_getMonthsInYear,
    batchConvertFromGregorian as calendar_batchConvertFromGregorian
} from './calendar.js';
