namespace SummitUI.Tests.Playwright.Otp;

public class OtpFormTests : SummitTestBase
{
    protected override string TestPagePath => "tests/otp/form";

    [Test]
    public async Task OtpForm_ShouldShowValidationError_WhenEmpty()
    {
        var submitButton = Page.Locator("[data-testid='submit-button']");
        
        // Submit without entering any value
        await submitButton.ClickAsync();
        
        // Should show validation error
        await Expect(Page.Locator("[data-testid='otp-validation']")).ToContainTextAsync("Code is required");
        
        // Result should be invalid
        await Expect(Page.Locator("[data-testid='last-result']")).ToContainTextAsync("invalid");
    }

    [Test]
    public async Task OtpForm_ShouldShowValidationError_WhenIncomplete()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        var submitButton = Page.Locator("[data-testid='submit-button']");
        
        // Enter only 3 digits
        await input.FocusAsync();
        await Page.Keyboard.TypeAsync("123");
        
        // Submit with incomplete value
        await submitButton.ClickAsync();
        
        // Should show validation error about length
        await Expect(Page.Locator("[data-testid='otp-validation']")).ToContainTextAsync("6 digits");
        
        // Result should be invalid
        await Expect(Page.Locator("[data-testid='last-result']")).ToContainTextAsync("invalid");
    }

    [Test]
    public async Task OtpForm_ShouldSubmitSuccessfully_WhenComplete()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        var submitButton = Page.Locator("[data-testid='submit-button']");
        
        // Enter full 6 digits
        await input.FocusAsync();
        await Page.Keyboard.TypeAsync("123456");
        
        // Verify value is set
        await Expect(Page.Locator("[data-testid='current-value']")).ToContainTextAsync("Value: 123456");
        
        // Submit
        await submitButton.ClickAsync();
        
        // Should succeed with the value
        await Expect(Page.Locator("[data-testid='last-result']")).ToContainTextAsync("valid:123456");
        await Expect(Page.Locator("[data-testid='submit-count']")).ToContainTextAsync("Submits: 1");
    }

    [Test]
    public async Task OtpForm_ShouldClearValidationError_WhenValueBecomeValid()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        var submitButton = Page.Locator("[data-testid='submit-button']");
        var validation = Page.Locator("[data-testid='otp-validation']");
        
        // Submit empty to trigger validation error
        await submitButton.ClickAsync();
        await Expect(validation).ToContainTextAsync("Code is required");
        
        // Enter full code
        await input.FocusAsync();
        await Page.Keyboard.TypeAsync("123456");
        
        // Submit again - should succeed
        await submitButton.ClickAsync();
        await Expect(Page.Locator("[data-testid='last-result']")).ToContainTextAsync("valid:123456");
    }

    [Test]
    public async Task OtpForm_ShouldUpdateModelValue_WhenTyping()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        
        // Type progressively and check model updates
        await input.FocusAsync();
        
        await Page.Keyboard.TypeAsync("1");
        await Expect(Page.Locator("[data-testid='current-value']")).ToContainTextAsync("Value: 1");
        
        await Page.Keyboard.TypeAsync("23");
        await Expect(Page.Locator("[data-testid='current-value']")).ToContainTextAsync("Value: 123");
        
        await Page.Keyboard.TypeAsync("456");
        await Expect(Page.Locator("[data-testid='current-value']")).ToContainTextAsync("Value: 123456");
    }
}
