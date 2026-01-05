namespace SummitUI;

/// <summary>
/// Configuration options for the alert dialog.
/// </summary>
public class AlertDialogOptions
{
    /// <summary>
    /// The title displayed in the alert dialog.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The text for the confirm button. Defaults to "Confirm".
    /// </summary>
    public string ConfirmText { get; set; } = "Confirm";

    /// <summary>
    /// The text for the cancel button. Defaults to "Cancel".
    /// </summary>
    public string CancelText { get; set; } = "Cancel";

    /// <summary>
    /// Whether the action is destructive (e.g., delete operations).
    /// When true, components will have the data-destructive attribute for styling.
    /// </summary>
    public bool IsDestructive { get; set; }

    /// <summary>
    /// Whether pressing Escape closes the dialog and returns false. Defaults to true.
    /// </summary>
    public bool AllowEscapeClose { get; set; } = true;

    /// <summary>
    /// Whether clicking the overlay closes the dialog and returns false. Defaults to false.
    /// </summary>
    public bool AllowOverlayClose { get; set; }
}
