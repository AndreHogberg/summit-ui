namespace SummitUI.Tests.Playwright.RadioGroup;

/// <summary>
/// Tests for RadioGroup form integration.
/// </summary>
public class RadioGroupFormTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/form";

    [Test]
    public async Task HiddenInput_ShouldBeRendered_WhenNameIsProvided()
    {
        var hiddenInput = Page.Locator("input[type='hidden'][name='plan']");
        await Expect(hiddenInput).ToHaveCountAsync(1);
    }

    [Test]
    public async Task HiddenInput_ShouldHaveCorrectValue()
    {
        var hiddenInput = Page.Locator("input[type='hidden'][name='plan']");

        // Initial value should be "basic"
        await Expect(hiddenInput).ToHaveValueAsync("basic");

        // Select standard
        var standardItem = Page.GetByTestId("form-standard");
        await standardItem.ClickAsync();

        // Hidden input should update
        await Expect(hiddenInput).ToHaveValueAsync("standard");
    }

    [Test]
    public async Task FormSubmission_ShouldIncludeValue()
    {
        var submitButton = Page.GetByTestId("submit-btn");
        var submittedValue = Page.GetByTestId("form-submitted-value");

        // Initial state
        await Expect(submittedValue).ToHaveTextAsync("Submitted value: (not submitted)");

        // Submit form
        await submitButton.ClickAsync();

        // Should show the submitted value
        await Expect(submittedValue).ToHaveTextAsync("Submitted value: basic");
    }

    [Test]
    public async Task FormSubmission_ShouldReflectSelectedValue()
    {
        var premiumItem = Page.GetByTestId("form-premium");
        var submitButton = Page.GetByTestId("submit-btn");
        var submittedValue = Page.GetByTestId("form-submitted-value");

        // Select premium
        await premiumItem.ClickAsync();

        // Submit form
        await submitButton.ClickAsync();

        // Should show premium as submitted value
        await Expect(submittedValue).ToHaveTextAsync("Submitted value: premium");
    }

    [Test]
    public async Task RequiredGroup_ShouldHave_AriaRequired()
    {
        var requiredGroup = Page.GetByTestId("required-radio-group");
        await Expect(requiredGroup).ToHaveAttributeAsync("aria-required", "true");
    }

    [Test]
    public async Task AriaLabelledBy_ShouldReference_LabelElement()
    {
        var radioGroup = Page.GetByTestId("labelledby-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("aria-labelledby", "shipping-label");

        // Verify the label element exists
        var label = Page.Locator("#shipping-label");
        await Expect(label).ToHaveTextAsync("Shipping Method");
    }

    [Test]
    public async Task AriaDescribedBy_ShouldReference_DescriptionElement()
    {
        var radioGroup = Page.GetByTestId("describedby-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("aria-describedby", "notification-description");

        // Verify the description element exists
        var description = Page.Locator("#notification-description");
        await Expect(description).ToContainTextAsync("Choose how you want to receive notifications");
    }

    [Test]
    public async Task EditForm_ShouldShowValidation_WhenNoValueSelected()
    {
        var submitButton = Page.GetByTestId("editform-submit-btn");
        var validationMessage = Page.GetByTestId("editform-validation");
        var submittedValue = Page.GetByTestId("editform-submitted-value");

        // Submit without selecting anything
        await submitButton.ClickAsync();

        // Validation message should appear
        await Expect(validationMessage).ToContainTextAsync("Please select a plan");

        // Form should not have submitted
        await Expect(submittedValue).ToHaveTextAsync("Submitted: (not submitted)");
    }

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_WhenValueSelected()
    {
        var professionalItem = Page.GetByTestId("editform-professional");
        var submitButton = Page.GetByTestId("editform-submit-btn");
        var submittedValue = Page.GetByTestId("editform-submitted-value");

        // Select professional
        await professionalItem.ClickAsync();

        // Submit form
        await submitButton.ClickAsync();

        // Should show the submitted value
        await Expect(submittedValue).ToHaveTextAsync("Submitted: professional");
    }

    [Test]
    public async Task EditForm_ValidationShouldClear_OnResubmit()
    {
        var submitButton = Page.GetByTestId("editform-submit-btn");
        var validationMessage = Page.GetByTestId("editform-validation");
        var starterItem = Page.GetByTestId("editform-starter");
        var submittedValue = Page.GetByTestId("editform-submitted-value");

        // Submit without selecting anything to trigger validation
        await submitButton.ClickAsync();

        // Validation message should appear
        await Expect(validationMessage).ToContainTextAsync("Please select a plan");

        // Select a value
        await starterItem.ClickAsync();

        // Submit again - form should succeed this time
        await submitButton.ClickAsync();

        // Form should have submitted successfully
        await Expect(submittedValue).ToHaveTextAsync("Submitted: starter");
    }
}
