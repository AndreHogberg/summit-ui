namespace SummitUI.Tests.Playwright.Dialog;

/// <summary>
/// Tests for Dialog focus management and scroll lock.
/// </summary>
public class DialogFocusTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dialog/focus";

    [Test]
    public async Task Dialog_ShouldTrapFocus_WithinContent()
    {
        var trigger = Page.GetByTestId("form-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("form-content");
        await Expect(content).ToBeVisibleAsync();

        var nameInput = Page.GetByTestId("name-input");
        var emailInput = Page.GetByTestId("email-input");
        var cancelButton = Page.GetByTestId("cancel-button");
        var saveButton = Page.GetByTestId("save-button");

        // Focus the name input
        await nameInput.FocusAsync();
        await Expect(nameInput).ToBeFocusedAsync();

        // Tab through elements
        await Page.Keyboard.PressAsync("Tab");
        await Expect(emailInput).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancelButton).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Tab");
        await Expect(saveButton).ToBeFocusedAsync();
        
        // Wrap around
        await Page.Keyboard.PressAsync("Tab");
        await Expect(nameInput).ToBeFocusedAsync();
    }

    [Test]
    public async Task Dialog_ShouldReturnFocus_ToTriggerOnClose()
    {
        var trigger = Page.GetByTestId("form-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("form-content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Dialog_ShouldPreventBodyScroll_WhenOpen()
    {
        var trigger = Page.GetByTestId("form-trigger");
        await trigger.ClickAsync();

        await Expect(Page.GetByTestId("form-content")).ToBeVisibleAsync();

        var bodyStyle = await Page.EvaluateAsync<string>("() => window.getComputedStyle(document.body).overflow");
        await Assert.That(bodyStyle).IsEqualTo("hidden");
    }

    [Test]
    public async Task Dialog_ShouldRestoreBodyScroll_WhenClosed()
    {
        var trigger = Page.GetByTestId("form-trigger");
        await trigger.ClickAsync();

        await Expect(Page.GetByTestId("form-content")).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("form-content")).Not.ToBeVisibleAsync();

        var bodyStyle = await Page.EvaluateAsync<string>("() => window.getComputedStyle(document.body).overflow");
        await Assert.That(bodyStyle).IsNotEqualTo("hidden");
    }
}
