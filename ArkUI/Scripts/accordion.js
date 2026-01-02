// accordion.js - ES6 module for accordion animations
// Note: Keyboard navigation has been moved to Blazor for better cross-render-mode compatibility.
// Only DOM measurement functionality remains here (cannot be done in pure Blazor).

/**
 * Set CSS variables for content height and width (for smooth animations).
 * This requires DOM measurement which cannot be done in pure Blazor.
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
