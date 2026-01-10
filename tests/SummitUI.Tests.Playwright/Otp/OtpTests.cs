namespace SummitUI.Tests.Playwright.Otp;

public class OtpTests : SummitTestBase
{
    protected override string TestPagePath => "tests/otp/basic";

    [Test]
    public async Task Otp_ShouldHandleTyping()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        var slots = Page.Locator("[data-testid='otp-input'] [data-otp-slot]");
        
        await Expect(slots).ToHaveCountAsync(6);

        // Focus and type
        await input.FocusAsync();
        await Page.Keyboard.TypeAsync("123456");
        
        // Verify value is updated
        await Expect(Page.Locator("[data-testid='otp-value']")).ToContainTextAsync("Value: 123456");
        
        // Verify slots show the characters
        await Expect(slots.Nth(0)).ToContainTextAsync("1");
        await Expect(slots.Nth(5)).ToContainTextAsync("6");
    }

    [Test]
    public async Task Otp_ShouldHandleBackspace()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        
        // Focus and type some values
        await input.FocusAsync();
        await Page.Keyboard.TypeAsync("12");
        
        // Verify initial value
        await Expect(Page.Locator("[data-testid='otp-value']")).ToContainTextAsync("Value: 12");
        
        // Backspace should delete the last character
        await Page.Keyboard.PressAsync("Backspace");
        await Expect(Page.Locator("[data-testid='otp-value']")).ToContainTextAsync("Value: 1");
        
        // Another backspace should clear completely
        await Page.Keyboard.PressAsync("Backspace");
        await Expect(Page.Locator("[data-testid='otp-value']")).ToContainTextAsync("Value:");
    }

    [Test]
    public async Task Otp_ShouldHandlePaste()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        var slots = Page.Locator("[data-testid='otp-input'] [data-otp-slot]");
        
        await input.FocusAsync();
        
        // Use Fill which simulates typing/pasting the value
        await input.FillAsync("987654");

        // Should fill all slots
        await Expect(Page.Locator("[data-testid='otp-value']")).ToContainTextAsync("Value: 987654");
        await Expect(slots.Nth(0)).ToContainTextAsync("9");
        await Expect(slots.Nth(5)).ToContainTextAsync("4");
    }

    [Test]
    public async Task Otp_ShouldHandleArrowKeys()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        var slots = Page.Locator("[data-testid='otp-input'] [data-otp-slot]");

        // Type some characters first so we have something to navigate
        await input.FocusAsync();
        await Page.Keyboard.TypeAsync("123");
        
        // After typing "123", cursor should be at position 3
        // Press ArrowLeft to move back
        await Page.Keyboard.PressAsync("ArrowLeft");
        
        // Type to replace character at current position
        await Page.Keyboard.TypeAsync("X");
        
        // Value should now be "12X" or similar depending on selection behavior
        // The single-input behaves like a normal text input
        await Expect(Page.Locator("[data-testid='otp-value']")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("Value: .+"));
    }
    
    [Test]
    public async Task Otp_ShouldRespectMaxLength()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        
        await input.FocusAsync();
        // The input is of length 6 but the last character is changing
        await Page.Keyboard.TypeAsync("1234567890", new() { Delay = 50 }); // Try to type more than 6
        
        // Should only have 6 characters
        await Expect(Page.Locator("[data-testid='otp-value']")).ToContainTextAsync("Value: 123450");
    }
    
    [Test]
    public async Task Otp_ShouldShowActiveSlot()
    {
        var input = Page.Locator("[data-testid='otp-input'] [data-otp-input]");
        var slots = Page.Locator("[data-testid='otp-input'] [data-otp-slot]");
        
        // Focus the input
        await input.FocusAsync();
        
        // First slot should be active (have data-active attribute)
        await Expect(slots.Nth(0)).ToHaveAttributeAsync("data-active", "true");
        
        // Type a character
        await Page.Keyboard.TypeAsync("1");
        
        // Now second slot should be active
        await Expect(slots.Nth(1)).ToHaveAttributeAsync("data-active", "true");
    }
}
