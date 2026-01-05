namespace SummitUI.Tests.Playwright.Select;

/// <summary>
/// Tests for Select ARIA attributes and basic accessibility compliance.
/// Uses a minimal dedicated test page for isolation.
/// </summary>
public class SelectAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/select/basic";

    #region ARIA Attributes on Trigger

    [Test]
    public async Task Trigger_ShouldHave_RoleCombobox()
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
    public async Task Trigger_ShouldHave_DataStateClosed_WhenClosed()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataPlaceholder_WhenNoSelection()
    {
        var trigger = Page.GetByTestId("trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-placeholder", "");
    }

    #endregion

    #region ARIA Attributes on Content

    [Test]
    public async Task Content_ShouldHave_RoleListbox()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("role", "listbox");
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexNegativeOne()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }

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
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Content_ShouldHave_DataSideAttribute()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        var dataSide = await content.GetAttributeAsync("data-side");

        await Assert.That(dataSide).IsNotNull();
    }

    #endregion

    #region ARIA Attributes on Items

    [Test]
    public async Task Item_ShouldHave_RoleOption()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await Expect(item).ToHaveAttributeAsync("role", "option");
    }

    [Test]
    public async Task Item_ShouldHave_AriaSelectedFalse_WhenNotSelected()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await Expect(item).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Item_ShouldHave_AriaSelectedTrue_WhenSelected()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Select the first item
        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        // Wait for dropdown to close
        await Expect(Page.GetByTestId("content")).Not.ToBeVisibleAsync();

        // Wait for debounce protection to expire
        await Page.WaitForTimeoutAsync(200);

        // Reopen
        await trigger.ClickAsync();
        await Expect(Page.GetByTestId("content")).ToBeVisibleAsync();

        // Check the selected item
        await Expect(item).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateUnchecked_WhenNotSelected()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await Expect(item).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateChecked_WhenSelected()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Select the first item
        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        // Wait for dropdown to close
        await Expect(Page.GetByTestId("content")).Not.ToBeVisibleAsync();

        // Wait for debounce protection to expire
        await Page.WaitForTimeoutAsync(200);

        // Reopen
        await trigger.ClickAsync();
        await Expect(Page.GetByTestId("content")).ToBeVisibleAsync();

        // Check the selected item
        await Expect(item).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Items_ShouldHave_UniqueIds()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var items = Page.Locator("[data-summit-select-item]");
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

    #region Placeholder State

    [Test]
    public async Task Value_ShouldHave_DataPlaceholder_WhenNoSelection()
    {
        // The SelectValue component is inside the trigger
        var trigger = Page.GetByTestId("trigger");
        var value = trigger.Locator("[data-summit-select-value]");
        await Expect(value).ToBeVisibleAsync();
        await Expect(value).ToHaveAttributeAsync("data-placeholder", "");
    }

    [Test]
    public async Task Value_ShouldNotHave_DataPlaceholder_AfterSelection()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        // Wait for selection to complete
        await Expect(Page.GetByTestId("content")).Not.ToBeVisibleAsync();

        // The SelectValue component is inside the trigger
        var value = trigger.Locator("[data-summit-select-value]");
        await Expect(value).ToBeVisibleAsync();
        await Expect(value).Not.ToHaveAttributeAsync("data-placeholder", "");
    }

    #endregion

    #region Selection Behavior

    [Test]
    public async Task Click_ShouldSelectItem()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var item = Page.GetByTestId("item-banana");
        await item.ClickAsync();

        // Dropdown should close
        await Expect(Page.GetByTestId("content")).Not.ToBeVisibleAsync();

        // Value should show selected item (trigger contains the SelectValue span)
        await Expect(trigger).ToContainTextAsync("Banana");
    }

    [Test]
    public async Task SelectedItem_ShouldBeHighlighted_WhenReopened()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        // Select second item
        var item = Page.GetByTestId("item-banana");
        await item.ClickAsync();

        // Wait for close
        await Expect(Page.GetByTestId("content")).Not.ToBeVisibleAsync();

        // Wait for debounce protection to expire
        await Page.WaitForTimeoutAsync(200);

        // Reopen
        await trigger.ClickAsync();
        await Expect(Page.GetByTestId("content")).ToBeVisibleAsync();

        // Selected item should be highlighted
        await Expect(item).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Focus_ShouldReturnToTrigger_AfterSelection()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var item = Page.GetByTestId("item-apple");
        await item.ClickAsync();

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Focus_ShouldReturnToTrigger_AfterEscapeClose()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Select_ShouldClose_OnOutsideClick()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Click outside the select
        await Page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Content Positioning

    [Test]
    public async Task Content_ShouldBePositioned_BelowTrigger_ByDefault()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Check that content is positioned below trigger
        var triggerBox = await trigger.BoundingBoxAsync();
        var contentBox = await content.BoundingBoxAsync();

        await Assert.That(contentBox!.Y).IsGreaterThan(triggerBox!.Y);
    }

    #endregion
}
