namespace SummitUI.Tests.Playwright.Popover;

/// <summary>
/// Tests for Popover Overlay and TrapFocus behavior.
/// </summary>
public class PopoverOverlayTests : SummitTestBase
{
    protected override string TestPagePath => "tests/popover/overlay";

    [Test]
    public async Task Content_ShouldHave_AriaModalTrue_WhenTrapFocus()
    {
        var trigger = Page.GetByTestId("overlay-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("overlay-content");
        await Expect(content).ToHaveAttributeAsync("aria-modal", "true");
    }

    [Test]
    public async Task Content_ShouldHave_AriaModalFalse_WhenNoTrapFocus()
    {
        var trigger = Page.GetByTestId("no-trap-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("no-trap-content");
        await Expect(content).ToHaveAttributeAsync("aria-modal", "false");
    }

    [Test]
    public async Task Overlay_ShouldHave_AriaHiddenTrue()
    {
        var trigger = Page.GetByTestId("overlay-trigger");
        await trigger.ClickAsync();

        var overlay = Page.GetByTestId("popover-overlay");
        await Expect(overlay).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Test]
    public async Task Overlay_ShouldClose_PopoverOnClick()
    {
        var trigger = Page.GetByTestId("overlay-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("overlay-content");
        await Expect(content).ToBeVisibleAsync();

        var overlay = Page.GetByTestId("popover-overlay");
        await overlay.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task PopoverWithTrapFocus_ShouldTrapFocusWithinContent()
    {
        var trigger = Page.GetByTestId("overlay-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("overlay-content");
        await Expect(content).ToBeVisibleAsync();

        var input = Page.GetByTestId("overlay-input");
        var closeButton = Page.GetByTestId("overlay-close");

        // Focus the input
        await input.FocusAsync();
        await Expect(input).ToBeFocusedAsync();

        // Tab to next element
        await Page.Keyboard.PressAsync("Tab");
        await Expect(closeButton).ToBeFocusedAsync();
        
        // Tab again should wrap to input
        await Page.Keyboard.PressAsync("Tab");
        await Expect(input).ToBeFocusedAsync();
    }
}
