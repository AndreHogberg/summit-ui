using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

using SummitUI.Utilities;

namespace SummitUI;

/// <summary>
/// An individual radio button item within a RadioGroupRoot.
/// Implements roving tabindex and keyboard navigation per WAI-ARIA patterns.
/// </summary>
public class RadioGroupItem : ComponentBase, IAsyncDisposable
{
    [Inject]
    private SummitUtilities SummitUtilities { get; set; } = default!;

    /// <summary>
    /// The cascaded radio group context from RadioGroupRoot.
    /// </summary>
    [CascadingParameter]
    private RadioGroupContext Context { get; set; } = default!;

    /// <summary>
    /// The unique value for this radio item.
    /// This value is used when the item is selected.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = default!;

    /// <summary>
    /// Whether this radio item is disabled.
    /// Disabled items cannot be selected or focused via keyboard navigation.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Child content with context providing checked and disabled state.
    /// </summary>
    [Parameter]
    public RenderFragment<RadioGroupItemContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the radio button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _isSubscribed;
    private bool _previousDisabled;
    private bool _jsInitialized;
    private ElementReference _elementRef;

    /// <summary>
    /// Whether this item is checked (selected).
    /// </summary>
    private bool IsChecked => Context.Value == Value;

    /// <summary>
    /// The effective disabled state (item or group level).
    /// </summary>
    private bool IsDisabled => Disabled || Context.Disabled;

    /// <summary>
    /// Whether this item should have tabindex="0" (be focusable via Tab).
    /// Only one item in the group should be focusable at a time (roving tabindex).
    /// </summary>
    private bool IsFocusable => Context.GetFocusableValue() == Value;

    /// <summary>
    /// The data-state attribute value.
    /// </summary>
    private string DataState => IsChecked ? "checked" : "unchecked";

    /// <summary>
    /// The element ID for this item.
    /// </summary>
    private string ItemId => Context.GetItemId(Value);

    /// <summary>
    /// The orientation string for data attribute.
    /// </summary>
    private string OrientationString => Context.Orientation == RadioGroupOrientation.Horizontal ? "horizontal" : "vertical";

    /// <summary>
    /// The context passed to child content.
    /// </summary>
    private RadioGroupItemContext ItemContext => new()
    {
        Checked = IsChecked,
        Disabled = IsDisabled
    };

    protected override void OnInitialized()
    {
        // Register with context for keyboard navigation
        Context.RegisterItem(Value, IsDisabled);
        _previousDisabled = IsDisabled;

        // Subscribe to state changes
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    protected override void OnParametersSet()
    {
        // Update registration if disabled state changed
        if (_previousDisabled != IsDisabled)
        {
            Context.UpdateItemDisabled(Value, IsDisabled);
            _previousDisabled = IsDisabled;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_jsInitialized)
        {
            await SummitUtilities.InitializeRadioItemAsync(_elementRef);
            _jsInitialized = true;
        }
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "role", "radio");
        builder.AddAttribute(3, "id", ItemId);
        builder.AddAttribute(4, "aria-checked", IsChecked ? "true" : "false");

        if (IsDisabled)
        {
            builder.AddAttribute(5, "aria-disabled", "true");
            builder.AddAttribute(6, "disabled", true);
        }

        // Roving tabindex: only the focusable item has tabindex="0"
        builder.AddAttribute(7, "tabindex", IsFocusable && !IsDisabled ? "0" : "-1");

        // Data attributes for styling
        builder.AddAttribute(8, "data-summit-radio-group-item", "");
        builder.AddAttribute(9, "data-state", DataState);
        builder.AddAttribute(10, "data-orientation", OrientationString);

        if (IsDisabled)
        {
            builder.AddAttribute(11, "data-disabled", "");
        }

        // Event handlers
        builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(13, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));

        builder.AddMultipleAttributes(14, AdditionalAttributes);
        builder.AddElementReferenceCapture(15, elementRef => _elementRef = elementRef);

        // Cascade item context to children (for RadioGroupIndicator)
        // and render child content with context parameter
        builder.OpenComponent<CascadingValue<RadioGroupItemContext>>(16);
        builder.AddComponentParameter(17, "Value", ItemContext);
        builder.AddComponentParameter(18, "IsFixed", false);
        builder.AddComponentParameter(19, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (ChildContent is not null)
            {
                childBuilder.AddContent(0, ChildContent(ItemContext));
            }
        }));
        builder.CloseComponent();

        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (IsDisabled) return;

        await Context.SelectValueAsync(Value);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (IsDisabled) return;

        switch (args.Key)
        {
            case " ": // Space - check the focused item
                await Context.SelectValueAsync(Value);
                break;

            case "ArrowDown":
                if (Context.Orientation == RadioGroupOrientation.Vertical)
                {
                    await NavigateAndSelectAsync(1);
                }
                break;

            case "ArrowUp":
                if (Context.Orientation == RadioGroupOrientation.Vertical)
                {
                    await NavigateAndSelectAsync(-1);
                }
                break;

            case "ArrowRight":
                if (Context.Orientation == RadioGroupOrientation.Horizontal)
                {
                    // Detect RTL using the element's computed style to account for inherited dir attributes
                    var isRtl = await SummitUtilities.IsElementRtlAsync(ItemId);
                    // In RTL, ArrowRight goes to previous; in LTR, it goes to next
                    var direction = isRtl ? -1 : 1;
                    await NavigateAndSelectAsync(direction);
                }
                break;

            case "ArrowLeft":
                if (Context.Orientation == RadioGroupOrientation.Horizontal)
                {
                    // Detect RTL using the element's computed style to account for inherited dir attributes
                    var isRtl = await SummitUtilities.IsElementRtlAsync(ItemId);
                    // In RTL, ArrowLeft goes to next; in LTR, it goes to previous
                    var direction = isRtl ? 1 : -1;
                    await NavigateAndSelectAsync(direction);
                }
                break;
        }
    }

    private async Task NavigateAndSelectAsync(int direction)
    {
        var nextValue = Context.GetNextValue(Value, direction);
        if (nextValue is null) return;

        // Select the next item (per WAI-ARIA pattern: arrow keys select)
        await Context.SelectValueAsync(nextValue);

        // Focus the next item
        var nextItemId = Context.GetItemId(nextValue);
        await SummitUtilities.FocusElementByIdAsync(nextItemId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        Context.UnregisterItem(Value);

        if (_jsInitialized)
        {
            await SummitUtilities.DestroyRadioItemAsync(_elementRef);
        }
    }
}
