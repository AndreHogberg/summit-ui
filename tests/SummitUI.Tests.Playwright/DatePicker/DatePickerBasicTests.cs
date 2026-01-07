namespace SummitUI.Tests.Playwright.DatePicker;

/// <summary>
/// Tests for DatePicker basic trigger and popover behavior.
/// </summary>
public class DatePickerBasicTests : SummitTestBase
{
    protected override string TestPagePath => "tests/datepicker/basic";

    [Test]
    public async Task Trigger_ShouldOpen_Popover()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldClose_OpenPopover()
    {
        var trigger = Page.GetByTestId("basic-trigger");

        // Open
        await trigger.ClickAsync();
        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        // Close by clicking trigger again
        await trigger.ClickAsync();
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Escape_ShouldClose_Popover()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task OutsideClick_ShouldClose_Popover()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToBeVisibleAsync();

        // Click outside the popover
        await Page.Locator("h1").ClickAsync();
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Disabled_Trigger_ShouldNot_OpenPopover()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await trigger.ClickAsync(new() { Force = true });

        // Content should not be visible (no content with disabled-content testid, 
        // but we can check it's not rendered)
        var section = Page.GetByTestId("disabled-section");
        var content = section.Locator("[data-summit-popover-content]");
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Disabled_Trigger_ShouldHave_DisabledAttribute()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await Expect(trigger).ToBeDisabledAsync();
    }

    [Test]
    public async Task ControlledOpen_ShouldWork_WithExternalToggle()
    {
        var content = Page.GetByTestId("controlled-content");
        var openDisplay = Page.GetByTestId("controlled-open");
        var externalToggle = Page.GetByTestId("external-toggle");

        // Initially closed
        await Expect(content).Not.ToBeVisibleAsync();
        await Assert.That(await openDisplay.TextContentAsync()).Contains("False");

        // Click external toggle to open
        await externalToggle.ClickAsync();
        await Expect(content).ToBeVisibleAsync();
        await Assert.That(await openDisplay.TextContentAsync()).Contains("True");

        // Click external toggle to close
        await externalToggle.ClickAsync();
        // First verify the display updates (confirms parent state changed)
        await Expect(openDisplay).ToContainTextAsync("False");
        // Then verify content becomes hidden
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldUpdate_ControlledState()
    {
        var trigger = Page.GetByTestId("controlled-trigger");
        var openDisplay = Page.GetByTestId("controlled-open");

        // Initially closed
        await Assert.That(await openDisplay.TextContentAsync()).Contains("False");

        // Click trigger to open
        await trigger.ClickAsync();
        await Assert.That(await openDisplay.TextContentAsync()).Contains("True");
    }
}
