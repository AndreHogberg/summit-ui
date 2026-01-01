// tabs.js - ES6 module for tabs keyboard navigation with RTL support

const tabsInstances = new Map();

/**
 * Check if the document direction is RTL.
 * @returns {boolean}
 */
function isRtl() {
    return document.dir === 'rtl' || document.documentElement.dir === 'rtl';
}

/**
 * Get all non-disabled trigger elements within a tabs list.
 * @param {HTMLElement} listElement
 * @returns {HTMLElement[]}
 */
function getTriggers(listElement) {
    return Array.from(
        listElement.querySelectorAll('[data-ark-tabs-trigger]:not([data-disabled])')
    );
}

/**
 * Initialize keyboard navigation for a tabs component.
 * @param {HTMLElement} listElement
 * @param {object} dotNetRef
 * @param {object} options
 */
export function initializeTabs(listElement, dotNetRef, options) {
    if (!listElement) return;

    const { orientation, loop, activationMode } = options;
    const isHorizontal = orientation === 'horizontal';

    function handleKeyDown(event) {
        const triggers = getTriggers(listElement);
        if (triggers.length === 0) return;

        const currentIndex = triggers.findIndex(t => t === document.activeElement);
        if (currentIndex === -1) return;

        // Determine navigation keys based on orientation and RTL
        const rtl = isRtl();
        let prevKey, nextKey;

        if (isHorizontal) {
            // In RTL, arrow directions are reversed for horizontal navigation
            prevKey = rtl ? 'ArrowRight' : 'ArrowLeft';
            nextKey = rtl ? 'ArrowLeft' : 'ArrowRight';
        } else {
            prevKey = 'ArrowUp';
            nextKey = 'ArrowDown';
        }

        let newIndex = currentIndex;

        switch (event.key) {
            case prevKey:
                event.preventDefault();
                newIndex = currentIndex - 1;
                if (newIndex < 0) {
                    newIndex = loop ? triggers.length - 1 : 0;
                }
                break;

            case nextKey:
                event.preventDefault();
                newIndex = currentIndex + 1;
                if (newIndex >= triggers.length) {
                    newIndex = loop ? 0 : triggers.length - 1;
                }
                break;

            case 'Home':
                event.preventDefault();
                newIndex = 0;
                break;

            case 'End':
                event.preventDefault();
                newIndex = triggers.length - 1;
                break;

            default:
                return;
        }

        if (newIndex !== currentIndex) {
            const newTrigger = triggers[newIndex];
            newTrigger.focus();

            // In auto mode, notify .NET to activate the tab
            if (activationMode === 'auto') {
                const value = newTrigger.getAttribute('data-value');
                if (value) {
                    dotNetRef.invokeMethodAsync('HandleTabActivation', value);
                }
            }
        }
    }

    listElement.addEventListener('keydown', handleKeyDown);

    tabsInstances.set(listElement, {
        cleanup: () => {
            listElement.removeEventListener('keydown', handleKeyDown);
        }
    });
}

/**
 * Cleanup keyboard event listeners.
 * @param {HTMLElement} listElement
 */
export function destroyTabs(listElement) {
    const instance = tabsInstances.get(listElement);
    if (instance) {
        instance.cleanup();
        tabsInstances.delete(listElement);
    }
}

/**
 * Focus a specific trigger by value.
 * @param {HTMLElement} listElement
 * @param {string} value
 */
export function focusTrigger(listElement, value) {
    if (!listElement) return;

    const trigger = listElement.querySelector(
        `[data-ark-tabs-trigger][data-value="${value}"]`
    );
    if (trigger) {
        trigger.focus();
    }
}
