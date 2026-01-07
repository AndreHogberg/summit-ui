using Microsoft.Playwright;

namespace SummitUI.Tests.Playwright.Calendar;

/// <summary>
/// Tests for Calendar selection behavior.
/// </summary>
public class CalendarSelectionTests : SummitTestBase
{
    protected override string TestPagePath => "tests/calendar/basic";

    [Test]
    public async Task Click_ShouldSelect_Date()
    {
        var section = Page.GetByTestId("basic-section");
        var dayButton = section.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").First;

        await dayButton.ClickAsync();

        await Expect(dayButton).ToHaveAttributeAsync("data-state", "selected");
    }

    [Test]
    public async Task Selection_ShouldUpdate_BoundValue()
    {
        var section = Page.GetByTestId("basic-section");
        var dayButton = section.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").First;
        var expectedDate = await dayButton.GetAttributeAsync("data-date");

        await dayButton.ClickAsync();

        var valueDisplay = Page.GetByTestId("basic-value");
        var displayText = await valueDisplay.TextContentAsync();
        await Assert.That(displayText).Contains(expectedDate!);
    }

    [Test]
    public async Task Selection_ShouldClear_PreviousSelection()
    {
        var section = Page.GetByTestId("basic-section");
        var dayButtons = section.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])");

        // Select first date
        var firstButton = dayButtons.Nth(5);
        await firstButton.ClickAsync();
        await Expect(firstButton).ToHaveAttributeAsync("data-state", "selected");

        // Select second date
        var secondButton = dayButtons.Nth(10);
        await secondButton.ClickAsync();

        // First should no longer be selected
        var firstState = await firstButton.GetAttributeAsync("data-state");
        await Assert.That(firstState).IsNotEqualTo("selected");

        // Second should be selected
        await Expect(secondButton).ToHaveAttributeAsync("data-state", "selected");
    }

    [Test]
    public async Task UnavailableDate_ShouldNot_BeSelectable()
    {
        var section = Page.GetByTestId("minmax-section");
        var unavailableButton = section.Locator("[data-summit-calendar-day][data-unavailable]").First;

        var count = await unavailableButton.CountAsync();
        if (count > 0)
        {
            await unavailableButton.ClickAsync(new() { Force = true });

            // Should still not be selected
            var state = await unavailableButton.GetAttributeAsync("data-state");
            await Assert.That(state).IsNotEqualTo("selected");
        }
    }

    [Test]
    public async Task OutsideMonthDate_CanBe_Selected()
    {
        var section = Page.GetByTestId("basic-section");
        var outsideButton = section.Locator("[data-summit-calendar-day][data-outside-month]").First;

        var count = await outsideButton.CountAsync();
        if (count > 0)
        {
            await outsideButton.ClickAsync();

            // Outside month dates can be selected
            await Expect(outsideButton).ToHaveAttributeAsync("data-state", "selected");
        }
    }
}
