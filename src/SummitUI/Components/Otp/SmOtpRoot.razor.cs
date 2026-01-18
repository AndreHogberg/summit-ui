using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SummitUI.Components.Otp;
using SummitUI.Interop;
using SummitUI.Services;

namespace SummitUI;

/// <summary>
/// A one-time password input component using a single hidden input with visual slots.
/// Provides better accessibility, native browser autofill, and mobile keyboard support.
/// </summary>
public partial class SmOtpRoot : IAsyncDisposable
{
    [Inject] private OtpJsInterop JsInterop { get; set; } = default!;
    [Inject] private ILiveAnnouncer? Announcer { get; set; }

    /// <summary>
    /// The number of OTP digits (required).
    /// </summary>
    [Parameter, EditorRequired]
    public int MaxLength { get; set; }

    /// <summary>
    /// The current OTP value.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Callback when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Expression for form field identification.
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Prevents interaction with the input.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Regex pattern to validate input (e.g., "[0-9]*" for digits only).
    /// </summary>
    [Parameter]
    public string? Pattern { get; set; }

    /// <summary>
    /// Placeholder characters shown in empty slots.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Text alignment option affecting selection behavior.
    /// </summary>
    [Parameter]
    public OtpTextAlign TextAlign { get; set; } = OtpTextAlign.Left;

    /// <summary>
    /// Mobile keyboard type. Default is "numeric".
    /// </summary>
    [Parameter]
    public string InputMode { get; set; } = "numeric";

    /// <summary>
    /// Autofill hint for browsers. Default is "one-time-code".
    /// </summary>
    [Parameter]
    public string AutoComplete { get; set; } = "one-time-code";

    /// <summary>
    /// Form field name for the hidden input.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// The id attribute for the input element. Required for label association using the 'for' attribute.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;label for="verification-code"&gt;Enter code&lt;/label&gt;
    /// &lt;SmOtpRoot Id="verification-code" MaxLength="6" /&gt;
    /// </code>
    /// </example>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Accessible label for the input. Use when a visible label is not present.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;SmOtpRoot AriaLabel="Enter 6-digit verification code" MaxLength="6" /&gt;
    /// </code>
    /// </example>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// ID of the element that labels this input. Use with visible labels that cannot use the 'for' attribute.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;span id="otp-label"&gt;Verification code&lt;/span&gt;
    /// &lt;SmOtpRoot AriaLabelledBy="otp-label" MaxLength="6" /&gt;
    /// </code>
    /// </example>
    [Parameter]
    public string? AriaLabelledBy { get; set; }

    /// <summary>
    /// Callback when all slots are filled.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnComplete { get; set; }

    /// <summary>
    /// Function to generate the completion announcement for screen readers.
    /// Receives the completed code value.
    /// If null, no announcement is made.
    /// </summary>
    /// <example>
    /// GetCompletionAnnouncement="@(code => $"Verification code {code} entered")"
    /// </example>
    [Parameter]
    public Func<string, string>? GetCompletionAnnouncement { get; set; }

    /// <summary>
    /// Function to transform pasted text (e.g., to remove dashes).
    /// </summary>
    [Parameter]
    public Func<string, string>? PasteTransformer { get; set; }

    /// <summary>
    /// Custom rendering template for slots. Receives OtpRenderContext.
    /// </summary>
    [Parameter]
    public RenderFragment<OtpRenderContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes for the container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter] private EditContext? EditContext { get; set; }

    // State
    private string _internalValue = "";
    private int? _selectionStart;
    private int? _selectionEnd;
    private bool _isFocused;
    private bool _isHovering;
    private string? _previousValue;
    private Regex? _regex;
    private FieldIdentifier? _fieldIdentifier;
    private DotNetObjectReference<SmOtpRoot>? _dotNetRef;
    private ElementReference _inputElement = default!;
    private ElementReference _containerElement = default!;
    private bool _initialized;

    private string CurrentValue => Value ?? _internalValue;

    protected override void OnInitialized()
    {
        _dotNetRef = DotNetObjectReference.Create(this);

        if (!string.IsNullOrEmpty(Pattern))
        {
            _regex = new Regex(Pattern);
        }

        if (EditContext != null && ValueExpression != null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }

        if (!string.IsNullOrEmpty(Value))
        {
            _internalValue = Value;
        }
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(Pattern) && (_regex == null || _regex.ToString() != Pattern))
        {
            _regex = new Regex(Pattern);
        }

        // Track previous value for onComplete detection
        _previousValue = CurrentValue;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized)
        {
            _initialized = true;
            await JsInterop.InitializeAsync(_inputElement, _containerElement, _dotNetRef!, MaxLength);
        }
    }

    /// <summary>
    /// Called from JavaScript when selection changes.
    /// </summary>
    [JSInvokable]
    public void OnSelectionChanged(int? start, int? end)
    {
        _selectionStart = start;
        _selectionEnd = end;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript when focus changes.
    /// </summary>
    [JSInvokable]
    public void OnFocusChanged(bool focused)
    {
        _isFocused = focused;
        if (!focused)
        {
            _selectionStart = null;
            _selectionEnd = null;
        }
        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript when hover state changes.
    /// </summary>
    [JSInvokable]
    public void OnHoverChanged(bool hovering)
    {
        _isHovering = hovering;
        StateHasChanged();
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        var newValue = (e.Value?.ToString() ?? "").Substring(0, Math.Min(e.Value?.ToString()?.Length ?? 0, MaxLength));

        // Validate against pattern
        if (!string.IsNullOrEmpty(newValue) && _regex != null && !_regex.IsMatch(newValue))
        {
            return;
        }

        await UpdateValueAsync(newValue);

        // Check for completion
        if (_previousValue != null &&
            _previousValue.Length < MaxLength &&
            newValue.Length == MaxLength)
        {
            await OnComplete.InvokeAsync(newValue);

            // Announce completion to screen readers
            if (GetCompletionAnnouncement is not null)
            {
                Announcer?.Announce(GetCompletionAnnouncement(newValue));
            }
        }

        _previousValue = newValue;
    }

    private async Task UpdateValueAsync(string newValue)
    {
        _internalValue = newValue;
        await ValueChanged.InvokeAsync(newValue);

        if (EditContext != null && _fieldIdentifier.HasValue)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }
    }

    private OtpRenderContext BuildRenderContext()
    {
        var value = CurrentValue;
        var slots = new List<OtpSlotState>(MaxLength);

        for (var i = 0; i < MaxLength; i++)
        {
            var isActive = _isFocused &&
                           _selectionStart.HasValue &&
                           _selectionEnd.HasValue &&
                           ((_selectionStart == _selectionEnd && i == _selectionStart) ||
                            (i >= _selectionStart && i < _selectionEnd));

            var ch = i < value.Length ? value[i] : (char?)null;
            var placeholderChar = value.Length == 0 && Placeholder != null && i < Placeholder.Length
                ? Placeholder[i]
                : (char?)null;

            slots.Add(new OtpSlotState
            {
                Index = i,
                Char = ch,
                PlaceholderChar = placeholderChar,
                IsActive = isActive
            });
        }

        return new OtpRenderContext
        {
            Slots = slots,
            IsFocused = _isFocused,
            IsHovering = !Disabled && _isHovering
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_initialized)
        {
            await JsInterop.DestroyAsync(_inputElement);
        }
        _dotNetRef?.Dispose();
    }
}
