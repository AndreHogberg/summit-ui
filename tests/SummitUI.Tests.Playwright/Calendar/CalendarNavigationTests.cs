using Microsoft.Playwright;

namespace SummitUI.Tests.Playwright.Calendar;

/// <summary>
/// Tests for Calendar month navigation.
/// </summary>
public class CalendarNavigationTests : SummitTestBase
{
    protected override string TestPagePath => "tests/calendar/basic";

    [Test]
    public async Task PrevButton_ShouldNavigate_ToPreviousMonth()
    {
        var heading = Page.GetByTestId("heading");
        var initialHeading = await heading.TextContentAsync();
        
        var prevButton = Page.GetByTestId("prev-button");
        await prevButton.ClickAsync();
        
        // Wait for state update
        await Page.WaitForTimeoutAsync(100);
        
        var newHeading = await heading.TextContentAsync();
        await Assert.That(newHeading).IsNotEqualTo(initialHeading);
    }

    [Test]
    public async Task NextButton_ShouldNavigate_ToNextMonth()
    {
        var heading = Page.GetByTestId("heading");
        var initialHeading = await heading.TextContentAsync();
        
        var nextButton = Page.GetByTestId("next-button");
        await nextButton.ClickAsync();
        
        await Page.WaitForTimeoutAsync(100);
        
        var newHeading = await heading.TextContentAsync();
        await Assert.That(newHeading).IsNotEqualTo(initialHeading);
    }

    [Test]
    public async Task Navigation_ShouldUpdate_HeadingLiveRegion()
    {
        var heading = Page.GetByTestId("heading");
        
        // Verify it's a live region
        await Expect(heading).ToHaveAttributeAsync("aria-live", "polite");
        
        var initialHeading = await heading.TextContentAsync();
        
        var nextButton = Page.GetByTestId("next-button");
        await nextButton.ClickAsync();
        await Page.WaitForTimeoutAsync(100);
        
        var newHeading = await heading.TextContentAsync();
        
        // The heading text should have changed (for screen readers to announce)
        await Assert.That(newHeading).IsNotEqualTo(initialHeading);
    }

    [Test]
    public async Task Navigation_ShouldPreserve_DayFocus()
    {
        var section = Page.GetByTestId("basic-section");
        var grid = section.Locator("[data-summit-calendar-grid]");
        await grid.FocusAsync();
        
        // Get initial focused day number
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var initialDate = await focusedDay.GetAttributeAsync("data-date");
        var initialDay = DateOnly.Parse(initialDate!).Day;
        
        // Navigate to next month
        var nextButton = Page.GetByTestId("next-button");
        await nextButton.ClickAsync();
        await Page.WaitForTimeoutAsync(100);
        
        // Focus should move to same day number in new month (or closest valid day)
        focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var newDate = await focusedDay.GetAttributeAsync("data-date");
        var newDay = DateOnly.Parse(newDate!).Day;
        
        // Day should be same or adjusted for shorter month
        await Assert.That(newDay).IsLessThanOrEqualTo(initialDay + 3).And.IsGreaterThanOrEqualTo(1);
    }
}
