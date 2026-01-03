using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Tests for the Select component's integration with Blazor EditForm.
/// Verifies model binding, validation, and form submission behavior.
/// </summary>
public class SelectEditFormTests : PageTest
{
    private const string SelectDemoUrl = "select";

    [Before(Test)]
    public async Task NavigateToSelectDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + SelectDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region Model Binding

    [Test]
    public async Task EditForm_ShouldBindSelectedValue_ToModel()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-category-trigger']");

        // Open the select
        await trigger.ClickAsync();

        // Wait for content to be visible
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Select "Electronics"
        var electronicsItem = content.Locator("[data-ark-select-item][data-value='electronics']");
        await electronicsItem.ClickAsync();

        // Verify the value is bound to the model
        var categoryValue = section.Locator("[data-testid='editform-category-value']");
        await Expect(categoryValue).ToHaveTextAsync("electronics");
    }

    [Test]
    public async Task EditForm_ShouldBindMultipleSelectValues_ToModel()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Select category
        var categoryTrigger = section.Locator("[data-testid='editform-category-trigger']");
        await categoryTrigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var clothingItem = content.Locator("[data-ark-select-item][data-value='clothing']");
        await clothingItem.ClickAsync();

        // Wait for dropdown to close
        await Expect(content).Not.ToBeVisibleAsync();

        // Small delay to avoid debounce
        await Page.WaitForTimeoutAsync(150);

        // Select priority
        var priorityTrigger = section.Locator("[data-testid='editform-priority-trigger']");
        await priorityTrigger.ClickAsync();

        content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var highItem = content.Locator("[data-ark-select-item][data-value='high']");
        await highItem.ClickAsync();

        // Verify both values are bound
        var categoryValue = section.Locator("[data-testid='editform-category-value']");
        var priorityValue = section.Locator("[data-testid='editform-priority-value']");

        await Expect(categoryValue).ToHaveTextAsync("clothing");
        await Expect(priorityValue).ToHaveTextAsync("high");
    }

    [Test]
    public async Task EditForm_ShouldUpdateModel_WhenSelectionChanges()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-category-trigger']");

        // Select first option
        await trigger.ClickAsync();
        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = content.Locator("[data-ark-select-item][data-value='electronics']");
        await electronicsItem.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(150);

        // Select different option
        await trigger.ClickAsync();
        content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var booksItem = content.Locator("[data-ark-select-item][data-value='books']");
        await booksItem.ClickAsync();

        // Verify model is updated to new value
        var categoryValue = section.Locator("[data-testid='editform-category-value']");
        await Expect(categoryValue).ToHaveTextAsync("books");
    }

    #endregion

    #region Form Submission

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_WhenRequiredFieldsFilled()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Select required category
        var categoryTrigger = section.Locator("[data-testid='editform-category-trigger']");
        await categoryTrigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = content.Locator("[data-ark-select-item][data-value='electronics']");
        await electronicsItem.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();

        // Submit form
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify success message appears
        var successMessage = section.Locator("[data-testid='editform-success-message']");
        await Expect(successMessage).ToBeVisibleAsync();
        await Expect(successMessage).ToHaveTextAsync("Form submitted successfully!");
    }

    [Test]
    public async Task EditForm_ShouldShowValidationError_WhenRequiredFieldEmpty()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit form without selecting required field
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify validation error appears
        var validationError = section.Locator(".validation-error");
        await Expect(validationError).ToBeVisibleAsync();
        await Expect(validationError).ToHaveTextAsync("Category is required");
    }

    [Test]
    public async Task EditForm_ShouldClearValidationError_WhenFieldFilled()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit form without selecting required field to trigger validation
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify validation error appears
        var validationError = section.Locator(".validation-error");
        await Expect(validationError).ToBeVisibleAsync();

        // Now select a value
        var categoryTrigger = section.Locator("[data-testid='editform-category-trigger']");
        await categoryTrigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = content.Locator("[data-ark-select-item][data-value='electronics']");
        await electronicsItem.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();

        // Submit again
        await submitButton.ClickAsync();

        // Success message should appear
        var successMessage = section.Locator("[data-testid='editform-success-message']");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Hidden Input for Form Submission

    [Test]
    public async Task EditForm_ShouldRenderHiddenInput_WithSelectedValue()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-category-trigger']");

        // Select a value
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = content.Locator("[data-ark-select-item][data-value='electronics']");
        await electronicsItem.ClickAsync();

        // Verify hidden input has the selected value
        var hiddenInput = section.Locator("input[type='hidden'][name='category']");
        await Expect(hiddenInput).ToHaveValueAsync("electronics");
    }

    [Test]
    public async Task EditForm_ShouldUpdateHiddenInput_WhenValueChanges()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-category-trigger']");

        // Select first value
        await trigger.ClickAsync();

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = content.Locator("[data-ark-select-item][data-value='electronics']");
        await electronicsItem.ClickAsync();

        var hiddenInput = section.Locator("input[type='hidden'][name='category']");
        await Expect(hiddenInput).ToHaveValueAsync("electronics");

        // Wait and select different value
        await Expect(content).Not.ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(150);

        await trigger.ClickAsync();
        content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var clothingItem = content.Locator("[data-ark-select-item][data-value='clothing']");
        await clothingItem.ClickAsync();

        // Hidden input should be updated
        await Expect(hiddenInput).ToHaveValueAsync("clothing");
    }

    #endregion

    #region Invalid State with EditForm

    [Test]
    public async Task EditForm_TriggerShouldHaveDataInvalid_WhenValidationFails()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit without filling required field
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Trigger should have data-invalid attribute
        var trigger = section.Locator("[data-testid='editform-category-trigger']");
        await Expect(trigger).ToHaveAttributeAsync("data-invalid", "");
    }

    [Test]
    public async Task EditForm_TriggerShouldHaveAriaInvalid_WhenValidationFails()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit without filling required field
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Trigger should have aria-invalid=true
        var trigger = section.Locator("[data-testid='editform-category-trigger']");
        await Expect(trigger).ToHaveAttributeAsync("aria-invalid", "true");
    }

    #endregion

    #region Keyboard Interaction with EditForm

    [Test]
    public async Task EditForm_ShouldBindValue_WhenSelectedViaKeyboard()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-category-trigger']");

        // Focus and open via keyboard
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Navigate to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Select via Enter
        await Page.Keyboard.PressAsync("Enter");

        // Verify value is bound
        var categoryValue = section.Locator("[data-testid='editform-category-value']");
        await Expect(categoryValue).ToHaveTextAsync("clothing");
    }

    [Test]
    public async Task EditForm_ShouldSubmit_ViaKeyboard()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-category-trigger']");

        // Select value via keyboard
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-ark-select-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Select first item
        await Page.Keyboard.PressAsync("Enter");

        await Expect(content).Not.ToBeVisibleAsync();

        // Tab to submit button and press Enter
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Verify success
        var successMessage = section.Locator("[data-testid='editform-success-message']");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Required Attribute

    [Test]
    public async Task EditForm_TriggerShouldHaveAriaRequired_WhenRequired()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-category-trigger']");

        await Expect(trigger).ToHaveAttributeAsync("aria-required", "true");
    }

    [Test]
    public async Task EditForm_TriggerShouldNotHaveAriaRequired_WhenNotRequired()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-priority-trigger']");

        // Priority field is not required, should not have aria-required
        var ariaRequired = await trigger.GetAttributeAsync("aria-required");
        await Assert.That(ariaRequired).IsNull().Or.IsEqualTo("false");
    }

    #endregion
}
