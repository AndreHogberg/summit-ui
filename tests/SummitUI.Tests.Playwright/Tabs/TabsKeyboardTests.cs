namespace SummitUI.Tests.Playwright.Tabs;

/// <summary>
/// Tests for Tab keyboard navigation.
/// </summary>
public class TabsKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/tabs/keyboard";

    #region Navigation

    [Test]
    public async Task ArrowRight_ShouldFocusNextTab()
    {
        var trigger1 = Page.GetByTestId("auto-trigger-account");
        var trigger2 = Page.GetByTestId("auto-trigger-password");

        await trigger1.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");

        await Expect(trigger2).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowLeft_ShouldFocusPreviousTab()
    {
        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await passwordTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowLeft");

        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await Expect(accountTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowRight_ShouldSkipDisabledTabs()
    {
        // Focus the password tab (second tab)
        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await passwordTrigger.FocusAsync();

        // Press ArrowRight - should skip the disabled "settings" tab and wrap to "account"
        await Page.Keyboard.PressAsync("ArrowRight");

        // Should wrap to the first tab (account), skipping disabled settings
        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await Expect(accountTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowLeft_ShouldSkipDisabledTabs()
    {
        // Focus the account tab (first tab)
        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await accountTrigger.FocusAsync();

        // Press ArrowLeft - should skip the disabled "settings" tab and wrap to "password"
        await Page.Keyboard.PressAsync("ArrowLeft");

        // Should wrap to the password tab, skipping disabled settings
        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await Expect(passwordTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Home_ShouldFocusFirstTab()
    {
        // Focus the password tab
        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await passwordTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("Home");

        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await Expect(accountTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task End_ShouldFocusLastEnabledTab()
    {
        // Focus the account tab
        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await accountTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("End");

        // Should focus the last enabled tab (password, since settings is disabled)
        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await Expect(passwordTrigger).ToBeFocusedAsync();
    }

    #endregion

    #region Auto Activation Mode

    [Test]
    public async Task ArrowKey_ShouldActivateTab_InAutoMode()
    {
        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await accountTrigger.FocusAsync();

        // Arrow to the password tab
        await Page.Keyboard.PressAsync("ArrowRight");

        // In auto mode, the tab should be activated on focus
        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
        await Expect(passwordTrigger).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Enter_ShouldActivateTab()
    {
        // Navigate to password tab first
        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await accountTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");

        // Focus password tab and press Enter
        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await passwordTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
    }

    [Test]
    public async Task Space_ShouldActivateTab()
    {
        var accountTrigger = Page.GetByTestId("auto-trigger-account");
        await accountTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");

        var passwordTrigger = Page.GetByTestId("auto-trigger-password");
        await passwordTrigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
    }

    #endregion

    #region Manual Activation Mode

    [Test]
    public async Task ArrowKey_ShouldNotActivateTab_InManualMode()
    {
        var trigger1 = Page.GetByTestId("manual-trigger-1");
        await trigger1.FocusAsync();

        // Arrow to the next tab
        await Page.Keyboard.PressAsync("ArrowRight");

        // In manual mode, the tab should NOT be activated on focus
        var trigger2 = Page.GetByTestId("manual-trigger-2");
        await Expect(trigger2).ToBeFocusedAsync();
        await Expect(trigger2).ToHaveAttributeAsync("data-state", "inactive");

        // First tab should still be active
        await Expect(trigger1).ToHaveAttributeAsync("data-state", "active");
    }

    [Test]
    public async Task Enter_ShouldActivateTab_InManualMode()
    {
        var trigger1 = Page.GetByTestId("manual-trigger-1");
        await trigger1.FocusAsync();

        // Arrow to the next tab
        await Page.Keyboard.PressAsync("ArrowRight");

        // Wait for focus to move to trigger2 before pressing Enter
        var trigger2 = Page.GetByTestId("manual-trigger-2");
        await Expect(trigger2).ToBeFocusedAsync();

        // Press Enter to activate
        await Page.Keyboard.PressAsync("Enter");

        // Now tab2 should be active
        await Expect(trigger2).ToHaveAttributeAsync("data-state", "active");
    }

    [Test]
    public async Task Space_ShouldActivateTab_InManualMode()
    {
        var trigger1 = Page.GetByTestId("manual-trigger-1");
        await trigger1.FocusAsync();

        // Arrow to the next tab
        await Page.Keyboard.PressAsync("ArrowRight");

        // Wait for focus to move to trigger2 before pressing Space
        var trigger2 = Page.GetByTestId("manual-trigger-2");
        await Expect(trigger2).ToBeFocusedAsync();

        // Press Space to activate
        await Page.Keyboard.PressAsync(" ");

        // Now tab2 should be active
        await Expect(trigger2).ToHaveAttributeAsync("data-state", "active");
    }

    #endregion

    #region Vertical Orientation

    [Test]
    public async Task ArrowDown_ShouldFocusNextTab_InVerticalMode()
    {
        var firstTrigger = Page.GetByTestId("vertical-trigger-first");
        await firstTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        var secondTrigger = Page.GetByTestId("vertical-trigger-second");
        await Expect(secondTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldFocusPreviousTab_InVerticalMode()
    {
        var secondTrigger = Page.GetByTestId("vertical-trigger-second");
        await secondTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        var firstTrigger = Page.GetByTestId("vertical-trigger-first");
        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task VerticalTabs_ShouldHave_VerticalOrientation()
    {
        var tabsList = Page.GetByTestId("vertical-tabs-list");
        await Expect(tabsList).ToHaveAttributeAsync("aria-orientation", "vertical");
        await Expect(tabsList).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region Wrap Around Navigation

    [Test]
    public async Task ArrowRight_ShouldWrapToFirst_WhenAtLast()
    {
        // Focus the last tab
        var thirdTrigger = Page.GetByTestId("wrap-trigger-third");
        await thirdTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowRight");

        // Should wrap to first tab
        var firstTrigger = Page.GetByTestId("wrap-trigger-first");
        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowLeft_ShouldWrapToLast_WhenAtFirst()
    {
        // Focus the first tab
        var firstTrigger = Page.GetByTestId("wrap-trigger-first");
        await firstTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowLeft");

        // Should wrap to last tab
        var thirdTrigger = Page.GetByTestId("wrap-trigger-third");
        await Expect(thirdTrigger).ToBeFocusedAsync();
    }

    #endregion
}
