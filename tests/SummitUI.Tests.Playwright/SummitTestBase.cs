using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Base class for all SummitUI Playwright tests.
/// Provides common setup, navigation, and utility methods for reliable testing.
/// </summary>
public abstract class SummitTestBase : PageTest
{
    /// <summary>
    /// The URL path for this test's dedicated page (e.g., "/tests/select/basic").
    /// Each test class should have its own minimal page for isolation.
    /// </summary>
    protected abstract string TestPagePath { get; }

    /// <summary>
    /// Navigates to the test page and waits for Blazor to be fully interactive.
    /// </summary>
    [Before(Test)]
    public virtual async Task NavigateToTestPage()
    {
        var fullUrl = Hooks.ServerUrl + TestPagePath;
        await Page.GotoAsync(fullUrl);
        await WaitForBlazorReady();
    }

    /// <summary>
    /// Waits for Blazor WebAssembly to fully hydrate and become interactive.
    /// This is more reliable than NetworkIdle for Blazor apps.
    /// </summary>
    protected async Task WaitForBlazorReady()
    {
        // Wait for the page-specific ready marker to be visible
        // Each test page should render a data-testid="page-ready" element when fully loaded
        await Page.WaitForSelectorAsync("[data-testid='page-ready']", new()
        {
            State = Microsoft.Playwright.WaitForSelectorState.Visible,
            Timeout = 15000
        });
    }

    /// <summary>
    /// Waits for an element to be stable (visible and not animating).
    /// Use this after opening dropdowns, dialogs, etc.
    /// </summary>
    protected async Task WaitForStable(Microsoft.Playwright.ILocator locator)
    {
        await locator.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Opens a select/dropdown and waits for content to be visible.
    /// </summary>
    protected async Task OpenSelectAsync(Microsoft.Playwright.ILocator trigger)
    {
        await trigger.ClickAsync();
        await Page.WaitForSelectorAsync("[data-summit-select-content][data-state='open']", new()
        {
            State = Microsoft.Playwright.WaitForSelectorState.Visible,
            Timeout = 5000
        });
    }

    /// <summary>
    /// Closes a select by pressing Escape and waits for content to be hidden.
    /// </summary>
    protected async Task CloseSelectAsync()
    {
        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForSelectorAsync("[data-summit-select-content]", new()
        {
            State = Microsoft.Playwright.WaitForSelectorState.Hidden,
            Timeout = 5000
        });
    }
}
