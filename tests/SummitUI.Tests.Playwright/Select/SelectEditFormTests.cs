namespace SummitUI.Tests.Playwright.Select;

/// <summary>
/// Tests for Select component's integration with Blazor EditForm.
/// Verifies model binding, validation, and form submission behavior.
/// </summary>
public class SelectEditFormTests : SummitTestBase
{
    protected override string TestPagePath => "tests/select/editform";

    #region Model Binding

    [Test]
    public async Task EditForm_ShouldBindSelectedValue_ToModel()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await categoryTrigger.ClickAsync();

        var content = Page.GetByTestId("category-content");
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = Page.GetByTestId("item-electronics");
        await electronicsItem.ClickAsync();

        // Verify the value is displayed
        var categoryValue = Page.GetByTestId("category-value");
        await Expect(categoryValue).ToHaveTextAsync("electronics");
    }

    [Test]
    public async Task EditForm_ShouldBindMultipleSelectValues_ToModel()
    {
        // Select category
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await categoryTrigger.ClickAsync();

        var categoryContent = Page.GetByTestId("category-content");
        await Expect(categoryContent).ToBeVisibleAsync();

        var clothingItem = Page.GetByTestId("item-clothing");
        await clothingItem.ClickAsync();

        await Expect(categoryContent).Not.ToBeVisibleAsync();

        // Select priority
        var priorityTrigger = Page.GetByTestId("priority-trigger");
        await priorityTrigger.ClickAsync();

        var priorityContent = Page.GetByTestId("priority-content");
        await Expect(priorityContent).ToBeVisibleAsync();

        var highItem = Page.GetByTestId("item-high");
        await highItem.ClickAsync();

        // Verify both values are bound
        var categoryValue = Page.GetByTestId("category-value");
        var priorityValue = Page.GetByTestId("priority-value");

        await Expect(categoryValue).ToHaveTextAsync("clothing");
        await Expect(priorityValue).ToHaveTextAsync("high");
    }

    [Test]
    public async Task EditForm_ShouldUpdateModel_WhenSelectionChanges()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");

        // Select first option
        await categoryTrigger.ClickAsync();
        await Expect(Page.GetByTestId("category-content")).ToBeVisibleAsync();

        var electronicsItem = Page.GetByTestId("item-electronics");
        await electronicsItem.ClickAsync();

        await Expect(Page.GetByTestId("category-content")).Not.ToBeVisibleAsync();

        // Wait for debounce protection to expire
        await Page.WaitForTimeoutAsync(200);

        // Select different option - use fresh locator after reopening
        await categoryTrigger.ClickAsync();
        await Expect(Page.GetByTestId("category-content")).ToBeVisibleAsync();

        var booksItem = Page.GetByTestId("item-books");
        await booksItem.ClickAsync();

        // Verify model is updated to new value
        var categoryValue = Page.GetByTestId("category-value");
        await Expect(categoryValue).ToHaveTextAsync("books");
    }

    #endregion

    #region Form Submission

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_WhenRequiredFieldsFilled()
    {
        // Select required category
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await categoryTrigger.ClickAsync();

        var content = Page.GetByTestId("category-content");
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = Page.GetByTestId("item-electronics");
        await electronicsItem.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();

        // Submit form
        var submitButton = Page.GetByTestId("submit-button");
        await submitButton.ClickAsync();

        // Verify success message appears
        var successMessage = Page.GetByTestId("success-message");
        await Expect(successMessage).ToBeVisibleAsync();
        await Expect(successMessage).ToHaveTextAsync("Form submitted successfully!");
    }

    [Test]
    public async Task EditForm_ShouldShowValidationError_WhenRequiredFieldEmpty()
    {
        // Submit form without selecting required field
        var submitButton = Page.GetByTestId("submit-button");
        await submitButton.ClickAsync();

        // Verify validation error appears
        var categoryError = Page.GetByTestId("category-error");
        await Expect(categoryError).ToBeVisibleAsync();
        await Expect(categoryError).ToHaveTextAsync("Category is required");
    }

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_AfterFillingRequiredField()
    {
        // Submit form without selecting required field to trigger validation
        var submitButton = Page.GetByTestId("submit-button");
        await submitButton.ClickAsync();

        // Verify validation error appears
        var categoryError = Page.GetByTestId("category-error");
        await Expect(categoryError).ToBeVisibleAsync();

        // Now select a value
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await categoryTrigger.ClickAsync();

        var content = Page.GetByTestId("category-content");
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = Page.GetByTestId("item-electronics");
        await electronicsItem.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();

        // Submit again
        await submitButton.ClickAsync();

        // Success message should appear
        var successMessage = Page.GetByTestId("success-message");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Invalid State Attributes

    [Test]
    public async Task Trigger_ShouldHaveDataInvalid_WhenValidationFails()
    {
        // Submit without filling required field
        var submitButton = Page.GetByTestId("submit-button");
        await submitButton.ClickAsync();

        // Trigger should have data-invalid attribute
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await Expect(categoryTrigger).ToHaveAttributeAsync("data-invalid", "");
    }

    [Test]
    public async Task Trigger_ShouldHaveAriaInvalid_WhenValidationFails()
    {
        // Submit without filling required field
        var submitButton = Page.GetByTestId("submit-button");
        await submitButton.ClickAsync();

        // Trigger should have aria-invalid=true
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await Expect(categoryTrigger).ToHaveAttributeAsync("aria-invalid", "true");
    }

    #endregion

    #region Required Attribute

    [Test]
    public async Task Trigger_ShouldHaveAriaRequired_WhenRequired()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await Expect(categoryTrigger).ToHaveAttributeAsync("aria-required", "true");
    }

    [Test]
    public async Task Trigger_ShouldNotHaveAriaRequired_WhenNotRequired()
    {
        var priorityTrigger = Page.GetByTestId("priority-trigger");

        // Priority field is not required, should not have aria-required="true"
        var ariaRequired = await priorityTrigger.GetAttributeAsync("aria-required");
        await Assert.That(ariaRequired).IsNull().Or.IsEqualTo("false");
    }

    #endregion

    #region Keyboard Interaction with EditForm

    [Test]
    public async Task EditForm_ShouldBindValue_WhenSelectedViaKeyboard()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");

        // Focus and open via keyboard
        await categoryTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("category-content");
        await Expect(content).ToBeVisibleAsync();

        // Navigate to second item
        await Page.Keyboard.PressAsync("ArrowDown");

        // Select via Enter
        await Page.Keyboard.PressAsync("Enter");

        // Verify value is bound (second item is "clothing")
        var categoryValue = Page.GetByTestId("category-value");
        await Expect(categoryValue).ToHaveTextAsync("clothing");
    }

    [Test]
    public async Task EditForm_ShouldSubmit_ViaKeyboard()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");

        // Select value via keyboard
        await categoryTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("category-content");
        await Expect(content).ToBeVisibleAsync();

        // Select first item
        await Page.Keyboard.PressAsync("Enter");

        await Expect(content).Not.ToBeVisibleAsync();

        // Tab to submit button and press Enter
        var submitButton = Page.GetByTestId("submit-button");
        await submitButton.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Verify success
        var successMessage = Page.GetByTestId("success-message");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Hidden Input for Form Submission

    [Test]
    public async Task EditForm_ShouldRenderHiddenInput_WithSelectedValue()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");

        // Select a value
        await categoryTrigger.ClickAsync();

        var content = Page.GetByTestId("category-content");
        await Expect(content).ToBeVisibleAsync();

        var electronicsItem = Page.GetByTestId("item-electronics");
        await electronicsItem.ClickAsync();

        // Verify hidden input has the selected value
        var hiddenInput = Page.Locator("input[type='hidden'][name='category']");
        await Expect(hiddenInput).ToHaveValueAsync("electronics");
    }

    [Test]
    public async Task EditForm_ShouldUpdateHiddenInput_WhenValueChanges()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");

        // Select first value
        await categoryTrigger.ClickAsync();
        await Expect(Page.GetByTestId("category-content")).ToBeVisibleAsync();

        var electronicsItem = Page.GetByTestId("item-electronics");
        await electronicsItem.ClickAsync();

        var hiddenInput = Page.Locator("input[type='hidden'][name='category']");
        await Expect(hiddenInput).ToHaveValueAsync("electronics");

        // Wait for debounce protection to expire
        await Expect(Page.GetByTestId("category-content")).Not.ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(200);

        // Select different value - use fresh locator after reopening
        await categoryTrigger.ClickAsync();
        await Expect(Page.GetByTestId("category-content")).ToBeVisibleAsync();

        var clothingItem = Page.GetByTestId("item-clothing");
        await clothingItem.ClickAsync();

        // Hidden input should be updated
        await Expect(hiddenInput).ToHaveValueAsync("clothing");
    }

    #endregion

    #region Label Association

    [Test]
    public async Task Trigger_ShouldHaveAriaLabelledby_WhenProvided()
    {
        var categoryTrigger = Page.GetByTestId("category-trigger");
        await Expect(categoryTrigger).ToHaveAttributeAsync("aria-labelledby", "category-label");
    }

    [Test]
    public async Task PriorityTrigger_ShouldHaveAriaLabelledby()
    {
        var priorityTrigger = Page.GetByTestId("priority-trigger");
        await Expect(priorityTrigger).ToHaveAttributeAsync("aria-labelledby", "priority-label");
    }

    #endregion
}
