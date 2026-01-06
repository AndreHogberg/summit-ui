using Microsoft.Playwright;

namespace SummitUI.Tests.Playwright.Calendar;

/// <summary>
/// Tests for Calendar keyboard navigation (WCAG compliant).
/// </summary>
public class CalendarKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/calendar/basic";

    /// <summary>
    /// Focuses the day button that is in the tab order (the one with tabindex="0").
    /// This simulates a user tabbing into the calendar.
    /// </summary>
    private async Task FocusCalendarDayAsync(ILocator section)
    {
        var focusableDay = section.Locator("[data-summit-calendar-day][tabindex='0']");
        await focusableDay.FocusAsync();
    }

    #region Arrow Key Navigation

    [Test]
    public async Task ArrowRight_ShouldMove_ToNextDay()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        // Get the initially focused date
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var initialDate = await focusedDay.GetAttributeAsync("data-date");
        
        await Page.Keyboard.PressAsync("ArrowRight");
        
        // The focused day should change
        focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var newDate = await focusedDay.GetAttributeAsync("data-date");
        
        await Assert.That(newDate).IsNotEqualTo(initialDate);
    }

    [Test]
    public async Task ArrowLeft_ShouldMove_ToPreviousDay()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        // Move right first to ensure we can move left
        await Page.Keyboard.PressAsync("ArrowRight");
        await Page.Keyboard.PressAsync("ArrowRight");
        
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var beforeDate = await focusedDay.GetAttributeAsync("data-date");
        
        await Page.Keyboard.PressAsync("ArrowLeft");
        
        focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var afterDate = await focusedDay.GetAttributeAsync("data-date");
        
        await Assert.That(afterDate).IsNotEqualTo(beforeDate);
    }

    [Test]
    public async Task ArrowDown_ShouldMove_ToNextWeek()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var initialDate = await focusedDay.GetAttributeAsync("data-date");
        var initialDateParsed = DateOnly.Parse(initialDate!);
        
        await Page.Keyboard.PressAsync("ArrowDown");
        
        focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var newDate = await focusedDay.GetAttributeAsync("data-date");
        var newDateParsed = DateOnly.Parse(newDate!);
        
        // Should be exactly 7 days later
        await Assert.That((newDateParsed.DayNumber - initialDateParsed.DayNumber)).IsEqualTo(7);
    }

    [Test]
    public async Task ArrowUp_ShouldMove_ToPreviousWeek()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        // Move down first
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var beforeDate = await focusedDay.GetAttributeAsync("data-date");
        var beforeDateParsed = DateOnly.Parse(beforeDate!);
        
        await Page.Keyboard.PressAsync("ArrowUp");
        
        focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var afterDate = await focusedDay.GetAttributeAsync("data-date");
        var afterDateParsed = DateOnly.Parse(afterDate!);
        
        // Should be exactly 7 days earlier
        await Assert.That((beforeDateParsed.DayNumber - afterDateParsed.DayNumber)).IsEqualTo(7);
    }

    #endregion

    #region Home and End Keys

    [Test]
    public async Task Home_ShouldMove_ToStartOfWeek()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        // Move to middle of week
        await Page.Keyboard.PressAsync("ArrowRight");
        await Page.Keyboard.PressAsync("ArrowRight");
        await Page.Keyboard.PressAsync("ArrowRight");
        
        await Page.Keyboard.PressAsync("Home");
        
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        await Expect(focusedDay).ToBeVisibleAsync();
    }

    [Test]
    public async Task End_ShouldMove_ToEndOfWeek()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        await Page.Keyboard.PressAsync("End");
        
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        await Expect(focusedDay).ToBeVisibleAsync();
    }

    #endregion

    #region Page Up/Down (Month Navigation)

    [Test]
    public async Task PageDown_ShouldNavigate_ToNextMonth()
    {
        var section = Page.GetByTestId("basic-section");
        var heading = section.Locator("[data-summit-calendar-heading]");
        var initialHeading = await heading.TextContentAsync();
        
        await FocusCalendarDayAsync(section);
        await Page.Keyboard.PressAsync("PageDown");
        
        // Wait for state update
        await Page.WaitForTimeoutAsync(100);
        
        var newHeading = await heading.TextContentAsync();
        await Assert.That(newHeading).IsNotEqualTo(initialHeading);
    }

    [Test]
    public async Task PageUp_ShouldNavigate_ToPreviousMonth()
    {
        var section = Page.GetByTestId("basic-section");
        var heading = section.Locator("[data-summit-calendar-heading]");
        
        await FocusCalendarDayAsync(section);
        
        // First go forward
        await Page.Keyboard.PressAsync("PageDown");
        await Page.WaitForTimeoutAsync(100);
        var afterForward = await heading.TextContentAsync();
        
        // Then go back
        await Page.Keyboard.PressAsync("PageUp");
        await Page.WaitForTimeoutAsync(100);
        var afterBack = await heading.TextContentAsync();
        
        await Assert.That(afterBack).IsNotEqualTo(afterForward);
    }

    [Test]
    public async Task ShiftPageDown_ShouldNavigate_ToNextYear()
    {
        var section = Page.GetByTestId("basic-section");
        var heading = section.Locator("[data-summit-calendar-heading]");
        var initialHeading = await heading.TextContentAsync();
        
        await FocusCalendarDayAsync(section);
        await Page.Keyboard.PressAsync("Shift+PageDown");
        
        await Page.WaitForTimeoutAsync(100);
        
        var newHeading = await heading.TextContentAsync();
        await Assert.That(newHeading).IsNotEqualTo(initialHeading);
        
        // The year should be different
        if (initialHeading!.Contains("2026"))
        {
            await Assert.That(newHeading).Contains("2027");
        }
    }

    [Test]
    public async Task ShiftPageUp_ShouldNavigate_ToPreviousYear()
    {
        var section = Page.GetByTestId("basic-section");
        var heading = section.Locator("[data-summit-calendar-heading]");
        
        await FocusCalendarDayAsync(section);
        
        // Go forward a year first
        await Page.Keyboard.PressAsync("Shift+PageDown");
        await Page.WaitForTimeoutAsync(100);
        var afterForward = await heading.TextContentAsync();
        
        // Then go back a year
        await Page.Keyboard.PressAsync("Shift+PageUp");
        await Page.WaitForTimeoutAsync(100);
        var afterBack = await heading.TextContentAsync();
        
        await Assert.That(afterBack).IsNotEqualTo(afterForward);
    }

    #endregion

    #region Selection with Enter/Space

    [Test]
    public async Task Enter_ShouldSelect_FocusedDate()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        // Navigate to a date
        await Page.Keyboard.PressAsync("ArrowRight");
        await Page.Keyboard.PressAsync("ArrowRight");
        
        await Page.Keyboard.PressAsync("Enter");
        
        // The focused date should now be selected
        var selectedDay = section.Locator("[data-summit-calendar-day][data-state='selected']");
        await Expect(selectedDay).ToBeVisibleAsync();
    }

    [Test]
    public async Task Space_ShouldSelect_FocusedDate()
    {
        var section = Page.GetByTestId("fixed-weeks-section");
        await FocusCalendarDayAsync(section);
        
        // Navigate to a date
        await Page.Keyboard.PressAsync("ArrowRight");
        
        await Page.Keyboard.PressAsync(" ");
        
        // The focused date should now be selected
        var selectedDay = section.Locator("[data-summit-calendar-day][data-state='selected']");
        await Expect(selectedDay).ToBeVisibleAsync();
    }

    [Test]
    public async Task Enter_ShouldKeepFocus_OnSelectedDate()
    {
        // Regression test: After navigating with arrows and pressing Enter,
        // focus should stay on the selected date, not jump back to a different date
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);
        
        // Get the initially focused date (should be 2026-01-07 based on test setup)
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var initialDate = await focusedDay.GetAttributeAsync("data-date");
        var initialDateParsed = DateOnly.Parse(initialDate!);
        
        // Navigate: ArrowDown (1 week = +7 days), then ArrowRight twice (+2 days)
        // Expected: 2026-01-07 + 7 + 2 = 2026-01-16
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowRight");
        await Page.Keyboard.PressAsync("ArrowRight");
        
        // Verify we're on the expected date before pressing Enter
        focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var beforeEnterDate = await focusedDay.GetAttributeAsync("data-date");
        var expectedDate = initialDateParsed.AddDays(9); // 7 + 2 = 9 days
        await Assert.That(beforeEnterDate).IsEqualTo(expectedDate.ToString("yyyy-MM-dd"));
        
        // Press Enter to select
        await Page.Keyboard.PressAsync("Enter");
        
        // After Enter, focus should stay on the same date (not jump back)
        focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        var afterEnterDate = await focusedDay.GetAttributeAsync("data-date");
        await Assert.That(afterEnterDate).IsEqualTo(beforeEnterDate);
        
        // And it should also be selected
        var selectedDay = section.Locator("[data-summit-calendar-day][data-state='selected']");
        var selectedDate = await selectedDay.GetAttributeAsync("data-date");
        await Assert.That(selectedDate).IsEqualTo(beforeEnterDate);
    }

    #endregion
}
