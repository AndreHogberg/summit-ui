using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Context that provides shared state between AlertDialog sub-components.
/// Exposes current options and completion callbacks.
/// </summary>
public sealed class AlertDialogContext
{
    /// <summary>
    /// Unique identifier for the alert dialog, used for ARIA relationships.
    /// </summary>
    public string DialogId { get; }

    /// <summary>
    /// Whether the alert dialog is currently open.
    /// </summary>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// Whether the dialog is currently animating closed.
    /// Used to keep content in DOM during close animations.
    /// </summary>
    public bool IsAnimatingClosed { get; set; }

    /// <summary>
    /// The message to display (from ConfirmAsync call).
    /// </summary>
    public string Message { get; internal set; } = "";

    /// <summary>
    /// The current options for this dialog instance.
    /// </summary>
    public AlertDialogOptions Options { get; internal set; } = new();

    /// <summary>
    /// Completes the dialog with the specified result (true = confirmed, false = cancelled).
    /// </summary>
    public Action<bool> Complete { get; internal set; } = _ => { };

    /// <summary>
    /// Notifies the provider that state has changed.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Reference to the content element (for focus management).
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Registers the content element reference.
    /// </summary>
    public Action<ElementReference> RegisterContent { get; internal set; } = _ => { };

    /// <summary>
    /// Creates a new alert dialog context with a unique ID.
    /// </summary>
    public AlertDialogContext()
    {
        DialogId = $"summit-alertdialog-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// ID for the title element (used for aria-labelledby).
    /// </summary>
    public string TitleId => $"{DialogId}-title";

    /// <summary>
    /// ID for the description element (used for aria-describedby).
    /// </summary>
    public string DescriptionId => $"{DialogId}-description";

    /// <summary>
    /// ID for the portal element.
    /// </summary>
    public string PortalId => $"{DialogId}-portal";
}
