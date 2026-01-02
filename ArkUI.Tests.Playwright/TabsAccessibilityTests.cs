using TUnit.Playwright;

namespace ArkUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the Tabs component.
/// Tests ARIA attributes, keyboard navigation, and focus management.
/// </summary>
public class TabsAccessibilityTests : PageTest
{
    private const string TabsDemoUrl = "Tabs";

    [Before(Test)]
    public async Task NavigateToTabsDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + TabsDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes on TabsList

    [Test]
    public async Task TabsList_ShouldHave_RoleTablist()
    {
        var tabsList = Page.Locator("[data-ark-tabs-list]").First;
        await Expect(tabsList).ToHaveAttributeAsync("role", "tablist");
    }

    [Test]
    public async Task TabsList_ShouldHave_AriaOrientationHorizontal_ByDefault()
    {
        var tabsList = Page.Locator("[data-ark-tabs-list]").First;
        await Expect(tabsList).ToHaveAttributeAsync("aria-orientation", "horizontal");
    }

    [Test]
    public async Task TabsList_ShouldHave_DataOrientationHorizontal_ByDefault()
    {
        var tabsList = Page.Locator("[data-ark-tabs-list]").First;
        await Expect(tabsList).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    #endregion

    #region ARIA Attributes on TabsTrigger

    [Test]
    public async Task Trigger_ShouldHave_RoleTab()
    {
        var trigger = Page.Locator("[data-ark-tabs-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("role", "tab");
    }

    [Test]
    public async Task Trigger_ShouldHave_UniqueId()
    {
        var triggers = Page.Locator("[data-ark-tabs-trigger]");
        var count = await triggers.CountAsync();

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await triggers.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaSelectedTrue_WhenActive()
    {
        var activeTrigger = Page.Locator("[data-ark-tabs-trigger][data-state='active']").First;
        await Expect(activeTrigger).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaSelectedFalse_WhenInactive()
    {
        var inactiveTrigger = Page.Locator("[data-ark-tabs-trigger][data-state='inactive']").First;
        await Expect(inactiveTrigger).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.Locator("[data-ark-tabs-trigger]").First;
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await Assert.That(ariaControls).IsNotNull();

        // Verify the content panel with that ID exists
        var content = Page.Locator($"[data-ark-tabs-content]#{ariaControls}");
        await Expect(content).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Trigger_ShouldHave_TabIndexZero_WhenActive()
    {
        var activeTrigger = Page.Locator("[data-ark-tabs-trigger][data-state='active']").First;
        await Expect(activeTrigger).ToHaveAttributeAsync("tabindex", "0");
    }

    [Test]
    public async Task Trigger_ShouldHave_TabIndexNegativeOne_WhenInactive()
    {
        var inactiveTrigger = Page.Locator("[data-ark-tabs-trigger][data-state='inactive']").First;
        await Expect(inactiveTrigger).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateActive_WhenSelected()
    {
        // First trigger is selected by default (account)
        var trigger = Page.Locator("[data-ark-tabs-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("data-state", "active");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateInactive_WhenNotSelected()
    {
        // Second trigger is not selected by default
        var trigger = Page.Locator("[data-ark-tabs-trigger]").Nth(1);
        await Expect(trigger).ToHaveAttributeAsync("data-state", "inactive");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataValue()
    {
        var trigger = Page.Locator("[data-ark-tabs-trigger]").First;
        var dataValue = await trigger.GetAttributeAsync("data-value");

        await Assert.That(dataValue).IsNotNull();
        await Assert.That(dataValue).IsEqualTo("account");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataOrientation()
    {
        var trigger = Page.Locator("[data-ark-tabs-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    #endregion

    #region ARIA Attributes on TabsContent

    [Test]
    public async Task Content_ShouldHave_RoleTabpanel()
    {
        var content = Page.Locator("[data-ark-tabs-content]").First;
        await Expect(content).ToHaveAttributeAsync("role", "tabpanel");
    }

    [Test]
    public async Task Content_ShouldHave_UniqueId()
    {
        var contents = Page.Locator("[data-ark-tabs-content]");
        var count = await contents.CountAsync();

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await contents.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTriggerId()
    {
        var content = Page.Locator("[data-ark-tabs-content]").First;
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsNotNull();

        // Verify the trigger with that ID exists
        var trigger = Page.Locator($"[data-ark-tabs-trigger]#{ariaLabelledby}");
        await Expect(trigger).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexZero()
    {
        var content = Page.Locator("[data-ark-tabs-content]").First;
        await Expect(content).ToHaveAttributeAsync("tabindex", "0");
    }

    [Test]
    public async Task Content_ShouldHave_DataStateActive_WhenVisible()
    {
        var content = Page.Locator("[data-ark-tabs-content][data-state='active']").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Content_ShouldHave_DataOrientation()
    {
        var content = Page.Locator("[data-ark-tabs-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    #endregion

    #region Disabled Tab Accessibility

    [Test]
    public async Task DisabledTrigger_ShouldHave_AriaDisabledTrue()
    {
        var disabledTrigger = Page.Locator("[data-ark-tabs-trigger][data-disabled]").First;
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabledAttribute()
    {
        var disabledTrigger = Page.Locator("[data-ark-tabs-trigger][data-disabled]");
        await Expect(disabledTrigger).ToHaveCountAsync(1);
    }

    [Test]
    public async Task DisabledTrigger_ShouldNotActivate_OnClick()
    {
        var disabledTrigger = Page.Locator("[data-ark-tabs-trigger][data-disabled]").First;
        var disabledValue = await disabledTrigger.GetAttributeAsync("data-value");

        // Force click on disabled trigger
        await disabledTrigger.ClickAsync(new() { Force = true });

        // The content for the disabled tab should not be visible
        var disabledContent = Page.Locator($"[data-ark-tabs-content][data-state='active']");
        var activeContentValue = await disabledContent.GetAttributeAsync("aria-labelledby");

        // Active content should not be for the disabled tab
        await Assert.That(activeContentValue).IsNotNull();
        await Assert.That(activeContentValue!.Contains(disabledValue!)).IsFalse();
    }

    #endregion

    #region Tab Selection

    [Test]
    public async Task Click_ShouldActivateTab()
    {
        // Click on the second tab (password)
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.ClickAsync();

        // Verify the tab is now active
        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
        await Expect(passwordTrigger).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Click_ShouldShowCorrespondingContent()
    {
        // Click on the password tab
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.ClickAsync();

        // Verify the password content is now visible
        var passwordContent = Page.Locator("[data-ark-tabs-content][data-state='active']");
        await Expect(passwordContent).ToContainTextAsync("Password settings content");
    }

    [Test]
    public async Task Click_ShouldDeactivatePreviousTab()
    {
        // Initially account tab is active
        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await Expect(accountTrigger).ToHaveAttributeAsync("data-state", "active");

        // Click on the password tab
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.ClickAsync();

        // Account tab should now be inactive
        await Expect(accountTrigger).ToHaveAttributeAsync("data-state", "inactive");
        await Expect(accountTrigger).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Click_ShouldUpdateTabIndex_RovingTabindex()
    {
        // Initially account tab has tabindex="0"
        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await Expect(accountTrigger).ToHaveAttributeAsync("tabindex", "0");

        // Click on the password tab
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.ClickAsync();

        // Password tab should now have tabindex="0"
        await Expect(passwordTrigger).ToHaveAttributeAsync("tabindex", "0");

        // Account tab should now have tabindex="-1"
        await Expect(accountTrigger).ToHaveAttributeAsync("tabindex", "-1");
    }

    #endregion

    #region Keyboard Navigation - Arrow Keys

    [Test]
    public async Task ArrowRight_ShouldFocusNextTab()
    {
        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await accountTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowRight");

        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await Expect(passwordTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowLeft_ShouldFocusPreviousTab()
    {
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowLeft");

        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await Expect(accountTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowRight_ShouldSkipDisabledTabs()
    {
        // Focus the password tab (second tab)
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.FocusAsync();

        // Press ArrowRight - should skip the disabled "settings" tab and wrap to "account"
        await Page.Keyboard.PressAsync("ArrowRight");

        // Should wrap to the first tab (account), skipping disabled settings
        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await Expect(accountTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowLeft_ShouldSkipDisabledTabs()
    {
        // Focus the account tab (first tab)
        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await accountTrigger.FocusAsync();

        // Press ArrowLeft - should skip the disabled "settings" tab and wrap to "password"
        await Page.Keyboard.PressAsync("ArrowLeft");

        // Should wrap to the password tab, skipping disabled settings
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await Expect(passwordTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Home_ShouldFocusFirstTab()
    {
        // Focus the password tab
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("Home");

        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await Expect(accountTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task End_ShouldFocusLastEnabledTab()
    {
        // Focus the account tab
        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await accountTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("End");

        // Should focus the last enabled tab (password, since settings is disabled)
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await Expect(passwordTrigger).ToBeFocusedAsync();
    }

    #endregion

    #region Keyboard Navigation - Tab Activation (Auto Mode)

    [Test]
    public async Task ArrowKey_ShouldActivateTab_InAutoMode()
    {
        // Default is auto activation mode
        var accountTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='account']");
        await accountTrigger.FocusAsync();

        // Arrow to the password tab
        await Page.Keyboard.PressAsync("ArrowRight");

        // In auto mode, the tab should be activated on focus
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
        await Expect(passwordTrigger).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Enter_ShouldActivateTab()
    {
        // Focus the password tab
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.FocusAsync();

        await Page.Keyboard.PressAsync("Enter");

        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
    }

    [Test]
    public async Task Space_ShouldActivateTab()
    {
        // Focus the password tab
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.FocusAsync();

        await Page.Keyboard.PressAsync(" ");

        await Expect(passwordTrigger).ToHaveAttributeAsync("data-state", "active");
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Tab_ShouldFocusActiveTabTrigger_FromOutside()
    {
        // Focus the active tab trigger directly and verify it can receive focus
        var activeTrigger = Page.Locator("[data-ark-tabs-trigger][data-state='active']");
        await activeTrigger.FocusAsync();

        // The active tab trigger should be focused
        await Expect(activeTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tab_ShouldMoveFocusToContent_AfterTrigger()
    {
        // Focus the active tab trigger
        var activeTrigger = Page.Locator("[data-ark-tabs-trigger][data-state='active']");
        await activeTrigger.FocusAsync();

        // Tab should move focus to the content panel
        await Page.Keyboard.PressAsync("Tab");

        var activeContent = Page.Locator("[data-ark-tabs-content][data-state='active']");
        await Expect(activeContent).ToBeFocusedAsync();
    }

    #endregion

    #region Content Visibility

    [Test]
    public async Task OnlyActiveContent_ShouldBeVisible()
    {
        // Account content should be visible (default active)
        var activeContent = Page.Locator("[data-ark-tabs-content][data-state='active']");
        await Expect(activeContent).ToBeVisibleAsync();
        await Expect(activeContent).ToContainTextAsync("Account settings content");

        // Only one content panel should be rendered at a time
        // (inactive content is not rendered, not just hidden)
        var allContents = Page.Locator("[data-ark-tabs-content]");
        await Expect(allContents).ToHaveCountAsync(1);
    }

    [Test]
    public async Task ContentVisibility_ShouldUpdate_OnTabChange()
    {
        // Initially account content is visible
        var accountContent = Page.Locator("[data-ark-tabs-content][aria-labelledby*='account']");
        await Expect(accountContent).ToBeVisibleAsync();
        await Expect(accountContent).ToContainTextAsync("Account settings content");

        // Click password tab
        var passwordTrigger = Page.Locator("[data-ark-tabs-trigger][data-value='password']");
        await passwordTrigger.ClickAsync();

        // Account content should no longer be in the DOM (component only renders when active)
        await Expect(accountContent).ToHaveCountAsync(0);

        // Password content should now be visible
        var passwordContent = Page.Locator("[data-ark-tabs-content][aria-labelledby*='password']");
        await Expect(passwordContent).ToBeVisibleAsync();
        await Expect(passwordContent).ToContainTextAsync("Password settings content");
    }

    #endregion

    #region ID Relationships

    [Test]
    public async Task TriggerAndContent_ShouldHave_MatchingIdRelationship()
    {
        // Get the first trigger
        var trigger = Page.Locator("[data-ark-tabs-trigger]").First;
        var triggerId = await trigger.GetAttributeAsync("id");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Get the corresponding content
        var content = Page.Locator($"#{ariaControls}");
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        // The content's aria-labelledby should point back to the trigger
        await Assert.That(ariaLabelledby).IsEqualTo(triggerId);
    }

    [Test]
    public async Task AllEnabledTriggers_ShouldHave_ValidAriaControls_WhenActive()
    {
        // Get all non-disabled triggers
        var triggers = Page.Locator("[data-ark-tabs-trigger]:not([data-disabled])");
        var count = await triggers.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var trigger = triggers.Nth(i);
            var triggerValue = await trigger.GetAttributeAsync("data-value");

            // Click the trigger to make it active (this ensures its content panel is rendered)
            await trigger.ClickAsync();

            // Now verify the aria-controls points to an existing content
            var ariaControls = await trigger.GetAttributeAsync("aria-controls");
            await Assert.That(ariaControls).IsNotNull();

            // The content panel should exist when this tab is active
            var referencedContent = Page.Locator($"[data-ark-tabs-content][id='{ariaControls}']");
            await Expect(referencedContent).ToHaveCountAsync(1);
        }
    }

    #endregion
}
