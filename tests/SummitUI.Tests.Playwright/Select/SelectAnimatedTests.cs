namespace SummitUI.Tests.Playwright.Select;

/// <summary>
/// Tests for Select component with CSS animations.
/// Verifies that keyboard navigation and interaction work correctly with animated content.
/// </summary>
public class SelectAnimatedTests : SummitTestBase
{
    protected override string TestPagePath => "tests/select/animated";

    #region Open/Close with Animation

    [Test]
    public async Task AnimatedSelect_ShouldOpen_OnClick()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task AnimatedSelect_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task AnimatedSelect_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task AnimatedSelect_ShouldOpen_OnArrowDown()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task AnimatedSelect_ShouldClose_OnEscape()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Press Escape to close
        await Page.Keyboard.PressAsync("Escape");

        // Content should close (after animation completes)
        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    #endregion

    #region Arrow Key Navigation with Animation

    [Test]
    public async Task AnimatedSelect_ArrowDown_ShouldHighlightNextItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // First item should be highlighted on open
        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");

        // Arrow down to second item
        await Page.Keyboard.PressAsync("ArrowDown");
        var secondItem = Page.GetByTestId("item-second");
        await Expect(secondItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task AnimatedSelect_ArrowUp_ShouldHighlightPreviousItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate down first
        await Page.Keyboard.PressAsync("ArrowDown");

        // Then back up
        await Page.Keyboard.PressAsync("ArrowUp");

        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task AnimatedSelect_ShouldNavigate_ThroughAllItems()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // First item should be highlighted on open
        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");

        // Arrow down to second item
        await Page.Keyboard.PressAsync("ArrowDown");
        var secondItem = Page.GetByTestId("item-second");
        await Expect(secondItem).ToHaveAttributeAsync("data-highlighted", "");

        // Arrow down to third item
        await Page.Keyboard.PressAsync("ArrowDown");
        var thirdItem = Page.GetByTestId("item-third");
        await Expect(thirdItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task AnimatedSelect_Home_ShouldHighlightFirstItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to last item
        await Page.Keyboard.PressAsync("End");

        // Press Home
        await Page.Keyboard.PressAsync("Home");

        var firstItem = Page.GetByTestId("item-first");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task AnimatedSelect_End_ShouldHighlightLastItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Press End
        await Page.Keyboard.PressAsync("End");

        var thirdItem = Page.GetByTestId("item-third");
        await Expect(thirdItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Selection with Animation

    [Test]
    public async Task AnimatedSelect_ShouldSelect_OnEnterKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to second item and select it
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("Enter");

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        var selectedValue = Page.GetByTestId("selected-value");
        await Expect(selectedValue).ToContainTextAsync("second");
    }

    [Test]
    public async Task AnimatedSelect_ShouldSelect_OnClick()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Click on second item
        var secondItem = Page.GetByTestId("item-second");
        await secondItem.ClickAsync();

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        var selectedValue = Page.GetByTestId("selected-value");
        await Expect(selectedValue).ToContainTextAsync("second");
    }

    [Test]
    public async Task AnimatedSelect_TriggerShouldShowSelectedValue()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Select third item
        var thirdItem = Page.GetByTestId("item-third");
        await thirdItem.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();

        // Trigger should show selected value text
        await Expect(trigger).ToContainTextAsync("Third");
    }

    #endregion

    #region Focus Management with Animation

    [Test]
    public async Task AnimatedSelect_FocusShouldReturnToTrigger_AfterEscape()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task AnimatedSelect_FocusShouldReturnToTrigger_AfterSelection()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Enter"); // Select first item

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    #endregion

    #region Animation State Attributes

    [Test]
    public async Task Content_ShouldHaveDataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Trigger_ShouldHaveDataStateClosed_Initially()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHaveDataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Trigger_ShouldHaveDataStateClosed_AfterClose()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    #endregion

    #region Outside Click with Animation

    [Test]
    public async Task AnimatedSelect_ShouldClose_OnOutsideClick()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Click outside
        await Page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Tab Behavior with Animation

    [Test]
    public async Task AnimatedSelect_ShouldClose_OnTab()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Tab");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion
}
