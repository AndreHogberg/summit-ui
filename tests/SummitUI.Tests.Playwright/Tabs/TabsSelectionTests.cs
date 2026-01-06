namespace SummitUI.Tests.Playwright.Tabs;

/// <summary>
/// Tests for Tab selection behavior.
/// </summary>
public class TabsSelectionTests : SummitTestBase
{
    protected override string TestPagePath => "tests/tabs/basic";

    [Test]
    public async Task Click_ShouldActivateTab()
    {
        var passwordTrigger = Page.GetByTestId("trigger-password");
        await passwordTrigger.ClickAsync();

        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
        await Expect(passwordTrigger).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Click_ShouldShowCorrespondingContent()
    {
        var passwordTrigger = Page.GetByTestId("trigger-password");
        await passwordTrigger.ClickAsync();

        var passwordContent = Page.GetByTestId("content-password");
        await Expect(passwordContent).ToBeVisibleAsync();
        await Expect(passwordContent).ToContainTextAsync("Password settings content");
    }

    [Test]
    public async Task Click_ShouldDeactivatePreviousTab()
    {
        // Initially account tab is active
        var accountTrigger = Page.GetByTestId("trigger-account");
        await Expect(accountTrigger).ToHaveAttributeAsync("data-state", "active");

        // Click on the password tab
        var passwordTrigger = Page.GetByTestId("trigger-password");
        await passwordTrigger.ClickAsync();

        // Account tab should now be inactive
        await Expect(accountTrigger).ToHaveAttributeAsync("data-state", "inactive");
        await Expect(accountTrigger).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Click_ShouldUpdateTabIndex_RovingTabindex()
    {
        // Initially account tab has tabindex="0"
        var accountTrigger = Page.GetByTestId("trigger-account");
        await Expect(accountTrigger).ToHaveAttributeAsync("tabindex", "0");

        // Click on the password tab
        var passwordTrigger = Page.GetByTestId("trigger-password");
        await passwordTrigger.ClickAsync();

        // Password tab should now have tabindex="0"
        await Expect(passwordTrigger).ToHaveAttributeAsync("tabindex", "0");

        // Account tab should now have tabindex="-1"
        await Expect(accountTrigger).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task ControlledMode_ShouldUpdateFromExternalSource()
    {
        // Click external button to set tab2
        var setTab2Btn = Page.GetByTestId("set-tab2");
        await setTab2Btn.ClickAsync();

        // Verify tab2 is active
        var trigger2 = Page.GetByTestId("controlled-trigger-2");
        await Expect(trigger2).ToHaveAttributeAsync("data-state", "active");

        // Verify tab1 is inactive
        var trigger1 = Page.GetByTestId("controlled-trigger-1");
        await Expect(trigger1).ToHaveAttributeAsync("data-state", "inactive");

        // Verify content changed
        var content2 = Page.GetByTestId("controlled-content-2");
        await Expect(content2).ToBeVisibleAsync();
    }

    [Test]
    public async Task ControlledMode_ShouldUpdateValueDisplay()
    {
        var valueDisplay = Page.GetByTestId("controlled-value");
        await Expect(valueDisplay).ToContainTextAsync("Current: tab1");

        // Click external button to set tab3
        var setTab3Btn = Page.GetByTestId("set-tab3");
        await setTab3Btn.ClickAsync();

        await Expect(valueDisplay).ToContainTextAsync("Current: tab3");
    }
}
