using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the Checkbox component.
/// Tests ARIA attributes, keyboard navigation, and focus management.
/// </summary>
public class CheckboxAccessibilityTests : PageTest
{
    private const string CheckboxDemoUrl = "checkbox";

    [Before(Test)]
    public async Task NavigateToCheckboxDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + CheckboxDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes on Checkbox

    [Test]
    public async Task Checkbox_ShouldHave_RoleCheckbox()
    {
        var checkbox = Page.Locator("[data-ark-checkbox]").First;
        await Expect(checkbox).ToHaveAttributeAsync("role", "checkbox");
    }

    [Test]
    public async Task Checkbox_ShouldHave_TypeButton()
    {
        var checkbox = Page.Locator("[data-ark-checkbox]").First;
        await Expect(checkbox).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task Checkbox_ShouldHave_AriaCheckedFalse_WhenUnchecked()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-unchecked']");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Checkbox_ShouldHave_AriaCheckedTrue_WhenChecked()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-checked']");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Checkbox_ShouldHave_AriaCheckedMixed_WhenIndeterminate()
    {
        // The "Select All" checkbox should be indeterminate initially
        var selectAllCheckbox = Page.Locator("[data-testid='select-all-checkbox']");
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("aria-checked", "mixed");
    }

    [Test]
    public async Task Checkbox_ShouldHave_DataStateUnchecked_WhenUnchecked()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-unchecked']");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Checkbox_ShouldHave_DataStateChecked_WhenChecked()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-checked']");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Checkbox_ShouldHave_DataStateIndeterminate_WhenIndeterminate()
    {
        var checkbox = Page.Locator("[data-testid='select-all-checkbox']");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "indeterminate");
    }

    [Test]
    public async Task Checkbox_ShouldHave_UniqueId()
    {
        var checkboxes = Page.Locator("[data-ark-checkbox]");
        var count = await checkboxes.CountAsync();

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await checkboxes.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    #endregion

    #region Disabled Checkbox Accessibility

    [Test]
    public async Task DisabledCheckbox_ShouldHave_DisabledAttribute()
    {
        var disabledCheckbox = Page.Locator("[data-testid='disabled-unchecked']");
        await Expect(disabledCheckbox).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledCheckbox_ShouldHave_AriaDisabled()
    {
        var disabledCheckbox = Page.Locator("[data-testid='disabled-unchecked']");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledCheckbox_ShouldHave_DataDisabled()
    {
        var disabledCheckbox = Page.Locator("[data-testid='disabled-unchecked']");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledCheckbox_ShouldNotToggle_OnClick()
    {
        var disabledCheckbox = Page.Locator("[data-testid='disabled-unchecked']");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");

        // Force click on disabled checkbox
        await disabledCheckbox.ClickAsync(new() { Force = true });

        // Should still be unchecked
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    #endregion

    #region Toggle Behavior (Click)

    [Test]
    public async Task Click_ShouldCheckUncheckedCheckbox()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-unchecked']");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        await checkbox.ClickAsync();

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Click_ShouldUncheckCheckedCheckbox()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-checked']");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");

        await checkbox.ClickAsync();

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Click_ShouldClearIndeterminateState()
    {
        var checkbox = Page.Locator("[data-testid='select-all-checkbox']");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "mixed");

        await checkbox.ClickAsync();

        // Should no longer be indeterminate
        var ariaChecked = await checkbox.GetAttributeAsync("aria-checked");
        await Assert.That(ariaChecked).IsNotEqualTo("mixed");
    }

    #endregion

    #region Keyboard Navigation

    [Test]
    public async Task Space_ShouldToggleCheckbox()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-unchecked']");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        await checkbox.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Enter_ShouldNotToggleCheckbox()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-unchecked']");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        await checkbox.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Enter should NOT toggle checkboxes (only Space should per WAI-ARIA)
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Space_ShouldNotToggle_DisabledCheckbox()
    {
        var disabledCheckbox = Page.Locator("[data-testid='disabled-unchecked']");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");

        await disabledCheckbox.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Tab_ShouldNavigateBetweenCheckboxes()
    {
        // Use data-testid selectors to get checkboxes that are adjacent in tab order
        // The basic-checkbox-unchecked and basic-checkbox-checked have a native input between them,
        // so we use the item checkboxes which are adjacent
        var firstCheckbox = Page.Locator("[data-testid='item1-checkbox']");
        var secondCheckbox = Page.Locator("[data-testid='item2-checkbox']");

        await firstCheckbox.FocusAsync();
        await Expect(firstCheckbox).ToBeFocusedAsync();

        // Tab to next focusable element
        await Page.Keyboard.PressAsync("Tab");

        // Second checkbox should be focused
        await Expect(secondCheckbox).ToBeFocusedAsync();
    }

    [Test]
    public async Task ShiftTab_ShouldNavigateBackwards()
    {
        // Use data-testid selectors to get checkboxes that are adjacent in tab order
        // The basic-checkbox-unchecked and basic-checkbox-checked have a native input between them,
        // so we use the item checkboxes which are adjacent
        var firstCheckbox = Page.Locator("[data-testid='item1-checkbox']");
        var secondCheckbox = Page.Locator("[data-testid='item2-checkbox']");

        await secondCheckbox.FocusAsync();
        await Expect(secondCheckbox).ToBeFocusedAsync();

        // Shift+Tab to previous element
        await Page.Keyboard.PressAsync("Shift+Tab");

        // First checkbox should be focused
        await Expect(firstCheckbox).ToBeFocusedAsync();
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Checkbox_ShouldReceiveFocus_OnClick()
    {
        var checkbox = Page.Locator("[data-ark-checkbox]").First;
        await checkbox.ClickAsync();

        await Expect(checkbox).ToBeFocusedAsync();
    }

    [Test]
    public async Task Checkbox_ShouldRetainFocus_AfterToggle()
    {
        var checkbox = Page.Locator("[data-ark-checkbox]").First;
        await checkbox.FocusAsync();
        await Expect(checkbox).ToBeFocusedAsync();

        // Click to toggle
        await checkbox.ClickAsync();

        // Focus should remain on the checkbox
        await Expect(checkbox).ToBeFocusedAsync();

        // Click again to toggle back
        await checkbox.ClickAsync();

        // Focus should still be on the checkbox
        await Expect(checkbox).ToBeFocusedAsync();
    }

    [Test]
    public async Task Checkbox_ShouldShowFocusIndicator()
    {
        var checkbox = Page.Locator("[data-ark-checkbox]").First;
        await checkbox.FocusAsync();

        // Checkbox should be focusable (has focus)
        await Expect(checkbox).ToBeFocusedAsync();
    }

    #endregion

    #region Checkbox Group Accessibility

    [Test]
    public async Task CheckboxGroup_ShouldHave_RoleGroup()
    {
        var groupHeading = Page.Locator("h2:has-text('Checkbox Group')").First;
        await groupHeading.ScrollIntoViewIfNeededAsync();

        var group = Page.Locator("[data-ark-checkbox-group]").First;
        await Expect(group).ToHaveAttributeAsync("role", "group");
    }

    [Test]
    public async Task CheckboxGroup_ShouldHave_AriaLabelledby()
    {
        var groupHeading = Page.Locator("h2:has-text('Checkbox Group')").First;
        await groupHeading.ScrollIntoViewIfNeededAsync();

        var group = Page.Locator("[data-ark-checkbox-group]").First;
        var ariaLabelledby = await group.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsNotNull();

        // Verify the label element exists with that ID
        var label = Page.Locator($"#{ariaLabelledby}");
        await Expect(label).ToHaveCountAsync(1);
    }

    [Test]
    public async Task CheckboxGroupLabel_ShouldHave_MatchingId()
    {
        var groupHeading = Page.Locator("h2:has-text('Checkbox Group')").First;
        await groupHeading.ScrollIntoViewIfNeededAsync();

        var group = Page.Locator("[data-ark-checkbox-group]").First;
        var ariaLabelledby = await group.GetAttributeAsync("aria-labelledby");

        var label = Page.Locator("[data-ark-checkbox-group-label]").First;
        var labelId = await label.GetAttributeAsync("id");

        await Assert.That(ariaLabelledby).IsEqualTo(labelId);
    }

    [Test]
    public async Task CheckboxGroup_ShouldManageValues()
    {
        var groupHeading = Page.Locator("h2:has-text('Checkbox Group')").First;
        await groupHeading.ScrollIntoViewIfNeededAsync();

        // Feature 1 and Feature 3 are checked by default
        var feature1Checkbox = Page.Locator("[data-ark-checkbox-group]").First
            .Locator("label:has-text('Feature 1')").Locator("[data-ark-checkbox]");
        var feature2Checkbox = Page.Locator("[data-ark-checkbox-group]").First
            .Locator("label:has-text('Feature 2')").Locator("[data-ark-checkbox]");

        await Expect(feature1Checkbox).ToHaveAttributeAsync("data-state", "checked");
        await Expect(feature2Checkbox).ToHaveAttributeAsync("data-state", "unchecked");

        // Click feature 2 to check it
        await feature2Checkbox.ClickAsync();
        await Expect(feature2Checkbox).ToHaveAttributeAsync("data-state", "checked");

        // Click feature 1 to uncheck it
        await feature1Checkbox.ClickAsync();
        await Expect(feature1Checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task DisabledGroup_ShouldHave_AriaDisabled()
    {
        var disabledGroupHeading = Page.Locator("h2:has-text('Disabled Group')");
        await disabledGroupHeading.ScrollIntoViewIfNeededAsync();

        var group = Page.Locator("[data-ark-checkbox-group][data-disabled]").First;
        await Expect(group).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledGroup_ShouldDisable_AllCheckboxes()
    {
        var disabledGroupHeading = Page.Locator("h2:has-text('Disabled Group')");
        await disabledGroupHeading.ScrollIntoViewIfNeededAsync();

        var group = Page.Locator("[data-ark-checkbox-group][data-disabled]").First;
        var checkboxes = group.Locator("[data-ark-checkbox]");
        var count = await checkboxes.CountAsync();

        for (var i = 0; i < count; i++)
        {
            await Expect(checkboxes.Nth(i)).ToBeDisabledAsync();
        }
    }

    #endregion

    #region Controlled Mode

    [Test]
    public async Task ControlledCheckbox_ShouldReflectExternalState()
    {
        var toggleButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Toggle Externally") });
        var checkbox = Page.Locator("[data-testid='controlled-checkbox']");

        // Initially unchecked
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");

        // Click external toggle button
        await toggleButton.ClickAsync();

        // Checkbox should now be checked
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");

        // Click again to toggle back
        await toggleButton.ClickAsync();

        // Checkbox should be unchecked again
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task ControlledCheckbox_ShouldUpdateExternalState_OnClick()
    {
        var toggleButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Toggle Externally") });
        var checkbox = Page.Locator("[data-testid='controlled-checkbox']");

        // Initially shows "Unchecked"
        await Expect(toggleButton).ToContainTextAsync("Unchecked");

        // Click the checkbox
        await checkbox.ClickAsync();

        // Button text should update to "Checked"
        await Expect(toggleButton).ToContainTextAsync("Checked");
    }

    #endregion

    #region Indeterminate Behavior

    [Test]
    public async Task SelectAll_ShouldCheckAllItems_WhenIndeterminate()
    {
        var selectAllCheckbox = Page.Locator("[data-testid='select-all-checkbox']");
        
        // Click select all
        await selectAllCheckbox.ClickAsync();

        // All item checkboxes should be checked
        var item1Checkbox = Page.Locator("[data-testid='item1-checkbox']");
        var item2Checkbox = Page.Locator("[data-testid='item2-checkbox']");
        var item3Checkbox = Page.Locator("[data-testid='item3-checkbox']");

        await Expect(item1Checkbox).ToHaveAttributeAsync("data-state", "checked");
        await Expect(item2Checkbox).ToHaveAttributeAsync("data-state", "checked");
        await Expect(item3Checkbox).ToHaveAttributeAsync("data-state", "checked");

        // Select all should now be checked (not indeterminate)
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task SelectAll_ShouldUncheckAllItems_WhenChecked()
    {
        var selectAllCheckbox = Page.Locator("[data-testid='select-all-checkbox']");
        
        // Click select all to check all items
        await selectAllCheckbox.ClickAsync();
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "checked");

        // Click again to uncheck all
        await selectAllCheckbox.ClickAsync();

        // All item checkboxes should be unchecked
        var item1Checkbox = Page.Locator("[data-testid='item1-checkbox']");
        var item2Checkbox = Page.Locator("[data-testid='item2-checkbox']");
        var item3Checkbox = Page.Locator("[data-testid='item3-checkbox']");

        await Expect(item1Checkbox).ToHaveAttributeAsync("data-state", "unchecked");
        await Expect(item2Checkbox).ToHaveAttributeAsync("data-state", "unchecked");
        await Expect(item3Checkbox).ToHaveAttributeAsync("data-state", "unchecked");

        // Select all should now be unchecked
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task SelectAll_ShouldBecomeIndeterminate_WhenSomeItemsChecked()
    {
        var selectAllCheckbox = Page.Locator("[data-testid='select-all-checkbox']");
        
        // First check all items
        await selectAllCheckbox.ClickAsync();
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "checked");

        // Now uncheck one item
        var firstItemCheckbox = Page.Locator("[data-testid='item1-checkbox']");
        await firstItemCheckbox.ClickAsync();

        // Select all should now be indeterminate
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("data-state", "indeterminate");
        await Expect(selectAllCheckbox).ToHaveAttributeAsync("aria-checked", "mixed");
    }

    #endregion

    #region Indicator Component

    [Test]
    public async Task CheckboxIndicator_ShouldRender_WhenChecked()
    {
        // Find the checked custom checkbox
        var checkedCheckbox = Page.Locator("[data-testid='custom-checked']");

        var indicator = checkedCheckbox.Locator("[data-ark-checkbox-indicator]");
        await Expect(indicator).ToBeVisibleAsync();
        await Expect(indicator).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task CheckboxIndicator_ShouldNotRender_WhenUnchecked()
    {
        // Find the unchecked custom checkbox
        var uncheckedCheckbox = Page.Locator("[data-testid='custom-unchecked']");

        var indicator = uncheckedCheckbox.Locator("[data-ark-checkbox-indicator]");
        await Expect(indicator).ToHaveCountAsync(0);
    }

    [Test]
    public async Task CheckboxIndicator_ShouldRender_WhenIndeterminate()
    {
        // The disabled indeterminate checkbox
        var indeterminateCheckbox = Page.Locator("[data-testid='disabled-indeterminate']");

        // Should contain the indeterminate indicator (-)
        await Expect(indeterminateCheckbox).ToContainTextAsync("-");
    }

    #endregion

    #region Label Association

    [Test]
    public async Task Label_ShouldToggleCheckbox_OnClick()
    {
        var checkbox = Page.Locator("[data-testid='basic-checkbox-unchecked']");
        var label = checkbox.Locator("xpath=ancestor::label");

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        // Click on the label text (not the checkbox itself)
        await label.ClickAsync();

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
    }

    #endregion
}
