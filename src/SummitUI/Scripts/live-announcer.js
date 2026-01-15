/**
 * LiveAnnouncer - Screen reader announcement utility
 * 
 * Inspired by React Aria's LiveAnnouncer pattern.
 * https://github.com/adobe/react-spectrum/blob/main/packages/@react-aria/live-announcer
 * Copyright 2020 Adobe. Licensed under Apache 2.0.
 * 
 * Uses vanilla DOM manipulation to ensure reliable screen reader announcements.
 * 
 * Key design decisions (from React Aria):
 * - Uses role="log" with aria-relevant="additions" for reliable announcements
 * - Appends new child nodes rather than replacing content
 * - 7000ms timeout before clearing announcements
 * - 100ms delay on first announcement (Safari compatibility)
 */

const LIVEREGION_TIMEOUT_DELAY = 7000;

// Visually hidden styles (copied from React Aria)
const visuallyHiddenStyles = {
    border: '0',
    clip: 'rect(0 0 0 0)',
    clipPath: 'inset(50%)',
    height: '1px',
    margin: '-1px',
    overflow: 'hidden',
    padding: '0',
    position: 'absolute',
    width: '1px',
    whiteSpace: 'nowrap'
};

let liveAnnouncerNode = null;
let assertiveLog = null;
let politeLog = null;
let isInitialized = false;
let pendingAnnouncements = [];

/**
 * Creates a log region for announcements.
 * @param {string} ariaLive - The aria-live value ('polite' or 'assertive')
 * @returns {HTMLElement} The log element
 */
function createLog(ariaLive) {
    const node = document.createElement('div');
    node.setAttribute('role', 'log');
    node.setAttribute('aria-live', ariaLive);
    node.setAttribute('aria-relevant', 'additions');
    return node;
}

/**
 * Initializes the live announcer by creating the DOM elements.
 * Must be called before any announcements can be made.
 */
export function initialize() {
    if (typeof document === 'undefined' || liveAnnouncerNode) {
        return;
    }

    liveAnnouncerNode = document.createElement('div');
    liveAnnouncerNode.dataset.summitLiveAnnouncer = 'true';
    Object.assign(liveAnnouncerNode.style, visuallyHiddenStyles);

    assertiveLog = createLog('assertive');
    liveAnnouncerNode.appendChild(assertiveLog);

    politeLog = createLog('polite');
    liveAnnouncerNode.appendChild(politeLog);

    document.body.prepend(liveAnnouncerNode);

    // Wait 100ms before processing announcements (Safari compatibility)
    // The live regions need time to be recognized by screen readers
    setTimeout(() => {
        isInitialized = true;
        // Process any pending announcements
        for (const pending of pendingAnnouncements) {
            announceInternal(pending.message, pending.assertiveness, pending.timeout);
        }
        pendingAnnouncements = [];
    }, 100);
}

/**
 * Destroys the live announcer and removes it from the DOM.
 */
export function destroy() {
    if (!liveAnnouncerNode) {
        return;
    }

    document.body.removeChild(liveAnnouncerNode);
    liveAnnouncerNode = null;
    assertiveLog = null;
    politeLog = null;
    isInitialized = false;
    pendingAnnouncements = [];
}

/**
 * Internal function to perform the actual announcement.
 */
function announceInternal(message, assertiveness, timeout) {
    if (!liveAnnouncerNode) {
        return;
    }

    const node = document.createElement('div');
    node.textContent = message;

    if (assertiveness === 'assertive') {
        assertiveLog?.appendChild(node);
    } else {
        politeLog?.appendChild(node);
    }

    // Remove the message after the timeout
    if (message !== '') {
        setTimeout(() => {
            node.remove();
        }, timeout);
    }
}

/**
 * Announces a message to screen readers.
 * @param {string} message - The message to announce
 * @param {string} assertiveness - 'polite' or 'assertive' (default: 'polite')
 * @param {number} timeout - Timeout before clearing (default: 7000ms)
 */
export function announce(message, assertiveness = 'polite', timeout = LIVEREGION_TIMEOUT_DELAY) {
    if (!message) {
        return;
    }

    // Initialize if not already done
    if (!liveAnnouncerNode) {
        initialize();
    }

    // If not yet initialized (waiting for 100ms delay), queue the announcement
    if (!isInitialized) {
        pendingAnnouncements.push({ message, assertiveness, timeout });
        return;
    }

    announceInternal(message, assertiveness, timeout);
}

/**
 * Clears all announcements for a given priority.
 * @param {string} assertiveness - 'polite' or 'assertive'
 */
export function clear(assertiveness) {
    if (!liveAnnouncerNode) {
        return;
    }

    if ((!assertiveness || assertiveness === 'assertive') && assertiveLog) {
        assertiveLog.innerHTML = '';
    }

    if ((!assertiveness || assertiveness === 'polite') && politeLog) {
        politeLog.innerHTML = '';
    }
}

/**
 * Clears all announcements.
 */
export function clearAll() {
    clear('assertive');
    clear('polite');
}
