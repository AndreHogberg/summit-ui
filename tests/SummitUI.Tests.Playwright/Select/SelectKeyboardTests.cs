namespace SummitUI.Tests.Playwright.Select;

/// <summary>
/// Tests for Select keyboard navigation.
/// Uses a minimal dedicated test page with multiple items for navigation testing.
/// </summary>
public class SelectKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/select/keyboard";

    #region Opening and Closing

    [Test]
    public async Task Select_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldOpen_OnArrowDownKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldOpen_OnArrowUpKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowUp");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldClose_OnEscapeKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldUpdateAriaExpanded_AfterEscapeClose()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Tab_ShouldCloseSelect()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

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
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // On open, first item is already highlighted
        // Press ArrowDown to move to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Second item should be highlighted
        var secondItem = Page.GetByTestId("item-second");
        await Expect(secondItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldHighlightPreviousItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Go to second item first
        await Page.Keyboard.PressAsync("ArrowDown");

        // Go back up to first item
        await Page.Keyboard.PressAsync("ArrowUp");

        // First item should be highlighted
        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Home_ShouldHighlightFirstItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate down a few items
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");

        // Press Home
        await Page.Keyboard.PressAsync("Home");

        // First item should be highlighted
        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task End_ShouldHighlightLastItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Press End
        await Page.Keyboard.PressAsync("End");

        // Last item should be highlighted
        var lastItem = Page.GetByTestId("item-fifth");
        await Expect(lastItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Item Selection

    [Test]
    public async Task Enter_ShouldSelectHighlightedItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Select it
        await Page.Keyboard.PressAsync("Enter");

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Selected value should be displayed in the trigger (shows the Label "Second Option")
        await Expect(trigger).ToContainTextAsync("Second");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaActivedescendant_WhenItemHighlighted()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to highlight an item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Trigger should have aria-activedescendant pointing to highlighted item
        await Expect(trigger).ToHaveAttributeAsync("aria-activedescendant", new System.Text.RegularExpressions.Regex(".+"));
    }

    #endregion
}
