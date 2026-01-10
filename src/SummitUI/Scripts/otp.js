/**
 * OTP Input JavaScript Module
 * Handles selection tracking, focus management, and password manager detection
 * for the single-input OTP architecture.
 */

// Global styles injection flag
let stylesInjected = false;

// Password manager badge detection
const PWM_BADGE_SPACE_WIDTH = '40px';

/**
 * Injects global CSS styles for the OTP input.
 * Only runs once per page load.
 */
function injectStyles() {
    if (stylesInjected) return;
    stylesInjected = true;

    const styleEl = document.createElement('style');
    styleEl.id = 'summitui-otp-style';
    document.head.appendChild(styleEl);

    if (styleEl.sheet) {
        const autofillStyles = `
            background: transparent !important;
            color: transparent !important;
            border-color: transparent !important;
            opacity: 0 !important;
            box-shadow: none !important;
            -webkit-box-shadow: none !important;
            -webkit-text-fill-color: transparent !important;
        `;

        safeInsertRule(styleEl.sheet, '[data-otp-input]::selection { background: transparent !important; color: transparent !important; }');
        safeInsertRule(styleEl.sheet, `[data-otp-input]:autofill { ${autofillStyles} }`);
        safeInsertRule(styleEl.sheet, `[data-otp-input]:-webkit-autofill { ${autofillStyles} }`);
        // iOS optimizations
        safeInsertRule(styleEl.sheet, `@supports (-webkit-touch-callout: none) { [data-otp-input] { letter-spacing: -.6em !important; font-weight: 100 !important; font-stretch: ultra-condensed; font-optical-sizing: none !important; left: -1px !important; right: 1px !important; } }`);
        // PWM badges should be interactive
        safeInsertRule(styleEl.sheet, `[data-otp-input] + * { pointer-events: all !important; }`);
    }
}

/**
 * Safely inserts a CSS rule into a stylesheet.
 */
function safeInsertRule(sheet, rule) {
    try {
        sheet.insertRule(rule);
    } catch (e) {
        console.warn('SummitUI OTP: Could not insert CSS rule:', rule);
    }
}

/**
 * Detects if a password manager badge is present in the page.
 */
function detectPasswordManager() {
    // Common password manager selectors
    const pwmSelectors = [
        // 1Password
        '[data-onepassword-extension]',
        'com-1password-button',
        // LastPass
        '[data-lastpass-icon-root]',
        '[data-lastpass-root]',
        // Dashlane
        '[data-dashlane-rid]',
        // Bitwarden
        '[data-bwi-selector]',
        // Generic autofill indicators
        'input:-internal-autofill-selected'
    ];

    for (const selector of pwmSelectors) {
        try {
            if (document.querySelector(selector)) {
                return true;
            }
        } catch {
            // Invalid selector, skip
        }
    }
    return false;
}

/**
 * Initializes the OTP input with event listeners.
 * @param {HTMLInputElement} element - The hidden input element.
 * @param {HTMLElement} container - The container element.
 * @param {DotNetObjectReference} dotNetRef - The .NET object reference for callbacks.
 * @param {number} maxLength - The maximum length of the OTP.
 */
export function initialize(element, container, dotNetRef, maxLength) {
    if (!element || !container) return;

    // Inject global styles
    injectStyles();

    // Store metadata
    const metadata = {
        prev: [element.selectionStart, element.selectionEnd, element.selectionDirection],
        maxLength
    };

    // Track selection changes
    const onSelectionChange = () => {
        if (document.activeElement !== element) {
            dotNetRef.invokeMethodAsync('OnSelectionChanged', null, null);
            return;
        }

        const _s = element.selectionStart;
        const _e = element.selectionEnd;
        const _dir = element.selectionDirection;
        const _ml = maxLength;
        const _val = element.value;
        const _prev = metadata.prev;

        let start = -1;
        let end = -1;
        let direction = undefined;

        if (_val.length !== 0 && _s !== null && _e !== null) {
            const isSingleCaret = _s === _e;
            const isInsertMode = _s === _val.length && _val.length < _ml;

            if (isSingleCaret && !isInsertMode) {
                const c = _s;
                if (c === 0) {
                    start = 0;
                    end = 1;
                    direction = 'forward';
                } else if (c === _ml) {
                    start = c - 1;
                    end = c;
                    direction = 'backward';
                } else if (_ml > 1 && _val.length > 1) {
                    let offset = 0;
                    if (_prev[0] !== null && _prev[1] !== null) {
                        direction = c < _prev[1] ? 'backward' : 'forward';
                        const wasPreviouslyInserting = _prev[0] === _prev[1] && _prev[0] < _ml;
                        if (direction === 'backward' && !wasPreviouslyInserting) {
                            offset = -1;
                        }
                    }
                    start = offset + c;
                    end = offset + c + 1;
                }
            }

            if (start !== -1 && end !== -1 && start !== end) {
                element.setSelectionRange(start, end, direction);
            }
        }

        // Update state
        const s = start !== -1 ? start : _s;
        const e = end !== -1 ? end : _e;
        const dir = direction ?? _dir;
        metadata.prev = [s, e, dir];

        dotNetRef.invokeMethodAsync('OnSelectionChanged', s, e);
    };

    document.addEventListener('selectionchange', onSelectionChange, { capture: true });

    // Focus handler
    const onFocus = () => {
        const start = Math.min(element.value.length, maxLength - 1);
        const end = element.value.length;
        element.setSelectionRange(start, end);
        dotNetRef.invokeMethodAsync('OnFocusChanged', true);
        dotNetRef.invokeMethodAsync('OnSelectionChanged', start, end);
    };

    // Blur handler
    const onBlur = () => {
        dotNetRef.invokeMethodAsync('OnFocusChanged', false);
    };

    // Hover handlers
    const onMouseEnter = () => {
        dotNetRef.invokeMethodAsync('OnHoverChanged', true);
    };

    const onMouseLeave = () => {
        dotNetRef.invokeMethodAsync('OnHoverChanged', false);
    };

    // Initial selection if focused
    if (document.activeElement === element) {
        onFocus();
    }

    element.addEventListener('focus', onFocus);
    element.addEventListener('blur', onBlur);
    container.addEventListener('mouseenter', onMouseEnter);
    container.addEventListener('mouseleave', onMouseLeave);

    // Track root height for font sizing
    const updateRootHeight = () => {
        container.style.setProperty('--otp-root-height', `${element.clientHeight}px`);
    };
    updateRootHeight();

    let resizeObserver = null;
    if (typeof ResizeObserver !== 'undefined') {
        resizeObserver = new ResizeObserver(updateRootHeight);
        resizeObserver.observe(element);
    }

    // Password manager detection
    let hasPwm = detectPasswordManager();
    if (hasPwm) {
        element.style.width = `calc(100% + ${PWM_BADGE_SPACE_WIDTH})`;
        element.style.clipPath = `inset(0 ${PWM_BADGE_SPACE_WIDTH} 0 0)`;
    }

    // Store cleanup function
    element._otpCleanup = () => {
        document.removeEventListener('selectionchange', onSelectionChange, { capture: true });
        element.removeEventListener('focus', onFocus);
        element.removeEventListener('blur', onBlur);
        container.removeEventListener('mouseenter', onMouseEnter);
        container.removeEventListener('mouseleave', onMouseLeave);
        if (resizeObserver) {
            resizeObserver.disconnect();
        }
    };

    return { hasPwm };
}

/**
 * Destroys the OTP input event listeners.
 * @param {HTMLInputElement} element - The hidden input element.
 */
export function destroy(element) {
    if (element && element._otpCleanup) {
        element._otpCleanup();
        delete element._otpCleanup;
    }
}

/**
 * Focuses the OTP input and optionally sets selection.
 * @param {HTMLInputElement} element - The hidden input element.
 * @param {number} maxLength - The maximum length.
 */
export function focus(element, maxLength) {
    if (element) {
        element.focus();
        const start = Math.min(element.value.length, maxLength - 1);
        const end = element.value.length;
        element.setSelectionRange(start, end);
    }
}

/**
 * Sets the selection range on the input.
 * @param {HTMLInputElement} element - The hidden input element.
 * @param {number} start - Selection start.
 * @param {number} end - Selection end.
 */
export function setSelection(element, start, end) {
    if (element) {
        element.setSelectionRange(start, end);
    }
}

// Legacy exports for backward compatibility (will be removed)
export function focusElement(element) {
    if (element) element.focus();
}

export function selectContent(element) {
    if (element) element.select();
}

export function registerInput(element, dotNetRef) {
    // Legacy - no longer used
    console.warn('SummitUI: otp_registerInput is deprecated. Use otp_initialize instead.');
}
