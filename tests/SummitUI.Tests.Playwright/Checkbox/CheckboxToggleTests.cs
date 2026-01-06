namespace SummitUI.Tests.Playwright.Checkbox;

/// <summary>
/// Tests for Checkbox toggle behavior (click and keyboard).
/// Verifies state changes and focus management.
/// </summary>
public class CheckboxToggleTests : SummitTestBase
{
    protected override string TestPagePath => "tests/checkbox/basic";

    #region Toggle Behavior (Click)

    [Test]
    public async Task Click_ShouldCheckUncheckedCheckbox()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        await checkbox.ClickAsync();

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Click_ShouldUncheckCheckedCheckbox()
    {
        var checkbox = Page.GetByTestId("checkbox-checked");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");

        await checkbox.ClickAsync();

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    #endregion

    #region Keyboard Navigation

    [Test]
    public async Task Space_ShouldToggleCheckbox()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        await checkbox.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Enter_ShouldNotToggleCheckbox()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        await checkbox.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Enter should NOT toggle checkboxes (only Space should per WAI-ARIA)
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Checkbox_ShouldReceiveFocus_OnClick()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await checkbox.ClickAsync();

        await Expect(checkbox).ToBeFocusedAsync();
    }

    [Test]
    public async Task Checkbox_ShouldRetainFocus_AfterToggle()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
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
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        await checkbox.FocusAsync();

        // Checkbox should be focusable (has focus)
        await Expect(checkbox).ToBeFocusedAsync();
    }

    #endregion

    #region Label Association

    [Test]
    public async Task Label_ShouldToggleCheckbox_OnClick()
    {
        var checkbox = Page.GetByTestId("checkbox-unchecked");
        var label = checkbox.Locator("xpath=ancestor::label");

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "false");

        // Click on the label text (not the checkbox itself)
        await label.ClickAsync();

        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
    }

    #endregion
}
