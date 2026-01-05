namespace SummitUI.Tests.Playwright.Select;

/// <summary>
/// Tests for Select with grouped items.
/// Verifies group labels, ARIA attributes for groups, and navigation behavior.
/// </summary>
public class SelectGroupedTests : SummitTestBase
{
    protected override string TestPagePath => "tests/select/grouped";

    #region Group Labels

    [Test]
    public async Task GroupLabel_ShouldBeVisible()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        var fruitsLabel = Page.GetByTestId("label-fruits");
        await Expect(fruitsLabel).ToBeVisibleAsync();
        await Expect(fruitsLabel).ToHaveTextAsync("Fruits");
    }

    [Test]
    public async Task AllGroupLabels_ShouldBeVisible()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        var fruitsLabel = Page.GetByTestId("label-fruits");
        var vegetablesLabel = Page.GetByTestId("label-vegetables");

        await Expect(fruitsLabel).ToBeVisibleAsync();
        await Expect(vegetablesLabel).ToBeVisibleAsync();
    }

    [Test]
    public async Task GroupLabels_ShouldNotBeSelectable()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Try to click on group label
        var fruitsLabel = Page.GetByTestId("label-fruits");
        await fruitsLabel.ClickAsync(new() { Force = true });

        // Content should still be open (label was not selected)
        await Expect(content).ToBeVisibleAsync();
    }

    #endregion

    #region Navigation with Groups

    [Test]
    public async Task ArrowDown_ShouldSkipGroupLabels()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // On open, first selectable item (Apple) should be highlighted, not the group label
        var appleItem = Page.GetByTestId("item-apple");
        await Expect(appleItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowDown_ShouldNavigate_ThroughItemsInGroup()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // First item (Apple) should be highlighted
        var appleItem = Page.GetByTestId("item-apple");
        await Expect(appleItem).ToHaveAttributeAsync("data-highlighted", "");

        // Navigate to second item (Banana)
        await Page.Keyboard.PressAsync("ArrowDown");
        var bananaItem = Page.GetByTestId("item-banana");
        await Expect(bananaItem).ToHaveAttributeAsync("data-highlighted", "");

        // Navigate to third item (Orange)
        await Page.Keyboard.PressAsync("ArrowDown");
        var orangeItem = Page.GetByTestId("item-orange");
        await Expect(orangeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowDown_ShouldCrossGroups_SkippingLabels()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate through all fruits to reach vegetables
        await Page.Keyboard.PressAsync("ArrowDown"); // Banana
        await Page.Keyboard.PressAsync("ArrowDown"); // Orange
        await Page.Keyboard.PressAsync("ArrowDown"); // Should skip "Vegetables" label and go to Carrot

        var carrotItem = Page.GetByTestId("item-carrot");
        await Expect(carrotItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldCrossGroups_SkippingLabels()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to a vegetable first
        await Page.Keyboard.PressAsync("ArrowDown"); // Banana
        await Page.Keyboard.PressAsync("ArrowDown"); // Orange
        await Page.Keyboard.PressAsync("ArrowDown"); // Carrot

        var carrotItem = Page.GetByTestId("item-carrot");
        await Expect(carrotItem).ToHaveAttributeAsync("data-highlighted", "");

        // Now go back up - should skip "Vegetables" label and go to Orange
        await Page.Keyboard.PressAsync("ArrowUp");
        var orangeItem = Page.GetByTestId("item-orange");
        await Expect(orangeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Home_ShouldGoToFirstItem_SkippingLabels()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to a vegetable
        await Page.Keyboard.PressAsync("End");
        var lastItem = Page.GetByTestId("item-spinach");
        await Expect(lastItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press Home - should go to first selectable item (Apple), skipping "Fruits" label
        await Page.Keyboard.PressAsync("Home");
        var appleItem = Page.GetByTestId("item-apple");
        await Expect(appleItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task End_ShouldGoToLastItem_InLastGroup()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Press End - should go to last selectable item (Spinach in Vegetables group)
        await Page.Keyboard.PressAsync("End");
        var spinachItem = Page.GetByTestId("item-spinach");
        await Expect(spinachItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Selection in Groups

    [Test]
    public async Task Click_ShouldSelectItem_InFirstGroup()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        var bananaItem = Page.GetByTestId("item-banana");
        await bananaItem.ClickAsync();

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        await Expect(trigger).ToContainTextAsync("Banana");
    }

    [Test]
    public async Task Click_ShouldSelectItem_InSecondGroup()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        var broccoliItem = Page.GetByTestId("item-broccoli");
        await broccoliItem.ClickAsync();

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        await Expect(trigger).ToContainTextAsync("Broccoli");
    }

    [Test]
    public async Task Enter_ShouldSelectHighlightedItem_InGroup()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to vegetable group
        await Page.Keyboard.PressAsync("ArrowDown"); // Banana
        await Page.Keyboard.PressAsync("ArrowDown"); // Orange
        await Page.Keyboard.PressAsync("ArrowDown"); // Carrot
        await Page.Keyboard.PressAsync("ArrowDown"); // Broccoli

        var broccoliItem = Page.GetByTestId("item-broccoli");
        await Expect(broccoliItem).ToHaveAttributeAsync("data-highlighted", "");

        // Select with Enter
        await Page.Keyboard.PressAsync("Enter");

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        await Expect(trigger).ToContainTextAsync("Broccoli");
    }

    #endregion

    #region ARIA Attributes for Groups

    [Test]
    public async Task Group_ShouldHave_RoleGroup()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        var fruitsGroup = Page.GetByTestId("group-fruits");
        await Expect(fruitsGroup).ToHaveAttributeAsync("role", "group");
    }

    [Test]
    public async Task Group_ShouldHave_AriaLabelledBy_ReferencingLabel()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        var fruitsGroup = Page.GetByTestId("group-fruits");
        var fruitsLabel = Page.GetByTestId("label-fruits");

        var labelId = await fruitsLabel.GetAttributeAsync("id");
        await Expect(fruitsGroup).ToHaveAttributeAsync("aria-labelledby", labelId!);
    }

    #endregion

    #region Typeahead in Groups

    [Test]
    public async Task Typeahead_ShouldWork_AcrossGroups()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type 'c' to match Carrot (in Vegetables group)
        await Page.Keyboard.TypeAsync("c", new() { Delay = 50 });

        var carrotItem = Page.GetByTestId("item-carrot");
        await Expect(carrotItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Typeahead_ShouldMatch_FirstOccurrence()
    {
        var trigger = Page.GetByTestId("trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("content");
        await Expect(content).ToBeVisibleAsync();

        // Type 'b' which matches both Banana (fruits) and Broccoli (vegetables)
        // Should match Banana first
        await Page.Keyboard.TypeAsync("b", new() { Delay = 50 });

        var bananaItem = Page.GetByTestId("item-banana");
        await Expect(bananaItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion
}
