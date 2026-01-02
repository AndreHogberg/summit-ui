/**
 * ArkUI Floating UI Module
 * Minimal wrapper around Floating UI for positioning floating elements.
 * All logic (keyboard, focus, events) is handled by Blazor.
 * This module only handles:
 * - Position calculation via FloatingUI
 * - Auto-update on scroll/resize
 * - Focus element (DOM operation)
 */

import {
    computePosition,
    autoUpdate,
    flip,
    shift,
    offset,
    arrow,
    limitShift,
    size
} from '@floating-ui/dom';

// Store instances for cleanup
const instances = new Map();

// Store outside click listeners
const outsideClickListeners = new Map();

// Store escape key listeners
const escapeKeyListeners = new Map();

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
 * Initialize floating positioning for an element.
 * @param {HTMLElement} referenceEl - The reference/anchor element
 * @param {HTMLElement} floatingEl - The floating element to position
 * @param {HTMLElement|null} arrowEl - Optional arrow element
 * @param {object} options - Positioning options
 * @returns {string} Instance ID for later cleanup
 */
export function initializeFloating(referenceEl, floatingEl, arrowEl, options) {
    if (!referenceEl || !floatingEl) return null;

    const instanceId = `floating-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    const placement = getPlacement(options.side || 'bottom', options.align || 'center');

    // Build middleware array
    const middleware = [
        offset({
            mainAxis: options.sideOffset || 0,
            crossAxis: options.alignOffset || 0
        })
    ];

    if (options.avoidCollisions !== false) {
        middleware.push(
            flip({ padding: options.collisionPadding || 8 }),
            shift({
                padding: options.collisionPadding || 8,
                limiter: limitShift()
            })
        );
    }

    // Add size middleware if constrainSize is enabled (for selects/dropdowns)
    if (options.constrainSize) {
        middleware.push(
            size({
                padding: options.collisionPadding || 8,
                apply({ availableWidth, availableHeight, elements }) {
                    Object.assign(elements.floating.style, {
                        maxWidth: `${availableWidth}px`,
                        maxHeight: `${availableHeight}px`
                    });
                }
            })
        );
    }

    if (arrowEl) {
        middleware.push(arrow({ element: arrowEl }));
    }

    // Position update function
    async function updatePosition() {
        const { x, y, placement: finalPlacement, middlewareData } = await computePosition(
            referenceEl,
            floatingEl,
            { placement, middleware }
        );

        Object.assign(floatingEl.style, {
            left: `${x}px`,
            top: `${y}px`,
            visibility: 'visible'
        });

        // Update data attributes for styling hooks
        const [side, align] = finalPlacement.split('-');
        floatingEl.setAttribute('data-side', side);
        if (align) {
            floatingEl.setAttribute('data-align', align);
        }

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

        return { x, y, placement: finalPlacement };
    }

    // Start auto-update (handles scroll, resize, etc.)
    const cleanupAutoUpdate = autoUpdate(
        referenceEl,
        floatingEl,
        updatePosition,
        {
            ancestorScroll: true,
            ancestorResize: true,
            elementResize: true,
            layoutShift: true
        }
    );

    // Store instance for cleanup
    instances.set(instanceId, {
        cleanup: cleanupAutoUpdate,
        updatePosition,
        referenceEl,
        floatingEl
    });

    return instanceId;
}

/**
 * Destroy floating positioning and cleanup.
 * @param {string} instanceId - The instance ID returned from initializeFloating
 */
export function destroyFloating(instanceId) {
    const instance = instances.get(instanceId);
    if (instance) {
        instance.cleanup();
        instances.delete(instanceId);
    }
}

/**
 * Manually trigger position update.
 * @param {string} instanceId - The instance ID
 */
export function updatePosition(instanceId) {
    const instance = instances.get(instanceId);
    if (instance) {
        return instance.updatePosition();
    }
    return Promise.resolve(null);
}

/**
 * Focus an element.
 * @param {HTMLElement} element - Element to focus
 */
export function focusElement(element) {
    element?.focus();
}

/**
 * Get focusable elements within a container.
 * @param {HTMLElement} container - Container element
 * @returns {HTMLElement[]} Array of focusable elements
 */
export function getFocusableElements(container) {
    if (!container) return [];
    
    const selectors = [
        'a[href]:not([disabled]):not([tabindex="-1"])',
        'button:not([disabled]):not([tabindex="-1"])',
        'input:not([disabled]):not([tabindex="-1"])',
        'select:not([disabled]):not([tabindex="-1"])',
        'textarea:not([disabled]):not([tabindex="-1"])',
        '[tabindex]:not([tabindex="-1"]):not([disabled])',
        '[contenteditable="true"]:not([disabled])'
    ].join(', ');

    const elements = Array.from(container.querySelectorAll(selectors));
    return elements.filter(el => 
        el.offsetParent !== null && 
        getComputedStyle(el).visibility !== 'hidden'
    );
}

/**
 * Focus the first focusable element within a container.
 * @param {HTMLElement} container - Container element
 */
export function focusFirstElement(container) {
    const elements = getFocusableElements(container);
    if (elements.length > 0) {
        elements[0].focus();
    } else if (container) {
        // Make container focusable and focus it
        if (!container.hasAttribute('tabindex')) {
            container.setAttribute('tabindex', '-1');
        }
        container.focus();
    }
}

/**
 * Focus the last focusable element within a container.
 * @param {HTMLElement} container - Container element
 */
export function focusLastElement(container) {
    const elements = getFocusableElements(container);
    if (elements.length > 0) {
        elements[elements.length - 1].focus();
    }
}

/**
 * Scroll an element into view within a container.
 * @param {HTMLElement} element - Element to scroll into view
 * @param {HTMLElement} container - Scrollable container
 */
export function scrollIntoView(element, container) {
    if (!element) return;
    
    if (container) {
        // Scroll within container
        const elementRect = element.getBoundingClientRect();
        const containerRect = container.getBoundingClientRect();
        
        if (elementRect.top < containerRect.top) {
            element.scrollIntoView({ block: 'nearest', behavior: 'instant' });
        } else if (elementRect.bottom > containerRect.bottom) {
            element.scrollIntoView({ block: 'nearest', behavior: 'instant' });
        }
    } else {
        element.scrollIntoView({ block: 'nearest', behavior: 'instant' });
    }
}

/**
 * Register an outside click listener for a floating element.
 * @param {HTMLElement} referenceEl - The reference/trigger element
 * @param {HTMLElement} floatingEl - The floating element
 * @param {object} dotNetRef - .NET object reference for callback
 * @param {string} methodName - Name of the method to call on outside click
 * @returns {string} Listener ID for cleanup
 */
export function registerOutsideClick(referenceEl, floatingEl, dotNetRef, methodName) {
    if (!referenceEl || !floatingEl || !dotNetRef) return null;

    const listenerId = `outside-click-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    let isDisposed = false;

    function handlePointerDown(event) {
        if (isDisposed) return;
        
        // Check if click is outside both reference and floating elements
        if (!floatingEl.contains(event.target) && !referenceEl.contains(event.target)) {
            dotNetRef.invokeMethodAsync(methodName).catch(() => {
                // DotNetObjectReference may have been disposed, ignore errors
            });
        }
    }

    // Use requestAnimationFrame to avoid catching the click that opened the floating element
    requestAnimationFrame(() => {
        if (!isDisposed) {
            document.addEventListener('pointerdown', handlePointerDown, true);
        }
    });

    outsideClickListeners.set(listenerId, {
        cleanup: () => {
            isDisposed = true;
            document.removeEventListener('pointerdown', handlePointerDown, true);
        }
    });

    return listenerId;
}

/**
 * Unregister an outside click listener.
 * @param {string} listenerId - The listener ID returned from registerOutsideClick
 */
export function unregisterOutsideClick(listenerId) {
    const listener = outsideClickListeners.get(listenerId);
    if (listener) {
        listener.cleanup();
        outsideClickListeners.delete(listenerId);
    }
}

/**
 * Register an Escape key listener.
 * @param {object} dotNetRef - .NET object reference for callback
 * @param {string} methodName - Name of the method to call on Escape key press
 * @returns {string} Listener ID for cleanup
 */
export function registerEscapeKey(dotNetRef, methodName) {
    if (!dotNetRef) return null;

    const listenerId = `escape-key-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    let isDisposed = false;

    function handleKeyDown(event) {
        if (isDisposed) return;
        
        if (event.key === 'Escape') {
            event.preventDefault();
            dotNetRef.invokeMethodAsync(methodName).catch(() => {
                // DotNetObjectReference may have been disposed, ignore errors
            });
        }
    }

    document.addEventListener('keydown', handleKeyDown);

    escapeKeyListeners.set(listenerId, {
        cleanup: () => {
            isDisposed = true;
            document.removeEventListener('keydown', handleKeyDown);
        }
    });

    return listenerId;
}

/**
 * Unregister an Escape key listener.
 * @param {string} listenerId - The listener ID returned from registerEscapeKey
 */
export function unregisterEscapeKey(listenerId) {
    const listener = escapeKeyListeners.get(listenerId);
    if (listener) {
        listener.cleanup();
        escapeKeyListeners.delete(listenerId);
    }
}
