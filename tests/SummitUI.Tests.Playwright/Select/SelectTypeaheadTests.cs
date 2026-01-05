namespace SummitUI.Tests.Playwright.Select;

/// <summary>
/// Tests for Select typeahead/search functionality.
/// Uses a dedicated test page with many items for typeahead testing.
/// </summary>
public class SelectTypeaheadTests : SummitTestBase
{
    protected override string TestPagePath => "tests/select/typeahead";

    #region Single Character Typeahead

    [Test]
    public async Task Typeahead_ShouldHighlight_ItemStartingWithTypedCharacter()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type 'g' to find grape
        await Page.Keyboard.TypeAsync("g", new() { Delay = 50 });

        // Grape should be highlighted
        var grapeItem = Page.GetByTestId("item-grape");
        await Expect(grapeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Typeahead_ShouldHighlight_FirstMatchingItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type 'p' - should match 'papaya' first (first 'p' item in the list)
        await Page.Keyboard.TypeAsync("p", new() { Delay = 50 });

        var papayaItem = Page.GetByTestId("item-papaya");
        await Expect(papayaItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Multi-Character Typeahead

    [Test]
    public async Task Typeahead_ShouldMatch_MultipleCharacters()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Wait for initial highlight to be applied (first item)
        var firstItem = Page.GetByTestId("item-apple");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");

        // Type 'ma' to find mango (not matching apple)
        await Page.Keyboard.TypeAsync("ma", new() { Delay = 100 });

        // Mango should be highlighted
        var mangoItem = Page.GetByTestId("item-mango");
        await Expect(mangoItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Typeahead_ShouldMatch_LongerPrefix()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type 'blu' to match blueberry specifically
        await Page.Keyboard.TypeAsync("blu", new() { Delay = 100 });

        var blueberryItem = Page.GetByTestId("item-blueberry");
        await Expect(blueberryItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Typeahead_ShouldDistinguish_BetweenGrapeAndGrapefruit()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type 'grapef' to specifically match grapefruit
        await Page.Keyboard.TypeAsync("grapef", new() { Delay = 100 });

        var grapefruitItem = Page.GetByTestId("item-grapefruit");
        await Expect(grapefruitItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Typeahead with Selection

    [Test]
    public async Task Typeahead_ShouldAllowSelection_AfterHighlighting()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type to highlight strawberry
        await Page.Keyboard.TypeAsync("str", new() { Delay = 100 });

        var strawberryItem = Page.GetByTestId("item-strawberry");
        await Expect(strawberryItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press Enter to select
        await Page.Keyboard.PressAsync("Enter");

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        await Expect(trigger).ToContainTextAsync("Strawberry");
    }

    #endregion

    #region Typeahead Edge Cases

    [Test]
    public async Task Typeahead_ShouldBeCase_Insensitive()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type uppercase 'O' to find orange
        await Page.Keyboard.TypeAsync("O", new() { Delay = 50 });

        var orangeItem = Page.GetByTestId("item-orange");
        await Expect(orangeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Typeahead_ShouldNotMatch_NonExistentPrefix()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // First item should be highlighted on open
        var firstItem = Page.GetByTestId("item-apple");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");

        // Type 'xyz' which doesn't match any item
        await Page.Keyboard.TypeAsync("xyz", new() { Delay = 100 });

        // First item should still be highlighted (no match found)
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Typeahead Buffer Reset

    [Test]
    public async Task Typeahead_ShouldResetBuffer_AfterDelay()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type 'a' to match apple
        await Page.Keyboard.TypeAsync("a", new() { Delay = 50 });
        var appleItem = Page.GetByTestId("item-apple");
        await Expect(appleItem).ToHaveAttributeAsync("data-highlighted", "");

        // Wait for buffer to reset (usually ~500ms-1s)
        await Page.WaitForTimeoutAsync(1500);

        // Type 'b' - should match banana (not 'ab' which wouldn't match)
        await Page.Keyboard.TypeAsync("b", new() { Delay = 50 });
        var bananaItem = Page.GetByTestId("item-banana");
        await Expect(bananaItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Typeahead with Arrow Keys

    [Test]
    public async Task Typeahead_ShouldWork_AfterArrowKeyNavigation()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate down a few items with arrow keys
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");

        // Now type to search
        await Page.Keyboard.TypeAsync("w", new() { Delay = 50 });

        // Watermelon should be highlighted
        var watermelonItem = Page.GetByTestId("item-watermelon");
        await Expect(watermelonItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowKeys_ShouldWork_AfterTypeahead()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type to highlight mango
        await Page.Keyboard.TypeAsync("man", new() { Delay = 100 });
        var mangoItem = Page.GetByTestId("item-mango");
        await Expect(mangoItem).ToHaveAttributeAsync("data-highlighted", "");

        // Navigate down from mango with arrow key
        await Page.Keyboard.PressAsync("ArrowDown");

        // Next item after mango (orange) should be highlighted
        var orangeItem = Page.GetByTestId("item-orange");
        await Expect(orangeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion
}
