/**
 * SummitUI Scroll Area JavaScript Module
 * Handles scroll tracking, thumb sizing/positioning, and visibility management
 * for custom scrollbars while preserving native scroll behavior.
 */

// Instance storage
const instances = new Map();

/**
 * Generates a unique instance ID
 */
function generateId() {
    return `scroll-area-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}

/**
 * Initialize a scroll area instance
 * @param {HTMLElement} viewportEl - The scrollable viewport element
 * @param {object} options - Configuration options
 * @param {string} options.type - 'hover' | 'scroll' | 'auto' | 'always'
 * @param {number} options.scrollHideDelay - Delay before hiding scrollbars (ms)
 * @param {string} options.dir - 'ltr' | 'rtl'
 * @param {object} dotNetRef - .NET object reference for callbacks
 * @returns {string} Instance ID
 */
export function initialize(viewportEl, options, dotNetRef) {
    if (!viewportEl) return null;

    const instanceId = generateId();
    const state = {
        viewportEl,
        options,
        dotNetRef,
        scrollbars: new Map(),
        resizeObserver: null,
        hideTimeouts: { vertical: null, horizontal: null },
        isHovering: false,
        isScrolling: false,
        pointerCapture: null
    };

    // Set up resize observer to track content/viewport size changes
    state.resizeObserver = new ResizeObserver(() => {
        updateOverflowState(instanceId);
        updateAllThumbs(instanceId);
    });
    state.resizeObserver.observe(viewportEl);

    // Also observe the first child (content) if it exists
    if (viewportEl.firstElementChild) {
        state.resizeObserver.observe(viewportEl.firstElementChild);
    }

    // Set up scroll listener
    viewportEl.addEventListener('scroll', () => handleScroll(instanceId), { passive: true });

    // Set up hover listeners for 'hover' type
    if (options.type === 'hover') {
        const rootEl = viewportEl.closest('[data-summit-scroll-area-root]');
        if (rootEl) {
            rootEl.addEventListener('pointerenter', () => handlePointerEnter(instanceId));
            rootEl.addEventListener('pointerleave', () => handlePointerLeave(instanceId));
            state.rootEl = rootEl;
        }
    }

    instances.set(instanceId, state);

    // Initial overflow check
    updateOverflowState(instanceId);

    return instanceId;
}

/**
 * Register a scrollbar with the scroll area
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 * @param {HTMLElement} scrollbarEl - The scrollbar track element
 * @param {HTMLElement} thumbEl - The thumb element
 */
export function registerScrollbar(instanceId, orientation, scrollbarEl, thumbEl) {
    const state = instances.get(instanceId);
    if (!state || !scrollbarEl || !thumbEl) return;

    state.scrollbars.set(orientation, {
        scrollbarEl,
        thumbEl,
        isDragging: false,
        dragStartPos: 0,
        dragStartScroll: 0
    });

    // Set up track click handler
    scrollbarEl.addEventListener('pointerdown', (e) => handleTrackClick(instanceId, orientation, e));

    // Set up thumb drag handlers
    thumbEl.addEventListener('pointerdown', (e) => handleThumbPointerDown(instanceId, orientation, e));

    // Update thumb size initially
    updateThumb(instanceId, orientation);

    // Update visibility based on type
    updateScrollbarVisibility(instanceId, orientation);
}

/**
 * Unregister a scrollbar
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 */
export function unregisterScrollbar(instanceId, orientation) {
    const state = instances.get(instanceId);
    if (!state) return;

    state.scrollbars.delete(orientation);
}

/**
 * Update the thumb size and position for a scrollbar
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 */
export function updateThumb(instanceId, orientation) {
    const state = instances.get(instanceId);
    if (!state) return;

    const scrollbar = state.scrollbars.get(orientation);
    if (!scrollbar) return;

    const { viewportEl } = state;
    const { thumbEl, scrollbarEl } = scrollbar;

    const isVertical = orientation === 'vertical';
    const viewportSize = isVertical ? viewportEl.clientHeight : viewportEl.clientWidth;
    const contentSize = isVertical ? viewportEl.scrollHeight : viewportEl.scrollWidth;
    const scrollPos = isVertical ? viewportEl.scrollTop : viewportEl.scrollLeft;
    const trackSize = isVertical ? scrollbarEl.clientHeight : scrollbarEl.clientWidth;

    // Calculate thumb size as ratio of viewport to content
    const thumbRatio = Math.min(viewportSize / contentSize, 1);
    const thumbSize = Math.max(thumbRatio * trackSize, 20); // Minimum 20px

    // Calculate thumb position
    const maxScroll = contentSize - viewportSize;
    const scrollRatio = maxScroll > 0 ? scrollPos / maxScroll : 0;
    const maxThumbPos = trackSize - thumbSize;
    const thumbPos = scrollRatio * maxThumbPos;

    // Apply styles
    if (isVertical) {
        thumbEl.style.height = `${thumbSize}px`;
        thumbEl.style.transform = `translateY(${thumbPos}px)`;
    } else {
        thumbEl.style.width = `${thumbSize}px`;
        thumbEl.style.transform = `translateX(${thumbPos}px)`;
    }
}

/**
 * Update overflow state and notify .NET
 * @param {string} instanceId - The scroll area instance ID
 */
function updateOverflowState(instanceId) {
    const state = instances.get(instanceId);
    if (!state) return;

    const { viewportEl, dotNetRef } = state;

    const hasVerticalOverflow = viewportEl.scrollHeight > viewportEl.clientHeight;
    const hasHorizontalOverflow = viewportEl.scrollWidth > viewportEl.clientWidth;

    // Only notify .NET if overflow state actually changed
    if (state.lastVerticalOverflow !== hasVerticalOverflow || 
        state.lastHorizontalOverflow !== hasHorizontalOverflow) {
        state.lastVerticalOverflow = hasVerticalOverflow;
        state.lastHorizontalOverflow = hasHorizontalOverflow;
        
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnOverflowChanged', hasVerticalOverflow, hasHorizontalOverflow);
        }
    }

    // Update visibility for all registered scrollbars
    for (const orientation of state.scrollbars.keys()) {
        updateScrollbarVisibility(instanceId, orientation);
    }
}

/**
 * Update visibility for a specific scrollbar
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 */
function updateScrollbarVisibility(instanceId, orientation) {
    const state = instances.get(instanceId);
    if (!state) return;

    const scrollbar = state.scrollbars.get(orientation);
    if (!scrollbar) return;

    const { viewportEl, options, dotNetRef, isHovering, isScrolling } = state;
    const { scrollbarEl } = scrollbar;

    const isVertical = orientation === 'vertical';
    const hasOverflow = isVertical
        ? viewportEl.scrollHeight > viewportEl.clientHeight
        : viewportEl.scrollWidth > viewportEl.clientWidth;

    let shouldShow = false;

    switch (options.type) {
        case 'always':
            shouldShow = true;
            break;
        case 'auto':
            shouldShow = hasOverflow;
            break;
        case 'hover':
            shouldShow = hasOverflow && isHovering;
            break;
        case 'scroll':
            shouldShow = hasOverflow && isScrolling;
            break;
    }

    const newState = shouldShow ? 'visible' : 'hidden';
    const lastStateKey = `lastState_${orientation}`;
    
    // Only update DOM and notify .NET if state actually changed
    if (scrollbar[lastStateKey] !== newState) {
        scrollbar[lastStateKey] = newState;
        scrollbarEl.setAttribute('data-state', newState);

        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnScrollbarStateChanged', orientation, newState);
        }
    }
}

/**
 * Handle scroll events
 * @param {string} instanceId - The scroll area instance ID
 */
function handleScroll(instanceId) {
    const state = instances.get(instanceId);
    if (!state) return;

    // Update thumb positions
    updateAllThumbs(instanceId);

    // Handle 'scroll' type visibility
    if (state.options.type === 'scroll') {
        state.isScrolling = true;

        // Show scrollbars
        for (const orientation of state.scrollbars.keys()) {
            updateScrollbarVisibility(instanceId, orientation);
        }

        // Clear existing timeouts
        clearTimeout(state.hideTimeouts.vertical);
        clearTimeout(state.hideTimeouts.horizontal);

        // Set hide timeout
        const timeout = setTimeout(() => {
            state.isScrolling = false;
            for (const orientation of state.scrollbars.keys()) {
                updateScrollbarVisibility(instanceId, orientation);
            }
        }, state.options.scrollHideDelay);

        state.hideTimeouts.vertical = timeout;
        state.hideTimeouts.horizontal = timeout;
    }
}

/**
 * Handle pointer entering the scroll area (for 'hover' type)
 * @param {string} instanceId - The scroll area instance ID
 */
function handlePointerEnter(instanceId) {
    const state = instances.get(instanceId);
    if (!state || state.options.type !== 'hover') return;

    state.isHovering = true;

    // Clear any hide timeouts
    clearTimeout(state.hideTimeouts.vertical);
    clearTimeout(state.hideTimeouts.horizontal);

    // Show scrollbars
    for (const orientation of state.scrollbars.keys()) {
        updateScrollbarVisibility(instanceId, orientation);
    }
}

/**
 * Handle pointer leaving the scroll area (for 'hover' type)
 * @param {string} instanceId - The scroll area instance ID
 */
function handlePointerLeave(instanceId) {
    const state = instances.get(instanceId);
    if (!state || state.options.type !== 'hover') return;

    // Don't hide if dragging
    for (const scrollbar of state.scrollbars.values()) {
        if (scrollbar.isDragging) return;
    }

    // Set hide timeout
    const timeout = setTimeout(() => {
        state.isHovering = false;
        for (const orientation of state.scrollbars.keys()) {
            updateScrollbarVisibility(instanceId, orientation);
        }
    }, state.options.scrollHideDelay);

    state.hideTimeouts.vertical = timeout;
    state.hideTimeouts.horizontal = timeout;
}

/**
 * Handle clicking on the scrollbar track (jump to position)
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 * @param {PointerEvent} e - The pointer event
 */
function handleTrackClick(instanceId, orientation, e) {
    const state = instances.get(instanceId);
    if (!state) return;

    const scrollbar = state.scrollbars.get(orientation);
    if (!scrollbar) return;

    // Ignore if clicking on thumb
    if (e.target === scrollbar.thumbEl || scrollbar.thumbEl.contains(e.target)) {
        return;
    }

    e.preventDefault();

    const { viewportEl } = state;
    const { scrollbarEl, thumbEl } = scrollbar;
    const isVertical = orientation === 'vertical';

    // Get click position relative to track
    const rect = scrollbarEl.getBoundingClientRect();
    const clickPos = isVertical ? e.clientY - rect.top : e.clientX - rect.left;
    const trackSize = isVertical ? rect.height : rect.width;
    const thumbSize = isVertical ? thumbEl.offsetHeight : thumbEl.offsetWidth;

    // Calculate target scroll position
    // We want the thumb center to be at the click position
    const thumbCenter = thumbSize / 2;
    const effectiveTrackSize = trackSize - thumbSize;
    const targetRatio = Math.max(0, Math.min(1, (clickPos - thumbCenter) / effectiveTrackSize));

    const contentSize = isVertical ? viewportEl.scrollHeight : viewportEl.scrollWidth;
    const viewportSize = isVertical ? viewportEl.clientHeight : viewportEl.clientWidth;
    const maxScroll = contentSize - viewportSize;
    const targetScroll = targetRatio * maxScroll;

    // Scroll to position
    if (isVertical) {
        viewportEl.scrollTop = targetScroll;
    } else {
        viewportEl.scrollLeft = targetScroll;
    }
}

/**
 * Handle pointer down on thumb (start dragging)
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 * @param {PointerEvent} e - The pointer event
 */
function handleThumbPointerDown(instanceId, orientation, e) {
    const state = instances.get(instanceId);
    if (!state) return;

    const scrollbar = state.scrollbars.get(orientation);
    if (!scrollbar) return;

    e.preventDefault();
    e.stopPropagation();

    const { viewportEl } = state;
    const { thumbEl } = scrollbar;
    const isVertical = orientation === 'vertical';

    // Capture pointer
    thumbEl.setPointerCapture(e.pointerId);
    state.pointerCapture = { element: thumbEl, pointerId: e.pointerId };

    // Store drag start state
    scrollbar.isDragging = true;
    scrollbar.dragStartPos = isVertical ? e.clientY : e.clientX;
    scrollbar.dragStartScroll = isVertical ? viewportEl.scrollTop : viewportEl.scrollLeft;

    // Add document-level listeners
    const onPointerMove = (moveEvent) => handleThumbPointerMove(instanceId, orientation, moveEvent);
    const onPointerUp = (upEvent) => {
        handleThumbPointerUp(instanceId, orientation, upEvent);
        document.removeEventListener('pointermove', onPointerMove);
        document.removeEventListener('pointerup', onPointerUp);
    };

    document.addEventListener('pointermove', onPointerMove);
    document.addEventListener('pointerup', onPointerUp);
}

/**
 * Handle pointer move while dragging thumb
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 * @param {PointerEvent} e - The pointer event
 */
function handleThumbPointerMove(instanceId, orientation, e) {
    const state = instances.get(instanceId);
    if (!state) return;

    const scrollbar = state.scrollbars.get(orientation);
    if (!scrollbar || !scrollbar.isDragging) return;

    const { viewportEl } = state;
    const { scrollbarEl, thumbEl, dragStartPos, dragStartScroll } = scrollbar;
    const isVertical = orientation === 'vertical';

    // Calculate drag delta
    const currentPos = isVertical ? e.clientY : e.clientX;
    const delta = currentPos - dragStartPos;

    // Convert delta to scroll delta
    const trackSize = isVertical ? scrollbarEl.clientHeight : scrollbarEl.clientWidth;
    const thumbSize = isVertical ? thumbEl.offsetHeight : thumbEl.offsetWidth;
    const contentSize = isVertical ? viewportEl.scrollHeight : viewportEl.scrollWidth;
    const viewportSize = isVertical ? viewportEl.clientHeight : viewportEl.clientWidth;

    const effectiveTrackSize = trackSize - thumbSize;
    const maxScroll = contentSize - viewportSize;
    const scrollDelta = effectiveTrackSize > 0 ? (delta / effectiveTrackSize) * maxScroll : 0;

    // Apply new scroll position
    const newScroll = dragStartScroll + scrollDelta;
    if (isVertical) {
        viewportEl.scrollTop = newScroll;
    } else {
        viewportEl.scrollLeft = newScroll;
    }
}

/**
 * Handle pointer up (stop dragging)
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 * @param {PointerEvent} e - The pointer event
 */
function handleThumbPointerUp(instanceId, orientation, e) {
    const state = instances.get(instanceId);
    if (!state) return;

    const scrollbar = state.scrollbars.get(orientation);
    if (!scrollbar) return;

    scrollbar.isDragging = false;

    // Release pointer capture
    if (state.pointerCapture) {
        try {
            state.pointerCapture.element.releasePointerCapture(state.pointerCapture.pointerId);
        } catch {
            // Ignore errors if pointer was already released
        }
        state.pointerCapture = null;
    }

    // For hover type, check if we should hide scrollbars
    if (state.options.type === 'hover' && !state.isHovering) {
        const timeout = setTimeout(() => {
            for (const orient of state.scrollbars.keys()) {
                updateScrollbarVisibility(instanceId, orient);
            }
        }, state.options.scrollHideDelay);
        state.hideTimeouts[orientation] = timeout;
    }
}

/**
 * Update all registered thumbs
 * @param {string} instanceId - The scroll area instance ID
 */
function updateAllThumbs(instanceId) {
    const state = instances.get(instanceId);
    if (!state) return;

    for (const orientation of state.scrollbars.keys()) {
        updateThumb(instanceId, orientation);
    }
}

/**
 * Scroll viewport to a specific position ratio (0-1)
 * @param {string} instanceId - The scroll area instance ID
 * @param {string} orientation - 'vertical' | 'horizontal'
 * @param {number} ratio - Position ratio (0-1)
 */
export function scrollToPosition(instanceId, orientation, ratio) {
    const state = instances.get(instanceId);
    if (!state) return;

    const { viewportEl } = state;
    const isVertical = orientation === 'vertical';

    const contentSize = isVertical ? viewportEl.scrollHeight : viewportEl.scrollWidth;
    const viewportSize = isVertical ? viewportEl.clientHeight : viewportEl.clientWidth;
    const maxScroll = contentSize - viewportSize;
    const targetScroll = ratio * maxScroll;

    if (isVertical) {
        viewportEl.scrollTop = targetScroll;
    } else {
        viewportEl.scrollLeft = targetScroll;
    }
}

/**
 * Get current scroll info
 * @param {string} instanceId - The scroll area instance ID
 * @returns {object} Scroll information
 */
export function getScrollInfo(instanceId) {
    const state = instances.get(instanceId);
    if (!state) return null;

    const { viewportEl } = state;

    return {
        scrollTop: viewportEl.scrollTop,
        scrollLeft: viewportEl.scrollLeft,
        scrollHeight: viewportEl.scrollHeight,
        scrollWidth: viewportEl.scrollWidth,
        clientHeight: viewportEl.clientHeight,
        clientWidth: viewportEl.clientWidth,
        hasVerticalOverflow: viewportEl.scrollHeight > viewportEl.clientHeight,
        hasHorizontalOverflow: viewportEl.scrollWidth > viewportEl.clientWidth
    };
}

/**
 * Destroy a scroll area instance and clean up resources
 * @param {string} instanceId - The scroll area instance ID
 */
export function destroy(instanceId) {
    const state = instances.get(instanceId);
    if (!state) return;

    // Disconnect resize observer
    if (state.resizeObserver) {
        state.resizeObserver.disconnect();
    }

    // Clear timeouts
    clearTimeout(state.hideTimeouts.vertical);
    clearTimeout(state.hideTimeouts.horizontal);

    // Release any pointer capture
    if (state.pointerCapture) {
        try {
            state.pointerCapture.element.releasePointerCapture(state.pointerCapture.pointerId);
        } catch {
            // Ignore
        }
    }

    // Remove hover listeners if attached
    if (state.rootEl && state.options.type === 'hover') {
        // Note: We don't store references to the handlers, so they'll be GC'd with the element
    }

    instances.delete(instanceId);
}
