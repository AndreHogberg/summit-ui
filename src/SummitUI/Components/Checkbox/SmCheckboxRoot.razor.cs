using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using SummitUI.Utilities;

namespace SummitUI;

/// <summary>
/// A fully accessible checkbox component with three states: unchecked, indeterminate, and checked.
/// Follows WAI-ARIA patterns for checkbox accessibility.
/// </summary>
public partial class SmCheckboxRoot : IAsyncDisposable
{
    [Inject]
    private SummitUtilities SummitUtilities { get; set; } = default!;

    /// <summary>
    /// Optional cascading parameter from CheckboxGroup.
    /// When present, the checkbox participates in group state management.
    /// </summary>
    [CascadingParameter]
    private CheckboxGroupContext? GroupContext { get; set; }

    /// <summary>
    /// Cascading EditContext from an EditForm parent.
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    /// <summary>
    /// Child content containing the checkbox indicator.
    /// A CheckboxContext is cascaded to child components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The controlled checked state. When provided, the component operates in controlled mode.
    /// Use with CheckedChanged for two-way binding.
    /// </summary>
    [Parameter]
    public bool? Checked { get; set; }

    /// <summary>
    /// Callback when the checked state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Expression for the checked value, used for EditForm binding and validation.
    /// </summary>
    [Parameter]
    public Expression<Func<bool>>? CheckedExpression { get; set; }

    /// <summary>
    /// The default checked state for uncontrolled mode.
    /// </summary>
    [Parameter]
    public bool DefaultChecked { get; set; }

    /// <summary>
    /// Whether the checkbox is in an indeterminate state.
    /// Useful for "select all" scenarios where some but not all items are selected.
    /// </summary>
    [Parameter]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// Callback when the indeterminate state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IndeterminateChanged { get; set; }

    /// <summary>
    /// The value of the checkbox when used within a CheckboxGroup.
    /// This value is added to the group's Values collection when checked.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// The name attribute for the hidden form input.
    /// When set (or inherited from CheckboxGroup), a hidden input is rendered for form submission.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Whether the checkbox is required for form submission.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Whether the checkbox is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// The id attribute for the checkbox element.
    /// When provided, this allows association with a <c>&lt;label for="..."&gt;</c> element.
    /// If not provided, an auto-generated id is used internally.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;label for="terms-checkbox"&gt;I agree to the terms&lt;/label&gt;
    /// &lt;SmCheckboxRoot Id="terms-checkbox"&gt;
    ///     &lt;SmCheckboxIndicator /&gt;
    /// &lt;/SmCheckboxRoot&gt;
    /// </code>
    /// </example>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Accessible label for the checkbox. Required when the checkbox is not wrapped in a label element.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Callback invoked when the checked state changes.
    /// </summary>
    [Parameter]
    public EventCallback<CheckedState> OnCheckedChange { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the checkbox element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly string _internalCheckboxId = $"summit-checkbox-{Guid.NewGuid():N}";

    /// <summary>
    /// The effective id used for the checkbox element.
    /// </summary>
    private string EffectiveId => Id ?? _internalCheckboxId;
    private ElementReference _elementRef;
    private bool _internalChecked;
    private bool _jsInitialized;
    private bool _isSubscribedToGroup;
    private FieldIdentifier? _fieldIdentifier;

    /// <summary>
    /// Whether we're operating in controlled mode.
    /// </summary>
    private bool IsControlled => Checked.HasValue;

    /// <summary>
    /// Whether we're part of a checkbox group.
    /// </summary>
    private bool IsInGroup => GroupContext is not null && !string.IsNullOrEmpty(Value);

    /// <summary>
    /// The effective disabled state (component or group level).
    /// </summary>
    private bool IsDisabled => Disabled || (GroupContext?.Disabled ?? false);

    /// <summary>
    /// The effective form name (from parameter or group).
    /// </summary>
    private string? EffectiveName => Name ?? GroupContext?.Name;

    /// <summary>
    /// The current checked state of the checkbox.
    /// </summary>
    private CheckedState CurrentState
    {
        get
        {
            if (Indeterminate)
            {
                return CheckedState.Indeterminate;
            }

            // If in a group, derive from group state
            if (IsInGroup)
            {
                return GroupContext!.IsChecked(Value!) ? CheckedState.Checked : CheckedState.Unchecked;
            }

            // Otherwise use controlled or internal state
            bool isChecked = IsControlled ? Checked!.Value : _internalChecked;
            return isChecked ? CheckedState.Checked : CheckedState.Unchecked;
        }
    }

    /// <summary>
    /// The aria-checked attribute value.
    /// </summary>
    private string AriaChecked => CurrentState switch
    {
        CheckedState.Checked => "true",
        CheckedState.Indeterminate => "mixed",
        _ => "false"
    };

    /// <summary>
    /// The data-state attribute value.
    /// </summary>
    private string DataState => CurrentState switch
    {
        CheckedState.Checked => "checked",
        CheckedState.Indeterminate => "indeterminate",
        _ => "unchecked"
    };

    /// <summary>
    /// The value for the hidden form input.
    /// </summary>
    private string HiddenInputValue => CurrentState == CheckedState.Checked ? (Value ?? "on") : "";

    /// <summary>
    /// The context passed to child content.
    /// </summary>
    private CheckboxContext CurrentContext => new()
    {
        State = CurrentState,
    };

    protected override void OnInitialized()
    {
        // Initialize internal state from default
        _internalChecked = DefaultChecked;

        // Subscribe to group state changes if in a group
        if (IsInGroup)
        {
            GroupContext!.OnStateChanged += HandleGroupStateChanged;
            _isSubscribedToGroup = true;
        }
    }

    protected override void OnParametersSet()
    {
        // Set up EditContext field identifier for validation (needs to happen after cascading parameters are set)
        if (!_fieldIdentifier.HasValue && EditContext is not null && CheckedExpression is not null)
        {
            _fieldIdentifier = FieldIdentifier.Create(CheckedExpression);
        }
    }

    private async void HandleGroupStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Only run JS interop when interactive
        if (!RendererInfo.IsInteractive) return;

        if (firstRender && !_jsInitialized)
        {
            _jsInitialized = true;
            await SummitUtilities.InitializeCheckboxAsync(_elementRef);
        }
    }

    private async Task HandleClickAsync()
    {
        if (IsDisabled) return;

        await ToggleAsync();
    }

    private async Task ToggleAsync()
    {
        // Clear indeterminate state on toggle
        if (Indeterminate)
        {
            await IndeterminateChanged.InvokeAsync(false);
        }

        // If in a group, delegate to group
        if (IsInGroup)
        {
            await GroupContext!.ToggleValueAsync(Value!);
            await OnCheckedChange.InvokeAsync(CurrentState);
            StateHasChanged();
            return;
        }

        // Toggle checked state
        var newChecked = CurrentState != CheckedState.Checked;

        // Update internal state for uncontrolled mode
        if (!IsControlled)
        {
            _internalChecked = newChecked;
        }

        await CheckedChanged.InvokeAsync(newChecked);

        // Notify EditContext of field change for validation
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }

        var newState = newChecked ? CheckedState.Checked : CheckedState.Unchecked;
        await OnCheckedChange.InvokeAsync(newState);

        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        // Unsubscribe from group state changes
        if (_isSubscribedToGroup && GroupContext is not null)
        {
            GroupContext.OnStateChanged -= HandleGroupStateChanged;
        }

        if (_jsInitialized)
        {
            try
            {
                await SummitUtilities.DestroyCheckboxAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Safe to ignore, JS resources are cleaned up by the browser
            }
        }
    }
}
