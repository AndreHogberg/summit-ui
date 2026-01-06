using Microsoft.Playwright;

namespace SummitUI.Tests.Playwright.Calendar;

/// <summary>
/// Tests for Calendar ARIA attributes and accessibility features.
/// </summary>
public class CalendarAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/calendar/basic";

    #region Grid ARIA Attributes

    [Test]
    public async Task Grid_ShouldHave_RoleApplication()
    {
        // Use role="application" to ensure screen readers pass through all keyboard events
        // This is necessary because the grid contains interactive buttons that need arrow key navigation
        var grid = Page.GetByTestId("grid");
        await Expect(grid).ToHaveAttributeAsync("role", "application");
        await Expect(grid).ToHaveAttributeAsync("aria-roledescription", "calendar grid");
    }

    [Test]
    public async Task Grid_ShouldHave_AriaLabelledBy()
    {
        var grid = Page.GetByTestId("grid");
        var ariaLabelledBy = await grid.GetAttributeAsync("aria-labelledby");
        await Assert.That(ariaLabelledBy).IsNotNull();
        
        // Verify the heading element exists with that ID
        var heading = Page.GetByTestId("heading");
        var headingId = await heading.GetAttributeAsync("id");
        await Assert.That(ariaLabelledBy).IsEqualTo(headingId);
    }

    #endregion

    #region Heading ARIA Attributes

    [Test]
    public async Task Heading_ShouldHave_AriaLivePolite()
    {
        var heading = Page.GetByTestId("heading");
        await Expect(heading).ToHaveAttributeAsync("aria-live", "polite");
    }

    [Test]
    public async Task Heading_ShouldHave_AriaAtomic()
    {
        var heading = Page.GetByTestId("heading");
        await Expect(heading).ToHaveAttributeAsync("aria-atomic", "true");
    }

    #endregion

    #region Day Button ARIA Attributes

    [Test]
    public async Task DayButton_ShouldHave_AriaLabel()
    {
        var section = Page.GetByTestId("basic-section");
        var dayButton = section.Locator("[data-summit-calendar-day]").First;
        var ariaLabel = await dayButton.GetAttributeAsync("aria-label");
        await Assert.That(ariaLabel).IsNotNull();
        await Assert.That(ariaLabel!.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task TodayButton_ShouldHave_AriaCurrentDate()
    {
        var section = Page.GetByTestId("basic-section");
        var todayButton = section.Locator("[data-summit-calendar-day][data-today]");
        
        // Today might not be visible if we're looking at a different month
        var count = await todayButton.CountAsync();
        if (count > 0)
        {
            await Expect(todayButton).ToHaveAttributeAsync("aria-current", "date");
        }
    }

    [Test]
    public async Task SelectedButton_ShouldHave_AriaSelected()
    {
        // Click a date to select it
        var section = Page.GetByTestId("basic-section");
        var dayButton = section.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").First;
        await dayButton.ClickAsync();
        
        // Verify aria-selected is set
        await Expect(dayButton).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task UnavailableButton_ShouldHave_AriaDisabled()
    {
        // Check the minmax section which has unavailable dates
        var section = Page.GetByTestId("minmax-section");
        var unavailableButton = section.Locator("[data-summit-calendar-day][data-unavailable]").First;
        
        var count = await unavailableButton.CountAsync();
        if (count > 0)
        {
            await Expect(unavailableButton).ToHaveAttributeAsync("aria-disabled", "true");
        }
    }

    #endregion

    #region Weekday Headers

    [Test]
    public async Task WeekdayHeader_ShouldHave_ScopeCol()
    {
        var section = Page.GetByTestId("basic-section");
        var headCell = section.Locator("[data-summit-calendar-head-cell]").First;
        await Expect(headCell).ToHaveAttributeAsync("scope", "col");
    }

    [Test]
    public async Task WeekdayHeader_ShouldHave_AbbrAttribute()
    {
        // Check the monday-start section which has explicit weekday test ids
        var section = Page.GetByTestId("monday-start-section");
        var headCell = section.GetByTestId("weekday-0");
        
        var abbr = await headCell.GetAttributeAsync("abbr");
        await Assert.That(abbr).IsNotNull();
        await Assert.That(abbr!.Length).IsGreaterThan(0);
    }

    #endregion

    #region Navigation Buttons

    [Test]
    public async Task PrevButton_ShouldHave_AriaLabel()
    {
        var prevButton = Page.GetByTestId("prev-button");
        var ariaLabel = await prevButton.GetAttributeAsync("aria-label");
        await Assert.That(ariaLabel).IsNotNull();
        await Assert.That(ariaLabel!.ToLower()).Contains("previous");
    }

    [Test]
    public async Task NextButton_ShouldHave_AriaLabel()
    {
        var nextButton = Page.GetByTestId("next-button");
        var ariaLabel = await nextButton.GetAttributeAsync("aria-label");
        await Assert.That(ariaLabel).IsNotNull();
        await Assert.That(ariaLabel!.ToLower()).Contains("next");
    }

    #endregion

    #region Disabled State

    [Test]
    public async Task DisabledCalendar_ShouldHave_DataDisabled()
    {
        var calendar = Page.GetByTestId("calendar-disabled");
        await Expect(calendar).ToHaveAttributeAsync("data-disabled", "true");
    }

    [Test]
    public async Task DisabledCalendar_DaysShould_BeDisabled()
    {
        var section = Page.GetByTestId("disabled-section");
        var dayButton = section.Locator("[data-summit-calendar-day]").First;
        await Expect(dayButton).ToBeDisabledAsync();
    }

    #endregion
}
