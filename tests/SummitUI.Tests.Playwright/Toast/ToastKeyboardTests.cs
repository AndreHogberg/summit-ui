namespace SummitUI.Tests.Playwright.Toast;

/// <summary>
/// Tests for Toast keyboard interactions and hotkey functionality.
/// </summary>
public class ToastKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/toast/accessibility";

    [Test]
    public async Task Hotkey_ShouldFocus_Viewport()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        // Press the hotkey (F8)
        await Page.Keyboard.PressAsync("F8");
        
        var viewport = Page.GetByTestId("toast-viewport");
        await Expect(viewport).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tab_ShouldNavigate_ThroughToastElements()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        // Focus the viewport first
        await Page.Keyboard.PressAsync("F8");
        
        // Tab to action button
        await Page.Keyboard.PressAsync("Tab");
        var action = Page.GetByTestId("foreground-action");
        await Expect(action).ToBeFocusedAsync();
        
        // Tab to close button
        await Page.Keyboard.PressAsync("Tab");
        var close = Page.GetByTestId("foreground-close");
        await Expect(close).ToBeFocusedAsync();
    }

    [Test]
    public async Task Enter_ShouldActivate_ActionButton()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var action = Page.GetByTestId("foreground-action");
        await action.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        
        // Toast should close after action
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToBeHiddenAsync();
    }

    [Test]
    public async Task Space_ShouldActivate_CloseButton()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var close = Page.GetByTestId("foreground-close");
        await close.FocusAsync();
        await Page.Keyboard.PressAsync("Space");
        
        // Toast should close
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToBeHiddenAsync();
    }
}
