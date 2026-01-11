namespace SummitUI.Tests.Playwright.Combobox;

/// <summary>
/// Tests for Combobox disabled states.
/// Uses the disabled test page with disabled root and disabled items.
/// </summary>
public class ComboboxDisabledTests : SummitTestBase
{
    protected override string TestPagePath => "tests/combobox/disabled";

    #region Disabled Combobox

    [Test]
    public async Task DisabledTrigger_ShouldHave_AriaDisabledTrue()
    {
        var trigger = Page.GetByTestId("trigger-disabled");
        await Expect(trigger).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabled()
    {
        var trigger = Page.GetByTestId("trigger-disabled");
        await Expect(trigger).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledInput_ShouldHave_DisabledAttribute()
    {
        var input = Page.GetByTestId("input-disabled");
        await Expect(input).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledInput_ShouldHave_AriaDisabledTrue()
    {
        var input = Page.GetByTestId("input-disabled");
        await Expect(input).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledCombobox_ShouldNotOpen_OnClick()
    {
        var input = Page.GetByTestId("input-disabled");
        await input.ClickAsync(new() { Force = true });

        await Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task DisabledCombobox_ShouldNotOpen_OnKeyboard()
    {
        var input = Page.GetByTestId("input-disabled");
        await input.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Disabled Items

    [Test]
    public async Task DisabledItem_ShouldHave_AriaDisabledTrue()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        var disabledItem = Page.GetByTestId("item-banana-disabled");
        await Expect(disabledItem).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledItem_ShouldHave_DataDisabled()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        var disabledItem = Page.GetByTestId("item-banana-disabled");
        await Expect(disabledItem).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledItem_ShouldNotBeSelectable()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        var content = Page.GetByTestId("content-disabled-items");
        await Expect(content).ToBeVisibleAsync();

        var disabledItem = Page.GetByTestId("item-banana-disabled");

        // Try to click disabled item
        await disabledItem.ClickAsync(new() { Force = true });

        // Item should not be selected
        await Expect(disabledItem).ToHaveAttributeAsync("aria-selected", "false");
    }

    #endregion

    #region Disabled Item Keyboard Navigation

    [Test]
    public async Task ArrowDown_ShouldSkipDisabledItems()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        var content = Page.GetByTestId("content-disabled-items");
        await Expect(content).ToBeVisibleAsync();

        // First item (Apple) should be highlighted on open
        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown - should skip Banana (disabled) and go to Orange
        await Page.Keyboard.PressAsync("ArrowDown");
        var orange = Page.GetByTestId("item-orange");
        await Expect(orange).ToHaveAttributeAsync("data-highlighted", "");

        // Verify Banana is NOT highlighted
        var banana = Page.GetByTestId("item-banana-disabled");
        await Expect(banana).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldSkipDisabledItems()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        // Navigate to Orange (skip banana)
        await Page.Keyboard.PressAsync("ArrowDown");
        var orange = Page.GetByTestId("item-orange");
        await Expect(orange).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowUp - should skip Banana (disabled) and go to Apple
        await Page.Keyboard.PressAsync("ArrowUp");
        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task DisabledItem_ShouldNeverBeHighlighted_ByKeyboard()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        // Navigate through all items multiple times
        for (var i = 0; i < 10; i++)
        {
            await Page.Keyboard.PressAsync("ArrowDown");
        }

        // Verify disabled items are NOT highlighted
        var bananaDisabled = Page.GetByTestId("item-banana-disabled");
        var grapeDisabled = Page.GetByTestId("item-grape-disabled");
        await Expect(bananaDisabled).Not.ToHaveAttributeAsync("data-highlighted", "");
        await Expect(grapeDisabled).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task DisabledItem_ShouldNotBeSelected_ByEnter()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        // Force highlight by hover (even though keyboard skips)
        var disabledItem = Page.GetByTestId("item-banana-disabled");
        await disabledItem.HoverAsync();

        // Try to select with Enter
        await Page.Keyboard.PressAsync("Enter");

        // Should not be selected
        await Expect(disabledItem).ToHaveAttributeAsync("aria-selected", "false");
    }

    #endregion

    #region Mixed Enabled and Disabled Items

    [Test]
    public async Task EnabledItems_CanBeSelected_WhenDisabledItemsPresent()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        // Select enabled item
        var apple = Page.GetByTestId("item-apple");
        await apple.ClickAsync();

        await Expect(apple).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Navigation_WorksCorrectly_WithMixedItems()
    {
        var input = Page.GetByTestId("input-disabled-items");
        await input.ClickAsync();

        // Start at Apple
        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");

        // Down -> Orange (skip Banana)
        await Page.Keyboard.PressAsync("ArrowDown");
        var orange = Page.GetByTestId("item-orange");
        await Expect(orange).ToHaveAttributeAsync("data-highlighted", "");

        // Down -> Mango (skip Grape)
        await Page.Keyboard.PressAsync("ArrowDown");
        var mango = Page.GetByTestId("item-mango");
        await Expect(mango).ToHaveAttributeAsync("data-highlighted", "");

        // Down -> wrap to Apple (skip disabled items)
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion
}
