using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Outlet component that renders all registered portal content.
/// Place this component once at the root of your application layout
/// (typically in App.razor or MainLayout.razor) to enable portal functionality.
/// 
/// <example>
/// <code>
/// &lt;!-- In App.razor or MainLayout.razor --&gt;
/// &lt;Router ...&gt;
///     ...
/// &lt;/Router&gt;
/// &lt;PortalOutlet /&gt;
/// </code>
/// </example>
/// </summary>
public class PortalOutlet : ComponentBase, IDisposable
{
    [Inject]
    private IPortalService PortalService { get; set; } = default!;

    private bool _isDisposed;

    protected override void OnInitialized()
    {
        PortalService.OnChange += HandlePortalChange;
    }

    private async void HandlePortalChange()
    {
        if (_isDisposed) return;
        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var sequence = 0;

        foreach (var (id, content) in PortalService.GetPortals())
        {
            // Each portal gets its own container div at the body level
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "id", id);
            builder.AddAttribute(sequence++, "data-summit-portal", true);
            builder.AddContent(sequence++, content);
            builder.CloseElement();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        PortalService.OnChange -= HandlePortalChange;
    }
}
