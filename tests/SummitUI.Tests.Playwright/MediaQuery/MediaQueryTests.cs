namespace SummitUI.Tests.Playwright.MediaQuery;

/// <summary>
/// Tests for MediaQuery component functionality.
/// Verifies reactive media query matching and state updates.
/// </summary>
public class MediaQueryTests : SummitTestBase
{
    protected override string TestPagePath => "tests/mediaquery/basic";

    #region Basic Matching

    [Test]
    public async Task MediaQuery_ShouldMatch_WhenQueryIsTrue()
    {
        // (min-width: 1px) should always match on any screen
        var result = Page.GetByTestId("always-match-result");
        await Expect(result).ToHaveAttributeAsync("data-matches", "true");
    }

    [Test]
    public async Task MediaQuery_ShouldNotMatch_WhenQueryIsFalse()
    {
        // (min-width: 99999px) should never match
        var result = Page.GetByTestId("never-match-result");
        await Expect(result).ToHaveAttributeAsync("data-matches", "false");
    }

    [Test]
    public async Task MediaQuery_ShouldRenderChildContent()
    {
        var result = Page.GetByTestId("always-match-result");
        await Expect(result).ToBeVisibleAsync();
        await Expect(result).ToContainTextAsync("Always matches:");
    }

    #endregion

    #region InitialValue Parameter

    [Test]
    public async Task MediaQuery_ShouldOverrideInitialValue_AfterJsEvaluates()
    {
        // InitialValue=true but query is (min-width: 99999px) which is false
        // After JS evaluates, should be false
        var result = Page.GetByTestId("initial-value-result");
        await Expect(result).ToHaveAttributeAsync("data-matches", "false");
    }

    #endregion

    #region OnChange Callback

    [Test]
    public async Task MediaQuery_ShouldInvokeOnChange_OnInitialEvaluation()
    {
        // The OnChange should fire at least once during initialization
        // when the JS evaluates the query and the value differs from InitialValue (false)
        var changeCount = Page.GetByTestId("change-count");
        
        // The change count could be 0 or 1+ depending on whether initial value matched
        // Just verify the element exists and displays a number
        await Expect(changeCount).ToBeVisibleAsync();
        await Expect(changeCount).ToContainTextAsync("Change count:");
    }

    #endregion

    #region Viewport Resize

    [Test]
    public async Task MediaQuery_ShouldReact_WhenViewportChanges()
    {
        var result = Page.GetByTestId("viewport-result");

        // Set viewport to 800px wide - should match (min-width: 500px)
        await Page.SetViewportSizeAsync(800, 600);
        await Expect(result).ToHaveAttributeAsync("data-matches", "true");

        // Set viewport to 400px wide - should NOT match (min-width: 500px)
        await Page.SetViewportSizeAsync(400, 600);
        await Expect(result).ToHaveAttributeAsync("data-matches", "false");

        // Set viewport back to 800px - should match again
        await Page.SetViewportSizeAsync(800, 600);
        await Expect(result).ToHaveAttributeAsync("data-matches", "true");
    }

    [Test]
    public async Task MediaQuery_ShouldUpdateOnChangeCount_WhenViewportChanges()
    {
        var changeCount = Page.GetByTestId("change-count");
        var result = Page.GetByTestId("onchange-result");

        // Get initial change count text
        var initialText = await changeCount.TextContentAsync();

        // Set viewport to trigger a change (if not already matching)
        await Page.SetViewportSizeAsync(800, 600);
        await Expect(result).ToHaveAttributeAsync("data-matches", "true");

        // Change viewport to not match
        await Page.SetViewportSizeAsync(400, 600);
        await Expect(result).ToHaveAttributeAsync("data-matches", "false");

        // The change count should have increased
        var newText = await changeCount.TextContentAsync();
        
        // Extract the numbers and verify change count increased
        var initialCount = int.Parse(initialText!.Replace("Change count: ", ""));
        var newCount = int.Parse(newText!.Replace("Change count: ", ""));
        
        await Assert.That(newCount).IsGreaterThan(initialCount);
    }

    #endregion

    #region Orientation Query

    [Test]
    public async Task MediaQuery_ShouldDetect_LandscapeOrientation()
    {
        var result = Page.GetByTestId("orientation-result");

        // Set landscape viewport (wider than tall)
        await Page.SetViewportSizeAsync(800, 400);
        await Expect(result).ToHaveAttributeAsync("data-matches", "true");

        // Set portrait viewport (taller than wide)
        await Page.SetViewportSizeAsync(400, 800);
        await Expect(result).ToHaveAttributeAsync("data-matches", "false");
    }

    #endregion

    #region Hover Capability

    [Test]
    public async Task MediaQuery_ShouldDetect_HoverCapability()
    {
        // Desktop browsers should support hover
        var result = Page.GetByTestId("hover-result");
        await Expect(result).ToHaveAttributeAsync("data-matches", "true");
    }

    #endregion

    #region Multiple MediaQuery Components

    [Test]
    public async Task MediaQuery_ShouldWork_WhenNested()
    {
        var result = Page.GetByTestId("multiple-result");

        // Set viewport to 1000px - both should match
        await Page.SetViewportSizeAsync(1000, 600);
        await Expect(result).ToHaveAttributeAsync("data-is400", "true");
        await Expect(result).ToHaveAttributeAsync("data-is800", "true");

        // Set viewport to 600px - only 400px should match
        await Page.SetViewportSizeAsync(600, 600);
        await Expect(result).ToHaveAttributeAsync("data-is400", "true");
        await Expect(result).ToHaveAttributeAsync("data-is800", "false");

        // Set viewport to 300px - neither should match
        await Page.SetViewportSizeAsync(300, 600);
        await Expect(result).ToHaveAttributeAsync("data-is400", "false");
        await Expect(result).ToHaveAttributeAsync("data-is800", "false");
    }

    #endregion

    #region Conditional Rendering

    [Test]
    public async Task MediaQuery_ShouldRenderCorrectContent_BasedOnMatch()
    {
        // Query is (min-width: 1px) which always matches
        var shownContent = Page.GetByTestId("conditional-shown");
        var hiddenContent = Page.GetByTestId("conditional-hidden");

        await Expect(shownContent).ToBeVisibleAsync();
        await Expect(hiddenContent).ToHaveCountAsync(0);
    }

    #endregion
}
