using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// A button that closes the parent toast when clicked.
/// </summary>
/// <remarks>
/// <para>
/// This component uses cascading values to get the toast key and close action.
/// It must be placed inside a <see cref="Toast{TContent}"/> component.
/// </para>
/// <para>
/// Users must provide their own accessible label via aria-label attribute.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;ToastCloseButton aria-label="Close notification" class="p-2"&gt;
///     ×
/// &lt;/ToastCloseButton&gt;
/// </code>
/// </example>
public class ToastCloseButton : ComponentBase
{
    [CascadingParameter(Name = "ToastKey")]
    private string ToastKey { get; set; } = default!;

    [CascadingParameter(Name = "CloseToast")]
    private Action<string>? CloseToast { get; set; }

    /// <summary>
    /// Child content for the button (icon, text, etc.).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string Element { get; set; } = "button";

    /// <summary>
    /// Additional attributes to apply to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;

        builder.OpenElement(seq++, Element);

        if (Element == "button")
        {
            builder.AddAttribute(seq++, "type", "button");
        }

        builder.AddAttribute(seq++, "data-summit-toast-close", "");
        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);
        builder.AddContent(seq++, ChildContent);
        builder.CloseElement();
    }

    private void HandleClick(MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(ToastKey)) return;
        CloseToast?.Invoke(ToastKey);
    }
}
