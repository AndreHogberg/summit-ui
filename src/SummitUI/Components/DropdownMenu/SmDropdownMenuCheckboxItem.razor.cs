using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// A checkbox menu item that can be toggled.
/// </summary>
public partial class SmDropdownMenuCheckboxItem : ComponentBase, IDisposable
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// The checked state of the checkbox.
    /// </summary>
    [Parameter]
    public bool Checked { get; set; }

    /// <summary>
    /// Callback when checked state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// The indeterminate state of the checkbox.
    /// </summary>
    [Parameter]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// Callback when indeterminate state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IndeterminateChanged { get; set; }

    /// <summary>
    /// Whether this item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Callback invoked when this item is selected.
    /// </summary>
    [Parameter]
    public EventCallback OnSelect { get; set; }

    /// <summary>
    /// Child content with checkbox context.
    /// </summary>
    [Parameter]
    public RenderFragment<DropdownMenuCheckboxItemContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string _itemId = "";
    private bool _isSubscribed;

    private string AriaChecked => Indeterminate ? "mixed" : Checked.ToString().ToLowerInvariant();
    private string DataState => Indeterminate ? "indeterminate" : (Checked ? "checked" : "unchecked");

    /// <summary>
    /// Whether this item is currently highlighted.
    /// </summary>
    private bool IsHighlighted => Context.HighlightedItemId == _itemId;

    protected override void OnInitialized()
    {
        _itemId = $"{Context.MenuId}-checkbox-{Guid.NewGuid():N}";

        if (!Disabled)
        {
            Context.RegisterItem(_itemId);
        }

        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        // Clear indeterminate state on click
        if (Indeterminate)
        {
            Indeterminate = false;
            await IndeterminateChanged.InvokeAsync(false);
        }

        // Toggle checked state
        Checked = !Checked;
        await CheckedChanged.InvokeAsync(Checked);
        await OnSelect.InvokeAsync();

        // Don't close menu for checkbox items
    }

    private async Task HandleMouseEnterAsync()
    {
        if (Disabled) return;

        await Context.SetHighlightedItemAsync(_itemId);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled) return;

        // Delegate keyboard handling to the content via context
        await Context.HandleKeyDownAsync(args.Key);
    }

    public void Dispose()
    {
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        if (!Disabled)
        {
            Context.UnregisterItem(_itemId);
        }
    }
}

/// <summary>
/// Context passed to checkbox item child content.
/// </summary>
public class DropdownMenuCheckboxItemContext
{
    /// <summary>
    /// Whether the checkbox is checked.
    /// </summary>
    public bool Checked { get; init; }

    /// <summary>
    /// Whether the checkbox is in indeterminate state.
    /// </summary>
    public bool Indeterminate { get; init; }
}
