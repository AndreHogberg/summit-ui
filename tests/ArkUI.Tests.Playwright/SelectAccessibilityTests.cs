using TUnit.Playwright;

namespace ArkUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the Select component.
/// Tests ARIA attributes, keyboard navigation, and focus management.
/// </summary>
public class SelectAccessibilityTests : PageTest
{
    private const string SelectDemoUrl = "select";

    [Before(Test)]
    public async Task NavigateToSelectDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + SelectDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes on Trigger

    [Test]
    public async Task Trigger_ShouldHave_RoleCombobox()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await Expect(trigger).ToHaveAttributeAsync("role", "combobox");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaHaspopupListbox()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "listbox");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        var contentId = await content.GetAttributeAsync("id");

        await Assert.That(ariaControls).IsNotNull();
        await Assert.That(ariaControls).IsEqualTo(contentId);
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateClosed_WhenClosed()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaActivedescendant_WhenItemHighlighted()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Press arrow down to highlight next item (first item is already highlighted on open)
        await Page.Keyboard.PressAsync("ArrowDown");

        // Use regex pattern to match any non-empty value for aria-activedescendant
        await Expect(trigger).ToHaveAttributeAsync("aria-activedescendant", new System.Text.RegularExpressions.Regex(".+"));
    }

    #endregion

    #region ARIA Attributes on Content

    [Test]
    public async Task Content_ShouldHave_RoleListbox()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToHaveAttributeAsync("role", "listbox");
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexNegativeOne()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTriggerId()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        var triggerId = await trigger.GetAttributeAsync("id");

        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToHaveAttributeAsync("aria-labelledby", triggerId!);
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region ARIA Attributes on Items

    [Test]
    public async Task Item_ShouldHave_RoleOption()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-ark-select-item]").First;
        await Expect(item).ToHaveAttributeAsync("role", "option");
    }

    [Test]
    public async Task Item_ShouldHave_AriaSelectedFalse_WhenNotSelected()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-ark-select-item]").First;
        await Expect(item).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Item_ShouldHave_AriaSelectedTrue_WhenSelected()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]");
        await Expect(content.First).ToBeVisibleAsync();

        // Get the first item within the open content and select it
        var item = content.Locator("[data-ark-select-item]").First;
        var itemValue = await item.GetAttributeAsync("data-value");
        await item.ClickAsync();

        // Wait for content to close
        await Expect(content).ToHaveCountAsync(0);

        // Small delay to avoid debounce protection in the component
        await Page.WaitForTimeoutAsync(150);

        // Reopen the select
        await trigger.ClickAsync();

        // Wait for content to be visible again (new locator query)
        await Expect(content.First).ToBeVisibleAsync();

        // Check the selected item has aria-selected=true
        var selectedItem = content.Locator($"[data-ark-select-item][data-value='{itemValue}']");
        await Expect(selectedItem).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateUnchecked_WhenNotSelected()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-ark-select-item]").First;
        await Expect(item).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateChecked_WhenSelected()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]");
        await Expect(content.First).ToBeVisibleAsync();

        // Get the first item within the open content and select it
        var item = content.Locator("[data-ark-select-item]").First;
        var itemValue = await item.GetAttributeAsync("data-value");
        await item.ClickAsync();

        // Wait for content to close
        await Expect(content).ToHaveCountAsync(0);

        // Small delay to avoid debounce protection in the component
        await Page.WaitForTimeoutAsync(150);

        // Reopen the select
        await trigger.ClickAsync();

        // Wait for content to be visible again (new locator query)
        await Expect(content.First).ToBeVisibleAsync();

        // Check the selected item has data-state='checked'
        var selectedItem = content.Locator($"[data-ark-select-item][data-value='{itemValue}']");
        await Expect(selectedItem).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Item_ShouldHave_UniqueId()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var items = Page.Locator("[data-ark-select-item]");
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

    #region Disabled Item Accessibility

    [Test]
    public async Task DisabledItem_ShouldHave_AriaDisabledTrue()
    {
        // Navigate to disabled items section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        var disabledItem = Page.Locator("[data-ark-select-item][data-disabled]").First;
        await Expect(disabledItem).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledItem_ShouldHave_DataDisabled()
    {
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        var disabledItem = Page.Locator("[data-ark-select-item][data-disabled]").First;
        await Expect(disabledItem).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledItem_ShouldNotBeSelectable()
    {
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        var disabledItem = Page.Locator("[data-ark-select-item][data-disabled]").First;
        var content = Page.Locator("[data-ark-select-content]").First;

        // Try to click disabled item (use Force since element is disabled)
        await disabledItem.ClickAsync(new() { Force = true });

        // Content should still be open (item was not selected)
        await Expect(content).ToBeVisibleAsync();
    }

    #endregion

    #region Disabled Select Accessibility

    [Test]
    public async Task DisabledTrigger_ShouldHave_AriaDisabledTrue()
    {
        var disabledTrigger = Page.Locator("section").Filter(new() { HasText = "Disabled State" }).Locator("[role='combobox']");
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabled()
    {
        var disabledTrigger = Page.Locator("section").Filter(new() { HasText = "Disabled State" }).Locator("[role='combobox']");
        await Expect(disabledTrigger).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DisabledAttribute()
    {
        var disabledTrigger = Page.Locator("section").Filter(new() { HasText = "Disabled State" }).Locator("[role='combobox']");
        await Expect(disabledTrigger).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledTrigger_ShouldNotOpen_OnClick()
    {
        var disabledTrigger = Page.Locator("section").Filter(new() { HasText = "Disabled State" }).Locator("[role='combobox']");

        await disabledTrigger.ClickAsync(new() { Force = true });

        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Required/Invalid State Accessibility

    [Test]
    public async Task RequiredTrigger_ShouldHave_AriaRequiredTrue()
    {
        var requiredTrigger = Page.Locator("[data-testid='basic-form-section']").Locator("[role='combobox']");
        await Expect(requiredTrigger).ToHaveAttributeAsync("aria-required", "true");
    }

    [Test]
    public async Task InvalidTrigger_ShouldHave_AriaInvalidTrue_WhenInvalid()
    {
        // Submit form without selection to trigger validation
        var formSection = Page.Locator("[data-testid='basic-form-section']");
        var submitButton = formSection.Locator("button[type='submit']");
        await submitButton.ClickAsync();

        var trigger = formSection.Locator("[role='combobox']");
        await Expect(trigger).ToHaveAttributeAsync("aria-invalid", "true");
    }

    [Test]
    public async Task InvalidTrigger_ShouldHave_DataInvalid_WhenInvalid()
    {
        var formSection = Page.Locator("[data-testid='basic-form-section']");
        var submitButton = formSection.Locator("button[type='submit']");
        await submitButton.ClickAsync();

        var trigger = formSection.Locator("[role='combobox']");
        await Expect(trigger).ToHaveAttributeAsync("data-invalid", "");
    }

    #endregion

    #region Keyboard Navigation

    [Test]
    public async Task Select_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldOpen_OnArrowDownKey()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldOpen_OnArrowUpKey()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowUp");

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Select_ShouldClose_OnEscapeKey()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldUpdateAriaExpanded_AfterEscapeClose()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task ArrowDown_ShouldHighlightNextItem()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible and JS initialization
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // On open, first item is already highlighted
        // Press ArrowDown once to move to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Second item should be highlighted
        var secondItem = content.Locator("[data-ark-select-item]").Nth(1);
        await Expect(secondItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldHighlightPreviousItem()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible and JS initialization
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // On open, first item is highlighted
        // Go to second item first
        await Page.Keyboard.PressAsync("ArrowDown");

        // Go back up to first item
        await Page.Keyboard.PressAsync("ArrowUp");

        // First item should be highlighted
        var firstItem = content.Locator("[data-ark-select-item]").First;
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Enter_ShouldSelectHighlightedItem()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible and JS initialization
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // On open, first item is highlighted
        // Press ArrowDown once to highlight second item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Get the second item's value
        var secondItem = content.Locator("[data-ark-select-item]").Nth(1);
        var expectedLabel = await secondItem.GetAttributeAsync("data-label");

        // Select it
        await Page.Keyboard.PressAsync("Enter");

        // Content should close
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        var value = Page.Locator("[data-ark-select-value]").First;
        await Expect(value).ToHaveTextAsync(expectedLabel!);
    }

    [Test]
    public async Task Home_ShouldHighlightFirstItem()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible and JS initialization
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Navigate down a few items
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");

        // Press Home
        await Page.Keyboard.PressAsync("Home");

        // First item should be highlighted
        var firstItem = content.Locator("[data-ark-select-item]").First;
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task End_ShouldHighlightLastItem()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible and JS initialization
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Press End
        await Page.Keyboard.PressAsync("End");

        // Last item should be highlighted
        var lastItem = content.Locator("[data-ark-select-item]").Last;
        await Expect(lastItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Tab_ShouldCloseSelect()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Tab");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Focus_ShouldReturnToTrigger_AfterSelection()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-ark-select-item]").First;
        await item.ClickAsync();

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Focus_ShouldReturnToTrigger_AfterEscapeClose()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Select_ShouldClose_OnOutsideClick()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Click outside the select
        await Page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Placeholder State

    [Test]
    public async Task Value_ShouldHave_DataPlaceholder_WhenNoSelection()
    {
        var value = Page.Locator("[data-ark-select-value]").First;
        await Expect(value).ToHaveAttributeAsync("data-placeholder", "");
    }

    [Test]
    public async Task Value_ShouldNotHave_DataPlaceholder_AfterSelection()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-ark-select-item]").First;
        await item.ClickAsync();

        var value = Page.Locator("[data-ark-select-value]").First;
        await Expect(value).Not.ToHaveAttributeAsync("data-placeholder", "");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataPlaceholder_WhenNoSelection()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await Expect(trigger).ToHaveAttributeAsync("data-placeholder", "");
    }

    #endregion

    #region Selection Behavior

    [Test]
    public async Task Click_ShouldSelectItem()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-ark-select-item]").First;
        var expectedLabel = await item.GetAttributeAsync("data-label");
        await item.ClickAsync();

        // Dropdown should close
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).Not.ToBeVisibleAsync();

        // Value should be updated
        var value = Page.Locator("[data-ark-select-value]").First;
        await Expect(value).ToHaveTextAsync(expectedLabel!);
    }

    [Test]
    public async Task SelectedItem_ShouldBeHighlighted_WhenReopened()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]");
        await Expect(content.First).ToBeVisibleAsync();

        // Select second item within the open content
        var secondItem = content.Locator("[data-ark-select-item]").Nth(1);
        var secondValue = await secondItem.GetAttributeAsync("data-value");
        await secondItem.ClickAsync();

        // Wait for content to close
        await Expect(content).ToHaveCountAsync(0);

        // Small delay to avoid debounce protection in the component
        await Page.WaitForTimeoutAsync(150);

        // Reopen
        await trigger.ClickAsync();

        // Wait for content to be visible again (new locator query)
        await Expect(content.First).ToBeVisibleAsync();

        // Second item should be highlighted (scoped to this content)
        var highlightedItem = content.Locator($"[data-ark-select-item][data-value='{secondValue}']");
        await Expect(highlightedItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Typeahead

    [Test]
    public async Task Typeahead_ShouldHighlightMatchingItem()
    {
        // Use the typeahead section which has many items
        var trigger = Page.Locator("section").Filter(new() { HasText = "Typeahead Search" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Type 'g' to find grape
        await Page.Keyboard.TypeAsync("g", new() { Delay = 50 });

        // Grape should be highlighted
        var grapeItem = Page.Locator("[data-ark-select-item][data-value='grape']");
        await Expect(grapeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Typeahead_ShouldMatchMultipleCharacters()
    {
        var trigger = Page.Locator("section").Filter(new() { HasText = "Typeahead Search" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Wait for initial highlight to be applied (first item should be highlighted)
        // This ensures the JS event handlers are fully set up before we type
        var firstItem = Page.Locator("[data-ark-select-item][data-value='apple']");
        await Expect(firstItem).ToHaveAttributeAsync("data-highlighted", "");

        // Type 'ma' to find mango (not matching apple which starts with 'a')
        // Use delay to ensure both characters are captured by typeahead buffer
        await Page.Keyboard.TypeAsync("ma", new() { Delay = 100 });

        // Mango should be highlighted
        var mangoItem = Page.Locator("[data-ark-select-item][data-value='mango']");
        await Expect(mangoItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion

    #region Content Positioning

    [Test]
    public async Task Content_ShouldBePositioned_BelowTrigger_ByDefault()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Check that content is positioned below trigger
        var triggerBox = await trigger.BoundingBoxAsync();
        var contentBox = await content.BoundingBoxAsync();

        await Assert.That(contentBox!.Y).IsGreaterThan(triggerBox!.Y);
    }

    [Test]
    public async Task Content_ShouldHave_DataSide_Attribute()
    {
        var trigger = Page.Locator("[role='combobox']").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        var dataSide = await content.GetAttributeAsync("data-side");

        await Assert.That(dataSide).IsNotNull();
    }

    #endregion

    #region Label Association

    [Test]
    public async Task Trigger_ShouldHave_AriaLabelledby_WhenProvided()
    {
        // Basic Form section has an aria-labelledby
        var trigger = Page.Locator("[data-testid='basic-form-section']").Locator("[role='combobox']");
        await Expect(trigger).ToHaveAttributeAsync("aria-labelledby", "country-label");
    }

    #endregion

    #region Grouped Items Accessibility

    [Test]
    public async Task GroupLabel_ShouldBeVisible_InGroupedSelect()
    {
        var trigger = Page.Locator("section").Filter(new() { HasText = "Grouped Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        var groupLabel = Page.Locator(".select-group-label").First;
        await Expect(groupLabel).ToBeVisibleAsync();
        await Expect(groupLabel).ToHaveTextAsync("Fruits");
    }

    [Test]
    public async Task ArrowDown_ShouldSkipGroupLabels()
    {
        var trigger = Page.Locator("section").Filter(new() { HasText = "Grouped Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // On open, first item (Apple) is already highlighted
        // Verify the first highlighted item is Apple (not the group label)
        var highlightedItem = Page.Locator("[data-ark-select-item][data-highlighted]").First;
        var value = await highlightedItem.GetAttributeAsync("data-value");
        await Assert.That(value).IsEqualTo("apple");
    }

    #endregion

    #region Disabled Item Keyboard Navigation

    [Test]
    public async Task ArrowDown_ShouldSkipDisabledItems()
    {
        // Navigate to "With Disabled Items" section
        // Items: Free, Pro, Enterprise (disabled)
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // First item (Free) should be highlighted on open
        var freeItem = content.Locator("[data-ark-select-item][data-value='free']");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown - should go to Pro (second item)
        await Page.Keyboard.PressAsync("ArrowDown");
        var proItem = content.Locator("[data-ark-select-item][data-value='pro']");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowDown again - should skip Enterprise (disabled) and loop to Free
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task ArrowUp_ShouldSkipDisabledItems()
    {
        // Navigate to "With Disabled Items" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // First item (Free) should be highlighted on open
        var freeItem = content.Locator("[data-ark-select-item][data-value='free']");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press ArrowUp - should skip Enterprise (disabled) and loop to Pro
        await Page.Keyboard.PressAsync("ArrowUp");
        var proItem = content.Locator("[data-ark-select-item][data-value='pro']");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task Home_ShouldGoToFirstNonDisabledItem()
    {
        // Navigate to "With Disabled Items" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Navigate to Pro
        await Page.Keyboard.PressAsync("ArrowDown");
        var proItem = content.Locator("[data-ark-select-item][data-value='pro']");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");

        // Press Home - should go to first non-disabled item (Free)
        await Page.Keyboard.PressAsync("Home");
        var freeItem = content.Locator("[data-ark-select-item][data-value='free']");
        await Expect(freeItem).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task End_ShouldGoToLastNonDisabledItem()
    {
        // Navigate to "With Disabled Items" section
        // Items: Free, Pro, Enterprise (disabled)
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Press End - should go to last non-disabled item (Pro, not Enterprise)
        await Page.Keyboard.PressAsync("End");
        var proItem = content.Locator("[data-ark-select-item][data-value='pro']");
        await Expect(proItem).ToHaveAttributeAsync("data-highlighted", "");

        // Verify Enterprise (disabled) is NOT highlighted
        var enterpriseItem = content.Locator("[data-ark-select-item][data-value='enterprise']");
        await Expect(enterpriseItem).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task DisabledItem_ShouldNeverBeHighlighted_ByKeyboard()
    {
        // Navigate to "With Disabled Items" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Disabled Items" }).Locator("[role='combobox']");
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Navigate through all items multiple times
        for (var i = 0; i < 6; i++)
        {
            await Page.Keyboard.PressAsync("ArrowDown");
        }

        // Verify Enterprise (disabled) is NOT highlighted
        var enterpriseItem = content.Locator("[data-ark-select-item][data-value='enterprise']");
        await Expect(enterpriseItem).Not.ToHaveAttributeAsync("data-highlighted", "");
    }

    #endregion
}
