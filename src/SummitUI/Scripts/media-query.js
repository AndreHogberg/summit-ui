/**
 * MediaQuery JavaScript Module
 * Provides reactive media query matching using the matchMedia API.
 */

/** @type {Map<string, { mql: MediaQueryList, handler: (e: MediaQueryListEvent) => void }>} */
const activeQueries = new Map();

/**
 * Register a media query listener that calls back to .NET when the match state changes.
 * @param {string} listenerId - Unique identifier for this listener
 * @param {string} query - CSS media query string
 * @param {object} dotNetRef - DotNet object reference for callbacks
 * @returns {boolean} - Initial match state
 */
export function register(listenerId, query, dotNetRef) {
    // Clean up any existing listener with the same ID
    if (activeQueries.has(listenerId)) {
        unregister(listenerId);
    }

    const mql = window.matchMedia(query);

    const handler = (e) => {
        dotNetRef.invokeMethodAsync('OnMediaQueryChanged', e.matches);
    };

    // Use addEventListener if available (modern browsers), otherwise addListener (legacy)
    if (mql.addEventListener) {
        mql.addEventListener('change', handler);
    } else {
        mql.addListener(handler);
    }

    activeQueries.set(listenerId, { mql, handler });

    return mql.matches;
}

/**
 * Unregister a media query listener.
 * @param {string} listenerId - The listener ID to unregister
 */
export function unregister(listenerId) {
    const entry = activeQueries.get(listenerId);
    if (!entry) return;

    const { mql, handler } = entry;

    // Use removeEventListener if available (modern browsers), otherwise removeListener (legacy)
    if (mql.removeEventListener) {
        mql.removeEventListener('change', handler);
    } else {
        mql.removeListener(handler);
    }

    activeQueries.delete(listenerId);
}

/**
 * Get the current match state for a media query without registering a listener.
 * @param {string} query - CSS media query string
 * @returns {boolean} - Current match state
 */
export function evaluate(query) {
    return window.matchMedia(query).matches;
}
