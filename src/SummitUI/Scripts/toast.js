/**
 * Toast utilities for SummitUI
 * Handles hotkey registration, swipe gesture support, and portal rendering for toast notifications
 */

// Store for registered hotkey handlers
const hotkeyHandlers = new WeakMap();

// Store for swipe gesture handlers
const swipeHandlers = new WeakMap();

// Store for portal containers (tracks original parent for cleanup)
const portalContainers = new Map();

/**
 * Move a toast region element to body to avoid CSS stacking context issues.
 * @param {string} elementId - ID of the element to move to body
 */
export function createPortal(elementId) {
    if (portalContainers.has(elementId)) return;

    const element = document.getElementById(elementId);
    if (!element) {
        console.warn(`[SummitUI Toast] Portal element not found: ${elementId}`);
        return;
    }

    // Check if already a direct child of body
    if (element.parentElement === document.body) {
        portalContainers.set(elementId, { element, originalParent: null });
        return;
    }

    // Store the original parent so we could restore if needed
    const originalParent = element.parentElement;
    
    // Move the element to body
    document.body.appendChild(element);
    
    portalContainers.set(elementId, { element, originalParent });
}

/**
 * Cleanup portal tracking (element is removed by Blazor)
 * @param {string} elementId - ID of the portaled element
 */
export function destroyPortal(elementId) {
    portalContainers.delete(elementId);
}

/**
 * Parses hotkey configuration into a matcher function.
 * @param {string[]} hotkey - Array of key codes (e.g., ["F8"] or ["altKey", "KeyT"])
 * @returns {function} Matcher function that returns true if the event matches the hotkey
 */
function createHotkeyMatcher(hotkey) {
    return (event) => {
        // Check for modifier keys
        const modifiers = {
            altKey: hotkey.includes('altKey'),
            ctrlKey: hotkey.includes('ctrlKey'),
            metaKey: hotkey.includes('metaKey'),
            shiftKey: hotkey.includes('shiftKey')
        };

        // Get the main key (non-modifier)
        const mainKey = hotkey.find(k => !['altKey', 'ctrlKey', 'metaKey', 'shiftKey'].includes(k));

        // Check all modifiers match
        if (modifiers.altKey !== event.altKey) return false;
        if (modifiers.ctrlKey !== event.ctrlKey) return false;
        if (modifiers.metaKey !== event.metaKey) return false;
        if (modifiers.shiftKey !== event.shiftKey) return false;

        // Check main key matches
        if (mainKey && event.code !== mainKey) return false;

        return true;
    };
}

/**
 * Registers a keyboard hotkey to focus the toast viewport.
 * @param {HTMLElement} element - The viewport element to focus
 * @param {string[]} hotkey - Array of key codes
 * @param {object} dotNetRef - .NET object reference for callbacks
 * @param {string} methodName - Name of the method to invoke
 */
export function registerHotkey(element, hotkey, dotNetRef, methodName) {
    if (!element || hotkeyHandlers.has(element)) return;

    const matcher = createHotkeyMatcher(hotkey);

    const handler = (event) => {
        if (matcher(event)) {
            event.preventDefault();
            element.focus();
            dotNetRef.invokeMethodAsync(methodName);
        }
    };

    document.addEventListener('keydown', handler);
    hotkeyHandlers.set(element, handler);
}

/**
 * Unregisters the keyboard hotkey handler.
 * @param {HTMLElement} element - The viewport element
 */
export function unregisterHotkey(element) {
    if (!element) return;

    const handler = hotkeyHandlers.get(element);
    if (handler) {
        document.removeEventListener('keydown', handler);
        hotkeyHandlers.delete(element);
    }
}

/**
 * Checks if a swipe delta exceeds the threshold in the specified direction.
 * @param {object} delta - The delta object with x and y values
 * @param {string} direction - The swipe direction
 * @param {number} threshold - The threshold in pixels
 * @returns {boolean}
 */
function isDeltaInDirection(delta, direction, threshold) {
    const { x, y } = delta;
    const absX = Math.abs(x);
    const absY = Math.abs(y);

    switch (direction) {
        case 'right':
            return x >= threshold;
        case 'left':
            return x <= -threshold;
        case 'up':
            return y <= -threshold;
        case 'down':
            return y >= threshold;
        default:
            return false;
    }
}

/**
 * Registers swipe gesture handling for a toast element.
 * @param {HTMLElement} element - The toast element
 * @param {string} direction - Swipe direction ("left", "right", "up", "down")
 * @param {number} threshold - Distance in pixels before triggering close
 * @param {object} dotNetRef - .NET object reference for callbacks
 */
export function registerSwipe(element, direction, threshold, dotNetRef) {
    if (!element || swipeHandlers.has(element)) return;

    let pointerStart = null;
    let swipeDelta = null;

    const handlePointerDown = (event) => {
        if (event.button !== 0) return; // Only handle primary button
        pointerStart = { x: event.clientX, y: event.clientY };
        element.style.userSelect = 'none';
        element.style.touchAction = 'none';
    };

    const handlePointerMove = (event) => {
        if (!pointerStart) return;

        const x = event.clientX - pointerStart.x;
        const y = event.clientY - pointerStart.y;
        const absX = Math.abs(x);
        const absY = Math.abs(y);

        // Determine if this is a swipe in the configured direction
        const isHorizontal = ['left', 'right'].includes(direction);
        const isVertical = ['up', 'down'].includes(direction);

        // Only start tracking if moving more in the swipe direction
        const isValidSwipe = (isHorizontal && absX > absY) || (isVertical && absY > absX);

        if (!swipeDelta && isValidSwipe) {
            swipeDelta = { x: 0, y: 0 };
            element.setAttribute('data-swipe', 'start');
            dotNetRef.invokeMethodAsync('HandleSwipeStart');
        }

        if (swipeDelta) {
            swipeDelta = { x, y };
            element.setAttribute('data-swipe', 'move');

            // Set CSS custom properties for animation
            element.style.setProperty('--summit-toast-swipe-move-x', `${x}px`);
            element.style.setProperty('--summit-toast-swipe-move-y', `${y}px`);

            dotNetRef.invokeMethodAsync('HandleSwipeMove', x, y);
        }
    };

    const handlePointerUp = (event) => {
        if (!pointerStart) return;

        element.style.userSelect = '';
        element.style.touchAction = '';

        if (swipeDelta) {
            const delta = swipeDelta;

            if (isDeltaInDirection(delta, direction, threshold)) {
                // Swipe completed - set end CSS variables
                element.setAttribute('data-swipe', 'end');
                element.style.removeProperty('--summit-toast-swipe-move-x');
                element.style.removeProperty('--summit-toast-swipe-move-y');
                element.style.setProperty('--summit-toast-swipe-end-x', `${delta.x}px`);
                element.style.setProperty('--summit-toast-swipe-end-y', `${delta.y}px`);

                dotNetRef.invokeMethodAsync('HandleSwipeEnd', delta.x, delta.y);
            } else {
                // Swipe cancelled
                element.setAttribute('data-swipe', 'cancel');
                element.style.removeProperty('--summit-toast-swipe-move-x');
                element.style.removeProperty('--summit-toast-swipe-move-y');
                element.style.removeProperty('--summit-toast-swipe-end-x');
                element.style.removeProperty('--summit-toast-swipe-end-y');

                dotNetRef.invokeMethodAsync('HandleSwipeCancel');
            }
        }

        pointerStart = null;
        swipeDelta = null;
    };

    const handlePointerCancel = () => {
        if (swipeDelta) {
            element.setAttribute('data-swipe', 'cancel');
            element.style.removeProperty('--summit-toast-swipe-move-x');
            element.style.removeProperty('--summit-toast-swipe-move-y');
            element.style.removeProperty('--summit-toast-swipe-end-x');
            element.style.removeProperty('--summit-toast-swipe-end-y');

            dotNetRef.invokeMethodAsync('HandleSwipeCancel');
        }

        element.style.userSelect = '';
        element.style.touchAction = '';
        pointerStart = null;
        swipeDelta = null;
    };

    element.addEventListener('pointerdown', handlePointerDown);
    element.addEventListener('pointermove', handlePointerMove);
    element.addEventListener('pointerup', handlePointerUp);
    element.addEventListener('pointercancel', handlePointerCancel);

    swipeHandlers.set(element, {
        pointerdown: handlePointerDown,
        pointermove: handlePointerMove,
        pointerup: handlePointerUp,
        pointercancel: handlePointerCancel
    });
}

/**
 * Unregisters swipe gesture handling.
 * @param {HTMLElement} element - The toast element
 */
export function unregisterSwipe(element) {
    if (!element) return;

    const handlers = swipeHandlers.get(element);
    if (handlers) {
        element.removeEventListener('pointerdown', handlers.pointerdown);
        element.removeEventListener('pointermove', handlers.pointermove);
        element.removeEventListener('pointerup', handlers.pointerup);
        element.removeEventListener('pointercancel', handlers.pointercancel);
        swipeHandlers.delete(element);
    }
}

/**
 * Focuses the toast viewport element.
 * @param {HTMLElement} element - The viewport element to focus
 */
export function focusViewport(element) {
    if (element) {
        element.focus();
    }
}
