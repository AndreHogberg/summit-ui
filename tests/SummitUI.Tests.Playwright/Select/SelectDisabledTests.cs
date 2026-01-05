namespace SummitUI.Tests.Playwright.Select;

/// <summary>
/// Tests for Select disabled states.
/// Uses a dedicated test page with disabled select and disabled items.
/// </summary>
public class SelectDisabledTests : SummitTestBase
{
    protected override string TestPagePath => "tests/select/disabled";

    #region Disabled Select

    [Test]
    public async Task DisabledTrigger_ShouldHave_AriaDisabledTrue()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await Expect(trigger).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabled()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DisabledAttribute()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await Expect(trigger).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledTrigger_ShouldNotOpen_OnClick()
    {
        var trigger = Page.GetByTestId("disabled-trigger");

        await trigger.ClickAsync(new() { Force = true });

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Disabled Items

    [Test]
    public async Task DisabledItem_ShouldHave_AriaDisabledTrue()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var disabledItem = Page.GetByTestId("item-enterprise");
        await Expect(disabledItem).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledItem_ShouldHave_DataDisabled()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var disabledItem = Page.GetByTestId("item-enterprise");
        await Expect(disabledItem).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledItem_ShouldNotBeSelectable()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("disabled-items-content");
        await Expect(content).ToBeVisibleAsync();

        var disabledItem = Page.GetByTestId("item-enterprise");

        // Try to click disabled item
        await disabledItem.ClickAsync(new() { Force = true });

        // Content should still be open (item was not selected)
        await Expect(content).ToBeVisibleAsync();
    }

    #endregion

    #region Disabled Item Keyboard Navigation

    [Test]
    public async Task ArrowDown_ShouldSkipDisabledItems()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("disabled-items-content");
        await Expect(content).ToBeVisibleAsync();

        // First item (Free) should be highlighted on open
        var freeItem = Page.GetByTestId("item-free");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown - should go to Pro (second item)
        await Page.Keyboard.PressAsync("ArrowDown");
        var proItem = Page.GetByTestId("item-pro");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown again - should skip Enterprise (disabled) and loop to Free
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldSkipDisabledItems()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("disabled-items-content");
        await Expect(content).ToBeVisibleAsync();

        // First item (Free) should be highlighted on open
        var freeItem = Page.GetByTestId("item-free");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowUp - should skip Enterprise (disabled) and loop to Pro
        await Page.Keyboard.PressAsync("ArrowUp");
        var proItem = Page.GetByTestId("item-pro");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Home_ShouldGoToFirstNonDisabledItem()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("disabled-items-content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to Pro
        await Page.Keyboard.PressAsync("ArrowDown");
        var proItem = Page.GetByTestId("item-pro");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press Home - should go to first non-disabled item (Free)
        await Page.Keyboard.PressAsync("Home");
        var freeItem = Page.GetByTestId("item-free");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task End_ShouldGoToLastNonDisabledItem()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("disabled-items-content");
        await Expect(content).ToBeVisibleAsync();

        // Press End - should go to last non-disabled item (Pro, not Enterprise)
        await Page.Keyboard.PressAsync("End");
        var proItem = Page.GetByTestId("item-pro");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");

        // Verify Enterprise (disabled) is NOT highlighted
        var enterpriseItem = Page.GetByTestId("item-enterprise");
        await Expect(enterpriseItem).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task DisabledItem_ShouldNeverBeHighlighted_ByKeyboard()
    {
        var trigger = Page.GetByTestId("disabled-items-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("disabled-items-content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate through all items multiple times
        for (var i = 0; i < 6; i++)
        {
            await Page.Keyboard.PressAsync("ArrowDown");
        }

        // Verify Enterprise (disabled) is NOT highlighted
        var enterpriseItem = Page.GetByTestId("item-enterprise");
        await Expect(enterpriseItem).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion
}
