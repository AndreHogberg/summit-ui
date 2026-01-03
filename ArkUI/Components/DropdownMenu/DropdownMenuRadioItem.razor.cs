using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// A radio menu item within a radio group.
/// </summary>
public partial class DropdownMenuRadioItem : ComponentBase, IDisposable
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuRadioGroupContext RadioContext { get; set; } = default!;

    /// <summary>
    /// The value of this radio item.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = default!;

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
    /// Child content with radio context.
    /// </summary>
    [Parameter]
    public RenderFragment<DropdownMenuRadioItemContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string _itemId = "";
    private bool _isSubscribed;

    private bool IsSelected => RadioContext.Value == Value;
    private string DataState => IsSelected ? "checked" : "unchecked";

    /// <summary>
    /// Whether this item is currently highlighted.
    /// </summary>
    private bool IsHighlighted => Context.HighlightedItemId == _itemId;

    protected override void OnInitialized()
    {
        _itemId = $"{Context.MenuId}-radio-{Guid.NewGuid():N}";
        
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

        await RadioContext.OnValueChangeAsync(Value);
        await OnSelect.InvokeAsync();

        // Don't close menu for radio items
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
/// Context passed to radio item child content.
/// </summary>
public class DropdownMenuRadioItemContext
{
    /// <summary>
    /// Whether the radio item is selected.
    /// </summary>
    public bool Checked { get; init; }
}
