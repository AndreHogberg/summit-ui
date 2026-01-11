namespace SummitUI.Tests.Playwright.AlertDialog;

/// <summary>
/// Tests for AlertDialog destructive mode behavior.
/// </summary>
public class AlertDialogDestructiveTests : SummitTestBase
{
    protected override string TestPagePath => "tests/alertdialog/destructive";

    [Test]
    public async Task DestructiveAlert_ShouldHave_DataDestructiveAttribute_OnContent()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToHaveAttributeAsync("data-destructive", "");
    }

    [Test]
    public async Task DestructiveAlert_ShouldHave_DataDestructiveAttribute_OnTitle()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        var title = Page.GetByTestId("alert-title");
        await Expect(title).ToHaveAttributeAsync("data-destructive", "");
    }

    [Test]
    public async Task DestructiveAlert_ShouldHave_DataDestructiveAttribute_OnDescription()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        var description = Page.GetByTestId("alert-description");
        await Expect(description).ToHaveAttributeAsync("data-destructive", "");
    }

    [Test]
    public async Task DestructiveAlert_ShouldHave_DataDestructiveAttribute_OnOverlay()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        var overlay = Page.GetByTestId("alert-overlay");
        await Expect(overlay).ToHaveAttributeAsync("data-destructive", "");
    }

    [Test]
    public async Task DestructiveAlert_ShouldFocus_CancelButtonFirst()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        var cancel = Page.GetByTestId("alert-cancel");
        await Expect(cancel).ToBeFocusedAsync();
    }

    [Test]
    public async Task NonDestructiveAlert_ShouldNotHave_DataDestructiveAttribute()
    {
        var trigger = Page.GetByTestId("normal-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        var hasDestructive = await content.GetAttributeAsync("data-destructive");
        await Assert.That(hasDestructive).IsNull();
    }

    [Test]
    public async Task DestructiveAlert_Confirm_ShouldReturn_TrueResult()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        var confirm = Page.GetByTestId("alert-confirm");
        await confirm.ClickAsync();

        var result = Page.GetByTestId("destructive-result");
        await Expect(result).ToHaveTextAsync("Deleted");
    }

    [Test]
    public async Task DestructiveAlert_Cancel_ShouldReturn_FalseResult()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        var cancel = Page.GetByTestId("alert-cancel");
        await cancel.ClickAsync();

        var result = Page.GetByTestId("destructive-result");
        await Expect(result).ToHaveTextAsync("Kept");
    }

    [Test]
    public async Task DestructiveAlert_Escape_ShouldReturn_FalseResult()
    {
        var trigger = Page.GetByTestId("destructive-trigger");
        await trigger.ClickAsync();

        await Page.Keyboard.PressAsync("Escape");

        var result = Page.GetByTestId("destructive-result");
        await Expect(result).ToHaveTextAsync("Kept");
    }
}
