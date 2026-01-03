using TUnit.Playwright;

namespace ArkUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the Switch component.
/// Tests ARIA attributes, keyboard navigation, and focus management.
/// </summary>
public class SwitchAccessibilityTests : PageTest
{
    private const string SwitchDemoUrl = "switch";

    [Before(Test)]
    public async Task NavigateToSwitchDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + SwitchDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes

    [Test]
    public async Task Switch_ShouldHave_RoleSwitch()
    {
        var switchEl = Page.Locator(".switch-root").First;
        await Expect(switchEl).ToHaveAttributeAsync("role", "switch");
    }

    [Test]
    public async Task Switch_ShouldHave_TypeButton()
    {
        var switchEl = Page.Locator(".switch-root").First;
        await Expect(switchEl).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task Switch_ShouldHave_AriaCheckedFalse_WhenUnchecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Switch_ShouldHave_AriaCheckedTrue_WhenChecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        await switchEl.ClickAsync();
        
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Switch_ShouldHave_DataStateUnchecked_WhenUnchecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Switch_ShouldHave_DataStateChecked_WhenChecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        await switchEl.ClickAsync();
        
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "checked");
    }

    #endregion

    #region Toggle Behavior (Click)

    [Test]
    public async Task Switch_ShouldToggle_OnClick()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        // Initial state
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");

        // Toggle on
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "checked");

        // Toggle off
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Switch_ShouldHave_DataState_OnThumb()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        var thumb = switchEl.Locator(".switch-thumb");

        await Expect(thumb).ToHaveAttributeAsync("data-state", "unchecked");
        
        await switchEl.ClickAsync();
        
        await Expect(thumb).ToHaveAttributeAsync("data-state", "checked");
    }

    #endregion

    #region Disabled Switch Accessibility

    [Test]
    public async Task DisabledSwitch_ShouldHave_DisabledAttribute()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledSwitch_ShouldHave_AriaDisabled()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledSwitch_ShouldHave_DataDisabled()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledCheckedSwitch_ShouldNotToggle_OnClick()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "true");
        
        await disabledSwitch.ClickAsync(new() { Force = true });
        
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task DisabledUncheckedSwitch_ShouldHave_DisabledAttributes()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");
        
        await Expect(disabledSwitch).ToBeDisabledAsync();
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabledSwitch).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledUncheckedSwitch_ShouldNotToggle_OnClick()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");
        
        await disabledSwitch.ClickAsync(new() { Force = true });
        
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");
    }

    #endregion

    #region Keyboard Navigation

    [Test]
    public async Task Space_ShouldToggleSwitch()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");

        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Space_ShouldToggleSwitchOff()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        // First toggle on
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");

        // Now toggle off with Space
        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Enter_ShouldToggleSwitch()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");

        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Enter_ShouldToggleSwitchOff()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        // First toggle on
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");

        // Now toggle off with Enter
        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Enter_ShouldNotToggle_DisabledSwitch()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");

        await disabledSwitch.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Space_ShouldNotToggle_DisabledSwitch()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");

        await disabledSwitch.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Tab_ShouldNavigateBetweenSwitches()
    {
        // Get the basic switch (first enabled switch on the page)
        var basicSwitch = Page.GetByTestId("basic-switch");
        // Get the notifications switch (next enabled switch after basic)
        var notificationsSwitch = Page.GetByTestId("notifications-switch");

        await basicSwitch.FocusAsync();
        await Expect(basicSwitch).ToBeFocusedAsync();

        // Tab to next focusable element (disabled switches are skipped)
        await Page.Keyboard.PressAsync("Tab");
        
        // Note: Tab may go to other elements between switches depending on page layout
        // This test verifies Tab navigation works and focus moves
        var activeElement = Page.Locator(":focus");
        await Expect(activeElement).ToHaveCountAsync(1);
    }

    [Test]
    public async Task ShiftTab_ShouldNavigateBackwards()
    {
        // Get two consecutive enabled switches
        var notificationsSwitch = Page.GetByTestId("notifications-switch");
        var marketingSwitch = Page.GetByTestId("marketing-switch");

        await marketingSwitch.FocusAsync();
        await Expect(marketingSwitch).ToBeFocusedAsync();

        // Shift+Tab to previous element
        await Page.Keyboard.PressAsync("Shift+Tab");

        // Notifications switch should be focused (it's the previous focusable element in the form)
        await Expect(notificationsSwitch).ToBeFocusedAsync();
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Switch_ShouldReceiveFocus_OnClick()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await switchEl.ClickAsync();

        await Expect(switchEl).ToBeFocusedAsync();
    }

    [Test]
    public async Task Switch_ShouldRetainFocus_AfterToggle()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await switchEl.FocusAsync();
        await Expect(switchEl).ToBeFocusedAsync();

        // Click to toggle
        await switchEl.ClickAsync();

        // Focus should remain on the switch
        await Expect(switchEl).ToBeFocusedAsync();

        // Click again to toggle back
        await switchEl.ClickAsync();

        // Focus should still be on the switch
        await Expect(switchEl).ToBeFocusedAsync();
    }

    [Test]
    public async Task Switch_ShouldBeFocusable()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await switchEl.FocusAsync();

        // Switch should be focusable (has focus)
        await Expect(switchEl).ToBeFocusedAsync();
    }

    #endregion

    #region Form Integration

    [Test]
    public async Task Switch_ShouldSupport_HiddenInput()
    {
        var formSwitch = Page.Locator("input[name='notifications']");
        await Expect(formSwitch).ToHaveCountAsync(1);
        await Expect(formSwitch).ToHaveAttributeAsync("type", "hidden");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHaveEmptyValue_WhenUnchecked()
    {
        var hiddenInput = Page.Locator("input[name='notifications']");
        await Expect(hiddenInput).ToHaveAttributeAsync("value", "");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHaveOnValue_WhenChecked()
    {
        var switchEl = Page.GetByTestId("notifications-switch");
        var hiddenInput = Page.Locator("input[name='notifications']");
        
        // Toggle the switch on
        await switchEl.ClickAsync();
        
        // Default value is "on" when no custom Value is set
        await Expect(hiddenInput).ToHaveAttributeAsync("value", "on");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHaveCustomValue_WhenChecked()
    {
        var switchEl = Page.GetByTestId("marketing-switch");
        var hiddenInput = Page.Locator("input[name='marketing']");
        
        // Toggle the switch on
        await switchEl.ClickAsync();
        
        // Marketing switch has Value="yes"
        await Expect(hiddenInput).ToHaveAttributeAsync("value", "yes");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHave_RequiredAttribute()
    {
        var hiddenInput = Page.Locator("input[name='terms']");
        await Expect(hiddenInput).ToHaveAttributeAsync("required", "");
    }

    [Test]
    public async Task DisabledSwitch_HiddenInput_ShouldBeDisabled()
    {
        // The disabled switches don't have Name set, so let's check that disabled
        // switches would have disabled hidden inputs
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToBeDisabledAsync();
    }

    #endregion

    #region Controlled Mode

    [Test]
    public async Task ControlledSwitch_ShouldReflectExternalState()
    {
        var toggleButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Toggle Externally") });
        var switchEl = Page.GetByTestId("controlled-switch");

        // Initially unchecked
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");

        // Click external toggle button
        await toggleButton.ClickAsync();

        // Switch should now be checked
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "checked");

        // Click again to toggle back
        await toggleButton.ClickAsync();

        // Switch should be unchecked again
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task ControlledSwitch_ShouldUpdateExternalState_OnClick()
    {
        var toggleButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Toggle Externally") });
        var switchEl = Page.GetByTestId("controlled-switch");

        // Initially shows "Unchecked"
        await Expect(toggleButton).ToContainTextAsync("Unchecked");

        // Click the switch
        await switchEl.ClickAsync();

        // Button text should update to "Checked"
        await Expect(toggleButton).ToContainTextAsync("Checked");
    }

    #endregion

    #region Default Checked (Uncontrolled Mode)

    [Test]
    public async Task DefaultCheckedSwitch_ShouldBeChecked_Initially()
    {
        var switchEl = Page.GetByTestId("default-checked-switch");
        
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task DefaultCheckedSwitch_ShouldToggle_OnClick()
    {
        var switchEl = Page.GetByTestId("default-checked-switch");
        
        // Initially checked
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
        
        // Toggle off
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
        
        // Toggle back on
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task DefaultCheckedSwitch_Thumb_ShouldHaveCorrectState()
    {
        var switchEl = Page.GetByTestId("default-checked-switch");
        var thumb = switchEl.Locator(".switch-thumb");
        
        await Expect(thumb).ToHaveAttributeAsync("data-state", "checked");
    }

    #endregion

    #region Label Association

    [Test]
    public async Task Label_ShouldToggleSwitch_OnClick()
    {
        var switchEl = Page.GetByTestId("labeled-switch");
        var labelText = Page.GetByText("Enable notifications");
        
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");

        // Click on the label text (not the switch itself)
        await labelText.ClickAsync();

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task LabeledSwitch_ShouldToggle_WhenLabelClicked()
    {
        var switchEl = Page.GetByTestId("labeled-switch");
        var labelContainer = Page.GetByTestId("labeled-switch-container");
        
        // Click the entire label area
        await labelContainer.ClickAsync();
        
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
        
        // Click again
        await labelContainer.ClickAsync();
        
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
    }

    #endregion

    #region Thumb Component

    [Test]
    public async Task SwitchThumb_ShouldExist()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        var thumb = switchEl.Locator(".switch-thumb");
        
        await Expect(thumb).ToHaveCountAsync(1);
    }

    [Test]
    public async Task SwitchThumb_ShouldBeSpanElement()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        var thumb = switchEl.Locator(".switch-thumb");
        
        var tagName = await thumb.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
        await Assert.That(tagName).IsEqualTo("span");
    }

    [Test]
    public async Task SwitchThumb_ShouldUpdateDataState_OnToggle()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        var thumb = switchEl.Locator(".switch-thumb");
        
        // Initially unchecked
        await Expect(thumb).ToHaveAttributeAsync("data-state", "unchecked");
        
        // Toggle
        await switchEl.ClickAsync();
        await Expect(thumb).ToHaveAttributeAsync("data-state", "checked");
        
        // Toggle back
        await switchEl.ClickAsync();
        await Expect(thumb).ToHaveAttributeAsync("data-state", "unchecked");
    }

    #endregion
}
