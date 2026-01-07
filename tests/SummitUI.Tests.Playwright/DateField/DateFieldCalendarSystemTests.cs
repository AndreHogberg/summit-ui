using Microsoft.Playwright;

using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DateField;

public class DateFieldCalendarSystemTests : SummitTestBase
{
    protected override string TestPagePath => "tests/date-field/calendar-systems";

    [Test]
    public async Task GregorianCalendar_ShouldDisplay_GregorianDate()
    {
        var section = Page.GetByTestId("gregorian-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // 2025-01-07 in Gregorian
        await Expect(yearSegment).ToHaveTextAsync("2025");
        await Expect(monthSegment).ToHaveTextAsync("01");
        await Expect(daySegment).ToHaveTextAsync("07");
    }

    [Test]
    public async Task BuddhistCalendar_ShouldDisplay_BuddhistYear()
    {
        var section = Page.GetByTestId("buddhist-section");
        var yearSegment = section.Locator("[data-segment='year']");
        
        // Buddhist year = Gregorian + 543, so 2025 = 2568
        // Use Expect with auto-waiting for async calendar info loading
        await Expect(yearSegment).ToHaveTextAsync("2568");
    }

    [Test]
    public async Task JapaneseCalendar_ShouldDisplay_EraYear()
    {
        var section = Page.GetByTestId("japanese-section");
        var yearSegment = section.Locator("[data-segment='year']");

        // Japanese era year (Reiwa started 2019, so 2025 = Reiwa 7)
        // Use Expect with auto-waiting for async calendar info loading
        await Expect(yearSegment).ToHaveTextAsync("0007");
    }

    [Test]
    public async Task PersianCalendar_ShouldDisplay_PersianDate()
    {
        var section = Page.GetByTestId("persian-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");

        // 2025-01-07 Gregorian = 1403-10-18 Persian (approximately)
        // Use Expect with auto-waiting for async calendar info loading
        await Expect(yearSegment).ToHaveTextAsync("1403");
        await Expect(monthSegment).ToHaveTextAsync("10");
        await Expect(daySegment).ToHaveTextAsync("18");
    }

    [Test]
    public async Task CalendarSegment_ShouldHave_CalendarAwareAriaValues()
    {
        var section = Page.GetByTestId("buddhist-section");
        var yearSegment = section.Locator("[data-segment='year']");

        // Wait for calendar info to load by checking the text first
        await Expect(yearSegment).ToHaveTextAsync("2568");
        
        // aria-valuenow should reflect the calendar year (2568 for Buddhist)
        await Expect(yearSegment).ToHaveAttributeAsync("aria-valuenow", "2568");
    }

    [Test]
    public async Task CalendarSystem_ShouldHave_ProperMonthMax()
    {
        // Hebrew calendar can have 13 months in a leap year
        var section = Page.GetByTestId("hebrew-section");
        var monthSegment = section.Locator("[data-segment='month']");
        
        // Wait for calendar info to load - we can't easily predict the exact month
        // but we can wait for it to have any aria-valuemax attribute
        await Expect(monthSegment).ToHaveAttributeAsync("aria-valuemax", new System.Text.RegularExpressions.Regex("^1[23]$"));
    }

    [Test]
    public async Task AutoDetectFormat_ShouldWork_WhenNoFormatProvided()
    {
        var section = Page.GetByTestId("auto-format-section");
        var segments = section.Locator("[data-segment]:not([data-segment='literal'])");
        
        // Should have year, month, day segments
        await Assert.That(await segments.CountAsync()).IsEqualTo(3);
        
        // Each segment should have a value
        var yearSegment = section.Locator("[data-segment='year']");
        await Expect(yearSegment).ToHaveTextAsync("2025");
    }

    [Test]
    public async Task DateTimeWithCalendar_ShouldWork()
    {
        var section = Page.GetByTestId("datetime-calendar-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var hourSegment = section.Locator("[data-segment='hour']");
        var minuteSegment = section.Locator("[data-segment='minute']");

        // Year should be Persian calendar year (with auto-wait)
        await Expect(yearSegment).ToHaveTextAsync("1403");
        
        // Time should still be in 24-hour format (14:30)
        await Expect(hourSegment).ToHaveTextAsync("14");
        await Expect(minuteSegment).ToHaveTextAsync("30");
    }

    [Test]
    public async Task ValueBinding_ShouldRemain_Gregorian()
    {
        // After interacting with the Persian calendar DateField,
        // the bound value should still be Gregorian
        var valueDisplay = Page.GetByTestId("datetime-value");
        
        // The displayed value should show Gregorian date
        await Expect(valueDisplay).ToContainTextAsync("2025-01-07");
    }
}
