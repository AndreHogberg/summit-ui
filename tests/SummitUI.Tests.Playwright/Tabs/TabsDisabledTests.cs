namespace SummitUI.Tests.Playwright.Tabs;

/// <summary>
/// Tests for disabled tab behavior.
/// </summary>
public class TabsDisabledTests : SummitTestBase
{
    protected override string TestPagePath => "tests/tabs/disabled";

    [Test]
    public async Task DisabledTrigger_ShouldHave_AriaDisabledTrue()
    {
        var disabledTrigger = Page.GetByTestId("trigger-settings");
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabledAttribute()
    {
        var disabledTrigger = Page.GetByTestId("trigger-settings");
        await Expect(disabledTrigger).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledTrigger_ShouldNotActivate_OnClick()
    {
        // Get initial active content
        var accountContent = Page.GetByTestId("content-account");
        await Expect(accountContent).ToBeVisibleAsync();

        // Force click on disabled trigger
        var disabledTrigger = Page.GetByTestId("trigger-settings");
        await disabledTrigger.ClickAsync(new() { Force = true });

        // Account content should still be visible (disabled tab should not activate)
        await Expect(accountContent).ToBeVisibleAsync();

        // Disabled trigger should still be inactive
        await Expect(disabledTrigger).ToHaveAttributeAsync("data-state", "inactive");
    }

    [Test]
    public async Task DisabledTrigger_ShouldNotHave_TabIndexZero()
    {
        var disabledTrigger = Page.GetByTestId("trigger-settings");
        await Expect(disabledTrigger).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task MultipleDisabledTabs_ShouldAllHave_AriaDisabledTrue()
    {
        var disabled1 = Page.GetByTestId("multi-trigger-1");
        var disabled3 = Page.GetByTestId("multi-trigger-3");
        var disabled5 = Page.GetByTestId("multi-trigger-5");

        await Expect(disabled1).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabled3).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabled5).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task EnabledTabs_ShouldNotHave_AriaDisabled()
    {
        var enabled2 = Page.GetByTestId("multi-trigger-2");
        var enabled4 = Page.GetByTestId("multi-trigger-4");

        // Enabled tabs should not have aria-disabled attribute
        var ariaDisabled2 = await enabled2.GetAttributeAsync("aria-disabled");
        var ariaDisabled4 = await enabled4.GetAttributeAsync("aria-disabled");

        await Assert.That(ariaDisabled2).IsNull();
        await Assert.That(ariaDisabled4).IsNull();
    }

    [Test]
    public async Task DynamicDisabled_ShouldToggle_AriaDisabledAttribute()
    {
        var dynamicTrigger = Page.GetByTestId("dynamic-trigger-second");
        
        // Initially disabled
        await Expect(dynamicTrigger).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(dynamicTrigger).ToHaveAttributeAsync("data-disabled", "");

        // Toggle to enabled
        var toggleBtn = Page.GetByTestId("toggle-disabled-btn");
        await toggleBtn.ClickAsync();

        // Should now be enabled
        var ariaDisabled = await dynamicTrigger.GetAttributeAsync("aria-disabled");
        await Assert.That(ariaDisabled).IsNull();
    }

    [Test]
    public async Task DynamicEnabled_ShouldBeClickable_AfterToggle()
    {
        var dynamicTrigger = Page.GetByTestId("dynamic-trigger-second");
        var toggleBtn = Page.GetByTestId("toggle-disabled-btn");

        // Toggle to enabled
        await toggleBtn.ClickAsync();

        // Should now be clickable
        await dynamicTrigger.ClickAsync();

        // Should be active
        await Expect(dynamicTrigger).ToHaveAttributeAsync("data-state", "active");
    }
}
