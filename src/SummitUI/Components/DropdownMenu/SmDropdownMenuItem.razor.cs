using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// A single menu item within the dropdown menu.
/// Implements menuitem role with full ARIA support.
/// Works in both root menu and submenus.
/// Supports the AsChild pattern for rendering custom elements (e.g., navigation links).
/// </summary>
public partial class SmDropdownMenuItem : ComponentBase, IDisposable
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
    /// When true, the component will not render a wrapper element.
    /// Instead, it passes attributes via context to the child element.
    /// The child must apply @attributes="context.Attrs" for proper functionality.
    /// </summary>
    [Parameter]
    public bool AsChild { get; set; }

    /// <summary>
    /// Child content. When AsChild is true, receives an AsChildContext with attributes to apply.
    /// </summary>
    [Parameter]
    public RenderFragment<AsChildContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private string _itemId = "";
    private bool _isSubscribed;
    private string? _registeredTextValue;
    private AsChildContext _context = default!;

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

        _context = new AsChildContext
        {
            Attrs = BuildAttributes(),
            RefCallback = el => _elementRef = el
        };
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

        // Rebuild AsChild context
        _context = new AsChildContext
        {
            Attrs = BuildAttributes(),
            RefCallback = el => _elementRef = el
        };
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private IReadOnlyDictionary<string, object> BuildAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["role"] = "menuitem",
            ["id"] = _itemId,
            ["tabindex"] = -1,
            ["data-summit-dropdown-menu-item"] = "",
            ["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync),
            ["onpointerenter"] = EventCallback.Factory.Create<PointerEventArgs>(this, HandlePointerEnterAsync),
            ["onkeydown"] = EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync),
            ["__internal_preventDefault_onkeydown"] = true,
            ["__internal_stopPropagation_onkeydown"] = true
        };

        if (Disabled)
        {
            attrs["aria-disabled"] = "true";
            attrs["data-disabled"] = "";
        }

        if (IsHighlighted)
        {
            attrs["data-highlighted"] = "";
        }

        // Merge additional attributes (consumer attributes win)
        if (AdditionalAttributes is not null)
        {
            foreach (var (key, value) in AdditionalAttributes)
            {
                attrs[key] = value;
            }
        }

        return attrs;
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
