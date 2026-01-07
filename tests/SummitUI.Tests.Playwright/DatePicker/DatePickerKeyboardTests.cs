namespace SummitUI.Tests.Playwright.DatePicker;

/// <summary>
/// Tests for DatePicker keyboard interaction.
/// </summary>
public class DatePickerKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/datepicker/basic";

    [Test]
    public async Task Trigger_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Escape_ShouldClose_AndReturnFocusToTrigger()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowKeys_ShouldNavigate_CalendarDays()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        // Get a day and focus it
        var dayButton = content.Locator("[data-summit-calendar-day]:not([data-unavailable]):not([data-outside-month])").First;
        await dayButton.FocusAsync();

        var initialDate = await dayButton.GetAttributeAsync("data-date");

        // Press right arrow
        await Page.Keyboard.PressAsync("ArrowRight");

        // Focus should have moved
        var focusedDay = content.Locator("[data-summit-calendar-day][data-focused]");
        var focusedDate = await focusedDay.GetAttributeAsync("data-date");

        await Assert.That(focusedDate).IsNotEqualTo(initialDate);
    }

    [Test]
    public async Task Enter_ShouldSelect_FocusedDay()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        // Find the currently focused day (determined by the component)
        var focusedDay = content.Locator("[data-summit-calendar-day][data-focused]");
        await Expect(focusedDay).ToBeVisibleAsync();
        var expectedDate = await focusedDay.GetAttributeAsync("data-date");

        // Ensure the focused day has browser focus
        await focusedDay.FocusAsync();

        // Press Enter to select
        await Page.Keyboard.PressAsync("Enter");

        // Verify value was updated and popover closed
        var valueDisplay = Page.GetByTestId("basic-value");
        var displayText = await valueDisplay.TextContentAsync();
        await Assert.That(displayText).Contains(expectedDate!);
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Space_ShouldSelect_FocusedDay()
    {
        var trigger = Page.GetByTestId("no-close-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("no-close-content");
        await Expect(content).ToBeVisibleAsync();

        // Find the currently focused day (determined by the component)
        var focusedDay = content.Locator("[data-summit-calendar-day][data-focused]");
        await Expect(focusedDay).ToBeVisibleAsync();
        var expectedDate = await focusedDay.GetAttributeAsync("data-date");

        // Ensure the focused day has browser focus
        await focusedDay.FocusAsync();

        // Press Space to select
        await Page.Keyboard.PressAsync(" ");

        // Verify value was updated (popover stays open due to CloseOnSelect=false)
        var valueDisplay = Page.GetByTestId("no-close-value");
        var displayText = await valueDisplay.TextContentAsync();
        await Assert.That(displayText).Contains(expectedDate!);
        await Expect(content).ToBeVisibleAsync();
    }
}
