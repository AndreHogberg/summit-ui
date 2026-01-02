/**
 * ArkUI Dropdown Menu JavaScript Module
 * Uses Floating UI for positioning and provides keyboard navigation
 */

import {
    computePosition,
    autoUpdate,
    flip,
    shift,
    offset,
    arrow,
    limitShift
} from '@floating-ui/dom';

// Store cleanup functions and state per dropdown menu instance
const menuInstances = new Map();

// Store portal containers
const portalContainers = new Map();

// Selector for focusable menu items
const MENU_ITEM_SELECTOR = '[data-ark-dropdown-menu-item]:not([data-disabled]), [data-ark-dropdown-menu-checkbox-item]:not([data-disabled]), [data-ark-dropdown-menu-radio-item]:not([data-disabled])';

/**
 * Convert side + align to Floating UI placement string
 * @param {string} side - 'top' | 'right' | 'bottom' | 'left'
 * @param {string} align - 'start' | 'center' | 'end'
 * @returns {string} Floating UI placement
 */
function getPlacement(side, align) {
    if (align === 'center') return side;
    return `${side}-${align}`;
}

/**
 * Get the opposite side for arrow positioning
 * @param {string} side
 * @returns {string}
 */
function getOppositeSide(side) {
    const opposites = {
        top: 'bottom',
        right: 'left',
        bottom: 'top',
        left: 'right'
    };
    return opposites[side] || 'top';
}

/**
 * Get all focusable menu items within a content element
 * @param {HTMLElement} contentEl
 * @returns {HTMLElement[]}
 */
function getMenuItems(contentEl) {
    return Array.from(contentEl.querySelectorAll(MENU_ITEM_SELECTOR));
}

/**
 * Get the currently focused menu item index
 * @param {HTMLElement[]} items
 * @returns {number}
 */
function getCurrentItemIndex(items) {
    const activeElement = document.activeElement;
    return items.indexOf(activeElement);
}

/**
 * Focus a menu item by index with looping support
 * @param {HTMLElement[]} items
 * @param {number} index
 * @param {boolean} loop
 */
function focusItemByIndex(items, index, loop) {
    if (items.length === 0) return;

    let targetIndex = index;
    if (loop) {
        if (index < 0) targetIndex = items.length - 1;
        if (index >= items.length) targetIndex = 0;
    } else {
        targetIndex = Math.max(0, Math.min(index, items.length - 1));
    }

    items[targetIndex]?.focus();
}

/**
 * Handle typeahead search within the menu
 * @param {HTMLElement} contentEl
 * @param {string} char
 * @param {object} state
 */
function handleTypeahead(contentEl, char, state) {
    const items = getMenuItems(contentEl);
    if (items.length === 0) return;

    // Clear search after delay
    clearTimeout(state.typeaheadTimeout);
    state.typeaheadSearch = (state.typeaheadSearch || '') + char.toLowerCase();

    state.typeaheadTimeout = setTimeout(() => {
        state.typeaheadSearch = '';
    }, 500);

    // Find matching item
    const currentIndex = getCurrentItemIndex(items);
    const startIndex = currentIndex >= 0 ? currentIndex + 1 : 0;

    // Search from current position, then wrap around
    for (let i = 0; i < items.length; i++) {
        const index = (startIndex + i) % items.length;
        const item = items[index];
        const text = item.textContent?.toLowerCase().trim() || '';

        if (text.startsWith(state.typeaheadSearch)) {
            item.focus();
            break;
        }
    }
}

/**
 * Initialize dropdown menu positioning and event listeners
 * @param {HTMLElement} triggerEl - Trigger button element
 * @param {HTMLElement} contentEl - Menu content element
 * @param {HTMLElement|null} arrowEl - Optional arrow element
 * @param {object} dotNetRef - .NET object reference for callbacks
 * @param {object} options - Positioning options
 */
export function initializeDropdownMenu(triggerEl, contentEl, arrowEl, dotNetRef, options) {
    if (!triggerEl || !contentEl) return;

    const placement = getPlacement(options.side, options.align);
    const state = {
        typeaheadSearch: '',
        typeaheadTimeout: null
    };

    // Build middleware array
    const middleware = [
        offset({
            mainAxis: options.sideOffset,
            crossAxis: options.alignOffset
        })
    ];

    if (options.avoidCollisions) {
        middleware.push(
            flip({ padding: options.collisionPadding }),
            shift({
                padding: options.collisionPadding,
                limiter: limitShift()
            })
        );
    }

    if (arrowEl) {
        middleware.push(arrow({ element: arrowEl }));
    }

    // Position update function
    async function updatePosition() {
        const { x, y, placement: finalPlacement, middlewareData } = await computePosition(
            triggerEl,
            contentEl,
            { placement, middleware }
        );

        Object.assign(contentEl.style, {
            left: `${x}px`,
            top: `${y}px`,
            visibility: 'visible' // Make visible after positioning to prevent flash in top-left corner
        });

        // Update data attributes for styling hooks
        const [side] = finalPlacement.split('-');
        contentEl.setAttribute('data-side', side);

        // Position arrow if present
        if (arrowEl && middlewareData.arrow) {
            const { x: arrowX, y: arrowY } = middlewareData.arrow;
            const staticSide = getOppositeSide(side);

            Object.assign(arrowEl.style, {
                left: arrowX != null ? `${arrowX}px` : '',
                top: arrowY != null ? `${arrowY}px` : '',
                right: '',
                bottom: '',
                [staticSide]: '-4px'
            });
        }
    }

    // Start auto-update (handles scroll, resize, etc.)
    const cleanupAutoUpdate = autoUpdate(
        triggerEl,
        contentEl,
        updatePosition,
        {
            ancestorScroll: true,
            ancestorResize: true,
            elementResize: true,
            layoutShift: true
        }
    );

    // Track if instance is disposed to prevent callbacks after cleanup
    let isDisposed = false;

    // Handle outside clicks
    function handleOutsideClick(event) {
        if (isDisposed) return;
        if (!contentEl.contains(event.target) && !triggerEl.contains(event.target)) {
            dotNetRef.invokeMethodAsync('HandleOutsideClick').catch(() => {
                // DotNetObjectReference may have been disposed, ignore errors
            });
        }
    }

    // Handle keyboard navigation within the menu
    function handleKeyDown(event) {
        if (isDisposed) return;
        const items = getMenuItems(contentEl);
        const currentIndex = getCurrentItemIndex(items);

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                focusItemByIndex(items, currentIndex + 1, options.loop);
                break;

            case 'ArrowUp':
                event.preventDefault();
                focusItemByIndex(items, currentIndex - 1, options.loop);
                break;

            case 'Home':
                event.preventDefault();
                focusItemByIndex(items, 0, false);
                break;

            case 'End':
                event.preventDefault();
                focusItemByIndex(items, items.length - 1, false);
                break;

            case 'Escape':
                if (options.closeOnEscape) {
                    event.preventDefault();
                    dotNetRef.invokeMethodAsync('HandleEscapeKey').catch(() => {
                        // DotNetObjectReference may have been disposed, ignore errors
                    });
                }
                break;

            case 'Enter':
            case ' ':
                // Let the item handle its own click
                if (document.activeElement?.matches(MENU_ITEM_SELECTOR)) {
                    event.preventDefault();
                    document.activeElement.click();
                }
                break;

            default:
                // Typeahead search for printable characters
                if (event.key.length === 1 && !event.ctrlKey && !event.metaKey && !event.altKey) {
                    handleTypeahead(contentEl, event.key, state);
                }
                break;
        }
    }

    // Handle item click to select
    function handleItemClick(event) {
        if (isDisposed) return;
        const item = event.target.closest('[data-ark-dropdown-menu-item]');
        if (item && !item.hasAttribute('data-disabled')) {
            dotNetRef.invokeMethodAsync('HandleItemSelect', item.dataset.value || '').catch(() => {
                // DotNetObjectReference may have been disposed, ignore errors
            });
        }
    }

    // Add event listeners
    if (options.closeOnOutsideClick) {
        // Use requestAnimationFrame to avoid catching the click that opened the menu
        // This is safer than setTimeout as it fires at the next frame
        requestAnimationFrame(() => {
            if (!isDisposed) {
                document.addEventListener('pointerdown', handleOutsideClick, true);
            }
        });
    }

    contentEl.addEventListener('keydown', handleKeyDown);
    contentEl.addEventListener('click', handleItemClick);

    // Focus first item when menu opens
    requestAnimationFrame(() => {
        if (isDisposed) return;
        const items = getMenuItems(contentEl);
        if (items.length > 0) {
            items[0].focus();
        } else {
            contentEl.focus();
        }
    });

    // Store cleanup function
    menuInstances.set(contentEl, {
        cleanup: () => {
            isDisposed = true;
            cleanupAutoUpdate();
            document.removeEventListener('pointerdown', handleOutsideClick, true);
            contentEl.removeEventListener('keydown', handleKeyDown);
            contentEl.removeEventListener('click', handleItemClick);
            clearTimeout(state.typeaheadTimeout);
        },
        updatePosition,
        triggerEl
    });
}

/**
 * Destroy dropdown menu and cleanup
 * @param {HTMLElement} contentEl - Menu content element
 */
export function destroyDropdownMenu(contentEl) {
    const instance = menuInstances.get(contentEl);
    if (instance) {
        instance.cleanup();
        // Return focus to trigger
        instance.triggerEl?.focus();
        menuInstances.delete(contentEl);
    }
}

/**
 * Manually update menu position
 * @param {HTMLElement} contentEl - Menu content element
 */
export function updatePosition(contentEl) {
    const instance = menuInstances.get(contentEl);
    if (instance) {
        instance.updatePosition();
    }
}

/**
 * Create a portal container at the specified location
 * @param {string} containerId - Unique ID for the container
 * @param {string|null} targetSelector - CSS selector for parent (default: body)
 */
export function createPortal(containerId, targetSelector) {
    if (portalContainers.has(containerId)) return;

    const container = document.createElement('div');
    container.id = containerId;
    container.setAttribute('data-ark-dropdown-menu-portal', '');
    container.style.position = 'absolute';
    container.style.top = '0';
    container.style.left = '0';
    container.style.zIndex = '9999';

    const target = targetSelector
        ? document.querySelector(targetSelector)
        : document.body;

    if (target) {
        target.appendChild(container);
        portalContainers.set(containerId, container);
    }
}

/**
 * Destroy and remove a portal container
 * @param {string} containerId - ID of the container to remove
 */
export function destroyPortal(containerId) {
    const container = portalContainers.get(containerId);
    if (container) {
        container.remove();
        portalContainers.delete(containerId);
    }
}

/**
 * Focus the first menu item
 * @param {HTMLElement} contentEl - Menu content element
 */
export function focusFirstItem(contentEl) {
    const items = getMenuItems(contentEl);
    if (items.length > 0) {
        items[0].focus();
    }
}

/**
 * Focus the last menu item
 * @param {HTMLElement} contentEl - Menu content element
 */
export function focusLastItem(contentEl) {
    const items = getMenuItems(contentEl);
    if (items.length > 0) {
        items[items.length - 1].focus();
    }
}

/**
 * Initialize trigger to prevent default scroll on arrow keys
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function initializeTrigger(triggerEl) {
    if (!triggerEl) return;

    function handleKeyDown(event) {
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
        }
    }

    triggerEl.addEventListener('keydown', handleKeyDown);

    // Store cleanup function on element
    triggerEl._arkCleanup = () => {
        triggerEl.removeEventListener('keydown', handleKeyDown);
    };
}

/**
 * Cleanup trigger event listeners
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function destroyTrigger(triggerEl) {
    if (triggerEl?._arkCleanup) {
        triggerEl._arkCleanup();
        delete triggerEl._arkCleanup;
    }
}
