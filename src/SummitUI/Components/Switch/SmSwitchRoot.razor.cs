using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace SummitUI;

/// <summary>
/// A toggle switch component that supports controlled and uncontrolled modes.
/// Implements the WAI-ARIA switch role with proper keyboard and accessibility support.
/// </summary>
public partial class SmSwitchRoot : ComponentBase
{
    /// <summary>
    /// The controlled checked state. When provided, the component operates in controlled mode.
    /// </summary>
    [Parameter]
    public bool? Checked { get; set; }

    /// <summary>
    /// The default checked state for uncontrolled mode.
    /// </summary>
    [Parameter]
    public bool DefaultChecked { get; set; }

    /// <summary>
    /// Callback when the checked state changes. Supports two-way binding with @bind-Checked.
    /// </summary>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Expression for the checked value, used for EditForm binding and validation.
    /// This is automatically provided when using @bind-Checked.
    /// </summary>
    [Parameter]
    public Expression<Func<bool>>? CheckedExpression { get; set; }

    /// <summary>
    /// Cascading EditContext from an EditForm parent.
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    /// <summary>
    /// Whether the switch is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether the switch is required for form validation.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// The name attribute for the hidden form input.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// The value submitted when the switch is checked. Defaults to "on".
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Child content (typically SmSwitchThumb).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _internalChecked;
    private SwitchContext _context = new();
    private FieldIdentifier? _fieldIdentifier;

    private bool IsControlled => Checked.HasValue;
    private bool IsChecked => IsControlled ? (Checked ?? false) : _internalChecked;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _internalChecked = DefaultChecked;
        UpdateContext();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // Set up EditContext field identifier for validation (needs to happen after cascading parameters are set)
        if (!_fieldIdentifier.HasValue && EditContext is not null && CheckedExpression is not null)
        {
            _fieldIdentifier = FieldIdentifier.Create(CheckedExpression);
        }

        UpdateContext();
    }

    private void UpdateContext()
    {
        _context = new SwitchContext
        {
            IsChecked = IsChecked
        };
    }

    private async Task HandleClick()
    {
        if (Disabled) return;

        var newChecked = !IsChecked;

        if (!IsControlled)
        {
            _internalChecked = newChecked;
        }

        UpdateContext();
        await CheckedChanged.InvokeAsync(newChecked);

        // Notify EditContext of field change for validation
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }
    }
}
