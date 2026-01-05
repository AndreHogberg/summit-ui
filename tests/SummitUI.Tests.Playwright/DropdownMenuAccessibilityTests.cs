using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the DropdownMenu component.
/// Tests ARIA attributes, keyboard navigation, and focus management.
/// </summary>
public class DropdownMenuAccessibilityTests : PageTest
{
    private const string DropdownMenuDemoUrl = "dropdown-menu";

    [Before(Test)]
    public async Task NavigateToDropdownMenuDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + DropdownMenuDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes on Trigger

    [Test]
    public async Task Trigger_ShouldHave_AriaHaspopupMenu()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "menu");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;

        // Open the menu first - aria-controls is only set when open
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var ariaControls = await trigger.GetAttributeAsync("aria-controls");
        var contentId = await content.GetAttributeAsync("id");

        await Assert.That(ariaControls).IsNotNull();
        await Assert.That(ariaControls).IsEqualTo(contentId);
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateClosed_WhenClosed()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region ARIA Attributes on Content

    [Test]
    public async Task Content_ShouldHave_RoleMenu()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToHaveAttributeAsync("role", "menu");
    }

    [Test]
    public async Task Content_ShouldHave_AriaOrientationVertical()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToHaveAttributeAsync("aria-orientation", "vertical");
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;

        // Open menu first
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Verify aria-labelledby is set and follows the expected pattern
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsNotNull();
        await Assert.That(ariaLabelledby!.Contains("summit-dropdown-menu")).IsTrue();
        await Assert.That(ariaLabelledby.EndsWith("-trigger")).IsTrue();
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region ARIA Attributes on Menu Items

    [Test]
    public async Task MenuItem_ShouldHave_RoleMenuitem()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(item).ToHaveAttributeAsync("role", "menuitem");
    }

    [Test]
    public async Task MenuItem_ShouldHave_TabIndexNegativeOne()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var item = Page.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(item).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task DisabledMenuItem_ShouldHave_AriaDisabledTrue()
    {
        // Open the "With Disabled" menu
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "With Disabled" });
        await trigger.ClickAsync();

        var disabledItem = Page.Locator("[data-summit-dropdown-menu-item][data-disabled]").First;
        await Expect(disabledItem).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledMenuItem_ShouldHave_DataDisabledAttribute()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "With Disabled" });
        await trigger.ClickAsync();

        var disabledItems = Page.Locator("[data-summit-dropdown-menu-item][data-disabled]");
        await Expect(disabledItems).ToHaveCountAsync(2);
    }

    #endregion

    #region Keyboard Navigation - Opening and Closing

    [Test]
    public async Task Menu_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Menu_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Menu_ShouldOpen_OnArrowDownKey()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Menu_ShouldClose_OnEscapeKey()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldUpdateAriaExpanded_AfterEscapeClose()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Keyboard Navigation - Arrow Keys

    [Test]
    public async Task ArrowDown_ShouldFocusNextItem()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        // First item should be focused initially
        var firstItem = Page.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();

        // Press ArrowDown to move to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        var secondItem = Page.Locator("[data-summit-dropdown-menu-item]").Nth(1);
        await Expect(secondItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldFocusPreviousItem()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        // Move to second item first
        await Page.Keyboard.PressAsync("ArrowDown");

        var secondItem = Page.Locator("[data-summit-dropdown-menu-item]").Nth(1);
        await Expect(secondItem).ToBeFocusedAsync();

        // Press ArrowUp to move back to first item
        await Page.Keyboard.PressAsync("ArrowUp");

        var firstItem = Page.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowDown_ShouldSkipDisabledItems()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "With Disabled" });
        await trigger.ClickAsync();

        // First item "Available Action" should be focused
        var firstItem = Page.Locator("[data-summit-dropdown-menu-item]:not([data-disabled])").First;
        await Expect(firstItem).ToBeFocusedAsync();

        // Press ArrowDown - should skip "Disabled Action" and go to "Another Available"
        await Page.Keyboard.PressAsync("ArrowDown");

        // Third item is second non-disabled item
        var secondEnabledItem = Page.Locator("[data-summit-dropdown-menu-item]:not([data-disabled])").Nth(1);
        await Expect(secondEnabledItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task Home_ShouldFocusFirstItem()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        // Wait for first item to be focused (menu is ready)
        var firstItem = Page.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();

        // Move to last item
        await Page.Keyboard.PressAsync("End");

        var lastItem = Page.Locator("[data-summit-dropdown-menu-item]").Last;
        await Expect(lastItem).ToBeFocusedAsync();

        // Press Home to jump to first item
        await Page.Keyboard.PressAsync("Home");

        await Expect(firstItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task End_ShouldFocusLastItem()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        // Wait for first item to be focused (menu is ready)
        var firstItem = Page.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();

        // Press End to jump to last item
        await Page.Keyboard.PressAsync("End");

        var lastItem = Page.Locator("[data-summit-dropdown-menu-item]").Last;
        await Expect(lastItem).ToBeFocusedAsync();
    }

    #endregion

    #region Keyboard Navigation - Item Selection

    [Test]
    public async Task Enter_ShouldSelectHighlightedItem()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Press Enter to select the highlighted item
        await Page.Keyboard.PressAsync("Enter");

        // Menu should close after selection
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Space_ShouldSelectHighlightedItem()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Press Space to select the highlighted item
        await Page.Keyboard.PressAsync(" ");

        // Menu should close after selection
        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Menu_ShouldClose_OnOutsideClick()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Click outside the menu
        await Page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Focus_ShouldReturnToTrigger_AfterEscapeClose()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task FirstItem_ShouldBeFocused_WhenMenuOpens()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var firstItem = Page.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();
    }

    #endregion

    #region Different Placements

    [Test]
    [Arguments("Bottom Start", "bottom")]
    [Arguments("Right Start", "right")]
    [Arguments("Top End", "top")]
    [Arguments("Left Center", "left")]
    public async Task Content_ShouldHave_CorrectDataSideAttribute(string buttonText, string expectedSide)
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = buttonText });

        // Wait for trigger to be visible, then scroll to center of viewport for placement test
        await Expect(trigger).ToBeVisibleAsync();
        await Page.EvaluateAsync(@"(element) => {
            const rect = element.getBoundingClientRect();
            const scrollY = window.scrollY + rect.top - (window.innerHeight / 2);
            window.scrollTo({ top: scrollY, behavior: 'instant' });
        }", await trigger.ElementHandleAsync());

        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-side", expectedSide);
    }

    #endregion

    #region Checkbox Items

    [Test]
    public async Task CheckboxItem_ShouldHave_RoleMenuitemcheckbox()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Settings Menu" });
        await trigger.ClickAsync();

        var checkboxItem = Page.Locator("[data-summit-dropdown-menu-checkbox-item]").First;
        await Expect(checkboxItem).ToHaveAttributeAsync("role", "menuitemcheckbox");
    }

    [Test]
    public async Task CheckboxItem_ShouldHave_AriaChecked()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Settings Menu" });
        await trigger.ClickAsync();

        // "Show Toolbar" is checked by default
        var checkboxItem = Page.Locator("[data-summit-dropdown-menu-checkbox-item]").First;
        await Expect(checkboxItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task CheckboxItem_ShouldToggle_OnClick()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Settings Menu" });
        await trigger.ClickAsync();

        // "Show Toolbar" checkbox is checked by default
        var checkboxItem = Page.Locator("[data-summit-dropdown-menu-checkbox-item]").First;
        await Expect(checkboxItem).ToHaveAttributeAsync("aria-checked", "true");

        // Click to toggle off
        await checkboxItem.ClickAsync();

        // Menu closes after selection - verify state changed via status text
        var statusText = Page.Locator("text=Toolbar: Off");
        await Expect(statusText).ToBeVisibleAsync();
    }

    #endregion

    #region Radio Items

    [Test]
    public async Task RadioItem_ShouldHave_RoleMenuitemradio()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Select Theme" });
        await trigger.ClickAsync();

        var radioItem = Page.Locator("[data-summit-dropdown-menu-radio-item]").First;
        await Expect(radioItem).ToHaveAttributeAsync("role", "menuitemradio");
    }

    [Test]
    public async Task RadioItem_ShouldHave_AriaChecked()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Select Theme" });
        await trigger.ClickAsync();

        // "System" is selected by default (third item)
        var systemRadio = Page.Locator("[data-summit-dropdown-menu-radio-item]").Nth(2);
        await Expect(systemRadio).ToHaveAttributeAsync("aria-checked", "true");

        // Other items should not be checked
        var lightRadio = Page.Locator("[data-summit-dropdown-menu-radio-item]").First;
        await Expect(lightRadio).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task RadioItem_ShouldChangeSelection_OnClick()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Select Theme" });
        await trigger.ClickAsync();

        // Verify "Light" is not checked initially
        var lightRadio = Page.Locator("[data-summit-dropdown-menu-radio-item]").First;
        await Expect(lightRadio).ToHaveAttributeAsync("aria-checked", "false");

        // Click on "Light" (first radio item)
        await lightRadio.ClickAsync();

        // Menu closes after selection - verify the state changed by checking the status text
        var statusText = Page.Locator("text=Selected theme: light");
        await Expect(statusText).ToBeVisibleAsync();
    }

    [Test]
    public async Task RadioGroup_ShouldHave_RoleGroup()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Select Theme" });
        await trigger.ClickAsync();

        var radioGroup = Page.Locator("[data-summit-dropdown-menu-radio-group]");
        await Expect(radioGroup).ToHaveAttributeAsync("role", "group");
    }

    [Test]
    public async Task RadioGroup_ShouldHave_AriaLabel()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Select Theme" });
        await trigger.ClickAsync();

        var radioGroup = Page.Locator("[data-summit-dropdown-menu-radio-group]");
        await Expect(radioGroup).ToHaveAttributeAsync("aria-label", "Theme selection");
    }

    #endregion

    #region Grouped Items

    [Test]
    public async Task Group_ShouldHave_RoleGroup()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Grouped Menu" });
        await trigger.ClickAsync();

        var group = Page.Locator("[data-summit-dropdown-menu-group]").First;
        await Expect(group).ToHaveAttributeAsync("role", "group");
    }

    [Test]
    public async Task GroupLabel_ShouldHave_IdForAriaLabelledby()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Grouped Menu" });
        await trigger.ClickAsync();

        var groupLabel = Page.Locator("[data-summit-dropdown-menu-group-label]").First;
        var labelId = await groupLabel.GetAttributeAsync("id");

        await Assert.That(labelId).IsNotNull();

        var group = Page.Locator("[data-summit-dropdown-menu-group]").First;
        var ariaLabelledby = await group.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsEqualTo(labelId);
    }

    #endregion

    #region Separator

    [Test]
    public async Task Separator_ShouldHave_RoleSeparator()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var separator = Page.Locator("[data-summit-dropdown-menu-separator]").First;
        await Expect(separator).ToHaveAttributeAsync("role", "separator");
    }

    [Test]
    public async Task Separator_ShouldHave_AriaOrientationHorizontal()
    {
        var trigger = Page.Locator("[data-summit-dropdown-menu-trigger]").First;
        await trigger.ClickAsync();

        var separator = Page.Locator("[data-summit-dropdown-menu-separator]").First;
        await Expect(separator).ToHaveAttributeAsync("aria-orientation", "horizontal");
    }

    #endregion

    #region Controlled Mode

    [Test]
    public async Task ControlledMenu_ShouldOpen_WhenExternalButtonClicked()
    {
        // Find the external toggle button (text includes state)
        var externalToggle = Page.Locator("button:has-text('Toggle Externally')");
        await Expect(externalToggle).ToBeVisibleAsync();

        // Click external button to open
        await externalToggle.ClickAsync();

        // The controlled menu content should be visible
        var controlledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Controlled Menu" });
        await Expect(controlledTrigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task ControlledMenu_ShouldClose_WhenTriggerClicked()
    {
        var externalToggle = Page.Locator("button:has-text('Toggle Externally')");
        await Expect(externalToggle).ToBeVisibleAsync();

        var controlledTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Controlled Menu" });

        // Initially closed
        await Expect(controlledTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        // Open via external toggle
        await externalToggle.ClickAsync();
        await Expect(controlledTrigger).ToHaveAttributeAsync("aria-expanded", "true");

        // Close by clicking the menu trigger itself (which calls the controlled toggle)
        await controlledTrigger.ClickAsync();
        await Expect(controlledTrigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Typeahead Search

    [Test]
    public async Task Typeahead_ShouldFocusMatchingItem()
    {
        // Use the keyboard navigation demo menu with fruit names
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Try Keyboard Navigation" });
        
        // Wait for trigger to be stable before interacting
        await Expect(trigger).ToBeVisibleAsync();
        await trigger.ClickAsync();

        // Wait for menu content to be visible and first item to be focused
        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();
        
        var firstItem = content.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();

        // Type 'c' to find "Cherry"
        await Page.Keyboard.TypeAsync("c");

        // Cherry should be focused
        var cherryItem = content.Locator("[data-summit-dropdown-menu-item]").Filter(new() { HasText = "Cherry" });
        await Expect(cherryItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task Typeahead_ShouldFocusFirstMatchingItem()
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Try Keyboard Navigation" });
        
        // Wait for trigger to be stable before interacting
        await Expect(trigger).ToBeVisibleAsync();
        await trigger.ClickAsync();

        // Wait for menu content to be visible and first item to be focused
        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();
        
        var firstItem = content.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();

        // Type 'b' to find "Banana"
        await Page.Keyboard.TypeAsync("b");

        // Banana should be focused
        var bananaItem = content.Locator("[data-summit-dropdown-menu-item]").Filter(new() { HasText = "Banana" });
        await Expect(bananaItem).ToBeFocusedAsync();
    }

    #endregion

    #region Animated Content

    [Test]
    public async Task AnimatedMenu_ShouldOpen_OnEnterKey()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-dropdown-menu-trigger]");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task AnimatedMenu_ShouldNavigate_WithArrowKeys()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-dropdown-menu-trigger]");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // First item should be focused on open
        var firstItem = content.Locator("[data-summit-dropdown-menu-item]").First;
        await Expect(firstItem).ToBeFocusedAsync();

        // Arrow down to second item
        await Page.Keyboard.PressAsync("ArrowDown");
        var secondItem = content.Locator("[data-summit-dropdown-menu-item]").Nth(1);
        await Expect(secondItem).ToBeFocusedAsync();

        // Arrow down to third item
        await Page.Keyboard.PressAsync("ArrowDown");
        var thirdItem = content.Locator("[data-summit-dropdown-menu-item]").Nth(2);
        await Expect(thirdItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task AnimatedMenu_ShouldActivateItem_OnEnterKey()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-dropdown-menu-trigger]");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Select first item with Enter
        await Page.Keyboard.PressAsync("Enter");

        // Menu should close after selection
        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task AnimatedMenu_ShouldClose_OnEscape()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-dropdown-menu-trigger]");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-summit-dropdown-menu-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Press Escape to close
        await Page.Keyboard.PressAsync("Escape");

        // Content should close (after animation completes)
        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion
}
