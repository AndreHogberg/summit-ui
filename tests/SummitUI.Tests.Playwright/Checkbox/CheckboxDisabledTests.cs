namespace SummitUI.Tests.Playwright.Checkbox;

/// <summary>
/// Tests for disabled Checkbox behavior.
/// Verifies disabled attributes and interaction prevention.
/// </summary>
public class CheckboxDisabledTests : SummitTestBase
{
    protected override string TestPagePath => "tests/checkbox/disabled";

    #region Disabled Attributes

    [Test]
    public async Task DisabledCheckbox_ShouldHave_DisabledAttribute()
    {
        var disabledCheckbox = Page.GetByTestId("disabled-unchecked");
        await Expect(disabledCheckbox).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledCheckbox_ShouldHave_AriaDisabled()
    {
        var disabledCheckbox = Page.GetByTestId("disabled-unchecked");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledCheckbox_ShouldHave_DataDisabled()
    {
        var disabledCheckbox = Page.GetByTestId("disabled-unchecked");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("data-disabled", "");
    }

    #endregion

    #region Disabled Behavior

    [Test]
    public async Task DisabledCheckbox_ShouldNotToggle_OnClick()
    {
        var disabledCheckbox = Page.GetByTestId("disabled-unchecked");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");

        // Force click on disabled checkbox
        await disabledCheckbox.ClickAsync(new() { Force = true });

        // Should still be unchecked
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Space_ShouldNotToggle_DisabledCheckbox()
    {
        var disabledCheckbox = Page.GetByTestId("disabled-unchecked");
        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");

        await disabledCheckbox.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(disabledCheckbox).ToHaveAttributeAsync("aria-checked", "false");
    }

    #endregion

    #region Disabled Indeterminate

    [Test]
    public async Task DisabledIndeterminate_ShouldHave_CorrectState()
    {
        var checkbox = Page.GetByTestId("disabled-indeterminate");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "indeterminate");
        await Expect(checkbox).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledIndeterminate_ShouldRenderIndicator()
    {
        var checkbox = Page.GetByTestId("disabled-indeterminate");
        // Should contain the indeterminate indicator (-)
        await Expect(checkbox).ToContainTextAsync("-");
    }

    #endregion
}
