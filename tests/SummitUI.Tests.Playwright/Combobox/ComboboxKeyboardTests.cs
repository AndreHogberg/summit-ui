namespace SummitUI.Tests.Playwright.Combobox;

/// <summary>
/// Tests for Combobox keyboard navigation.
/// Uses the keyboard test page with multiple items for navigation testing.
/// </summary>
public class ComboboxKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/combobox/keyboard";

    #region Opening and Closing

    [Test]
    public async Task Combobox_ShouldOpen_OnInputClick()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldOpen_OnArrowDown()
    {
        var input = Page.GetByTestId("input");
        await input.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldOpen_OnArrowUp()
    {
        var input = Page.GetByTestId("input");
        await input.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowUp");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldOpen_OnEnter_WhenClosed()
    {
        var input = Page.GetByTestId("input");
        await input.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldClose_OnEscapeKey()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Input_ShouldUpdateAriaExpanded_AfterEscapeClose()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await Expect(input).ToHaveAttributeAsync("aria-expanded", "true");

        await Page.Keyboard.PressAsync("Escape");

        await Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Tab_ShouldCloseCombobox()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Tab");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Arrow Key Navigation

    [Test]
    public async Task ArrowDown_ShouldHighlightNextItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // First item should be highlighted on open
        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown to move to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        var secondItem = Page.GetByTestId("item-second");
        await Expect(secondItem).ToHaveAttributeAsync("data-highlighted", "");
        await Expect(firstItem).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldHighlightPreviousItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Go to second item first
        await Page.Keyboard.PressAsync("ArrowDown");

        var secondItem = Page.GetByTestId("item-second");
        await Expect(secondItem).ToHaveAttributeAsync("data-highlighted", "");

        // Go back up to first item
        await Page.Keyboard.PressAsync("ArrowUp");

        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowDown_ShouldWrapToFirst_WhenAtLast()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Press End to go to last item
        await Page.Keyboard.PressAsync("Control+End");
        var lastItem = Page.GetByTestId("item-last");
        await Expect(lastItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown to wrap to first
        await Page.Keyboard.PressAsync("ArrowDown");

        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldWrapToLast_WhenAtFirst()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // First item is highlighted on open
        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowUp to wrap to last
        await Page.Keyboard.PressAsync("ArrowUp");

        var lastItem = Page.GetByTestId("item-last");
        await Expect(lastItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task CtrlHome_ShouldHighlightFirstItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Navigate down a few items
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");

        // Press Ctrl+Home
        await Page.Keyboard.PressAsync("Control+Home");

        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task CtrlEnd_ShouldHighlightLastItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Press Ctrl+End
        await Page.Keyboard.PressAsync("Control+End");

        var lastItem = Page.GetByTestId("item-last");
        await Expect(lastItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Item Selection

    [Test]
    public async Task Enter_ShouldToggleHighlightedItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        var secondItem = Page.GetByTestId("item-second");
        await Expect(secondItem).ToHaveAttributeAsync("data-highlighted", "");

        // Select it
        await Page.Keyboard.PressAsync("Enter");

        // Item should be selected
        await Expect(secondItem).ToHaveAttributeAsync("aria-selected", "true");

        // Content should stay open (multi-select)
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Enter_ShouldDeselectItem_WhenAlreadySelected()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Select first item
        var firstItem = Page.GetByTestId("item-first");
        await Page.Keyboard.PressAsync("Enter");
        await Expect(firstItem).ToHaveAttributeAsync("aria-selected", "true");

        // Press Enter again to deselect
        await Page.Keyboard.PressAsync("Enter");
        await Expect(firstItem).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Input_ShouldHave_AriaActivedescendant_WhenItemHighlighted()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Navigate to highlight an item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Input should have aria-activedescendant pointing to highlighted item
        await Expect(input).ToHaveAttributeAsync("aria-activedescendant", new System.Text.RegularExpressions.Regex(".+"));
    }

    #endregion

    #region Backspace Behavior

    [Test]
    public async Task Backspace_ShouldRemoveLastSelectedValue_WhenInputEmpty()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Select first and second items
        await Page.Keyboard.PressAsync("Enter"); // Select first
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("Enter"); // Select second

        // Verify both are selected
        var badge1 = Page.GetByTestId("badge-first");
        var badge2 = Page.GetByTestId("badge-second");
        await Expect(badge1).ToBeVisibleAsync();
        await Expect(badge2).ToBeVisibleAsync();

        // Press Backspace with empty input
        await Page.Keyboard.PressAsync("Backspace");

        // Second badge should be removed (last selected)
        await Expect(badge2).Not.ToBeVisibleAsync();
        await Expect(badge1).ToBeVisibleAsync();
    }

    [Test]
    public async Task Backspace_ShouldNotRemoveValue_WhenInputHasText()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Select first item
        await Page.Keyboard.PressAsync("Enter");

        // Type something in input
        await input.PressSequentiallyAsync("test");

        // Press Backspace - should delete text, not remove selection
        await Page.Keyboard.PressAsync("Backspace");

        var badge = Page.GetByTestId("badge-first");
        await Expect(badge).ToBeVisibleAsync();
    }

    #endregion
}
