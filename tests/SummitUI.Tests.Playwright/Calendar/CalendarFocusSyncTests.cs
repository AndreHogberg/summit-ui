using Microsoft.Playwright;

namespace SummitUI.Tests.Playwright.Calendar;

/// <summary>
/// Tests for Calendar focus synchronization between DOM focus and data-focused attribute.
/// These tests verify that keyboard navigation correctly moves both the logical focus
/// (data-focused) and the actual DOM focus (:focus) to the same element.
/// </summary>
public class CalendarFocusSyncTests : SummitTestBase
{
    protected override string TestPagePath => "tests/calendar/basic";

    /// <summary>
    /// Focuses the day button that is in the tab order (the one with tabindex="0").
    /// </summary>
    private async Task FocusCalendarDayAsync(ILocator section)
    {
        var focusableDay = section.Locator("[data-summit-calendar-day][tabindex='0']");
        await focusableDay.FocusAsync();
    }

    /// <summary>
    /// Gets the date string of the element that currently has DOM focus,
    /// along with the test section it's in.
    /// </summary>
    private async Task<(string? date, string? section)> GetDomFocusedDateWithSectionAsync()
    {
        var result = await Page.EvaluateAsync<Dictionary<string, string?>>(@"() => {
            const el = document.activeElement;
            if (el && el.hasAttribute('data-summit-calendar-day')) {
                const date = el.getAttribute('data-date');
                const sectionEl = el.closest('[data-testid]');
                const section = sectionEl ? sectionEl.getAttribute('data-testid') : 'no-section';
                return { date: date, section: section };
            }
            return { date: null, section: null };
        }");
        return (result?.GetValueOrDefault("date"), result?.GetValueOrDefault("section"));
    }

    /// <summary>
    /// Gets the date string of the element that currently has DOM focus.
    /// </summary>
    private async Task<string?> GetDomFocusedDateAsync()
    {
        return await Page.EvaluateAsync<string?>(@"() => {
            const el = document.activeElement;
            if (el && el.hasAttribute('data-summit-calendar-day')) {
                return el.getAttribute('data-date');
            }
            return null;
        }");
    }

    /// <summary>
    /// Gets the date string of the element that has the data-focused attribute.
    /// Waits for the element to be present with a short timeout.
    /// </summary>
    private async Task<string?> GetDataFocusedDateAsync(ILocator section, int timeoutMs = 1000)
    {
        var focusedDay = section.Locator("[data-summit-calendar-day][data-focused]");
        
        try
        {
            // Wait for the focused day to be visible
            await focusedDay.First.WaitForAsync(new() { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            return null;
        }
        
        var count = await focusedDay.CountAsync();
        if (count == 0) return null;
        if (count > 1)
        {
            // Multiple elements have data-focused - this is a bug
            throw new InvalidOperationException($"Multiple elements ({count}) have data-focused attribute");
        }
        return await focusedDay.GetAttributeAsync("data-date");
    }

    /// <summary>
    /// Verifies that DOM focus and data-focused attribute are on the same element.
    /// </summary>
    private async Task AssertFocusSyncedAsync(ILocator section, string expectedDate)
    {
        var domFocusedDate = await GetDomFocusedDateAsync();
        var dataFocusedDate = await GetDataFocusedDateAsync(section);

        await Assert.That(domFocusedDate)
            .IsEqualTo(expectedDate)
            .Because("DOM focus should be on the expected date");

        await Assert.That(dataFocusedDate)
            .IsEqualTo(expectedDate)
            .Because("data-focused attribute should be on the expected date");

        await Assert.That(domFocusedDate)
            .IsEqualTo(dataFocusedDate)
            .Because("DOM focus and data-focused should be on the same element");
    }

    #region Cross-Month Navigation Focus Sync

    [Test]
    public async Task ArrowDown_CrossingMonthBoundary_ShouldSyncFocus()
    {
        // This test navigates from a date in one month to a date in the next month
        // and verifies that both DOM focus and data-focused are synchronized

        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);

        // Get initial state
        var initialDataFocused = await GetDataFocusedDateAsync(section);
        var initialDomFocused = await GetDomFocusedDateAsync();

        // They should start in sync
        await Assert.That(initialDomFocused).IsEqualTo(initialDataFocused);

        // Navigate down several weeks to cross month boundary
        // We'll keep going until we cross into a new month
        var currentDate = DateOnly.Parse(initialDataFocused!);
        var initialMonth = currentDate.Month;
        var iterations = 0;
        const int maxIterations = 6; // Max 6 weeks to cross a month

        while (iterations < maxIterations)
        {
            await Page.Keyboard.PressAsync("ArrowDown");
            await Page.WaitForTimeoutAsync(100); // Wait for state update

            var newDataFocused = await GetDataFocusedDateAsync(section);
            
            // Debug: if no focused element, dump what we can find
            if (newDataFocused == null)
            {
                var allDays = await section.Locator("[data-summit-calendar-day]").CountAsync();
                var domFocus = await GetDomFocusedDateAsync();
                Console.WriteLine($"DEBUG: After ArrowDown #{iterations + 1}: data-focused=null, DOM focus={domFocus}, total days={allDays}");
                
                // Try waiting a bit more
                await Page.WaitForTimeoutAsync(500);
                newDataFocused = await GetDataFocusedDateAsync(section);
                Console.WriteLine($"DEBUG: After extra wait: data-focused={newDataFocused}");
            }
            
            if (newDataFocused == null)
            {
                throw new InvalidOperationException($"No data-focused element found after ArrowDown iteration {iterations + 1}");
            }
            
            currentDate = DateOnly.Parse(newDataFocused);
            iterations++;

            if (currentDate.Month != initialMonth)
            {
                // We've crossed the month boundary - this is the critical moment
                break;
            }
        }

        // Verify we actually crossed a month boundary
        await Assert.That(currentDate.Month).IsNotEqualTo(initialMonth)
            .Because("Test should have navigated to a different month");

        // Now verify focus is synced after crossing the month boundary
        var finalDataFocused = await GetDataFocusedDateAsync(section);
        var finalDomFocused = await GetDomFocusedDateAsync();

        await Assert.That(finalDomFocused).IsEqualTo(finalDataFocused)
            .Because("After crossing month boundary, DOM focus and data-focused should be synced");
    }

    [Test]
    public async Task ArrowUp_CrossingMonthBoundary_ShouldSyncFocus()
    {
        var section = Page.GetByTestId("basic-section");

        // Capture ALL console logs
        var consoleLogs = new List<string>();
        Page.Console += (_, msg) => consoleLogs.Add($"[{msg.Type}] {msg.Text}");

        // Also listen for all focus events at document level
        await Page.EvaluateAsync(@"() => {
            window.__focusEvents = [];
            document.addEventListener('focusin', (e) => {
                const date = e.target.getAttribute?.('data-date') || 'N/A';
                const tag = e.target.tagName;
                const id = e.target.id || 'no-id';
                window.__focusEvents.push(`FOCUSIN: ${tag}#${id} date=${date} at ${Date.now()}`);
            }, true);
            document.addEventListener('focusout', (e) => {
                const date = e.target.getAttribute?.('data-date') || 'N/A';
                const tag = e.target.tagName;
                const id = e.target.id || 'no-id';
                window.__focusEvents.push(`FOCUSOUT: ${tag}#${id} date=${date} at ${Date.now()}`);
            }, true);
        }");

        await FocusCalendarDayAsync(section);

        // First navigate forward into the month so we have room to go back
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.WaitForTimeoutAsync(100);

        var currentDataFocused = await GetDataFocusedDateAsync(section);
        var currentDate = DateOnly.Parse(currentDataFocused!);
        var currentMonth = currentDate.Month;

        Console.WriteLine($"Starting from: {currentDataFocused}, Month: {currentMonth}");

        // Navigate up until we cross into the previous month
        var iterations = 0;
        const int maxIterations = 6;

        while (iterations < maxIterations)
        {
            Console.WriteLine($"Pressing ArrowUp (iteration {iterations + 1})...");
            await Page.Keyboard.PressAsync("ArrowUp");
            // Give more time for Blazor to complete the render cycle and JS focus
            await Page.WaitForTimeoutAsync(150);

            var newDataFocused = await GetDataFocusedDateAsync(section);
            var newDomFocused = await GetDomFocusedDateAsync();
            Console.WriteLine($"  After ArrowUp: data-focused={newDataFocused}, DOM focus={newDomFocused}");

            currentDate = DateOnly.Parse(newDataFocused!);
            iterations++;

            if (currentDate.Month != currentMonth)
            {
                Console.WriteLine($"  Crossed month boundary! Now in month {currentDate.Month}");
                break;
            }
        }

        // Give extra time after the month change for focus to settle
        await Page.WaitForTimeoutAsync(200);

        // Get all focus events that happened
        var focusEvents = await Page.EvaluateAsync<string[]>("() => window.__focusEvents || []");
        Console.WriteLine("\n--- Focus Events ---");
        foreach (var evt in focusEvents)
        {
            Console.WriteLine(evt);
        }
        Console.WriteLine("--- End Focus Events ---\n");

        // Print console logs
        Console.WriteLine("--- Browser Console Logs ---");
        foreach (var log in consoleLogs.Where(l => l.Contains("SummitUI")))
        {
            Console.WriteLine(log);
        }
        Console.WriteLine("--- End Console Logs ---\n");

        // Verify focus is synced
        var finalDataFocused = await GetDataFocusedDateAsync(section);
        var finalDomFocused = await GetDomFocusedDateAsync();

        // Get more info about where DOM focus is
        var focusInfo = await Page.EvaluateAsync<string>(@"() => {
            const el = document.activeElement;
            if (!el) return 'No active element';
            const tag = el.tagName;
            const date = el.getAttribute?.('data-date') || 'no-date';
            const testId = el.closest?.('[data-testid]')?.getAttribute?.('data-testid') || 'no-section';
            const html = el.outerHTML?.substring(0, 200) || 'no-html';
            return `Tag: ${tag}, Date: ${date}, Section: ${testId}, HTML: ${html}`;
        }");

        Console.WriteLine($"Final: data-focused={finalDataFocused}, DOM focus={finalDomFocused}");
        Console.WriteLine($"Focus Info: {focusInfo}");

        await Assert.That(finalDomFocused).IsEqualTo(finalDataFocused)
            .Because("After crossing month boundary backwards, DOM focus and data-focused should be synced");
    }

    [Test]
    public async Task EnterAfterCrossMonthNavigation_ShouldSelectFocusedDate()
    {
        // This is the specific bug scenario: navigate across month boundary, then press Enter
        // The selected date should match the focused date (data-focused), not some other date

        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);

        // Navigate down until we cross a month boundary
        var initialDataFocused = await GetDataFocusedDateAsync(section);
        var currentDate = DateOnly.Parse(initialDataFocused!);
        var initialMonth = currentDate.Month;

        for (int i = 0; i < 6; i++)
        {
            await Page.Keyboard.PressAsync("ArrowDown");
            await Page.WaitForTimeoutAsync(50);

            var newDataFocused = await GetDataFocusedDateAsync(section);
            currentDate = DateOnly.Parse(newDataFocused!);

            if (currentDate.Month != initialMonth)
            {
                break;
            }
        }

        // Verify we crossed a month
        await Assert.That(currentDate.Month).IsNotEqualTo(initialMonth);

        // Get the focused date before pressing Enter
        var focusedBeforeEnter = await GetDataFocusedDateAsync(section);

        // Press Enter to select
        await Page.Keyboard.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(50);

        // The selected date should match what was focused
        var selectedDay = section.Locator("[data-summit-calendar-day][data-state='selected']");
        var selectedDate = await selectedDay.GetAttributeAsync("data-date");

        await Assert.That(selectedDate).IsEqualTo(focusedBeforeEnter)
            .Because("Enter should select the date that had data-focused, not some other date");
    }

    [Test]
    public async Task SpaceAfterCrossMonthNavigation_ShouldSelectFocusedDate()
    {
        // Same test as above but with Space key
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);

        var initialDataFocused = await GetDataFocusedDateAsync(section);
        var currentDate = DateOnly.Parse(initialDataFocused!);
        var initialMonth = currentDate.Month;

        for (int i = 0; i < 6; i++)
        {
            await Page.Keyboard.PressAsync("ArrowDown");
            await Page.WaitForTimeoutAsync(50);

            var newDataFocused = await GetDataFocusedDateAsync(section);
            currentDate = DateOnly.Parse(newDataFocused!);

            if (currentDate.Month != initialMonth)
            {
                break;
            }
        }

        var focusedBeforeSpace = await GetDataFocusedDateAsync(section);

        await Page.Keyboard.PressAsync(" ");
        await Page.WaitForTimeoutAsync(50);

        var selectedDay = section.Locator("[data-summit-calendar-day][data-state='selected']");
        var selectedDate = await selectedDay.GetAttributeAsync("data-date");

        await Assert.That(selectedDate).IsEqualTo(focusedBeforeSpace)
            .Because("Space should select the date that had data-focused");
    }

    #endregion

    #region Only One Focus Indicator

    [Test]
    public async Task OnlyOneElementShouldHaveDataFocused()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);

        // Navigate around
        await Page.Keyboard.PressAsync("ArrowRight");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowLeft");

        // Count elements with data-focused
        var focusedElements = section.Locator("[data-summit-calendar-day][data-focused]");
        var count = await focusedElements.CountAsync();

        await Assert.That(count).IsEqualTo(1)
            .Because("Only one calendar day should have data-focused attribute at a time");
    }

    [Test]
    public async Task AfterCrossMonthNavigation_OnlyOneElementShouldHaveDataFocused()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);

        // Navigate across month boundary
        for (int i = 0; i < 6; i++)
        {
            await Page.Keyboard.PressAsync("ArrowDown");
        }
        await Page.WaitForTimeoutAsync(50);

        // Count elements with data-focused
        var focusedElements = section.Locator("[data-summit-calendar-day][data-focused]");
        var count = await focusedElements.CountAsync();

        await Assert.That(count).IsEqualTo(1)
            .Because("After cross-month navigation, only one day should have data-focused");
    }

    #endregion

    #region Focus Ring Consistency

    [Test]
    public async Task DomFocusAndDataFocused_ShouldAlwaysMatch_DuringNavigation()
    {
        var section = Page.GetByTestId("basic-section");
        await FocusCalendarDayAsync(section);

        // Perform various navigation actions and verify sync after each
        var actions = new[] { "ArrowRight", "ArrowDown", "ArrowLeft", "ArrowUp", "ArrowDown", "ArrowDown" };

        foreach (var action in actions)
        {
            await Page.Keyboard.PressAsync(action);
            await Page.WaitForTimeoutAsync(50);

            var domFocused = await GetDomFocusedDateAsync();
            var dataFocused = await GetDataFocusedDateAsync(section);

            await Assert.That(domFocused).IsEqualTo(dataFocused)
                .Because($"After {action}, DOM focus and data-focused should match");
        }
    }

    #endregion
}
