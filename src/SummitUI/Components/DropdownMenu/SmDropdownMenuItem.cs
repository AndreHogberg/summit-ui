using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// A single menu item within the dropdown menu.
/// Implements menuitem role with full ARIA support.
/// Works in both root menu and submenus.
/// </summary>
public class SmDropdownMenuItem : ComponentBase, IDisposable
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuSubContext? SubContext { get; set; }

    /// <summary>
    /// Whether this item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Text value for typeahead search. If not provided, typeahead won't work for this item.
    /// </summary>
    [Parameter]
    [EditorRequired]
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
    /// Whether this item is inside a submenu.
    /// </summary>
    private bool IsInSubmenu => SubContext != null;

    /// <summary>
    /// Whether this item is currently highlighted.
    /// </summary>
    private bool IsHighlighted => IsInSubmenu 
        ? SubContext!.HighlightedItemId == _itemId 
        : Context.HighlightedItemId == _itemId;

    protected override void OnInitialized()
    {
        _itemId = IsInSubmenu 
            ? $"{SubContext!.SubMenuId}-item-{Guid.NewGuid():N}"
            : $"{Context.MenuId}-item-{Guid.NewGuid():N}";

        if (!Disabled)
        {
            if (IsInSubmenu)
            {
                SubContext!.RegisterItem(_itemId);
            }
            else
            {
                Context.RegisterItem(_itemId);
            }
        }

        Context.OnStateChanged += HandleStateChanged;
        if (IsInSubmenu)
        {
            SubContext!.OnStateChanged += HandleStateChanged;
        }
        _isSubscribed = true;
    }

    protected override void OnParametersSet()
    {
        // Register/update text value for typeahead if changed
        if (!Disabled && TextValue != _registeredTextValue)
        {
            if (_registeredTextValue != null)
            {
                if (IsInSubmenu)
                {
                    SubContext!.UnregisterItemLabel(_itemId);
                }
                else
                {
                    Context.UnregisterItemLabel(_itemId);
                }
            }

            if (!string.IsNullOrEmpty(TextValue))
            {
                if (IsInSubmenu)
                {
                    SubContext!.RegisterItemLabel(_itemId, TextValue);
                }
                else
                {
                    Context.RegisterItemLabel(_itemId, TextValue);
                }
            }

            _registeredTextValue = TextValue;
        }
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "menuitem");
        builder.AddAttribute(2, "id", _itemId);
        builder.AddAttribute(3, "tabindex", "-1");
        if (Disabled)
        {
            builder.AddAttribute(4, "aria-disabled", "true");
        }
        builder.AddAttribute(5, "data-summit-dropdown-menu-item", "");
        if (Disabled)
        {
            builder.AddAttribute(6, "data-disabled", "");
        }
        if (IsHighlighted)
        {
            builder.AddAttribute(7, "data-highlighted", "");
        }
        builder.AddAttribute(8, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(9, "onpointerenter", EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerEnterAsync));
        builder.AddAttribute(10, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddEventPreventDefaultAttribute(11, "onkeydown", true);
        builder.AddEventStopPropagationAttribute(12, "onkeydown", true);
        builder.AddMultipleAttributes(13, AdditionalAttributes);
        builder.AddContent(14, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        await OnClick.InvokeAsync(args);
        await OnSelect.InvokeAsync();
        
        // Close entire menu tree (including parent menus)
        await Context.CloseAsync();
    }

    private async Task HandlePointerEnterAsync(PointerEventArgs args)
    {
        if (Disabled) return;

        // When hovering over a regular item, close any open submenus at this level
        if (IsInSubmenu)
        {
            // Close any nested submenu that might be open
            if (SubContext!.ActiveNestedSubContext != null)
            {
                await SubContext.ActiveNestedSubContext.CloseAllAsync();
                SubContext.ActiveNestedSubContext = null;
            }
            await SubContext.SetHighlightedItemAsync(_itemId);
        }
        else
        {
            // Close any submenu that might be open at root level
            await Context.CloseAllSubMenusAsync();
            await Context.SetHighlightedItemAsync(_itemId);
        }
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled) return;

        // Delegate keyboard handling to the appropriate context
        if (IsInSubmenu)
        {
            await SubContext!.HandleKeyDownAsync(args.Key);
        }
        else
        {
            await Context.HandleKeyDownAsync(args.Key);
        }
    }

    public void Dispose()
    {
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
            if (IsInSubmenu)
            {
                SubContext!.OnStateChanged -= HandleStateChanged;
            }
        }

        if (!Disabled)
        {
            if (IsInSubmenu)
            {
                SubContext!.UnregisterItem(_itemId);
            }
            else
            {
                Context.UnregisterItem(_itemId);
            }
        }

        if (_registeredTextValue != null)
        {
            if (IsInSubmenu)
            {
                SubContext!.UnregisterItemLabel(_itemId);
            }
            else
            {
                Context.UnregisterItemLabel(_itemId);
            }
        }
    }
}
