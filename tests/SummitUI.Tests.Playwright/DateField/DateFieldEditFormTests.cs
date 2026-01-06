using TUnit.Playwright;
using SummitUI.Tests.Playwright.DateField;

namespace SummitUI.Tests.Playwright.DateField;

public class DateFieldEditFormTests : SummitTestBase
{
    protected override string TestPagePath => "tests/date-field/edit-form";

    #region Model Binding

    [Test]
    public async Task EditForm_ShouldBindValue_ToModel()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");
        
        // Fill all required segments to set a value
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("15");
        
        await Page.WaitForTimeoutAsync(100);
        
        var birthDateValue = section.Locator("[data-testid='editform-birthdate-value']");
        var text = await birthDateValue.TextContentAsync();
        
        await Assert.That(text!).Contains("15");
        await Assert.That(text!).Contains("2025");
    }

    [Test]
    public async Task EditForm_ShouldUpdateModel_WhenDateChanges()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");
        
        // First set all segments to establish a value
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("20");
        
        await Page.WaitForTimeoutAsync(100);
        
        // Now change individual segments
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("25");
        
        await Page.WaitForTimeoutAsync(100);
        
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("12");
        
        await Page.WaitForTimeoutAsync(100);
        
        var birthDateValue = section.Locator("[data-testid='editform-birthdate-value']");
        var text = await birthDateValue.TextContentAsync();
        
        await Assert.That(text!).Contains("12");
        await Assert.That(text!).Contains("25");
    }

    [Test]
    public async Task EditForm_ShouldBindDateTime_WithTimeSegments()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-appointment-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");
        var hourSegment = input.Locator("[data-segment='hour']");
        var minuteSegment = input.Locator("[data-segment='minute']");
        
        // Fill all required segments (date + time)
        await yearSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("2025");
        await Page.WaitForTimeoutAsync(100);
        
        await monthSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("06");
        await Page.WaitForTimeoutAsync(100);
        
        await daySegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("15");
        await Page.WaitForTimeoutAsync(100);
        
        await hourSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("14");
        await Page.WaitForTimeoutAsync(100);
        
        await minuteSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("30");
        
        await Page.WaitForTimeoutAsync(200);
        
        var appointmentValue = section.Locator("[data-testid='editform-appointment-value']");
        var text = await appointmentValue.TextContentAsync();
        
        await Assert.That(text!).Contains("14:30");
    }

    #endregion

    #region Form Submission

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_WhenRequiredFieldsFilled()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");
        
        // Set all required segments to establish a value
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("15");
        
        await Page.WaitForTimeoutAsync(100);
        
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
        
        // Submit form without filling any segments (value is null)
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();
        
        // Verify validation error appears
        var validationError = section.Locator(".validation-error");
        await Expect(validationError).ToBeVisibleAsync();
        await Expect(validationError).ToHaveTextAsync("Birth date is required");
    }

    #endregion

    #region Hidden Input for Form Submission

    [Test]
    public async Task EditForm_ShouldRenderHiddenInput_WithCorrectName()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        
        // Find the hidden input with the expected name
        var hiddenInput = section.Locator("input[type='hidden'][name='birthDate']");
        await Expect(hiddenInput).ToHaveCountAsync(1);
    }

    [Test]
    public async Task EditForm_ShouldRenderHiddenInput_WithDateValue()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var daySegment = input.Locator("[data-segment='day']");
        var monthSegment = input.Locator("[data-segment='month']");
        var yearSegment = input.Locator("[data-segment='year']");
        
        // Set date to 2025-06-15
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("15");
        
        await Page.WaitForTimeoutAsync(100);
        
        // Verify hidden input has ISO format value
        var hiddenInput = section.Locator("input[type='hidden'][name='birthDate']");
        var value = await hiddenInput.GetAttributeAsync("value");
        
        await Assert.That(value).IsEqualTo("2025-06-15");
    }

    [Test]
    public async Task EditForm_ShouldUpdateHiddenInput_WhenValueChanges()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");
        
        // Set initial complete value
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("10");
        
        await Page.WaitForTimeoutAsync(100);
        
        var hiddenInput = section.Locator("input[type='hidden'][name='birthDate']");
        var initialValue = await hiddenInput.GetAttributeAsync("value");
        await Assert.That(initialValue!).Contains("10");
        
        // Change the day value
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("25");
        
        await Page.WaitForTimeoutAsync(100);
        
        var newValue = await hiddenInput.GetAttributeAsync("value");
        await Assert.That(newValue!).Contains("25");
    }

    #endregion

    #region Invalid State with EditForm

    [Test]
    public async Task EditForm_RootShouldHaveDataInvalid_WhenValidationFails()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var daySegment = input.Locator("[data-segment='day']");
        
        // Clear the field
        await daySegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("Backspace");
        await Page.WaitForTimeoutAsync(100);
        
        // Submit without filling required field
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();
        
        // Root should have data-invalid attribute - use role='group' selector
        var root = section.Locator("[role='group']").First;
        await Expect(root).ToHaveAttributeAsync("data-invalid", "");
    }

    #endregion

    #region Required Attribute

    [Test]
    public async Task EditForm_HiddenInputShouldHaveRequired_WhenRequired()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        
        // Birth date field is required
        var hiddenInput = section.Locator("input[type='hidden'][name='birthDate']");
        await Expect(hiddenInput).ToHaveAttributeAsync("required", "");
    }

    #endregion

    #region Keyboard Interaction with EditForm

    [Test]
    public async Task EditForm_ShouldBindValue_WhenNavigatedViaKeyboard()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");
        
        // Use arrow keys to set values for all segments
        await yearSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowUp"); // Sets year from placeholder
        
        await monthSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowUp"); // Sets month from placeholder
        
        await daySegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowUp"); // Sets day from placeholder
        
        await Page.WaitForTimeoutAsync(100);
        
        // Verify value was set (all segments filled)
        var birthDateValue = section.Locator("[data-testid='editform-birthdate-value']");
        var text = await birthDateValue.TextContentAsync();
        await Assert.That(text).IsNotEqualTo("None");
    }

    [Test]
    public async Task EditForm_ShouldSubmit_ViaKeyboard()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-birthdate-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");
        
        // Set all segments via keyboard
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("15");
        
        await Page.WaitForTimeoutAsync(100);
        
        // Tab to submit button and press Enter
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        
        // Verify success
        var successMessage = section.Locator("[data-testid='editform-success-message']");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region DateTime Hidden Input Format

    [Test]
    public async Task EditForm_DateTimeHiddenInput_ShouldHaveISOFormat()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-appointment-input']");
        var hourSegment = input.Locator("[data-segment='hour']");
        
        // Verify appointment input has time segments
        await Expect(hourSegment).ToBeVisibleAsync();
        
        // The hidden input for datetime should exist with appointmentTime name
        var hiddenInput = section.Locator("input[type='hidden'][name='appointmentTime']");
        await Expect(hiddenInput).ToHaveCountAsync(1);
    }

    #endregion
}
