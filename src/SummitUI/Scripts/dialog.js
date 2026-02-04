/**
 * Dialog utilities for SummitUI
 * Handles scroll locking for modal dialogs with proper iOS Safari support
 */

// Track active scroll locks (supports nested dialogs)
let scrollLockCount = 0;
let originalStyles = null;
let scrollPosition = 0;
let scrollingElement = null;
let scrollLockHandlers = null;

function isInDialogContent(target) {
    if (!(target instanceof Element)) return false;
    return !!target.closest('[data-summit-dialog-content]');
}

function preventBackgroundScroll(event) {
    if (isInDialogContent(event.target)) return;
    event.preventDefault();
}

/**
 * Locks body scroll to prevent background scrolling when a modal is open.
 * Supports nested locks - multiple dialogs can call this, and scroll
 * will only be restored when all dialogs are closed.
 */
export function lockScroll() {
    scrollLockCount++;

    if (scrollLockCount === 1) {
        // Store current scroll position from the actual scrolling element
        scrollingElement = document.scrollingElement || document.documentElement;
        scrollPosition = scrollingElement?.scrollTop ?? window.pageYOffset ?? document.documentElement.scrollTop;

        // Store original styles
        originalStyles = {
            bodyOverflow: document.body.style.overflow,
            bodyPosition: document.body.style.position,
            bodyTop: document.body.style.top,
            bodyLeft: document.body.style.left,
            bodyRight: document.body.style.right,
            bodyWidth: document.body.style.width,
            bodyPaddingRight: document.body.style.paddingRight,
            bodyScrollbarGutter: document.body.style.scrollbarGutter,
            htmlOverflow: document.documentElement.style.overflow,
            htmlPaddingRight: document.documentElement.style.paddingRight,
            htmlScrollbarGutter: document.documentElement.style.scrollbarGutter
        };

        // Use fixed positioning to preserve visual scroll position while locking
        // This prevents sticky header/sidebar jumping when scroll is locked
        document.body.style.position = 'fixed';
        document.body.style.top = `-${scrollPosition}px`;
        document.body.style.left = '0';
        document.body.style.right = '0';
        document.body.style.width = '100%';

        // Reset scroll position to avoid double-offset when body is fixed
        const resetScroll = () => {
            const target = scrollingElement || document.documentElement;
            try {
                if (target) target.scrollTop = 0;
                document.body.scrollTop = 0;
                document.documentElement.scrollTop = 0;
                window.scrollTo(0, 0);
            } catch {
                // Ignore
            }
        };

        resetScroll();
        requestAnimationFrame(resetScroll);
        setTimeout(resetScroll, 0);

        // Prevent wheel/touch/scroll keys from scrolling background
        scrollLockHandlers = {
            wheel: (event) => preventBackgroundScroll(event),
            touchmove: (event) => preventBackgroundScroll(event),
            keydown: (event) => {
                if (isInDialogContent(event.target)) return;
                const keys = [
                    'ArrowUp',
                    'ArrowDown',
                    'PageUp',
                    'PageDown',
                    'Home',
                    'End',
                    'Space'
                ];
                if (keys.includes(event.code) || keys.includes(event.key)) {
                    event.preventDefault();
                }
            }
        };

        document.addEventListener('wheel', scrollLockHandlers.wheel, { passive: false });
        document.addEventListener('touchmove', scrollLockHandlers.touchmove, { passive: false });
        document.addEventListener('keydown', scrollLockHandlers.keydown, { passive: false });
    }
}

/**
 * Unlocks body scroll. Only actually unlocks when all nested locks are released.
 */
export function unlockScroll() {
    scrollLockCount--;

    if (scrollLockCount <= 0) {
        scrollLockCount = 0;

        if (originalStyles) {
            // Restore original styles
            document.body.style.overflow = originalStyles.bodyOverflow;
            document.body.style.position = originalStyles.bodyPosition;
            document.body.style.top = originalStyles.bodyTop;
            document.body.style.left = originalStyles.bodyLeft;
            document.body.style.right = originalStyles.bodyRight;
            document.body.style.width = originalStyles.bodyWidth;
            document.body.style.paddingRight = originalStyles.bodyPaddingRight;
            document.body.style.scrollbarGutter = originalStyles.bodyScrollbarGutter;
            document.documentElement.style.overflow = originalStyles.htmlOverflow;
            document.documentElement.style.paddingRight = originalStyles.htmlPaddingRight;
            document.documentElement.style.scrollbarGutter = originalStyles.htmlScrollbarGutter;

            // Restore scroll position
            const target = scrollingElement || document.documentElement;
            if (target) target.scrollTop = scrollPosition;
            document.body.scrollTop = scrollPosition;
            document.documentElement.scrollTop = scrollPosition;
            window.scrollTo(0, scrollPosition);

            if (scrollLockHandlers) {
                document.removeEventListener('wheel', scrollLockHandlers.wheel);
                document.removeEventListener('touchmove', scrollLockHandlers.touchmove);
                document.removeEventListener('keydown', scrollLockHandlers.keydown);
                scrollLockHandlers = null;
            }

            originalStyles = null;
        }
    }
}

/**
 * Force unlocks all scroll locks. Use for cleanup during navigation or errors.
 */
export function forceUnlockScroll() {
    scrollLockCount = 0;

    if (originalStyles) {
        document.body.style.overflow = originalStyles.bodyOverflow;
        document.body.style.position = originalStyles.bodyPosition;
        document.body.style.top = originalStyles.bodyTop;
        document.body.style.left = originalStyles.bodyLeft;
        document.body.style.right = originalStyles.bodyRight;
        document.body.style.width = originalStyles.bodyWidth;
        document.body.style.paddingRight = originalStyles.bodyPaddingRight;
        document.body.style.scrollbarGutter = originalStyles.bodyScrollbarGutter;
        document.documentElement.style.overflow = originalStyles.htmlOverflow;
        document.documentElement.style.paddingRight = originalStyles.htmlPaddingRight;
        document.documentElement.style.scrollbarGutter = originalStyles.htmlScrollbarGutter;

        const target = scrollingElement || document.documentElement;
        if (target) target.scrollTop = scrollPosition;
        document.body.scrollTop = scrollPosition;
        document.documentElement.scrollTop = scrollPosition;
        window.scrollTo(0, scrollPosition);

        if (scrollLockHandlers) {
            document.removeEventListener('wheel', scrollLockHandlers.wheel);
            document.removeEventListener('touchmove', scrollLockHandlers.touchmove);
            document.removeEventListener('keydown', scrollLockHandlers.keydown);
            scrollLockHandlers = null;
        }

        originalStyles = null;
    }
}

/**
 * Creates a portal element in document.body for rendering dialog content.
 * @param {string} id - Unique ID for the portal element
 * @returns {HTMLElement} The created portal element
 */
export function createPortal(id) {
    let portal = document.getElementById(id);
    if (!portal) {
        portal = document.createElement('div');
        portal.id = id;
        portal.setAttribute('data-summit-dialog-portal', '');
        document.body.appendChild(portal);
    }
    return portal;
}

/**
 * Destroys a portal element.
 * @param {string} id - ID of the portal element to destroy
 */
export function destroyPortal(id) {
    const portal = document.getElementById(id);
    if (portal) {
        portal.remove();
    }
}

/**
 * Gets the current scroll lock count (useful for debugging).
 * @returns {number} The number of active scroll locks
 */
export function getScrollLockCount() {
    return scrollLockCount;
}
