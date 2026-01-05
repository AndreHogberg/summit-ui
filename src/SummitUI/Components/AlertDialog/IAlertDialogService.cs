namespace SummitUI;

/// <summary>
/// Service for displaying programmatic alert dialogs with async confirmation.
/// Similar to JavaScript's window.confirm() but returns a ValueTask for async handling.
/// </summary>
public interface IAlertDialogService
{
    /// <summary>
    /// Event raised when an alert dialog should be shown.
    /// </summary>
    event Action<AlertDialogRequest>? OnShow;

    /// <summary>
    /// Shows an alert dialog with the specified message and returns true if confirmed.
    /// </summary>
    /// <param name="message">The message to display in the dialog.</param>
    /// <param name="options">Optional configuration for the dialog appearance.</param>
    /// <returns>True if the user confirmed, false if cancelled or dismissed.</returns>
    ValueTask<bool> ConfirmAsync(string message, AlertDialogOptions? options = null);
}

/// <summary>
/// Internal request object for alert dialog display.
/// </summary>
public class AlertDialogRequest
{
    /// <summary>
    /// The message to display in the dialog.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Configuration options for the dialog.
    /// </summary>
    public AlertDialogOptions Options { get; init; } = new();

    /// <summary>
    /// Task completion source to signal when the user responds.
    /// </summary>
    internal TaskCompletionSource<bool> CompletionSource { get; } = new();
}
