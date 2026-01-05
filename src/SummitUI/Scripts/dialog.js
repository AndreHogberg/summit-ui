/**
 * Dialog utilities for SummitUI
 * Handles scroll locking for modal dialogs with proper iOS Safari support
 */

// Track active scroll locks (supports nested dialogs)
let scrollLockCount = 0;
let originalStyles = null;
let scrollPosition = 0;

/**
 * Locks body scroll to prevent background scrolling when a modal is open.
 * Supports nested locks - multiple dialogs can call this, and scroll
 * will only be restored when all dialogs are closed.
 */
export function lockScroll() {
    scrollLockCount++;

    if (scrollLockCount === 1) {
        // Store current scroll position
        scrollPosition = window.pageYOffset || document.documentElement.scrollTop;

        // Store original body styles
        originalStyles = {
            overflow: document.body.style.overflow,
            position: document.body.style.position,
            top: document.body.style.top,
            left: document.body.style.left,
            right: document.body.style.right,
            width: document.body.style.width,
            paddingRight: document.body.style.paddingRight
        };

        // Calculate scrollbar width to prevent layout shift
        const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;

        // Check for iOS Safari which needs special handling
        const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;

        if (isIOS) {
            // iOS requires position: fixed to truly lock scroll
            // This prevents the rubber-band scrolling effect
            document.body.style.position = 'fixed';
            document.body.style.top = `-${scrollPosition}px`;
            document.body.style.left = '0';
            document.body.style.right = '0';
            document.body.style.width = '100%';
        }

        document.body.style.overflow = 'hidden';

        // Compensate for scrollbar removal to prevent layout shift
        if (scrollbarWidth > 0) {
            document.body.style.paddingRight = `${scrollbarWidth}px`;
        }
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
            const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;

            // Restore original styles
            document.body.style.overflow = originalStyles.overflow;
            document.body.style.position = originalStyles.position;
            document.body.style.top = originalStyles.top;
            document.body.style.left = originalStyles.left;
            document.body.style.right = originalStyles.right;
            document.body.style.width = originalStyles.width;
            document.body.style.paddingRight = originalStyles.paddingRight;

            // Restore scroll position for iOS
            if (isIOS) {
                window.scrollTo(0, scrollPosition);
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
        const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;

        document.body.style.overflow = originalStyles.overflow;
        document.body.style.position = originalStyles.position;
        document.body.style.top = originalStyles.top;
        document.body.style.left = originalStyles.left;
        document.body.style.right = originalStyles.right;
        document.body.style.width = originalStyles.width;
        document.body.style.paddingRight = originalStyles.paddingRight;

        if (isIOS) {
            window.scrollTo(0, scrollPosition);
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
