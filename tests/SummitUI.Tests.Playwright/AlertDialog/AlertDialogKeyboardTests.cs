namespace SummitUI.Tests.Playwright.AlertDialog;

/// <summary>
/// Tests for AlertDialog keyboard interactions.
/// </summary>
public class AlertDialogKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/alertdialog/basic";

    [Test]
    public async Task Escape_ShouldClose_AlertDialog()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Escape_ShouldReturn_FalseResult()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        var result = Page.GetByTestId("basic-result");
        await Expect(result).ToHaveTextAsync("Cancelled");
    }

    [Test]
    public async Task Enter_OnConfirmButton_ShouldConfirm()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var confirm = Page.GetByTestId("alert-confirm");
        await confirm.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("alert-content");
        await Expect(content).Not.ToBeVisibleAsync();

        var result = Page.GetByTestId("basic-result");
        await Expect(result).ToHaveTextAsync("Confirmed");
    }

    [Test]
    public async Task Enter_OnCancelButton_ShouldCancel()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var cancel = Page.GetByTestId("alert-cancel");
        await cancel.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.GetByTestId("alert-content");
        await Expect(content).Not.ToBeVisibleAsync();

        var result = Page.GetByTestId("basic-result");
        await Expect(result).ToHaveTextAsync("Cancelled");
    }

    [Test]
    public async Task Space_OnConfirmButton_ShouldConfirm()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var confirm = Page.GetByTestId("alert-confirm");
        await confirm.FocusAsync();
        await Page.Keyboard.PressAsync("Space");

        var content = Page.GetByTestId("alert-content");
        await Expect(content).Not.ToBeVisibleAsync();

        var result = Page.GetByTestId("basic-result");
        await Expect(result).ToHaveTextAsync("Confirmed");
    }

    [Test]
    public async Task Space_OnCancelButton_ShouldCancel()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var cancel = Page.GetByTestId("alert-cancel");
        await cancel.FocusAsync();
        await Page.Keyboard.PressAsync("Space");

        var content = Page.GetByTestId("alert-content");
        await Expect(content).Not.ToBeVisibleAsync();

        var result = Page.GetByTestId("basic-result");
        await Expect(result).ToHaveTextAsync("Cancelled");
    }
}
