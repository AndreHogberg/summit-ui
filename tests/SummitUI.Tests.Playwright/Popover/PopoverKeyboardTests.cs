namespace SummitUI.Tests.Playwright.Popover;

/// <summary>
/// Tests for Popover keyboard interaction.
/// </summary>
public class PopoverKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/popover/keyboard";

    [Test]
    public async Task Popover_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.GetByTestId("keyboard-trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("keyboard-content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Popover_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.GetByTestId("keyboard-trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.GetByTestId("keyboard-content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Popover_ShouldClose_OnEscapeKey()
    {
        var trigger = Page.GetByTestId("keyboard-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("keyboard-content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }
}
