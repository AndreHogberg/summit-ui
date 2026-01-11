namespace SummitUI.Tests.Playwright.AlertDialog;

/// <summary>
/// Tests for AlertDialog focus management and scroll lock.
/// </summary>
public class AlertDialogFocusTests : SummitTestBase
{
    protected override string TestPagePath => "tests/alertdialog/focus";

    [Test]
    public async Task AlertDialog_ShouldTrapFocus_WithinContent()
    {
        var trigger = Page.GetByTestId("focus-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToBeVisibleAsync();

        var cancelButton = Page.GetByTestId("alert-cancel");
        var confirmButton = Page.GetByTestId("alert-confirm");

        // Focus the cancel button
        await cancelButton.FocusAsync();
        await Expect(cancelButton).ToBeFocusedAsync();

        // Tab to confirm button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(confirmButton).ToBeFocusedAsync();

        // Tab should wrap around to cancel button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancelButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task AlertDialog_ShouldReturnFocus_ToTriggerOnClose()
    {
        var trigger = Page.GetByTestId("focus-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task AlertDialog_ShouldReturnFocus_ToTriggerOnCancel()
    {
        var trigger = Page.GetByTestId("focus-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToBeVisibleAsync();

        var cancel = Page.GetByTestId("alert-cancel");
        await cancel.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task AlertDialog_ShouldReturnFocus_ToTriggerOnConfirm()
    {
        var trigger = Page.GetByTestId("focus-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToBeVisibleAsync();

        var confirm = Page.GetByTestId("alert-confirm");
        await confirm.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task AlertDialog_ShouldPreventBodyScroll_WhenOpen()
    {
        var trigger = Page.GetByTestId("focus-trigger");
        await trigger.ClickAsync();

        await Expect(Page.GetByTestId("alert-content")).ToBeVisibleAsync();

        var bodyStyle = await Page.EvaluateAsync<string>("() => window.getComputedStyle(document.body).overflow");
        await Assert.That(bodyStyle).IsEqualTo("hidden");
    }

    [Test]
    public async Task AlertDialog_ShouldRestoreBodyScroll_WhenClosed()
    {
        var trigger = Page.GetByTestId("focus-trigger");
        await trigger.ClickAsync();

        await Expect(Page.GetByTestId("alert-content")).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("alert-content")).Not.ToBeVisibleAsync();

        var bodyStyle = await Page.EvaluateAsync<string>("() => window.getComputedStyle(document.body).overflow");
        await Assert.That(bodyStyle).IsNotEqualTo("hidden");
    }
}
