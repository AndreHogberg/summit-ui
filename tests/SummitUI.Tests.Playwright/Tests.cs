using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

public class Tests : PageTest
{
    [Test]
    public async Task BlazorAppLoads()
    {
        // Navigate to the Blazor application running via WebApplicationFactory
        await Page.GotoAsync(Hooks.ServerUrl);

        // Wait for Blazor to fully load
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // Verify the page loaded successfully
        await Expect(Page).Not.ToHaveTitleAsync(string.Empty);
    }
}
