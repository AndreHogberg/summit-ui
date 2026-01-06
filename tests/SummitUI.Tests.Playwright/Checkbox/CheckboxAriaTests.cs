namespace SummitUI.Tests.Playwright.Checkbox;

/// <summary>
/// Tests for ARIA attributes on Checkbox components.
/// Verifies proper accessibility attributes and unique IDs.
/// </summary>
public class CheckboxAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/checkbox/basic";

    #region ARIA Attributes

    [Test]
    public async Task Checkbox_ShouldHave_RoleCheckbox()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await Expect(checkbox).ToHaveAttributeAsync("role", "checkbox");
    }

    [Test]
    public async Task Checkbox_ShouldHave_TypeButton()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await Expect(checkbox).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task Checkbox_ShouldHave_AriaCheckedFalse_WhenUnchecked()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Checkbox_ShouldHave_AriaCheckedTrue_WhenChecked()
    {
        var checkbox = Page.GetByTestId("checkbox-checked");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Checkbox_ShouldHave_DataStateUnchecked_WhenUnchecked()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Checkbox_ShouldHave_DataStateChecked_WhenChecked()
    {
        var checkbox = Page.GetByTestId("checkbox-checked");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Checkbox_ShouldHave_UniqueId()
    {
        var checkboxes = Page.Locator("[data-summit-checkbox]");
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

    #region Indicator Component

    [Test]
    public async Task CheckboxIndicator_ShouldRender_WhenChecked()
    {
        var checkedCheckbox = Page.GetByTestId("custom-checked");

        var indicator = checkedCheckbox.Locator("[data-summit-checkbox-indicator]");
        await Expect(indicator).ToBeVisibleAsync();
        await Expect(indicator).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task CheckboxIndicator_ShouldNotRender_WhenUnchecked()
    {
        var uncheckedCheckbox = Page.GetByTestId("custom-unchecked");

        var indicator = uncheckedCheckbox.Locator("[data-summit-checkbox-indicator]");
        await Expect(indicator).ToHaveCountAsync(0);
    }

    #endregion

    #region Controlled Mode

    [Test]
    public async Task ControlledCheckbox_ShouldReflectExternalState()
    {
        var toggleButton = Page.GetByTestId("toggle-button");
        var checkbox = Page.GetByTestId("controlled-checkbox");

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
        var toggleButton = Page.GetByTestId("toggle-button");
        var checkbox = Page.GetByTestId("controlled-checkbox");

        // Initially shows "Unchecked"
        await Expect(toggleButton).ToContainTextAsync("Unchecked");

        // Click the checkbox
        await checkbox.ClickAsync();

        // Button text should update to "Checked"
        await Expect(toggleButton).ToContainTextAsync("Checked");
    }

    #endregion
}
