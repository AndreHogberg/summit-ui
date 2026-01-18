using Microsoft.AspNetCore.Components;

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
public partial class SmPortalOutlet : IDisposable
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

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        PortalService.OnChange -= HandlePortalChange;
    }
}
