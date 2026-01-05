namespace SummitUI;

/// <summary>
/// Default implementation of <see cref="IAlertDialogService"/>.
/// </summary>
public class AlertDialogService : IAlertDialogService
{
    /// <inheritdoc />
    public event Action<AlertDialogRequest>? OnShow;

    /// <inheritdoc />
    public async ValueTask<bool> ConfirmAsync(string message, AlertDialogOptions? options = null)
    {
        var request = new AlertDialogRequest
        {
            Message = message,
            Options = options ?? new AlertDialogOptions()
        };

        OnShow?.Invoke(request);

        return await request.CompletionSource.Task;
    }
}
