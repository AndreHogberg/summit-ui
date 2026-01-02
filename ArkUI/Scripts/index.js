/**
 * ArkUI JavaScript Bundle
 * Single entry point that exports all component modules with prefixed names
 * to avoid conflicts and enable tree-shaking where possible.
 */

// Accordion exports
export {
    initializeAccordion as accordion_initializeAccordion,
    destroyAccordion as accordion_destroyAccordion,
    focusTrigger as accordion_focusTrigger,
    setContentHeight as accordion_setContentHeight
} from './accordion.js';

// Tabs exports
export {
    initializeTabs as tabs_initializeTabs,
    destroyTabs as tabs_destroyTabs,
    focusTrigger as tabs_focusTrigger
} from './tabs.js';

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

// Popover exports
export {
    initializePopover as popover_initializePopover,
    destroyPopover as popover_destroyPopover,
    updatePosition as popover_updatePosition,
    createPortal as popover_createPortal,
    destroyPortal as popover_destroyPortal,
    focusFirstElement as popover_focusFirstElement,
    focusElement as popover_focusElement
} from './popover.js';

// Select exports
export {
    initializeSelect as select_initializeSelect,
    destroySelect as select_destroySelect,
    updatePosition as select_updatePosition,
    highlightItem as select_highlightItem,
    highlightFirst as select_highlightFirst,
    highlightLast as select_highlightLast,
    focusTrigger as select_focusTrigger,
    registerTrigger as select_registerTrigger,
    unregisterTrigger as select_unregisterTrigger
} from './select.js';

// Dropdown Menu exports
export {
    initializeDropdownMenu as dropdownMenu_initializeDropdownMenu,
    destroyDropdownMenu as dropdownMenu_destroyDropdownMenu,
    updatePosition as dropdownMenu_updatePosition,
    createPortal as dropdownMenu_createPortal,
    destroyPortal as dropdownMenu_destroyPortal,
    focusFirstItem as dropdownMenu_focusFirstItem,
    focusLastItem as dropdownMenu_focusLastItem
} from './dropdown-menu.js';
