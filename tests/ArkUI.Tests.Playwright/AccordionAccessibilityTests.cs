using TUnit.Playwright;

namespace ArkUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the Accordion component.
/// Tests ARIA attributes, keyboard navigation, and focus management.
/// </summary>
public class AccordionAccessibilityTests : PageTest
{
    private const string AccordionDemoUrl = "Accordion";

    [Before(Test)]
    public async Task NavigateToAccordionDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + AccordionDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes on AccordionItem

    [Test]
    public async Task Item_ShouldHave_DataStateOpen_WhenExpanded()
    {
        // First item is expanded by default (item-1)
        var item = Page.Locator("[data-ark-accordion-item][data-state='open']").First;
        await Expect(item).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateClosed_WhenCollapsed()
    {
        // Second item is collapsed by default
        var items = Page.Locator("h2:has-text('Single Mode') + [data-ark-accordion-item], h2:has-text('Single Mode') ~ [data-ark-accordion-item]");
        // Use the first accordion section's items
        var singleModeSection = Page.Locator("h2:has-text('Single Mode (Default)')").First;
        var accordion = singleModeSection.Locator("~ *").Locator("[data-ark-accordion-item]").First;
        
        // Get second item in first accordion (collapsed)
        var allItems = Page.Locator("[data-ark-accordion-item]");
        var secondItem = allItems.Nth(1);
        await Expect(secondItem).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Item_ShouldHave_DataOrientation()
    {
        var item = Page.Locator("[data-ark-accordion-item]").First;
        await Expect(item).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    [Test]
    public async Task Item_ShouldHave_DataDisabled_WhenDisabled()
    {
        var disabledItem = Page.Locator("[data-ark-accordion-item][data-disabled]").First;
        await Expect(disabledItem).ToHaveCountAsync(1);
    }

    #endregion

    #region ARIA Attributes on AccordionHeader

    [Test]
    public async Task Header_ShouldHave_RoleHeading()
    {
        var header = Page.Locator("[data-ark-accordion-header]").First;
        await Expect(header).ToHaveAttributeAsync("role", "heading");
    }

    [Test]
    public async Task Header_ShouldHave_AriaLevel()
    {
        var header = Page.Locator("[data-ark-accordion-header]").First;
        var ariaLevel = await header.GetAttributeAsync("aria-level");
        await Assert.That(ariaLevel).IsNotNull();
        // Default level should be a valid heading level (1-6)
        var level = int.Parse(ariaLevel!);
        await Assert.That(level).IsGreaterThanOrEqualTo(1);
        await Assert.That(level).IsLessThanOrEqualTo(6);
    }

    [Test]
    public async Task Header_ShouldHave_DataOrientation()
    {
        var header = Page.Locator("[data-ark-accordion-header]").First;
        await Expect(header).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region ARIA Attributes on AccordionTrigger

    [Test]
    public async Task Trigger_ShouldHave_TypeButton()
    {
        var trigger = Page.Locator("[data-ark-accordion-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task Trigger_ShouldHave_UniqueId()
    {
        var triggers = Page.Locator("[data-ark-accordion-trigger]");
        var count = await triggers.CountAsync();

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await triggers.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        // First trigger in single mode is expanded by default
        var trigger = Page.Locator("[data-ark-accordion-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        // Second trigger is collapsed by default
        var trigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        // Get expanded trigger so content is rendered
        var trigger = Page.Locator("[data-ark-accordion-trigger][aria-expanded='true']").First;
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await Assert.That(ariaControls).IsNotNull();

        // Verify the content panel with that ID exists
        var content = Page.Locator($"[data-ark-accordion-content]#{ariaControls}");
        await Expect(content).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenExpanded()
    {
        var trigger = Page.Locator("[data-ark-accordion-trigger][aria-expanded='true']").First;
        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateClosed_WhenCollapsed()
    {
        var trigger = Page.Locator("[data-ark-accordion-trigger][aria-expanded='false']").First;
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataOrientation()
    {
        var trigger = Page.Locator("[data-ark-accordion-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region ARIA Attributes on AccordionContent

    [Test]
    public async Task Content_ShouldHave_RoleRegion()
    {
        var content = Page.Locator("[data-ark-accordion-content]").First;
        await Expect(content).ToHaveAttributeAsync("role", "region");
    }

    [Test]
    public async Task Content_ShouldHave_UniqueId()
    {
        // Click multiple items to expand them (use multiple mode section)
        var multipleModeHeading = Page.Locator("h2:has-text('Multiple Mode')");
        await multipleModeHeading.ScrollIntoViewIfNeededAsync();

        // Multiple mode has multiple items open by default
        var contents = Page.Locator("[data-ark-accordion-content]");
        var count = await contents.CountAsync();

        // Should have at least 2 content panels visible (from multiple mode defaults)
        await Assert.That(count).IsGreaterThanOrEqualTo(2);

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await contents.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTriggerId()
    {
        var content = Page.Locator("[data-ark-accordion-content]").First;
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsNotNull();

        // Verify the trigger with that ID exists
        var trigger = Page.Locator($"[data-ark-accordion-trigger]#{ariaLabelledby}");
        await Expect(trigger).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenVisible()
    {
        var content = Page.Locator("[data-ark-accordion-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Content_ShouldHave_DataOrientation()
    {
        var content = Page.Locator("[data-ark-accordion-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region Disabled Item Accessibility

    [Test]
    public async Task DisabledTrigger_ShouldHave_DisabledAttribute()
    {
        var disabledTrigger = Page.Locator("[data-ark-accordion-trigger][data-disabled]").First;
        await Expect(disabledTrigger).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabled()
    {
        var disabledTrigger = Page.Locator("[data-ark-accordion-trigger][data-disabled]");
        await Expect(disabledTrigger).ToHaveCountAsync(1);
    }

    [Test]
    public async Task DisabledItem_ShouldNotToggle_OnClick()
    {
        var disabledTrigger = Page.Locator("[data-ark-accordion-trigger][data-disabled]").First;

        // Verify it's closed initially
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        // Force click on disabled trigger
        await disabledTrigger.ClickAsync(new() { Force = true });

        // Should still be closed
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task DisabledItem_ShouldHave_DataDisabled()
    {
        var disabledItem = Page.Locator("[data-ark-accordion-item][data-disabled]").First;
        await Expect(disabledItem).ToHaveAttributeAsync("data-disabled", "");
    }

    #endregion

    #region Toggle Behavior (Click)

    [Test]
    public async Task Click_ShouldExpandCollapsedItem()
    {
        // Get second trigger (collapsed)
        var trigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Click_ShouldCollapseExpandedItem()
    {
        // First trigger is expanded by default
        var trigger = Page.Locator("[data-ark-accordion-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Click_ShouldShowContent_WhenExpanded()
    {
        // Get second trigger (collapsed) and expand it
        var trigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await trigger.ClickAsync();

        // Content should now be visible
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Click_ShouldHideContent_WhenCollapsed()
    {
        // First trigger is expanded by default
        var trigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Content should be visible initially
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToBeVisibleAsync();

        await trigger.ClickAsync();

        // Content should be hidden (either removed from DOM or hidden)
        await Expect(content).ToHaveCountAsync(0);
    }

    [Test]
    public async Task Click_ShouldUpdateDataState_OnItem()
    {
        // Get first item (expanded) and its trigger
        var item = Page.Locator("[data-ark-accordion-item]").First;
        var trigger = item.Locator("[data-ark-accordion-trigger]");

        await Expect(item).ToHaveAttributeAsync("data-state", "open");

        await trigger.ClickAsync();

        await Expect(item).ToHaveAttributeAsync("data-state", "closed");
    }

    #endregion

    #region Keyboard Navigation - Enter/Space

    [Test]
    public async Task Enter_ShouldToggleItem()
    {
        // Get second trigger (collapsed)
        var trigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Space_ShouldToggleItem()
    {
        // Get second trigger (collapsed)
        var trigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Enter_ShouldNotToggle_DisabledItem()
    {
        var disabledTrigger = Page.Locator("[data-ark-accordion-trigger][data-disabled]").First;
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        await disabledTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Space_ShouldNotToggle_DisabledItem()
    {
        var disabledTrigger = Page.Locator("[data-ark-accordion-trigger][data-disabled]").First;
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        await disabledTrigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Keyboard Navigation - Arrow Keys

    [Test]
    public async Task ArrowDown_ShouldMoveFocusToNextTrigger()
    {
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var secondTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(secondTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldMoveFocusToPreviousTrigger()
    {
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var secondTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);

        await secondTrigger.FocusAsync();
        await Expect(secondTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowDown_ShouldLoopToFirst_WhenAtLast()
    {
        // The first accordion has 3 items, so item-3 is the last
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var thirdTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(2);

        await thirdTrigger.FocusAsync();
        await Expect(thirdTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        // Should loop to the first trigger
        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldLoopToLast_WhenAtFirst()
    {
        // The first accordion has 3 items
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var thirdTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(2);

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        // Should loop to the last trigger (third item)
        await Expect(thirdTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Home_ShouldMoveFocusToFirstTrigger()
    {
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var thirdTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(2);

        await thirdTrigger.FocusAsync();
        await Expect(thirdTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Home");

        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task End_ShouldMoveFocusToLastTrigger()
    {
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var thirdTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(2);

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("End");

        await Expect(thirdTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowDown_ShouldSkipDisabledTriggers()
    {
        // Navigate to disabled items section
        var disabledHeading = Page.Locator("h2:has-text('With Disabled Items')");
        await disabledHeading.ScrollIntoViewIfNeededAsync();

        // Focus the first enabled item
        var enabledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button,
            new() { Name = "Enabled Item", Exact = true });
        await enabledTrigger.FocusAsync();
        await Expect(enabledTrigger).ToBeFocusedAsync();

        // Arrow down should skip the disabled item and go to "Another Enabled Item"
        await Page.Keyboard.PressAsync("ArrowDown");

        var anotherEnabledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button,
            new() { Name = "Another Enabled Item" });
        await Expect(anotherEnabledTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldSkipDisabledTriggers()
    {
        // Navigate to disabled items section
        var disabledHeading = Page.Locator("h2:has-text('With Disabled Items')");
        await disabledHeading.ScrollIntoViewIfNeededAsync();

        // Focus "Another Enabled Item" (last in this accordion)
        var anotherEnabledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button,
            new() { Name = "Another Enabled Item" });
        await anotherEnabledTrigger.FocusAsync();
        await Expect(anotherEnabledTrigger).ToBeFocusedAsync();

        // Arrow up should skip the disabled item and go to "Enabled Item"
        await Page.Keyboard.PressAsync("ArrowUp");

        var enabledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button,
            new() { Name = "Enabled Item", Exact = true });
        await Expect(enabledTrigger).ToBeFocusedAsync();
    }

    #endregion

    #region ID Relationships

    [Test]
    public async Task TriggerAndContent_ShouldHave_MatchingIdRelationship()
    {
        // Get an expanded trigger so content is rendered
        var trigger = Page.Locator("[data-ark-accordion-trigger][aria-expanded='true']").First;
        var triggerId = await trigger.GetAttributeAsync("id");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Get the corresponding content
        var content = Page.Locator($"#{ariaControls}");
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        // The content's aria-labelledby should point back to the trigger
        await Assert.That(ariaLabelledby).IsEqualTo(triggerId);
    }

    [Test]
    public async Task AllExpandedTriggers_ShouldHave_ValidAriaControls()
    {
        // Get all expanded triggers
        var triggers = Page.Locator("[data-ark-accordion-trigger][aria-expanded='true']");
        var count = await triggers.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var trigger = triggers.Nth(i);
            var ariaControls = await trigger.GetAttributeAsync("aria-controls");
            await Assert.That(ariaControls).IsNotNull();

            // The content panel should exist when this accordion item is expanded
            var referencedContent = Page.Locator($"[data-ark-accordion-content][id='{ariaControls}']");
            await Expect(referencedContent).ToHaveCountAsync(1);
        }
    }

    #endregion

    #region Multiple Mode Behavior

    [Test]
    public async Task MultipleMode_ShouldAllowMultipleItemsOpen()
    {
        // Navigate to Multiple Mode section
        var multipleModeHeading = Page.Locator("h2:has-text('Multiple Mode')");
        await multipleModeHeading.ScrollIntoViewIfNeededAsync();

        // Multiple mode has feature-1 and feature-2 open by default
        // Count expanded items in the multiple mode accordion
        // Find triggers after the "Multiple Mode" heading
        var triggers = Page.Locator("[data-ark-accordion-trigger][aria-expanded='true']");
        var expandedCount = await triggers.CountAsync();

        // Should have at least 2 expanded (from DefaultValues)
        await Assert.That(expandedCount).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task MultipleMode_ShouldNotCloseOtherItems_WhenOpeningNew()
    {
        // Navigate to Multiple Mode section
        var multipleModeHeading = Page.Locator("h2:has-text('Multiple Mode')");
        await multipleModeHeading.ScrollIntoViewIfNeededAsync();

        // Get initial count of expanded items
        var initialExpandedTriggers = Page.Locator("[data-ark-accordion-trigger][aria-expanded='true']");
        var initialCount = await initialExpandedTriggers.CountAsync();

        // Find a collapsed trigger in the Multiple Mode section and click it
        // The third item (feature-3) should be collapsed by default
        var collapsedTrigger = Page.Locator("[data-ark-accordion-trigger]").Filter(
            new() { HasText = "Feature 3" });
        await Expect(collapsedTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        await collapsedTrigger.ClickAsync();

        // Now should have one more expanded item
        var newExpandedTriggers = Page.Locator("[data-ark-accordion-trigger][aria-expanded='true']");
        var newCount = await newExpandedTriggers.CountAsync();

        await Assert.That(newCount).IsEqualTo(initialCount + 1);
    }

    #endregion

    #region Non-Collapsible Mode Behavior

    [Test]
    public async Task NonCollapsible_ShouldNotCloseLastOpenItem()
    {
        // Navigate to Non-Collapsible section
        var nonCollapsibleHeading = Page.Locator("h2:has-text('Non-Collapsible Single Mode')");
        await nonCollapsibleHeading.ScrollIntoViewIfNeededAsync();

        // Find the expanded trigger in this section
        var expandedTrigger = Page.Locator("[data-ark-accordion-trigger]").Filter(
            new() { HasText = "Always One Open" });
        await Expect(expandedTrigger).ToHaveAttributeAsync("aria-expanded", "true");

        // Try to close it by clicking
        await expandedTrigger.ClickAsync();

        // Should still be expanded (non-collapsible mode)
        await Expect(expandedTrigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task NonCollapsible_ShouldAllowSwitchingToAnotherItem()
    {
        // Navigate to Non-Collapsible section
        var nonCollapsibleHeading = Page.Locator("h2:has-text('Non-Collapsible Single Mode')");
        await nonCollapsibleHeading.ScrollIntoViewIfNeededAsync();

        // Find the first (expanded) trigger
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").Filter(
            new() { HasText = "Always One Open" });
        await Expect(firstTrigger).ToHaveAttributeAsync("aria-expanded", "true");

        // Find the second (collapsed) trigger and click it
        var secondTrigger = Page.Locator("[data-ark-accordion-trigger]").Filter(
            new() { HasText = "Try Closing Me" });
        await Expect(secondTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        await secondTrigger.ClickAsync();

        // Second should now be expanded
        await Expect(secondTrigger).ToHaveAttributeAsync("aria-expanded", "true");
        // First should now be collapsed
        await Expect(firstTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Content Visibility

    [Test]
    public async Task ExpandedContent_ShouldBeVisible()
    {
        var content = Page.Locator("[data-ark-accordion-content][data-state='open']").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task CollapsedContent_ShouldNotBeInDOM()
    {
        // Get a collapsed trigger's aria-controls
        var collapsedTrigger = Page.Locator("[data-ark-accordion-trigger][aria-expanded='false']").First;
        var ariaControls = await collapsedTrigger.GetAttributeAsync("aria-controls");

        // The content should not be in the DOM (component doesn't render when collapsed)
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToHaveCountAsync(0);
    }

    [Test]
    public async Task ContentVisibility_ShouldUpdate_OnToggle()
    {
        // Get second trigger (collapsed) and its aria-controls
        var trigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Content should not exist initially
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToHaveCountAsync(0);

        // Click to expand
        await trigger.ClickAsync();

        // Content should now be visible
        await Expect(content).ToBeVisibleAsync();
        await Expect(content).ToHaveAttributeAsync("data-state", "open");

        // Click to collapse
        await trigger.ClickAsync();

        // Content should be removed from DOM
        await Expect(content).ToHaveCountAsync(0);
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Trigger_ShouldReceiveFocus_OnClick()
    {
        var trigger = Page.Locator("[data-ark-accordion-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Trigger_ShouldRetainFocus_AfterToggle()
    {
        var trigger = Page.Locator("[data-ark-accordion-trigger]").First;
        await trigger.FocusAsync();
        await Expect(trigger).ToBeFocusedAsync();

        // Click to toggle (collapse)
        await trigger.ClickAsync();

        // Focus should remain on the trigger
        await Expect(trigger).ToBeFocusedAsync();

        // Click to toggle again (expand)
        await trigger.ClickAsync();

        // Focus should still be on the trigger
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tab_ShouldNavigateBetweenTriggers()
    {
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var secondTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        // Tab to next focusable element
        await Page.Keyboard.PressAsync("Tab");

        // Second trigger should be focused
        await Expect(secondTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ShiftTab_ShouldNavigateBackwards()
    {
        var firstTrigger = Page.Locator("[data-ark-accordion-trigger]").First;
        var secondTrigger = Page.Locator("[data-ark-accordion-trigger]").Nth(1);

        await secondTrigger.FocusAsync();
        await Expect(secondTrigger).ToBeFocusedAsync();

        // Shift+Tab to previous element
        await Page.Keyboard.PressAsync("Shift+Tab");

        // First trigger should be focused
        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task DisabledTrigger_ShouldNotReceiveFocus_OnTab()
    {
        // Navigate to disabled items section
        var disabledHeading = Page.Locator("h2:has-text('With Disabled Items')");
        await disabledHeading.ScrollIntoViewIfNeededAsync();

        // Focus the first enabled item in this section (use GetByRole with Exact match)
        var enabledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, 
            new() { Name = "Enabled Item", Exact = true });
        await enabledTrigger.FocusAsync();
        await Expect(enabledTrigger).ToBeFocusedAsync();

        // Tab forward - should skip disabled and go to next enabled
        await Page.Keyboard.PressAsync("Tab");

        // Disabled trigger should NOT be focused
        var disabledTrigger = Page.Locator("[data-ark-accordion-trigger][data-disabled]");
        await Expect(disabledTrigger).Not.ToBeFocusedAsync();

        // Next enabled trigger should be focused
        var anotherEnabledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button,
            new() { Name = "Another Enabled Item" });
        await Expect(anotherEnabledTrigger).ToBeFocusedAsync();
    }

    #endregion
}
