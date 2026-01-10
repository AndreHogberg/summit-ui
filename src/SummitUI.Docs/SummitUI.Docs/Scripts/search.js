// Search keyboard shortcut module for SummitUI Docs
// Handles Cmd+K (Mac) / Ctrl+K (Windows/Linux) global keyboard shortcut

let dotNetRef = null;

/**
 * Initialize the search keyboard shortcut
 * @param {object} dotNetReference - .NET object reference for callbacks
 */
export function init(dotNetReference) {
    dotNetRef = dotNetReference;
    document.addEventListener('keydown', handleKeyDown);
}

/**
 * Dispose the keyboard shortcut listener
 */
export function dispose() {
    document.removeEventListener('keydown', handleKeyDown);
    dotNetRef = null;
}

/**
 * Handle keydown events
 * @param {KeyboardEvent} e 
 */
function handleKeyDown(e) {
    // Check for Cmd+K (Mac) or Ctrl+K (Windows/Linux)
    if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault();
        
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnSearchShortcut');
        }
    }
}
