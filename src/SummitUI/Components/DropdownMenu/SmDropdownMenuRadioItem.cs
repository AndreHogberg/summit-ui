using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// A radio menu item within a radio group.
/// </summary>
public class SmDropdownMenuRadioItem : ComponentBase, IDisposable
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "menuitemradio");
        builder.AddAttribute(2, "id", _itemId);
        builder.AddAttribute(3, "tabindex", "-1");
        builder.AddAttribute(4, "aria-checked", IsSelected.ToString().ToLowerInvariant());
        if (Disabled)
        {
            builder.AddAttribute(5, "aria-disabled", "true");
        }
        builder.AddAttribute(6, "data-summit-dropdown-menu-radio-item", "");
        builder.AddAttribute(7, "data-state", DataState);
        if (Disabled)
        {
            builder.AddAttribute(8, "data-disabled", "");
        }
        if (IsHighlighted)
        {
            builder.AddAttribute(9, "data-highlighted", "");
        }
        builder.AddAttribute(10, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(11, "onmouseenter", EventCallback.Factory.Create<MouseEventArgs>(this, HandleMouseEnterAsync));
        builder.AddAttribute(12, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddEventPreventDefaultAttribute(13, "onkeydown", true);
        builder.AddEventStopPropagationAttribute(14, "onkeydown", true);
        builder.AddMultipleAttributes(15, AdditionalAttributes);

        if (ChildContent is not null)
        {
            builder.AddContent(16, ChildContent(new DropdownMenuRadioItemContext { Checked = IsSelected }));
        }

        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        await RadioContext.OnValueChangeAsync(Value);
        await OnSelect.InvokeAsync();

        // Close menu after radio selection (single choice made)
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
