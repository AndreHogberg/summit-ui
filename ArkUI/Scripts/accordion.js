// accordion.js - ES6 module for accordion keyboard navigation with RTL support and animations

const accordionInstances = new Map();

/**
 * Check if the document direction is RTL.
 * @returns {boolean}
 */
function isRtl() {
    return document.dir === 'rtl' || document.documentElement.dir === 'rtl';
}

/**
 * Get all non-disabled trigger elements within an accordion.
 * @param {HTMLElement} rootElement
 * @returns {HTMLElement[]}
 */
function getTriggers(rootElement) {
    return Array.from(
        rootElement.querySelectorAll('[data-ark-accordion-trigger]:not([disabled])')
    );
}

/**
 * Initialize keyboard navigation for an accordion component.
 * @param {HTMLElement} rootElement
 * @param {object} dotNetRef
 * @param {object} options
 */
export function initializeAccordion(rootElement, dotNetRef, options) {
    if (!rootElement) return;

    const { orientation, loop } = options;
    const isVertical = orientation === 'vertical';

    function handleKeyDown(event) {
        // Only handle events from triggers
        if (!event.target.hasAttribute('data-ark-accordion-trigger')) return;

        const triggers = getTriggers(rootElement);
        if (triggers.length === 0) return;

        const currentIndex = triggers.findIndex(t => t === event.target);
        if (currentIndex === -1) return;

        // Determine navigation keys based on orientation and RTL
        const rtl = isRtl();
        let prevKey, nextKey;

        if (isVertical) {
            prevKey = 'ArrowUp';
            nextKey = 'ArrowDown';
        } else {
            // In RTL, arrow directions are reversed for horizontal navigation
            prevKey = rtl ? 'ArrowRight' : 'ArrowLeft';
            nextKey = rtl ? 'ArrowLeft' : 'ArrowRight';
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
        }
    }

    rootElement.addEventListener('keydown', handleKeyDown);

    accordionInstances.set(rootElement, {
        cleanup: () => {
            rootElement.removeEventListener('keydown', handleKeyDown);
        }
    });
}

/**
 * Cleanup keyboard event listeners.
 * @param {HTMLElement} rootElement
 */
export function destroyAccordion(rootElement) {
    const instance = accordionInstances.get(rootElement);
    if (instance) {
        instance.cleanup();
        accordionInstances.delete(rootElement);
    }
}

/**
 * Focus a specific trigger by its ID.
 * @param {HTMLElement} rootElement
 * @param {string} value
 */
export function focusTrigger(rootElement, value) {
    if (!rootElement) return;

    // Find trigger by looking at aria-controls which contains the value
    const triggers = getTriggers(rootElement);
    const trigger = triggers.find(t => {
        const controls = t.getAttribute('aria-controls');
        return controls && controls.includes(value);
    });

    if (trigger) {
        trigger.focus();
    }
}

/**
 * Set CSS variable for content height (for smooth animations).
 * @param {HTMLElement} contentElement
 */
export function setContentHeight(contentElement) {
    if (!contentElement) return;

    // Temporarily make visible to measure
    const wasHidden = contentElement.hidden;
    if (wasHidden) {
        contentElement.style.visibility = 'hidden';
        contentElement.style.position = 'absolute';
        contentElement.hidden = false;
    }

    const height = contentElement.scrollHeight;
    const width = contentElement.scrollWidth;

    contentElement.style.setProperty('--ark-accordion-content-height', `${height}px`);
    contentElement.style.setProperty('--ark-accordion-content-width', `${width}px`);

    // Restore hidden state
    if (wasHidden) {
        contentElement.hidden = true;
        contentElement.style.visibility = '';
        contentElement.style.position = '';
    }
}
