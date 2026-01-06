namespace SummitUI.Tests.Playwright.Checkbox;

/// <summary>
/// Tests for Checkbox indeterminate state behavior.
/// Verifies the "select all" pattern with parent/child checkboxes.
/// </summary>
public class CheckboxIndeterminateTests : SummitTestBase
{
    protected override string TestPagePath => "tests/checkbox/indeterminate";

    #region Indeterminate State Attributes

    [Test]
    public async Task Checkbox_ShouldHave_AriaCheckedMixed_WhenIndeterminate()
    {
        var selectAllCheckbox = Page.GetByTestId("select-all-checkbox");
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("aria-checked", "mixed");
    }

    [Test]
    public async Task Checkbox_ShouldHave_DataStateIndeterminate_WhenIndeterminate()
    {
        var selectAllCheckbox = Page.GetByTestId("select-all-checkbox");
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "indeterminate");
    }

    [Test]
    public async Task Click_ShouldClearIndeterminateState()
    {
        var checkbox = Page.GetByTestId("select-all-checkbox");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "mixed");

        await checkbox.ClickAsync();

        // Should no longer be indeterminate
        var ariaChecked = await checkbox.GetAttributeAsync("aria-checked");
        await Assert.That(ariaChecked).IsNotEqualTo("mixed");
    }

    #endregion

    #region Select All Behavior

    [Test]
    public async Task SelectAll_ShouldCheckAllItems_WhenIndeterminate()
    {
        var selectAllCheckbox = Page.GetByTestId("select-all-checkbox");
        
        // Click select all
        await selectAllCheckbox.ClickAsync();

        // All item checkboxes should be checked
        var item1Checkbox = Page.GetByTestId("item1-checkbox");
        var item2Checkbox = Page.GetByTestId("item2-checkbox");
        var item3Checkbox = Page.GetByTestId("item3-checkbox");

        await Expect(item1Checkbox).ToHaveAttributeAsync("data-state", "checked");
        await Expect(item2Checkbox).ToHaveAttributeAsync("data-state", "checked");
        await Expect(item3Checkbox).ToHaveAttributeAsync("data-state", "checked");

        // Select all should now be checked (not indeterminate)
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task SelectAll_ShouldUncheckAllItems_WhenChecked()
    {
        var selectAllCheckbox = Page.GetByTestId("select-all-checkbox");
        
        // Click select all to check all items
        await selectAllCheckbox.ClickAsync();
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "checked");

        // Click again to uncheck all
        await selectAllCheckbox.ClickAsync();

        // All item checkboxes should be unchecked
        var item1Checkbox = Page.GetByTestId("item1-checkbox");
        var item2Checkbox = Page.GetByTestId("item2-checkbox");
        var item3Checkbox = Page.GetByTestId("item3-checkbox");

        await Expect(item1Checkbox).ToHaveAttributeAsync("data-state", "unchecked");
        await Expect(item2Checkbox).ToHaveAttributeAsync("data-state", "unchecked");
        await Expect(item3Checkbox).ToHaveAttributeAsync("data-state", "unchecked");

        // Select all should now be unchecked
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task SelectAll_ShouldBecomeIndeterminate_WhenSomeItemsChecked()
    {
        var selectAllCheckbox = Page.GetByTestId("select-all-checkbox");
        
        // First check all items
        await selectAllCheckbox.ClickAsync();
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "checked");

        // Now uncheck one item
        var firstItemCheckbox = Page.GetByTestId("item1-checkbox");
        await firstItemCheckbox.ClickAsync();

        // Select all should now be indeterminate
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "indeterminate");
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("aria-checked", "mixed");
    }

    #endregion

    #region Tab Navigation

    [Test]
    public async Task Tab_ShouldNavigateBetweenCheckboxes()
    {
        var item1Checkbox = Page.GetByTestId("item1-checkbox");
        var item2Checkbox = Page.GetByTestId("item2-checkbox");

        await item1Checkbox.FocusAsync();
        await Expect(item1Checkbox).ToBeFocusedAsync();

        // Tab to next focusable element
        await Page.Keyboard.PressAsync("Tab");

        // Second checkbox should be focused
        await Expect(item2Checkbox).ToBeFocusedAsync();
    }

    [Test]
    public async Task ShiftTab_ShouldNavigateBackwards()
    {
        var item1Checkbox = Page.GetByTestId("item1-checkbox");
        var item2Checkbox = Page.GetByTestId("item2-checkbox");

        await item2Checkbox.FocusAsync();
        await Expect(item2Checkbox).ToBeFocusedAsync();

        // Shift+Tab to previous element
        await Page.Keyboard.PressAsync("Shift+Tab");

        // First checkbox should be focused
        await Expect(item1Checkbox).ToBeFocusedAsync();
    }

    #endregion
}
