namespace SummitUI.Tests.Playwright.DatePicker;

/// <summary>
/// Tests for DatePicker date selection behavior.
/// </summary>
public class DatePickerSelectionTests : SummitTestBase
{
    protected override string TestPagePath => "tests/datepicker/basic";

    [Test]
    public async Task SelectingDate_ShouldUpdate_BoundValue()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        // Select a day (first available day in the current month)
        var dayButton = content.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").First;
        var expectedDate = await dayButton.GetAttributeAsync("data-date");
        await dayButton.ClickAsync();

        // Verify value was updated
        var valueDisplay = Page.GetByTestId("basic-value");
        var displayText = await valueDisplay.TextContentAsync();
        await Assert.That(displayText).Contains(expectedDate!);
    }

    [Test]
    public async Task SelectingDate_ShouldClose_PopoverByDefault()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        // Select a day
        var dayButton = content.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").First;
        await dayButton.ClickAsync();

        // Popover should close (CloseOnSelect defaults to true)
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task SelectingDate_ShouldNotClose_WhenCloseOnSelectIsFalse()
    {
        var trigger = Page.GetByTestId("no-close-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("no-close-content");
        await Expect(content).ToBeVisibleAsync();

        // Select a day
        var dayButton = content.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").First;
        await dayButton.ClickAsync();

        // Popover should remain open
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task SelectingDate_ShouldUpdate_ValueDisplayWithCloseOnSelectFalse()
    {
        var trigger = Page.GetByTestId("no-close-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("no-close-content");
        await Expect(content).ToBeVisibleAsync();

        // Select first day
        var dayButtons = content.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])");
        var firstDay = dayButtons.Nth(5);
        var expectedDate1 = await firstDay.GetAttributeAsync("data-date");
        await firstDay.ClickAsync();

        // Verify value was updated
        var valueDisplay = Page.GetByTestId("no-close-value");
        var displayText = await valueDisplay.TextContentAsync();
        await Assert.That(displayText).Contains(expectedDate1!);

        // Select different day
        var secondDay = dayButtons.Nth(10);
        var expectedDate2 = await secondDay.GetAttributeAsync("data-date");
        await secondDay.ClickAsync();

        // Verify value was updated again
        displayText = await valueDisplay.TextContentAsync();
        await Assert.That(displayText).Contains(expectedDate2!);
    }

    [Test]
    public async Task SelectedDate_ShouldShow_SelectedState()
    {
        // Use no-close section so popover stays open after selection
        var trigger = Page.GetByTestId("no-close-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("no-close-content");
        await Expect(content).ToBeVisibleAsync();

        var dayButton = content.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").Nth(5);

        await dayButton.ClickAsync();
        await Expect(dayButton).ToHaveAttributeAsync("data-state", "selected");
    }

    [Test]
    public async Task UnavailableDate_ShouldNot_BeSelectable()
    {
        var trigger = Page.GetByTestId("minmax-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("minmax-content");
        await Expect(content).ToBeVisibleAsync();

        var unavailableButton = content.Locator("[data-summit-calendar-day][data-unavailable]").First;

        var count = await unavailableButton.CountAsync();
        if (count > 0)
        {
            await unavailableButton.ClickAsync(new() { Force = true });

            // Should not be selected
            var state = await unavailableButton.GetAttributeAsync("data-state");
            await Assert.That(state).IsNotEqualTo("selected");
        }
    }

    [Test]
    public async Task SelectingDate_ShouldUpdate_FieldSegments_WhenFieldHasNoExplicitBinding()
    {
        // This test verifies that DatePickerField syncs with DatePickerRoot value
        // when no explicit @bind-Value is provided on the field
        var trigger = Page.GetByTestId("field-sync-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("field-sync-content");
        await Expect(content).ToBeVisibleAsync();

        // Get the field input container to check segment values
        var fieldInput = Page.GetByTestId("field-sync-input");

        // Initially, segments should show placeholders (no value selected)
        var initialSegments = fieldInput.Locator("[data-segment]:not([data-segment='literal'])");
        var initialFirstSegment = initialSegments.First;
        var initialPlaceholder = await initialFirstSegment.GetAttributeAsync("data-placeholder");
        await Assert.That(initialPlaceholder).IsNotNull();

        // Select a day from the calendar
        var dayButton = content.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").Nth(10);
        var expectedDate = await dayButton.GetAttributeAsync("data-date");
        await dayButton.ClickAsync();

        // Verify the value display was updated (this works via root binding)
        var valueDisplay = Page.GetByTestId("field-sync-value");
        var displayText = await valueDisplay.TextContentAsync();
        await Assert.That(displayText).Contains(expectedDate!);

        // Verify that the field segments now show actual values (not placeholders)
        // After selection, segments should no longer have data-placeholder attribute
        var segments = fieldInput.Locator("[data-segment]:not([data-segment='literal'])");
        var firstSegment = segments.First;

        // The segment should now have a value and not be in placeholder state
        var placeholder = await firstSegment.GetAttributeAsync("data-placeholder");
        await Assert.That(placeholder).IsNull();
    }
}
