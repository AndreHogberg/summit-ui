namespace SummitUI.Tests.Playwright.Accordion;

/// <summary>
/// Tests for Accordion disabled item behavior.
/// Verifies disabled attributes, no toggle on click, and keyboard navigation skipping.
/// </summary>
public class AccordionDisabledTests : SummitTestBase
{
    protected override string TestPagePath => "tests/accordion/disabled";

    #region Disabled Item Attributes

    [Test]
    public async Task DisabledItem_ShouldHave_DataDisabled()
    {
        var disabledItem = Page.GetByTestId("item-disabled-1");
        await Expect(disabledItem).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DisabledAttribute()
    {
        var disabledTrigger = Page.GetByTestId("trigger-disabled-1");
        await Expect(disabledTrigger).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabled()
    {
        var disabledTrigger = Page.GetByTestId("trigger-disabled-1");
        await Expect(disabledTrigger).ToHaveAttributeAsync("data-disabled", "");
    }

    #endregion

    #region Disabled Item Behavior

    [Test]
    public async Task DisabledItem_ShouldNotToggle_OnClick()
    {
        var disabledTrigger = Page.GetByTestId("trigger-disabled-1");

        // Verify it's closed initially
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        // Force click on disabled trigger
        await disabledTrigger.ClickAsync(new() { Force = true });

        // Should still be closed
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Enter_ShouldNotToggle_DisabledItem()
    {
        var disabledTrigger = Page.GetByTestId("trigger-disabled-1");
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        await disabledTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Space_ShouldNotToggle_DisabledItem()
    {
        var disabledTrigger = Page.GetByTestId("trigger-disabled-1");
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        await disabledTrigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Keyboard Navigation - Skip Disabled

    [Test]
    public async Task ArrowDown_ShouldSkipDisabledTriggers()
    {
        var enabledTrigger = Page.GetByTestId("trigger-enabled-1");
        var anotherEnabledTrigger = Page.GetByTestId("trigger-enabled-2");

        await enabledTrigger.FocusAsync();
        await Expect(enabledTrigger).ToBeFocusedAsync();

        // Arrow down should skip the disabled item and go to "Another Enabled Item"
        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(anotherEnabledTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldSkipDisabledTriggers()
    {
        var enabledTrigger = Page.GetByTestId("trigger-enabled-1");
        var anotherEnabledTrigger = Page.GetByTestId("trigger-enabled-2");

        await anotherEnabledTrigger.FocusAsync();
        await Expect(anotherEnabledTrigger).ToBeFocusedAsync();

        // Arrow up should skip the disabled item and go to "Enabled Item"
        await Page.Keyboard.PressAsync("ArrowUp");

        await Expect(enabledTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task DisabledTrigger_ShouldNotReceiveFocus_OnTab()
    {
        var enabledTrigger = Page.GetByTestId("trigger-enabled-1");
        var disabledTrigger = Page.GetByTestId("trigger-disabled-1");
        var anotherEnabledTrigger = Page.GetByTestId("trigger-enabled-2");

        await enabledTrigger.FocusAsync();
        await Expect(enabledTrigger).ToBeFocusedAsync();

        // Tab forward - should skip disabled and go to next enabled
        await Page.Keyboard.PressAsync("Tab");

        // Disabled trigger should NOT be focused
        await Expect(disabledTrigger).Not.ToBeFocusedAsync();

        // Next enabled trigger should be focused
        await Expect(anotherEnabledTrigger).ToBeFocusedAsync();
    }

    #endregion
}
