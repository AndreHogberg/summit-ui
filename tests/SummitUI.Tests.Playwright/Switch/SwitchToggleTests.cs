namespace SummitUI.Tests.Playwright.Switch;

/// <summary>
/// Tests for Switch toggle behavior and thumb component.
/// </summary>
public class SwitchToggleTests : SummitTestBase
{
    protected override string TestPagePath => "tests/switch/basic";

    #region Toggle Behavior

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
