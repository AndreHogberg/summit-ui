namespace SummitUI.Tests.Playwright.Tabs;

/// <summary>
/// Tests for Tab focus management.
/// </summary>
public class TabsFocusTests : SummitTestBase
{
    protected override string TestPagePath => "tests/tabs/basic";

    [Test]
    public async Task ActiveTabTrigger_ShouldReceiveFocus()
    {
        var activeTrigger = Page.GetByTestId("trigger-account");
        await activeTrigger.FocusAsync();

        await Expect(activeTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tab_ShouldMoveFocusToContent_AfterTrigger()
    {
        // Focus the active tab trigger
        var activeTrigger = Page.GetByTestId("trigger-account");
        await activeTrigger.FocusAsync();

        // Tab should move focus to the content panel
        await Page.Keyboard.PressAsync("Tab");

        var activeContent = Page.GetByTestId("content-account");
        await Expect(activeContent).ToBeFocusedAsync();
    }

    [Test]
    public async Task Content_ShouldBeFocusable()
    {
        var content = Page.GetByTestId("content-account");
        await content.FocusAsync();

        await Expect(content).ToBeFocusedAsync();
    }

    [Test]
    public async Task FocusShouldRemainOnNewTab_AfterActivation()
    {
        var accountTrigger = Page.GetByTestId("trigger-account");
        await accountTrigger.FocusAsync();

        // Navigate to and activate password tab
        await Page.Keyboard.PressAsync("ArrowRight");

        var passwordTrigger = Page.GetByTestId("trigger-password");
        await Expect(passwordTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task OnlyActiveTrigger_ShouldHaveTabIndexZero()
    {
        var accountTrigger = Page.GetByTestId("trigger-account");
        var passwordTrigger = Page.GetByTestId("trigger-password");
        var settingsTrigger = Page.GetByTestId("trigger-settings");

        // Active tab has tabindex 0
        await Expect(accountTrigger).ToHaveAttributeAsync("tabindex", "0");
        
        // Inactive tabs have tabindex -1
        await Expect(passwordTrigger).ToHaveAttributeAsync("tabindex", "-1");
        await Expect(settingsTrigger).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task TabIndex_ShouldUpdate_OnTabChange()
    {
        var accountTrigger = Page.GetByTestId("trigger-account");
        var passwordTrigger = Page.GetByTestId("trigger-password");

        // Initially account has tabindex 0
        await Expect(accountTrigger).ToHaveAttributeAsync("tabindex", "0");
        await Expect(passwordTrigger).ToHaveAttributeAsync("tabindex", "-1");

        // Click password tab
        await passwordTrigger.ClickAsync();

        // Now password has tabindex 0
        await Expect(passwordTrigger).ToHaveAttributeAsync("tabindex", "0");
        await Expect(accountTrigger).ToHaveAttributeAsync("tabindex", "-1");
    }
}
