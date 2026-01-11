namespace SummitUI.Tests.Playwright.Combobox;

/// <summary>
/// Tests for Combobox multi-selection behavior.
/// Uses the basic test page with editable combobox.
/// </summary>
public class ComboboxSelectionTests : SummitTestBase
{
    protected override string TestPagePath => "tests/combobox/basic";

    #region Multi-Select Behavior

    [Test]
    public async Task CanSelectMultipleItems()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Select multiple items
        var apple = Page.GetByTestId("item-apple");
        var banana = Page.GetByTestId("item-banana");
        var orange = Page.GetByTestId("item-orange");

        await apple.ClickAsync();
        await banana.ClickAsync();
        await orange.ClickAsync();

        // All should be selected
        await Expect(apple).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(banana).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(orange).ToHaveAttributeAsync("aria-selected", "true");

        // Badges should be visible
        await Expect(Page.GetByTestId("badge-apple")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("badge-banana")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("badge-orange")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DropdownStaysOpen_AfterSelection()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        var apple = Page.GetByTestId("item-apple");
        await apple.ClickAsync();

        // Dropdown should still be open for multi-select
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task SelectionCount_UpdatesCorrectly()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var selectionCount = Page.GetByTestId("selection-count");
        await Expect(selectionCount).ToContainTextAsync("0 selected");

        // Select items
        await Page.GetByTestId("item-apple").ClickAsync();
        await Expect(selectionCount).ToContainTextAsync("1 selected");

        await Page.GetByTestId("item-banana").ClickAsync();
        await Expect(selectionCount).ToContainTextAsync("2 selected");

        await Page.GetByTestId("item-orange").ClickAsync();
        await Expect(selectionCount).ToContainTextAsync("3 selected");
    }

    [Test]
    public async Task DeselectingItem_UpdatesSelectionCount()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Select two items
        await Page.GetByTestId("item-apple").ClickAsync();
        await Page.GetByTestId("item-banana").ClickAsync();

        var selectionCount = Page.GetByTestId("selection-count");
        await Expect(selectionCount).ToContainTextAsync("2 selected");

        // Deselect one
        await Page.GetByTestId("item-apple").ClickAsync();
        await Expect(selectionCount).ToContainTextAsync("1 selected");
    }

    #endregion

    #region Badge Interaction

    [Test]
    public async Task Badge_ShowsCorrectLabel()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await Page.GetByTestId("item-apple").ClickAsync();

        var badge = Page.GetByTestId("badge-apple");
        await Expect(badge).ToContainTextAsync("Apple");
    }

    [Test]
    public async Task Badge_RemoveButton_DeselectsItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await Page.GetByTestId("item-apple").ClickAsync();

        // Close dropdown
        await Page.Keyboard.PressAsync("Escape");

        // Click remove button
        await Page.GetByTestId("remove-apple").ClickAsync();

        // Badge should be removed
        await Expect(Page.GetByTestId("badge-apple")).Not.ToBeVisibleAsync();

        // Selection count should update
        var selectionCount = Page.GetByTestId("selection-count");
        await Expect(selectionCount).ToContainTextAsync("0 selected");
    }

    [Test]
    public async Task Badge_RemoveButton_UpdatesItemState_WhenDropdownReopened()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await Page.GetByTestId("item-apple").ClickAsync();

        // Close dropdown
        await Page.Keyboard.PressAsync("Escape");

        // Remove via badge
        await Page.GetByTestId("remove-apple").ClickAsync();

        // Reopen dropdown
        await input.ClickAsync();

        // Item should show as not selected
        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("aria-selected", "false");
    }

    #endregion

    #region Item State Attributes

    [Test]
    public async Task SelectedItem_HasDataSelectedAttribute()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).Not.ToHaveAttributeAsync("data-selected", "");

        await apple.ClickAsync();
        await Expect(apple).ToHaveAttributeAsync("data-selected", "");
    }

    [Test]
    public async Task HighlightedItem_HasDataHighlightedAttribute()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // First item should be highlighted on open
        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task MouseEnter_HighlightsItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var banana = Page.GetByTestId("item-banana");
        await banana.HoverAsync();

        await Expect(banana).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task OnlyOneItem_CanBeHighlighted_AtATime()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var apple = Page.GetByTestId("item-apple");
        var banana = Page.GetByTestId("item-banana");

        // First item highlighted on open
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");

        // Hover over banana
        await banana.HoverAsync();

        // Only banana should be highlighted
        await Expect(banana).ToHaveAttributeAsync("data-highlighted", "");
        await Expect(apple).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion
}
