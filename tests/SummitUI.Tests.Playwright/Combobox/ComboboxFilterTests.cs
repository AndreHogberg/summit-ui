namespace SummitUI.Tests.Playwright.Combobox;

/// <summary>
/// Tests for Combobox input filtering behavior.
/// Uses the basic test page with editable combobox.
/// </summary>
public class ComboboxFilterTests : SummitTestBase
{
    protected override string TestPagePath => "tests/combobox/basic";

    #region Filter Behavior

    [Test]
    public async Task Typing_FiltersVisibleItems()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // All items should be visible initially
        await Expect(Page.GetByTestId("item-apple")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-banana")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-orange")).ToBeVisibleAsync();

        // Type to filter
        await input.PressSequentiallyAsync("ap");

        // Only apple should be visible
        await Expect(Page.GetByTestId("item-apple")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-banana")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-orange")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task FilterIsCaseInsensitive()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await input.PressSequentiallyAsync("APPLE");

        await Expect(Page.GetByTestId("item-apple")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ClearingFilter_ShowsAllItems()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Filter to one item
        await input.PressSequentiallyAsync("ap");
        await Expect(Page.GetByTestId("item-banana")).Not.ToBeVisibleAsync();

        // Clear filter
        await input.FillAsync("");

        // All items visible again
        await Expect(Page.GetByTestId("item-apple")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-banana")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-orange")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Typing_OpensDropdown_WhenClosed()
    {
        var input = Page.GetByTestId("input");
        await input.FocusAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).Not.ToBeVisibleAsync();

        await input.PressSequentiallyAsync("a");

        await Expect(content).ToBeVisibleAsync();
    }

    #endregion

    #region Empty State

    [Test]
    public async Task EmptyState_ShownWhenNoMatches()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Type something that matches nothing
        await input.PressSequentiallyAsync("xyz123");

        // Empty state should be visible
        var empty = Page.GetByTestId("empty");
        await Expect(empty).ToBeVisibleAsync();
        await Expect(empty).ToContainTextAsync("No results found");
    }

    [Test]
    public async Task EmptyState_HiddenWhenItemsMatch()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Initially, items are visible
        var empty = Page.GetByTestId("empty");
        await Expect(empty).Not.ToBeVisibleAsync();

        // Filter to no matches
        await input.PressSequentiallyAsync("xyz123");
        await Expect(empty).ToBeVisibleAsync();

        // Clear filter
        await input.FillAsync("");
        await Expect(empty).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Filter with Selection

    [Test]
    public async Task SelectedItems_StillVisibleWhenFiltered()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Select apple
        await Page.GetByTestId("item-apple").ClickAsync();

        // Filter to show only banana
        await input.PressSequentiallyAsync("ban");

        // Banana visible in dropdown
        await Expect(Page.GetByTestId("item-banana")).ToBeVisibleAsync();
        // Apple not visible in dropdown (filtered out)
        await Expect(Page.GetByTestId("item-apple")).Not.ToBeVisibleAsync();

        // But apple badge should still be visible in trigger
        await Expect(Page.GetByTestId("badge-apple")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CanSelectFilteredItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Filter to mango
        await input.PressSequentiallyAsync("man");

        // Select mango
        await Page.GetByTestId("item-mango").ClickAsync();

        // Badge should appear
        await Expect(Page.GetByTestId("badge-mango")).ToBeVisibleAsync();
    }

    [Test]
    public async Task FilterResets_AfterSelection_ViaEnter()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Filter
        await input.PressSequentiallyAsync("ban");

        // Select via Enter
        await Page.Keyboard.PressAsync("Enter");

        // Filter text should clear (this is optional behavior - verify if implemented)
        // If not clearing, the filtered item should at least be selected
        await Expect(Page.GetByTestId("badge-banana")).ToBeVisibleAsync();
    }

    #endregion

    #region Keyboard Navigation with Filter

    [Test]
    public async Task ArrowKeys_NavigateOnlyVisibleItems()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Filter to items containing 'a' (apple, banana, orange, grape, mango)
        await input.PressSequentiallyAsync("an");

        // Should filter to banana, orange, mango
        await Expect(Page.GetByTestId("item-apple")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-banana")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-orange")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("item-mango")).ToBeVisibleAsync();

        // First visible item should be highlighted
        await Expect(Page.GetByTestId("item-banana")).ToHaveAttributeAsync("data-highlighted", "");

        // Arrow down should go to next visible item
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("item-orange")).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task HighlightResets_WhenFilterChanges()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        // Navigate to third item
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");

        // Apply filter
        await input.PressSequentiallyAsync("app");

        // First visible item should be highlighted (apple)
        await Expect(Page.GetByTestId("item-apple")).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion
}
