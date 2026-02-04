/**
 * SummitUI Floating UI Module
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

// Stack-based escape key handling for nested dialogs.
// The stack ensures only the topmost dialog receives escape key events.
// Format: [{ id, dotNetRef, methodName }, ...]
const escapeKeyStack = [];

// Global escape key handler - only triggers the topmost listener
function handleGlobalEscapeKey(event) {
    if (event.key !== 'Escape' || escapeKeyStack.length === 0) return;

    event.preventDefault();

    // Only invoke the topmost (last) handler
    const top = escapeKeyStack[escapeKeyStack.length - 1];
    top.dotNetRef.invokeMethodAsync(top.methodName).catch(() => {
        // DotNetObjectReference may have been disposed, ignore errors
    });
}

// Store pending animation watchers for cleanup
const animationWatchers = new Map();

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
 * @returns {Promise<string>} Instance ID for later cleanup
 */
export async function initializeFloating(referenceEl, floatingEl, arrowEl, options) {
    if (!referenceEl || !floatingEl) return null;

    const instanceId = `floating-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    const placement = getPlacement(options.side || options.Side || 'bottom', options.align || options.Align || 'center');
    
    // Normalize options to handle both camelCase and PascalCase from .NET serialization
    const constrainSize = options.constrainSize || options.ConstrainSize || false;

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
    // Track if initial size has been applied to prevent bouncing on scroll
    let initialSizeApplied = false;
    if (constrainSize) {
        middleware.push(
            size({
                padding: options.collisionPadding || 8,
                apply({ availableWidth, availableHeight, elements }) {
                    // Only apply size constraints on initial positioning
                    // Applying on every scroll causes bouncing due to layout thrashing
                    if (!initialSizeApplied) {
                        Object.assign(elements.floating.style, {
                            maxWidth: `${availableWidth}px`,
                            maxHeight: `${availableHeight}px`
                        });
                        initialSizeApplied = true;
                    }
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

    // Do an initial position update before returning
    // This ensures the element is visible when the function returns
    await updatePosition();

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
 * Focus an element with retry mechanism for animated elements.
 * Elements with CSS animations starting at opacity:0 may reject focus initially.
 * This retries focus up to 5 times with 20ms delays to allow the animation
 * to progress past the invisible state.
 * @param {HTMLElement} element - Element to focus
 */
export function focusElement(element) {
    // Check if element is a valid DOM element with focus capability
    // Empty Blazor ElementReference arrives as {id: "", context: null} which is truthy but not focusable
    if (!element || typeof element.focus !== 'function') return;
    
    function tryFocus(attempts) {
        element.focus();
        // If focus didn't succeed and we have attempts left, retry
        if (document.activeElement !== element && attempts > 0) {
            setTimeout(() => tryFocus(attempts - 1), 20);
        }
    }
    
    // First attempt after one frame to let CSS apply
    requestAnimationFrame(() => tryFocus(5));
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
 * Scroll an item into view by its data-value attribute within a container.
 * @param {HTMLElement} container - Container element
 * @param {string} itemValue - The data-value attribute value of the item to scroll to
 */
export function scrollItemIntoView(container, itemValue) {
    if (!container || !itemValue) return;
    
    const item = container.querySelector(`[data-value="${CSS.escape(itemValue)}"]`);
    if (item) {
        item.scrollIntoView({ block: 'nearest', behavior: 'instant' });
    }
}

/**
 * Click an element by its ID.
 * @param {string} elementId - The ID of the element to click
 */
export function clickElementById(elementId) {
    if (!elementId) return;
    
    const element = document.getElementById(elementId);
    if (element) {
        element.click();
    }
}

/**
 * Scroll an element into view by its ID.
 * @param {string} elementId - The ID of the element to scroll into view
 */
export function scrollElementIntoViewById(elementId) {
    if (!elementId) return;
    
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ block: 'nearest', behavior: 'instant' });
    }
}

/**
 * Focus an element by its ID with retry mechanism for animated elements.
 * Elements with CSS animations starting at opacity:0 may reject focus initially.
 * This retries focus up to 5 times with 20ms delays to allow the animation
 * to progress past the invisible state.
 * @param {string} elementId - The ID of the element to focus
 */
export function focusElementById(elementId) {
    if (!elementId) return;
    
    const element = document.getElementById(elementId);
    if (!element) return;
    
    function tryFocus(attempts) {
        element.focus();
        // If focus didn't succeed and we have attempts left, retry
        if (document.activeElement !== element && attempts > 0) {
            setTimeout(() => tryFocus(attempts - 1), 20);
        }
    }
    
    // First attempt after one frame to let CSS apply
    requestAnimationFrame(() => tryFocus(5));
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
 * Uses a stack-based approach so only the topmost dialog receives the escape key.
 * @param {object} dotNetRef - .NET object reference for callback
 * @param {string} methodName - Name of the method to call on Escape key press
 * @returns {string} Listener ID for cleanup
 */
export function registerEscapeKey(dotNetRef, methodName) {
    if (!dotNetRef) return null;

    const listenerId = `escape-key-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

    // Add to the stack (newest/topmost at the end)
    escapeKeyStack.push({ id: listenerId, dotNetRef, methodName });

    // Register the global listener only when the first dialog opens
    if (escapeKeyStack.length === 1) {
        document.addEventListener('keydown', handleGlobalEscapeKey);
    }

    return listenerId;
}

/**
 * Unregister an Escape key listener.
 * @param {string} listenerId - The listener ID returned from registerEscapeKey
 */
export function unregisterEscapeKey(listenerId) {
    const index = escapeKeyStack.findIndex(entry => entry.id === listenerId);
    if (index !== -1) {
        escapeKeyStack.splice(index, 1);
    }

    // Remove the global listener when no dialogs are open
    if (escapeKeyStack.length === 0) {
        document.removeEventListener('keydown', handleGlobalEscapeKey);
    }
}

/**
 * Wait for all animations on an element to complete, then invoke a callback.
 * If no animations are running, calls back immediately.
 * This enables animation-aware presence management (hide after animations finish).
 * @param {HTMLElement} element - The element to watch for animations
 * @param {object} dotNetCallback - .NET object reference with invokeMethodAsync
 * @param {string} methodName - Name of the .NET method to call when animations complete
 */
export function waitForAnimationsComplete(element, dotNetCallback, methodName) {
    if (!element) {
        dotNetCallback.invokeMethodAsync(methodName);
        return;
    }

    // Cancel any existing watcher for this element
    cancelAnimationWatcher(element);

    // Check if getAnimations is supported
    if (typeof element.getAnimations !== 'function') {
        dotNetCallback.invokeMethodAsync(methodName);
        return;
    }

    // Use requestAnimationFrame to ensure we catch animations that just started
    const frameId = requestAnimationFrame(() => {
        const animations = element.getAnimations();

        if (animations.length === 0) {
            // No animations running, call back immediately
            animationWatchers.delete(element);
            dotNetCallback.invokeMethodAsync(methodName);
            return;
        }

        // Wait for all animations to complete
        Promise.allSettled(animations.map(a => a.finished))
            .then(() => {
                animationWatchers.delete(element);
                dotNetCallback.invokeMethodAsync(methodName);
            });
    });

    // Store for potential cleanup
    animationWatchers.set(element, { frameId });
}

/**
 * Cancel any pending animation watcher for an element.
 * Call this when the element is being disposed or state changes again.
 * @param {HTMLElement} element - The element to cancel watching
 */
export function cancelAnimationWatcher(element) {
    if (!element) return;
    
    const watcher = animationWatchers.get(element);
    if (watcher) {
        if (watcher.frameId) {
            cancelAnimationFrame(watcher.frameId);
        }
        animationWatchers.delete(element);
    }
}

/**
 * Get menu item IDs in DOM order within a container.
 * Used for keyboard navigation to ensure items are navigated in visual order.
 * @param {HTMLElement} container - The menu content container
 * @param {string[]} registeredIds - Array of IDs that have been registered
 * @returns {string[]} Array of IDs in DOM order
 */
export function getMenuItemsInDomOrder(container, registeredIds) {
    if (!container || !registeredIds || registeredIds.length === 0) {
        return registeredIds || [];
    }
    
    // Create a Set for O(1) lookup
    const registeredSet = new Set(registeredIds);
    
    // Query all menu items in DOM order
    const allMenuItems = container.querySelectorAll('[role="menuitem"]');
    
    // Filter to only include registered items, maintaining DOM order
    const orderedIds = [];
    for (const item of allMenuItems) {
        if (item.id && registeredSet.has(item.id)) {
            orderedIds.push(item.id);
        }
    }
    
    return orderedIds;
}
