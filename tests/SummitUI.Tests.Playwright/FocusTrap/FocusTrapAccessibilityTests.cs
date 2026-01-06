using Microsoft.Playwright;
using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.FocusTrap;

public class FocusTrapAccessibilityTests : SummitTestBase
{
    protected override string TestPagePath => "tests/focus-trap/basic";

    [Test]
    public async Task ZeroItems_ShouldTrapFocusInContainer()
    {
        var openButton = Page.GetByTestId("open-zero-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("zero-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        await Page.WaitForTimeoutAsync(200);

        // Focus should be within the trap
        var focusTrapContainer = Page.Locator("[data-summit-focus-trap]");
        var isFocusInTrap = await focusTrapContainer.EvaluateAsync<bool>(
            "el => el.contains(document.activeElement) || el === document.activeElement");

        await Assert.That(isFocusInTrap).IsTrue();
        
        // Tab should not escape
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.GetByTestId("external-button")).Not.ToBeFocusedAsync();
    }

    [Test]
    public async Task OneItem_ShouldAutoFocusAndCycle()
    {
        await Page.GetByTestId("open-one-item").ClickAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await Expect(closeButton).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Tab");
        await Expect(closeButton).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(closeButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task TwoItems_ShouldCycleBetweenButtons()
    {
        await Page.GetByTestId("open-two-items").ClickAsync();

        var cancel = Page.GetByTestId("two-items-cancel");
        var confirm = Page.GetByTestId("two-items-confirm");

        await Expect(cancel).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Tab");
        await Expect(confirm).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancel).ToBeFocusedAsync();
    }

    [Test]
    public async Task MultipleItems_ShouldTabThroughAll()
    {
        await Page.GetByTestId("open-multiple-items").ClickAsync();

        var name = Page.GetByTestId("multiple-items-input");
        var email = Page.GetByTestId("multiple-items-email");
        var cancel = Page.GetByTestId("multiple-items-cancel");
        var submit = Page.GetByTestId("multiple-items-submit");

        await Expect(name).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(email).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancel).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(submit).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(name).ToBeFocusedAsync();
    }

    [Test]
    public async Task FocusTrap_ShouldReturnFocusToTrigger()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        await Page.GetByTestId("one-item-close").ClickAsync();
        await Expect(openButton).ToBeFocusedAsync();
    }
}
