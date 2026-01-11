using Microsoft.AspNetCore.Components;

using SummitUI.Base;

namespace SummitUI;

/// <summary>
/// Context that provides shared state between AlertDialog sub-components.
/// Exposes current options and completion callbacks.
/// </summary>
public sealed class AlertDialogContext : OpenCloseContextBase
{
    /// <summary>
    /// Creates a new alert dialog context with a unique ID.
    /// </summary>
    public AlertDialogContext() : base("alertdialog")
    {
    }

    /// <summary>
    /// Unique identifier for the alert dialog, used for ARIA relationships.
    /// </summary>
    public string DialogId => ComponentId;

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
    /// ID for the title element (used for aria-labelledby).
    /// </summary>
    public string TitleId => GetElementId("title");

    /// <summary>
    /// ID for the description element (used for aria-describedby).
    /// </summary>
    public string DescriptionId => GetElementId("description");
}
