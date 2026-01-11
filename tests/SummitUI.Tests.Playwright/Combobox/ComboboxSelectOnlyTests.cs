namespace SummitUI.Tests.Playwright.Combobox;

/// <summary>
/// Tests for Combobox select-only mode (no input, trigger-based).
/// Uses the select-only test page where the trigger has the combobox role.
/// </summary>
public class ComboboxSelectOnlyTests : SummitTestBase
{
    protected override string TestPagePath => "tests/combobox/select-only";

    #region ARIA Attributes on Trigger (Select-Only Mode)

    [Test]
    public async Task Trigger_ShouldHave_RoleCombobox_InSelectOnlyMode()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("role", "combobox");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaHaspopupListbox()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "listbox");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.GetByTestId("trigger");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        var contentId = await content.GetAttributeAsync("id");

        await Assert.That(ariaControls).IsNotNull();
        await Assert.That(ariaControls).IsEqualTo(contentId);
    }

    [Test]
    public async Task Trigger_ShouldBeFocusable()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("tabindex", "0");
    }

    #endregion

    #region Opening and Closing

    [Test]
    public async Task Combobox_ShouldOpen_OnClick()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldOpen_OnArrowDownKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldOpen_OnArrowUpKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowUp");

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldClose_OnEscapeKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Combobox_ShouldClose_OnTabKey()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Tab");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Keyboard Navigation (Select-Only Mode)

    [Test]
    public async Task ArrowDown_ShouldHighlightNextItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // First item should be highlighted on open
        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown
        await Page.Keyboard.PressAsync("ArrowDown");

        var banana = Page.GetByTestId("item-banana");
        await Expect(banana).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldHighlightPreviousItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Navigate to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Go back up
        await Page.Keyboard.PressAsync("ArrowUp");

        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Home_ShouldHighlightFirstItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Navigate down
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");

        // Press Home
        await Page.Keyboard.PressAsync("Home");

        var apple = Page.GetByTestId("item-apple");
        await Expect(apple).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task End_ShouldHighlightLastItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Press End
        await Page.Keyboard.PressAsync("End");

        var mango = Page.GetByTestId("item-mango");
        await Expect(mango).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Enter_ShouldToggleHighlightedItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Select first item
        var apple = Page.GetByTestId("item-apple");
        await Page.Keyboard.PressAsync("Enter");

        await Expect(apple).ToHaveAttributeAsync("aria-selected", "true");

        // Content stays open for multi-select
        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Space_ShouldToggleHighlightedItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Select first item
        var apple = Page.GetByTestId("item-apple");
        await Page.Keyboard.PressAsync(" ");

        await Expect(apple).ToHaveAttributeAsync("aria-selected", "true");
    }

    #endregion

    #region Multi-Select in Select-Only Mode

    [Test]
    public async Task CanSelectMultipleItems_InSelectOnlyMode()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Select first item
        await Page.Keyboard.PressAsync("Enter");
        // Navigate and select second
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("Enter");

        var apple = Page.GetByTestId("item-apple");
        var banana = Page.GetByTestId("item-banana");
        await Expect(apple).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(banana).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task SelectionCount_DisplaysInTrigger()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Select two items
        await Page.GetByTestId("item-apple").ClickAsync();
        await Page.GetByTestId("item-banana").ClickAsync();

        // Close dropdown
        await Page.Keyboard.PressAsync("Escape");

        // Trigger should show selection count
        await Expect(trigger).ToContainTextAsync("2 selected");
    }

    [Test]
    public async Task Trigger_ShowsPlaceholder_WhenNoSelection()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToContainTextAsync("Select fruits...");
    }

    #endregion

    #region Content ARIA Attributes (Select-Only Mode)

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTriggerId()
    {
        var trigger = Page.GetByTestId("trigger");
        var triggerId = await trigger.GetAttributeAsync("id");

        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("aria-labelledby", triggerId!);
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaActivedescendant_WhenItemHighlighted()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Navigate to highlight an item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Trigger should have aria-activedescendant
        await Expect(trigger).ToHaveAttributeAsync("aria-activedescendant", new System.Text.RegularExpressions.Regex(".+"));
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Focus_ShouldReturnToTrigger_AfterEscapeClose()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Combobox_ShouldClose_OnOutsideClick()
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
}
