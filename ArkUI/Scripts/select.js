/**
 * ArkUI Select JavaScript Module
 * Handles keyboard navigation, typeahead search, positioning, and focus management
 * Uses Floating UI for positioning
 */

import {
    computePosition,
    autoUpdate,
    flip,
    shift,
    offset,
    size,
    limitShift
} from '@floating-ui/dom';

// Store cleanup functions and state per select instance
const selectInstances = new Map();

// Store trigger handlers separately (active before dropdown opens)
const triggerHandlers = new Map();

// Typeahead state
const typeaheadState = {
    buffer: '',
    timeout: null,
    DELAY: 500
};

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
 * Get all non-disabled item elements within the content
 * @param {HTMLElement} contentElement
 * @returns {HTMLElement[]}
 */
function getItems(contentElement) {
    return Array.from(
        contentElement.querySelectorAll('[data-ark-select-item]:not([data-disabled])')
    );
}

/**
 * Get all item elements (including disabled) within the content
 * @param {HTMLElement} contentElement
 * @returns {HTMLElement[]}
 */
function getAllItems(contentElement) {
    return Array.from(
        contentElement.querySelectorAll('[data-ark-select-item]')
    );
}

/**
 * Get the currently highlighted item
 * @param {HTMLElement} contentElement
 * @returns {HTMLElement|null}
 */
function getHighlightedItem(contentElement) {
    return contentElement.querySelector('[data-ark-select-item][data-highlighted]');
}

/**
 * Clear highlight from all items
 * @param {HTMLElement} contentElement
 */
function clearHighlight(contentElement) {
    const items = getAllItems(contentElement);
    items.forEach(item => item.removeAttribute('data-highlighted'));
}

/**
 * Highlight a specific item by element
 * @param {HTMLElement} contentElement
 * @param {HTMLElement} itemElement
 * @param {object} dotNetRef
 */
function highlightItemElement(contentElement, itemElement, dotNetRef) {
    clearHighlight(contentElement);
    itemElement.setAttribute('data-highlighted', '');
    
    // Scroll item into view
    itemElement.scrollIntoView({ block: 'nearest' });
    
    // Notify .NET of highlight change
    const value = itemElement.getAttribute('data-value');
    if (value && dotNetRef) {
        dotNetRef.invokeMethodAsync('HandleHighlightChange', value);
    }
}

/**
 * Highlight item by value
 * @param {HTMLElement} contentElement
 * @param {string} value
 */
export function highlightItem(contentElement, value) {
    if (!contentElement) return;
    
    const instance = selectInstances.get(contentElement);
    const item = contentElement.querySelector(`[data-ark-select-item][data-value="${CSS.escape(value)}"]`);
    
    if (item && !item.hasAttribute('data-disabled')) {
        highlightItemElement(contentElement, item, instance?.dotNetRef);
    }
}

/**
 * Highlight the first non-disabled item
 * @param {HTMLElement} contentElement
 */
export function highlightFirst(contentElement) {
    if (!contentElement) return;
    
    const instance = selectInstances.get(contentElement);
    const items = getItems(contentElement);
    
    if (items.length > 0) {
        highlightItemElement(contentElement, items[0], instance?.dotNetRef);
    }
}

/**
 * Highlight the last non-disabled item
 * @param {HTMLElement} contentElement
 */
export function highlightLast(contentElement) {
    if (!contentElement) return;
    
    const instance = selectInstances.get(contentElement);
    const items = getItems(contentElement);
    
    if (items.length > 0) {
        highlightItemElement(contentElement, items[items.length - 1], instance?.dotNetRef);
    }
}

/**
 * Highlight the next non-disabled item
 * @param {HTMLElement} contentElement
 * @param {object} dotNetRef
 */
function highlightNext(contentElement, dotNetRef) {
    const items = getItems(contentElement);
    if (items.length === 0) return;
    
    const current = getHighlightedItem(contentElement);
    let nextIndex = 0;
    
    if (current) {
        const currentIndex = items.indexOf(current);
        nextIndex = currentIndex + 1;
        if (nextIndex >= items.length) {
            nextIndex = 0; // Loop to start
        }
    }
    
    highlightItemElement(contentElement, items[nextIndex], dotNetRef);
}

/**
 * Highlight the previous non-disabled item
 * @param {HTMLElement} contentElement
 * @param {object} dotNetRef
 */
function highlightPrevious(contentElement, dotNetRef) {
    const items = getItems(contentElement);
    if (items.length === 0) return;
    
    const current = getHighlightedItem(contentElement);
    let prevIndex = items.length - 1;
    
    if (current) {
        const currentIndex = items.indexOf(current);
        prevIndex = currentIndex - 1;
        if (prevIndex < 0) {
            prevIndex = items.length - 1; // Loop to end
        }
    }
    
    highlightItemElement(contentElement, items[prevIndex], dotNetRef);
}

/**
 * Handle typeahead search
 * @param {string} character
 * @param {HTMLElement} contentElement
 * @param {object} dotNetRef
 */
function handleTypeahead(character, contentElement, dotNetRef) {
    // Clear existing timeout
    if (typeaheadState.timeout) {
        clearTimeout(typeaheadState.timeout);
    }
    
    // Add character to buffer
    typeaheadState.buffer += character.toLowerCase();
    
    // Find matching item
    const items = getItems(contentElement);
    const matchingItem = items.find(item => {
        const label = item.getAttribute('data-label')?.toLowerCase() || 
                      item.textContent?.trim().toLowerCase() || '';
        return label.startsWith(typeaheadState.buffer);
    });
    
    if (matchingItem) {
        highlightItemElement(contentElement, matchingItem, dotNetRef);
    }
    
    // Clear buffer after delay
    typeaheadState.timeout = setTimeout(() => {
        typeaheadState.buffer = '';
    }, typeaheadState.DELAY);
}

/**
 * Select the currently highlighted item
 * @param {HTMLElement} contentElement
 * @param {object} dotNetRef
 */
function selectHighlightedItem(contentElement, dotNetRef) {
    const highlighted = getHighlightedItem(contentElement);
    console.log('selectHighlightedItem called, highlighted:', highlighted);
    if (highlighted) {
        const value = highlighted.getAttribute('data-value');
        const label = highlighted.getAttribute('data-label') || highlighted.textContent?.trim();
        if (value && dotNetRef) {
            console.log('Calling HandleItemSelect with:', value, label);
            dotNetRef.invokeMethodAsync('HandleItemSelect', value, label)
                .then(() => console.log('HandleItemSelect completed'))
                .catch(err => console.error('HandleItemSelect failed:', err));
        }
    }
}

/**
 * Initialize select positioning and event listeners
 * @param {HTMLElement} triggerEl - Trigger button element
 * @param {HTMLElement} contentEl - Select content element
 * @param {object} dotNetRef - .NET object reference for callbacks
 * @param {object} options - Positioning and behavior options
 */
export function initializeSelect(triggerEl, contentEl, dotNetRef, options) {
    if (!triggerEl || !contentEl) return;
    
    const placement = getPlacement(options.side, options.align);
    
    // Build middleware array
    const middleware = [
        offset({
            mainAxis: options.sideOffset,
            crossAxis: options.alignOffset
        })
    ];
    
    if (options.avoidCollisions) {
        middleware.push(
            flip({ padding: options.collisionPadding }),
            shift({
                padding: options.collisionPadding,
                limiter: limitShift()
            })
        );
    }
    
    // Add size middleware to constrain content to viewport
    middleware.push(
        size({
            padding: options.collisionPadding,
            apply({ availableWidth, availableHeight, elements }) {
                Object.assign(elements.floating.style, {
                    maxWidth: `${availableWidth}px`,
                    maxHeight: `${availableHeight}px`
                });
            }
        })
    );
    
    // Position update function
    async function updatePositionInternal() {
        const { x, y, placement: finalPlacement } = await computePosition(
            triggerEl,
            contentEl,
            { placement, middleware }
        );
        
        Object.assign(contentEl.style, {
            left: `${x}px`,
            top: `${y}px`,
            visibility: 'visible' // Make visible after positioning to prevent flash in top-left corner
        });
        
        // Update data attributes for styling hooks
        const [side] = finalPlacement.split('-');
        contentEl.setAttribute('data-side', side);
    }
    
    // Start auto-update (handles scroll, resize, etc.)
    const cleanupAutoUpdate = autoUpdate(
        triggerEl,
        contentEl,
        updatePositionInternal,
        {
            ancestorScroll: true,
            ancestorResize: true,
            elementResize: true,
            layoutShift: true
        }
    );
    
    // Track if instance is disposed to prevent callbacks after cleanup
    let isDisposed = false;

    // Handle keyboard navigation on trigger (focus stays on trigger with aria-activedescendant pattern)
    function handleTriggerKeyDown(event) {
        if (isDisposed) return;
        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                highlightNext(contentEl, dotNetRef);
                break;
                
            case 'ArrowUp':
                event.preventDefault();
                highlightPrevious(contentEl, dotNetRef);
                break;
                
            case 'Home':
                event.preventDefault();
                highlightFirst(contentEl);
                break;
                
            case 'End':
                event.preventDefault();
                highlightLast(contentEl);
                break;
                
            case 'Enter':
            case ' ':
                event.preventDefault();
                event.stopPropagation(); // Prevent event bubbling
                event.stopImmediatePropagation(); // Prevent other handlers on same element
                selectHighlightedItem(contentEl, dotNetRef);
                break;
                
            case 'Escape':
                if (options.closeOnEscape) {
                    event.preventDefault();
                    dotNetRef.invokeMethodAsync('HandleEscapeKey').catch(() => {
                        // DotNetObjectReference may have been disposed, ignore errors
                    });
                }
                break;
                
            case 'Tab':
                // Close on tab and allow natural tab behavior
                dotNetRef.invokeMethodAsync('HandleClose').catch(() => {
                    // DotNetObjectReference may have been disposed, ignore errors
                });
                break;
                
            default:
                // Handle typeahead for printable characters
                if (event.key.length === 1 && !event.ctrlKey && !event.metaKey && !event.altKey) {
                    event.preventDefault();
                    handleTypeahead(event.key, contentEl, dotNetRef);
                }
                break;
        }
    }
    
    // Handle keyboard navigation on content (backup for when content somehow receives focus)
    function handleContentKeyDown(event) {
        // Delegate to trigger handler - same logic
        handleTriggerKeyDown(event);
    }
    
    // Handle outside clicks
    function handleOutsideClick(event) {
        if (isDisposed) return;
        if (!contentEl.contains(event.target) && !triggerEl.contains(event.target)) {
            if (options.closeOnOutsideClick) {
                dotNetRef.invokeMethodAsync('HandleOutsideClick').catch(() => {
                    // DotNetObjectReference may have been disposed, ignore errors
                });
            }
        }
    }
    
    // Handle item clicks
    function handleItemClick(event) {
        if (isDisposed) return;
        const item = event.target.closest('[data-ark-select-item]');
        if (item && !item.hasAttribute('data-disabled')) {
            const value = item.getAttribute('data-value');
            const label = item.getAttribute('data-label') || item.textContent?.trim();
            if (value) {
                dotNetRef.invokeMethodAsync('HandleItemSelect', value, label).catch(() => {
                    // DotNetObjectReference may have been disposed, ignore errors
                });
            }
        }
    }
    
    // Handle item hover
    function handleItemMouseEnter(event) {
        const item = event.target.closest('[data-ark-select-item]');
        if (item && !item.hasAttribute('data-disabled')) {
            highlightItemElement(contentEl, item, dotNetRef);
        }
    }
    
    // Add event listeners
    triggerEl.addEventListener('keydown', handleTriggerKeyDown);
    contentEl.addEventListener('keydown', handleContentKeyDown);
    contentEl.addEventListener('click', handleItemClick);
    contentEl.addEventListener('mouseover', handleItemMouseEnter);
    
    // Delay outside click listener to avoid catching the click that opened the select
    // Use requestAnimationFrame - safer than setTimeout as it fires at the next frame
    requestAnimationFrame(() => {
        if (!isDisposed) {
            document.addEventListener('pointerdown', handleOutsideClick, true);
        }
    });
    
    // Highlight selected item or first item
    const selectedValue = options.selectedValue;
    if (selectedValue) {
        highlightItem(contentEl, selectedValue);
    } else {
        highlightFirst(contentEl);
    }
    
    // Store cleanup function and state
    selectInstances.set(contentEl, {
        cleanup: () => {
            isDisposed = true;
            cleanupAutoUpdate();
            triggerEl.removeEventListener('keydown', handleTriggerKeyDown);
            contentEl.removeEventListener('keydown', handleContentKeyDown);
            contentEl.removeEventListener('click', handleItemClick);
            contentEl.removeEventListener('mouseover', handleItemMouseEnter);
            document.removeEventListener('pointerdown', handleOutsideClick, true);
            
            // Clear typeahead
            if (typeaheadState.timeout) {
                clearTimeout(typeaheadState.timeout);
                typeaheadState.buffer = '';
            }
        },
        updatePosition: updatePositionInternal,
        triggerEl,
        dotNetRef
    });
}

/**
 * Destroy select and cleanup
 * @param {HTMLElement} contentEl - Select content element
 */
export function destroySelect(contentEl) {
    const instance = selectInstances.get(contentEl);
    if (instance) {
        instance.cleanup();
        // Return focus to trigger
        instance.triggerEl?.focus();
        selectInstances.delete(contentEl);
    }
}

/**
 * Manually update select position
 * @param {HTMLElement} contentEl - Select content element
 */
export function updatePosition(contentEl) {
    const instance = selectInstances.get(contentEl);
    if (instance) {
        instance.updatePosition();
    }
}

/**
 * Focus the trigger element
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function focusTrigger(triggerEl) {
    triggerEl?.focus();
}

/**
 * Register trigger element to prevent default scroll behavior on arrow keys
 * Call this on first render of the trigger, before any dropdown interaction
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function registerTrigger(triggerEl) {
    if (!triggerEl || triggerHandlers.has(triggerEl)) return;
    
    function handleKeyDown(event) {
        // Prevent default scroll behavior for navigation keys
        if (['ArrowDown', 'ArrowUp', 'Home', 'End', ' ', 'Enter'].includes(event.key)) {
            event.preventDefault();
        }
    }
    
    triggerEl.addEventListener('keydown', handleKeyDown);
    triggerHandlers.set(triggerEl, handleKeyDown);
}

/**
 * Unregister trigger element keyboard handler
 * @param {HTMLElement} triggerEl - Trigger element
 */
export function unregisterTrigger(triggerEl) {
    if (!triggerEl) return;
    
    const handler = triggerHandlers.get(triggerEl);
    if (handler) {
        triggerEl.removeEventListener('keydown', handler);
        triggerHandlers.delete(triggerEl);
    }
}
