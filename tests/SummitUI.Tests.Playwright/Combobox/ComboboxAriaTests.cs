namespace SummitUI.Tests.Playwright.Combobox;

/// <summary>
/// Tests for Combobox ARIA attributes and basic accessibility compliance.
/// Uses the basic test page with an editable combobox (with input).
/// </summary>
public class ComboboxAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/combobox/basic";

    #region ARIA Attributes on Input (Combobox Role)

    [Test]
    public async Task Input_ShouldHave_RoleCombobox()
    {
        var input = Page.GetByTestId("input");
        await Expect(input).ToHaveAttributeAsync("role", "combobox");
    }

    [Test]
    public async Task Input_ShouldHave_AriaHaspopupListbox()
    {
        var input = Page.GetByTestId("input");
        await Expect(input).ToHaveAttributeAsync("aria-haspopup", "listbox");
    }

    [Test]
    public async Task Input_ShouldHave_AriaAutocompleteList()
    {
        var input = Page.GetByTestId("input");
        await Expect(input).ToHaveAttributeAsync("aria-autocomplete", "list");
    }

    [Test]
    public async Task Input_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var input = Page.GetByTestId("input");
        await Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Input_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await Expect(input).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Input_ShouldHave_AriaControls_MatchingContentId()
    {
        var input = Page.GetByTestId("input");
        var ariaControls = await input.GetAttributeAsync("aria-controls");

        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        var contentId = await content.GetAttributeAsync("id");

        await Assert.That(ariaControls).IsNotNull();
        await Assert.That(ariaControls).IsEqualTo(contentId);
    }

    [Test]
    public async Task Input_ShouldHave_DataStateClosed_WhenClosed()
    {
        var input = Page.GetByTestId("input");
        await Expect(input).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Input_ShouldHave_DataStateOpen_WhenOpen()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await Expect(input).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region ARIA Attributes on Trigger

    [Test]
    public async Task Trigger_ShouldHave_DataPlaceholder_WhenNoSelection()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-placeholder", "");
    }

    [Test]
    public async Task Trigger_ShouldNotHave_DataPlaceholder_AfterSelection()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        // Wait for selection to register
        await Page.WaitForTimeoutAsync(100);

        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).Not.ToHaveAttributeAsync("data-placeholder", "");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateAttribute()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    #endregion

    #region ARIA Attributes on Content

    [Test]
    public async Task Content_ShouldHave_RoleListbox()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("role", "listbox");
    }

    [Test]
    public async Task Content_ShouldHave_AriaMultiselectableTrue()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("aria-multiselectable", "true");
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexNegativeOne()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingInputId()
    {
        var input = Page.GetByTestId("input");
        var inputId = await input.GetAttributeAsync("id");

        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("aria-labelledby", inputId!);
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region ARIA Attributes on Items

    [Test]
    public async Task Item_ShouldHave_RoleOption()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await Expect(item).ToHaveAttributeAsync("role", "option");
    }

    [Test]
    public async Task Item_ShouldHave_AriaSelectedFalse_WhenNotSelected()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await Expect(item).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Item_ShouldHave_AriaSelectedTrue_WhenSelected()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        // Content stays open for multi-select
        await Expect(item).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateUnchecked_WhenNotSelected()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await Expect(item).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateChecked_WhenSelected()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        await Expect(item).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Items_ShouldHave_UniqueIds()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var items = Page.Locator("[data-summit-combobox-item]");
        var count = await items.CountAsync();

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await items.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    #endregion

    #region Selection Behavior

    [Test]
    public async Task Click_ShouldToggleItem_NotCloseDropdown()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-banana");
        await item.ClickAsync();

        // Dropdown should stay open for multi-select
        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Item should be selected
        await Expect(item).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task MultipleItems_CanBeSelected()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var apple = Page.GetByTestId("item-apple");
        var banana = Page.GetByTestId("item-banana");

        await apple.ClickAsync();
        await banana.ClickAsync();

        await Expect(apple).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(banana).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task SelectedItem_CanBeDeselected_ByClickingAgain()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");

        // Select
        await item.ClickAsync();
        await Expect(item).ToHaveAttributeAsync("aria-selected", "true");

        // Deselect
        await item.ClickAsync();
        await Expect(item).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task SelectedItem_ShowsBadge_InTrigger()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        var badge = Page.GetByTestId("badge-apple");
        await Expect(badge).ToBeVisibleAsync();
        await Expect(badge).ToContainTextAsync("Apple");
    }

    [Test]
    public async Task Badge_RemoveButton_DeselectsItem()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        // Close the dropdown
        await Page.Keyboard.PressAsync("Escape");

        // Click remove button on badge
        var removeButton = Page.GetByTestId("remove-apple");
        await removeButton.ClickAsync();

        // Badge should be gone
        var badge = Page.GetByTestId("badge-apple");
        await Expect(badge).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Focus_ShouldStayOnInput_AfterSelection()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        // Focus should return to input
        await Expect(input).ToBeFocusedAsync();
    }

    [Test]
    public async Task Focus_ShouldReturnToInput_AfterEscapeClose()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(input).ToBeFocusedAsync();
    }

    [Test]
    public async Task Combobox_ShouldClose_OnOutsideClick()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Click outside the combobox
        await Page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Content Positioning

    [Test]
    public async Task Content_ShouldBePositioned_BelowTrigger_ByDefault()
    {
        var input = Page.GetByTestId("input");
        await input.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Check that content is positioned below trigger
        var trigger = Page.GetByTestId("trigger");
        var triggerBox = await trigger.BoundingBoxAsync();
        var contentBox = await content.BoundingBoxAsync();

        await Assert.That(contentBox!.Y).IsGreaterThan(triggerBox!.Y);
    }

    #endregion
}
