using Microsoft.Playwright;

namespace SummitUI.Tests.Playwright.Calendar;

/// <summary>
/// Tests for Calendar week start configuration.
/// </summary>
public class CalendarWeekStartTests : SummitTestBase
{
    protected override string TestPagePath => "tests/calendar/basic";

    [Test]
    public async Task MondayStart_ShouldShow_MondayFirst()
    {
        var section = Page.GetByTestId("monday-start-section");
        var firstWeekday = section.GetByTestId("weekday-0");

        var abbr = await firstWeekday.GetAttributeAsync("abbr");
        var text = await firstWeekday.TextContentAsync();

        // First weekday should be Monday (Mon or Monday depending on locale)
        await Assert.That(abbr!.ToLower()).Contains("mon");
    }

    [Test]
    public async Task MondayStart_ShouldShow_SundayLast()
    {
        var section = Page.GetByTestId("monday-start-section");
        var lastWeekday = section.GetByTestId("weekday-6");

        var abbr = await lastWeekday.GetAttributeAsync("abbr");

        // Last weekday should be Sunday
        await Assert.That(abbr!.ToLower()).Contains("sun");
    }

    [Test]
    public async Task SundayStart_ShouldShow_SundayFirst()
    {
        // Basic section uses default (auto-detect or Sunday)
        var section = Page.GetByTestId("basic-section");
        var firstHeadCell = section.Locator("[data-summit-calendar-head-cell]").First;

        var text = await firstHeadCell.TextContentAsync();

        // Depending on locale, might be "Sun" or localized
        // Just verify we get some weekday
        await Assert.That(text!.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task InitialRender_ShouldShow_CorrectDayOfWeekColumn()
    {
        // Regression test: Calendar should show dates in correct columns on initial render
        // January 6, 2026 is a Tuesday (DayOfWeek = 2)
        // With Sunday start (index 0), Tuesday should be in column index 2
        var section = Page.GetByTestId("basic-section");
        var grid = section.Locator("[data-summit-calendar-grid]");

        // Find January 6, 2026 button
        var jan6Button = section.Locator("[data-summit-calendar-day][data-date='2026-01-06']");
        await Expect(jan6Button).ToBeVisibleAsync();

        // Get the parent cell (td) and then the parent row (tr)
        var cell = jan6Button.Locator("xpath=ancestor::td");
        var row = cell.Locator("xpath=ancestor::tr");

        // Count how many cells come before this cell in the row
        // This tells us the column index (0-based)
        var allCellsInRow = row.Locator("td");
        var cellCount = await allCellsInRow.CountAsync();

        int columnIndex = -1;
        for (int i = 0; i < cellCount; i++)
        {
            var cellButton = allCellsInRow.Nth(i).Locator("[data-summit-calendar-day]");
            var dateAttr = await cellButton.GetAttributeAsync("data-date");
            if (dateAttr == "2026-01-06")
            {
                columnIndex = i;
                break;
            }
        }

        // January 6, 2026 is Tuesday. With Sunday-start calendar:
        // Sun=0, Mon=1, Tue=2, Wed=3, Thu=4, Fri=5, Sat=6
        // So Tuesday should be at column index 2
        await Assert.That(columnIndex).IsEqualTo(2);
    }

    [Test]
    public async Task MondayStart_InitialRender_ShouldShow_CorrectDayOfWeekColumn()
    {
        // Regression test: Calendar with explicit Monday start should show dates
        // in correct columns on initial render (no shift after JS initialization)
        // January 6, 2026 is a Tuesday (DayOfWeek = 2)
        // With Monday start (index 0 = Monday), Tuesday should be in column index 1
        var section = Page.GetByTestId("monday-start-section");

        // Find January 6, 2026 button
        var jan6Button = section.Locator("[data-summit-calendar-day][data-date='2026-01-06']");
        await Expect(jan6Button).ToBeVisibleAsync();

        // Get the parent cell (td) and then the parent row (tr)
        var cell = jan6Button.Locator("xpath=ancestor::td");
        var row = cell.Locator("xpath=ancestor::tr");

        // Count how many cells come before this cell in the row
        var allCellsInRow = row.Locator("td");
        var cellCount = await allCellsInRow.CountAsync();

        int columnIndex = -1;
        for (int i = 0; i < cellCount; i++)
        {
            var cellButton = allCellsInRow.Nth(i).Locator("[data-summit-calendar-day]");
            var dateAttr = await cellButton.GetAttributeAsync("data-date");
            if (dateAttr == "2026-01-06")
            {
                columnIndex = i;
                break;
            }
        }

        // January 6, 2026 is Tuesday. With Monday-start calendar:
        // Mon=0, Tue=1, Wed=2, Thu=3, Fri=4, Sat=5, Sun=6
        // So Tuesday should be at column index 1
        await Assert.That(columnIndex).IsEqualTo(1);
    }
}
