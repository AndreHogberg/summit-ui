using TUnit.Playwright;

namespace ArkUI.Tests.Playwright;

/// <summary>
/// Tests for the Checkbox component's integration with Blazor EditForm.
/// Verifies model binding, validation, and form submission behavior.
/// </summary>
public class CheckboxEditFormTests : PageTest
{
    private const string CheckboxDemoUrl = "checkbox";

    [Before(Test)]
    public async Task NavigateToCheckboxDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + CheckboxDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region Model Binding

    [Test]
    public async Task EditForm_ShouldBindCheckedValue_ToModel()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var checkbox = section.Locator("[data-testid='editform-terms-checkbox']");

        // Initially unchecked
        var termsValue = section.Locator("[data-testid='editform-terms-value']");
        await Expect(termsValue).ToHaveTextAsync("False");

        // Click to check
        await checkbox.ClickAsync();

        // Verify the value is bound to the model
        await Expect(termsValue).ToHaveTextAsync("True");
    }

    [Test]
    public async Task EditForm_ShouldBindMultipleCheckboxValues_ToModel()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var termsCheckbox = section.Locator("[data-testid='editform-terms-checkbox']");
        var newsletterCheckbox = section.Locator("[data-testid='editform-newsletter-checkbox']");
        var notificationsCheckbox = section.Locator("[data-testid='editform-notifications-checkbox']");

        // Check all checkboxes
        await termsCheckbox.ClickAsync();
        await newsletterCheckbox.ClickAsync();
        await notificationsCheckbox.ClickAsync();

        // Verify all values are bound
        var termsValue = section.Locator("[data-testid='editform-terms-value']");
        var newsletterValue = section.Locator("[data-testid='editform-newsletter-value']");
        var notificationsValue = section.Locator("[data-testid='editform-notifications-value']");

        await Expect(termsValue).ToHaveTextAsync("True");
        await Expect(newsletterValue).ToHaveTextAsync("True");
        await Expect(notificationsValue).ToHaveTextAsync("True");
    }

    [Test]
    public async Task EditForm_ShouldUpdateModel_WhenCheckboxToggled()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var checkbox = section.Locator("[data-testid='editform-terms-checkbox']");
        var termsValue = section.Locator("[data-testid='editform-terms-value']");

        // Check
        await checkbox.ClickAsync();
        await Expect(termsValue).ToHaveTextAsync("True");

        // Uncheck
        await checkbox.ClickAsync();
        await Expect(termsValue).ToHaveTextAsync("False");
    }

    #endregion

    #region Form Submission

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_WhenRequiredFieldChecked()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Check required terms checkbox
        var termsCheckbox = section.Locator("[data-testid='editform-terms-checkbox']");
        await termsCheckbox.ClickAsync();

        // Submit form
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify success message appears
        var successMessage = section.Locator("[data-testid='editform-success-message']");
        await Expect(successMessage).ToBeVisibleAsync();
        await Expect(successMessage).ToHaveTextAsync("Form submitted successfully!");
    }

    [Test]
    public async Task EditForm_ShouldShowValidationError_WhenRequiredFieldNotChecked()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit form without checking required field
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify validation error appears
        var validationError = section.Locator(".validation-error");
        await Expect(validationError).ToBeVisibleAsync();
        await Expect(validationError).ToContainTextAsync("You must accept the terms");
    }

    [Test]
    public async Task EditForm_ShouldClearValidationError_WhenFieldChecked()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit form without checking required field to trigger validation
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify validation error appears
        var validationError = section.Locator(".validation-error");
        await Expect(validationError).ToBeVisibleAsync();

        // Now check the required field
        var termsCheckbox = section.Locator("[data-testid='editform-terms-checkbox']");
        await termsCheckbox.ClickAsync();

        // Submit again
        await submitButton.ClickAsync();

        // Success message should appear
        var successMessage = section.Locator("[data-testid='editform-success-message']");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Hidden Input for Form Submission

    [Test]
    public async Task EditForm_ShouldRenderHiddenInput_WithCheckedValue()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var termsCheckbox = section.Locator("[data-testid='editform-terms-checkbox']");

        // Check the checkbox
        await termsCheckbox.ClickAsync();

        // Verify hidden input has the value
        var hiddenInput = section.Locator("input[type='hidden'][name='acceptTerms']");
        await Expect(hiddenInput).ToHaveValueAsync("on");
    }

    [Test]
    public async Task EditForm_ShouldUpdateHiddenInput_WhenValueChanges()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var termsCheckbox = section.Locator("[data-testid='editform-terms-checkbox']");

        // Check the checkbox
        await termsCheckbox.ClickAsync();

        var hiddenInput = section.Locator("input[type='hidden'][name='acceptTerms']");
        await Expect(hiddenInput).ToHaveValueAsync("on");

        // Uncheck the checkbox
        await termsCheckbox.ClickAsync();

        // Hidden input value should be empty
        await Expect(hiddenInput).ToHaveValueAsync("");
    }

    #endregion

    #region Keyboard Interaction with EditForm

    [Test]
    public async Task EditForm_ShouldBindValue_WhenToggledViaKeyboard()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var checkbox = section.Locator("[data-testid='editform-terms-checkbox']");
        var termsValue = section.Locator("[data-testid='editform-terms-value']");

        // Focus and toggle via keyboard
        await checkbox.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        // Verify value is bound
        await Expect(termsValue).ToHaveTextAsync("True");
    }

    [Test]
    public async Task EditForm_ShouldNotToggle_WhenEnterPressed()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var checkbox = section.Locator("[data-testid='editform-terms-checkbox']");
        var termsValue = section.Locator("[data-testid='editform-terms-value']");

        // Focus and press Enter (should not toggle per WAI-ARIA)
        await checkbox.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Value should still be false
        await Expect(termsValue).ToHaveTextAsync("False");
    }

    [Test]
    public async Task EditForm_ShouldSubmit_ViaKeyboard()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var checkbox = section.Locator("[data-testid='editform-terms-checkbox']");

        // Check via keyboard
        await checkbox.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        // Tab to submit button and press Enter
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Verify success
        var successMessage = section.Locator("[data-testid='editform-success-message']");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Data State Attributes

    [Test]
    public async Task Checkbox_ShouldHaveDataStateChecked_WhenChecked()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var checkbox = section.Locator("[data-testid='editform-terms-checkbox']");

        await checkbox.ClickAsync();

        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task Checkbox_ShouldHaveDataStateUnchecked_WhenUnchecked()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var checkbox = section.Locator("[data-testid='editform-terms-checkbox']");

        await Expect(checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    #endregion

    #region Tab Navigation

    [Test]
    public async Task Tab_ShouldNavigateBetweenCheckboxes_InEditForm()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var termsCheckbox = section.Locator("[data-testid='editform-terms-checkbox']");
        var newsletterCheckbox = section.Locator("[data-testid='editform-newsletter-checkbox']");

        await termsCheckbox.FocusAsync();
        await Expect(termsCheckbox).ToBeFocusedAsync();

        // Tab to next checkbox
        await Page.Keyboard.PressAsync("Tab");

        // Newsletter checkbox should be focused
        await Expect(newsletterCheckbox).ToBeFocusedAsync();
    }

    #endregion
}
