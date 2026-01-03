/**
 * ArkUI JavaScript Bundle
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
    unregisterEscapeKey as floating_unregisterEscapeKey
} from './floating.js';

// Accordion exports
// Note: Keyboard navigation moved to Blazor. Only DOM measurement and scroll prevention remains.
export {
    setContentHeight as accordion_setContentHeight,
    registerTrigger as accordion_registerTrigger,
    unregisterTrigger as accordion_unregisterTrigger
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

// Dropdown Menu exports (portal and trigger functionality only - keyboard nav handled by Blazor)
export {
    createPortal as dropdownMenu_createPortal,
    destroyPortal as dropdownMenu_destroyPortal,
    initializeTrigger as dropdownMenu_initializeTrigger,
    destroyTrigger as dropdownMenu_destroyTrigger
} from './dropdown-menu.js';
