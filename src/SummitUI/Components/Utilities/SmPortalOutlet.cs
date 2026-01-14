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
public class SmPortalOutlet : ComponentBase, IDisposable
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
        var portals = PortalService.GetPortals().ToList();
        
        for (var i = 0; i < portals.Count; i++)
        {
            var (id, content) = portals[i];
            var baseSequence = i * 10; // Use stable sequences per portal
            
            // Each portal gets its own container div at the body level
            builder.OpenElement(baseSequence, "div");
            builder.AddAttribute(baseSequence + 1, "id", id);
            builder.AddAttribute(baseSequence + 2, "data-summit-portal", true);
            builder.AddContent(baseSequence + 3, content);
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
