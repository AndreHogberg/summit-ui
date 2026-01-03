/**
 * SummitUI Popover JavaScript Module
 * Uses Floating UI for positioning and focus-trap for focus management
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

import * as focusTrap from './focus-trap.js';

// Store cleanup functions and state per popover instance
const popoverInstances = new Map();

// Store portal containers
const portalContainers = new Map();

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
 * Initialize popover positioning and event listeners
 * @param {HTMLElement} triggerEl - Trigger button element
 * @param {HTMLElement} contentEl - Popover content element
 * @param {HTMLElement|null} arrowEl - Optional arrow element
 * @param {object} dotNetRef - .NET object reference for callbacks
 * @param {object} options - Positioning options
 */
export function initializePopover(triggerEl, contentEl, arrowEl, dotNetRef, options) {
    if (!triggerEl || !contentEl) return;

    const placement = getPlacement(options.side, options.align);

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

    // Handle escape key
    function handleEscapeKey(event) {
        if (isDisposed) return;
        if (event.key === 'Escape') {
            event.preventDefault();
            dotNetRef.invokeMethodAsync('HandleEscapeKey').catch(() => {
                // DotNetObjectReference may have been disposed, ignore errors
            });
        }
    }

    // Focus trap implementation using the reusable focus-trap module
    let focusTrapId = null;
    if (options.trapFocus) {
        focusTrapId = focusTrap.activate(contentEl, {
            autoFocus: true,
            returnFocus: false // We handle return focus ourselves
        });
    }

    // Add event listeners
    if (options.closeOnOutsideClick) {
        // Use requestAnimationFrame to avoid catching the click that opened the popover
        // This is safer than setTimeout as it fires at the next frame
        requestAnimationFrame(() => {
            if (!isDisposed) {
                document.addEventListener('pointerdown', handleOutsideClick, true);
            }
        });
    }
    if (options.closeOnEscape) {
        document.addEventListener('keydown', handleEscapeKey);
    }

    // Focus the content element (if not using focus trap, which handles this)
    if (!options.trapFocus) {
        contentEl.focus();
    }

    // Store cleanup function
    popoverInstances.set(contentEl, {
        cleanup: () => {
            isDisposed = true;
            cleanupAutoUpdate();
            document.removeEventListener('pointerdown', handleOutsideClick, true);
            document.removeEventListener('keydown', handleEscapeKey);
            if (focusTrapId) {
                focusTrap.deactivate(focusTrapId);
            }
        },
        updatePosition,
        triggerEl
    });
}

/**
 * Destroy popover and cleanup
 * @param {HTMLElement} contentEl - Popover content element
 */
export function destroyPopover(contentEl) {
    const instance = popoverInstances.get(contentEl);
    if (instance) {
        instance.cleanup();
        // Return focus to trigger
        instance.triggerEl?.focus();
        popoverInstances.delete(contentEl);
    }
}

/**
 * Manually update popover position
 * @param {HTMLElement} contentEl - Popover content element
 */
export function updatePosition(contentEl) {
    const instance = popoverInstances.get(contentEl);
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
    container.setAttribute('data-ark-popover-portal', '');
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
 * Focus the first focusable element within a container
 * @param {HTMLElement} containerEl - Container element
 */
export function focusFirstElement(containerEl) {
    focusTrap.focusFirst(containerEl);
}

/**
 * Focus a specific element
 * @param {HTMLElement} element - Element to focus
 */
export function focusElement(element) {
    focusTrap.focusElement(element);
}
