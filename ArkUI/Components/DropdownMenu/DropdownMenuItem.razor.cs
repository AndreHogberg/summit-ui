using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// A single menu item within the dropdown menu.
/// Implements menuitem role with full ARIA support.
/// </summary>
public partial class DropdownMenuItem : ComponentBase, IDisposable
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// Whether this item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Text value for typeahead search. If not provided, typeahead won't work for this item.
    /// </summary>
    [Parameter]
    public string? TextValue { get; set; }

    /// <summary>
    /// Callback invoked when this item is selected.
    /// </summary>
    [Parameter]
    public EventCallback OnSelect { get; set; }

    /// <summary>
    /// Callback invoked when this item is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string _itemId = "";
    private bool _isSubscribed;
    private string? _registeredTextValue;

    /// <summary>
    /// Whether this item is currently highlighted.
    /// </summary>
    private bool IsHighlighted => Context.HighlightedItemId == _itemId;

    protected override void OnInitialized()
    {
        _itemId = $"{Context.MenuId}-item-{Guid.NewGuid():N}";
        
        if (!Disabled)
        {
            Context.RegisterItem(_itemId);
        }

        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    protected override void OnParametersSet()
    {
        // Register/update text value for typeahead if changed
        if (!Disabled && TextValue != _registeredTextValue)
        {
            if (_registeredTextValue != null)
            {
                Context.UnregisterItemLabel(_itemId);
            }
            
            if (!string.IsNullOrEmpty(TextValue))
            {
                Context.RegisterItemLabel(_itemId, TextValue);
            }
            
            _registeredTextValue = TextValue;
        }
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        await OnClick.InvokeAsync(args);
        await OnSelect.InvokeAsync();
        await Context.SelectItemAsync();
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
        
        if (_registeredTextValue != null)
        {
            Context.UnregisterItemLabel(_itemId);
        }
    }
}
