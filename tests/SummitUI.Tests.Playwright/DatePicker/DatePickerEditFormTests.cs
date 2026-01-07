namespace SummitUI.Tests.Playwright.DatePicker;

/// <summary>
/// Tests for DatePicker integration with Blazor EditForm and form validation.
/// </summary>
public class DatePickerEditFormTests : SummitTestBase
{
    protected override string TestPagePath => "tests/datepicker/edit-form";

    #region Model Binding via Segments

    [Test]
    public async Task EditForm_ShouldBindValue_WhenSegmentsTyped()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");

        // Fill all segments
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("15");

        await Page.WaitForTimeoutAsync(100);

        var valueDisplay = section.Locator("[data-testid='editform-value']");
        var text = await valueDisplay.TextContentAsync();

        await Assert.That(text!).Contains("2025-06-15");
    }

    [Test]
    public async Task EditForm_ShouldUpdateModel_WhenSegmentChanges()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");

        // Set initial value
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("10");

        await Page.WaitForTimeoutAsync(100);

        // Change day
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("25");

        await Page.WaitForTimeoutAsync(100);

        var valueDisplay = section.Locator("[data-testid='editform-value']");
        var text = await valueDisplay.TextContentAsync();

        await Assert.That(text!).Contains("25");
    }

    #endregion

    #region Model Binding via Calendar Selection

    [Test]
    public async Task EditForm_ShouldBindValue_WhenCalendarDateSelected()
    {
        var section = Page.Locator("[data-testid='calendar-selection-section']");
        var trigger = section.Locator("[data-testid='calendar-trigger']");

        // Open calendar
        await trigger.ClickAsync();

        var content = section.Locator("[data-testid='calendar-content']");
        await Expect(content).ToBeVisibleAsync();

        // Click a day that is not outside the current month
        var dayButton = content.Locator(".calendar-day:not([data-outside-month]):not([data-unavailable])").First;
        await dayButton.ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        // Verify value is set
        var valueDisplay = section.Locator("[data-testid='calendar-value']");
        var text = await valueDisplay.TextContentAsync();

        await Assert.That(text!).IsNotEqualTo("Value: None");
    }

    [Test]
    public async Task EditForm_ShouldUpdateHiddenInput_WhenCalendarDateSelected()
    {
        var section = Page.Locator("[data-testid='calendar-selection-section']");
        var trigger = section.Locator("[data-testid='calendar-trigger']");

        // Open calendar
        await trigger.ClickAsync();

        var content = section.Locator("[data-testid='calendar-content']");
        await Expect(content).ToBeVisibleAsync();

        // Click a day
        var dayButton = content.Locator(".calendar-day:not([data-outside-month]):not([data-unavailable])").First;
        await dayButton.ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        // Verify hidden input has value
        var hiddenInput = section.Locator("input[type='hidden'][name='eventDate']");
        var value = await hiddenInput.GetAttributeAsync("value");

        await Assert.That(value).IsNotNull();
        await Assert.That(value!).IsNotEqualTo("");
    }

    #endregion

    #region Form Submission

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_WhenDateSelected()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-trigger']");

        // Open calendar and select a date
        await trigger.ClickAsync();

        var content = section.Locator("[data-testid='editform-content']");
        await Expect(content).ToBeVisibleAsync();

        var dayButton = content.Locator(".calendar-day:not([data-outside-month]):not([data-unavailable])").First;
        await dayButton.ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        // Submit form
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify success
        var successMessage = section.Locator("[data-testid='editform-success']");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditForm_ShouldShowValidationError_WhenRequiredFieldEmpty()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit without selecting a date
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Verify validation error
        var validationError = section.Locator("[data-testid='editform-validation']");
        await Expect(validationError).ToBeVisibleAsync();
        await Expect(validationError).ToHaveTextAsync("Birth date is required");
    }

    [Test]
    public async Task EditForm_ShouldSubmitSuccessfully_WhenSegmentsTyped()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");

        // Fill all segments
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

        // Verify success
        var successMessage = section.Locator("[data-testid='editform-success']");
        await Expect(successMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Hidden Input

    [Test]
    public async Task EditForm_ShouldRenderHiddenInput_WithCorrectName()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        var hiddenInput = section.Locator("input[type='hidden'][name='birthDate']");
        await Expect(hiddenInput).ToHaveCountAsync(1);
    }

    [Test]
    public async Task EditForm_ShouldRenderHiddenInput_WithRequiredAttribute()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        var hiddenInput = section.Locator("input[type='hidden'][name='birthDate']");
        await Expect(hiddenInput).ToHaveAttributeAsync("required", "");
    }

    [Test]
    public async Task EditForm_HiddenInputShouldHaveISOFormat_WhenValueSet()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");

        // Set date
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("15");

        await Page.WaitForTimeoutAsync(100);

        var hiddenInput = section.Locator("input[type='hidden'][name='birthDate']");
        var value = await hiddenInput.GetAttributeAsync("value");

        await Assert.That(value).IsEqualTo("2025-06-15");
    }

    #endregion

    #region Validation State

    [Test]
    public async Task EditForm_FieldShouldHaveDataInvalid_WhenValidationFails()
    {
        var section = Page.Locator("[data-testid='editform-section']");

        // Submit without value to trigger validation
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Field should have data-invalid attribute
        var field = section.Locator("[data-testid='editform-field']");
        await Expect(field).ToHaveAttributeAsync("data-invalid", "");
    }

    [Test]
    public async Task EditForm_FieldShouldNotHaveDataInvalid_WhenValueSet()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var trigger = section.Locator("[data-testid='editform-trigger']");

        // Select a date via calendar
        await trigger.ClickAsync();

        var content = section.Locator("[data-testid='editform-content']");
        var dayButton = content.Locator(".calendar-day:not([data-outside-month]):not([data-unavailable])").First;
        await dayButton.ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        // Submit to validate
        var submitButton = section.Locator("[data-testid='editform-submit']");
        await submitButton.ClickAsync();

        // Field should not have data-invalid
        var field = section.Locator("[data-testid='editform-field']");
        await Expect(field).Not.ToHaveAttributeAsync("data-invalid", "");
    }

    #endregion

    #region Min/Max Constraints

    [Test]
    public async Task EditForm_ShouldHaveDataInvalid_WhenValueOutOfRange()
    {
        var section = Page.Locator("[data-testid='minmax-section']");
        var input = section.Locator("[data-testid='minmax-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");

        // Set a date in the past (before min date which is today)
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2020");
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("01");
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("01");

        await Page.WaitForTimeoutAsync(100);

        // Field should have data-invalid due to out of range
        var field = section.Locator("[data-testid='minmax-field']");
        await Expect(field).ToHaveAttributeAsync("data-invalid", "");
    }

    [Test]
    public async Task EditForm_ShouldNotHaveDataInvalid_WhenValueInRange()
    {
        var section = Page.Locator("[data-testid='minmax-section']");
        var trigger = section.Locator("[data-testid='minmax-trigger']");

        // Open calendar and select a valid date (within range)
        await trigger.ClickAsync();

        var content = section.Locator("[data-testid='minmax-content']");
        await Expect(content).ToBeVisibleAsync();

        // Click a day that is not disabled (within min/max range)
        var dayButton = content.Locator(".calendar-day:not([data-outside-month]):not(:disabled)").First;
        await dayButton.ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        // Field should not have data-invalid
        var field = section.Locator("[data-testid='minmax-field']");
        await Expect(field).Not.ToHaveAttributeAsync("data-invalid", "");
    }

    #endregion

    #region Keyboard Interaction with EditForm

    [Test]
    public async Task EditForm_ShouldBindValue_WhenNavigatedViaArrowKeys()
    {
        var section = Page.Locator("[data-testid='editform-section']");
        var input = section.Locator("[data-testid='editform-input']");
        var yearSegment = input.Locator("[data-segment='year']");
        var monthSegment = input.Locator("[data-segment='month']");
        var daySegment = input.Locator("[data-segment='day']");

        // Use arrow keys to set values
        await yearSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowUp");

        await monthSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowUp");

        await daySegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowUp");

        await Page.WaitForTimeoutAsync(100);

        // Verify value was set
        var valueDisplay = section.Locator("[data-testid='editform-value']");
        var text = await valueDisplay.TextContentAsync();
        await Assert.That(text!).IsNotEqualTo("Value: None");
    }

    [Test]
    public async Task EditForm_ShouldSelectDate_ViaKeyboardInCalendar()
    {
        var section = Page.Locator("[data-testid='calendar-selection-section']");
        var trigger = section.Locator("[data-testid='calendar-trigger']");

        // Open calendar via keyboard
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = section.Locator("[data-testid='calendar-content']");
        await Expect(content).ToBeVisibleAsync();

        // Find the focused day (auto-focused by the calendar) and give it browser focus
        var focusedDay = content.Locator("[data-summit-calendar-day][data-focused]");
        await Expect(focusedDay).ToBeVisibleAsync();
        await focusedDay.FocusAsync();

        // Press Enter to select
        await Page.Keyboard.PressAsync("Enter");

        await Page.WaitForTimeoutAsync(100);

        // Verify value was set
        var valueDisplay = section.Locator("[data-testid='calendar-value']");
        var text = await valueDisplay.TextContentAsync();
        await Assert.That(text!).IsNotEqualTo("Value: None");
    }

    #endregion
}
