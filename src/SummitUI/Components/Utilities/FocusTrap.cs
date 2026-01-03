using SummitUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace SummitUI;

/// <summary>
/// A component that traps keyboard focus within its children.
/// Useful for modals, dialogs, popovers, and other overlay components
/// that need to contain focus for accessibility.
/// </summary>
public class FocusTrap : ComponentBase, IAsyncDisposable
{
    [Inject]
    private FocusTrapJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// The content to render within the focus trap.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether the focus trap is active. When true, focus cannot leave the container.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether to automatically focus the first focusable element when activated.
    /// </summary>
    [Parameter]
    public bool AutoFocus { get; set; } = true;

    /// <summary>
    /// Whether to return focus to the previously focused element when deactivated.
    /// </summary>
    [Parameter]
    public bool ReturnFocus { get; set; } = true;

    /// <summary>
    /// Callback invoked when the focus trap is activated.
    /// </summary>
    [Parameter]
    public EventCallback OnActivated { get; set; }

    /// <summary>
    /// Callback invoked when the focus trap is deactivated.
    /// </summary>
    [Parameter]
    public EventCallback OnDeactivated { get; set; }

    private ElementReference _containerRef;
    private string? _trapId;
    private bool _isActive;
    private bool _isDisposed;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-ark-focus-trap", _isActive ? "active" : "inactive");
        builder.AddElementReferenceCapture(2, elementReference => _containerRef = elementReference);
        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsActive && !_isActive)
        {
            await ActivateAsync();
        }
        else if (!IsActive && _isActive)
        {
            await DeactivateAsync();
        }
    }

    private async Task ActivateAsync()
    {
        if (_isActive) return;

        try
        {
            var options = new FocusTrapOptions
            {
                AutoFocus = AutoFocus,
                ReturnFocus = ReturnFocus
            };

            _trapId = await JsInterop.ActivateAsync(_containerRef, options);
            _isActive = true;
            await OnActivated.InvokeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    private async Task DeactivateAsync()
    {
        if (!_isActive || string.IsNullOrEmpty(_trapId)) return;

        try
        {
            await JsInterop.DeactivateAsync(_trapId);
            _isActive = false;
            _trapId = null;
            await OnDeactivated.InvokeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Manually focus the first focusable element in the container.
    /// </summary>
    public async ValueTask FocusFirstAsync()
    {
        await JsInterop.FocusFirstAsync(_containerRef);
    }

    /// <summary>
    /// Manually focus the last focusable element in the container.
    /// </summary>
    public async ValueTask FocusLastAsync()
    {
        await JsInterop.FocusLastAsync(_containerRef);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await DeactivateAsync();
    }
}
