namespace SummitUI.Tests.Playwright.AlertDialog;

/// <summary>
/// Tests for AlertDialog ARIA attributes and basic behavior.
/// </summary>
public class AlertDialogAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/alertdialog/basic";

    [Test]
    public async Task Content_ShouldHave_RoleAlertDialog()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToHaveAttributeAsync("role", "alertdialog");
    }

    [Test]
    public async Task Content_ShouldHave_AriaModalTrue()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToHaveAttributeAsync("aria-modal", "true");
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTitleId()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        var title = Page.GetByTestId("alert-title");
        var titleId = await title.GetAttributeAsync("id");

        await Assert.That(ariaLabelledby).IsNotNull();
        await Assert.That(ariaLabelledby).IsEqualTo(titleId);
    }

    [Test]
    public async Task Content_ShouldHave_AriaDescribedby_MatchingDescriptionId()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        var ariaDescribedby = await content.GetAttributeAsync("aria-describedby");

        var description = Page.GetByTestId("alert-description");
        var descriptionId = await description.GetAttributeAsync("id");

        await Assert.That(ariaDescribedby).IsNotNull();
        await Assert.That(ariaDescribedby).IsEqualTo(descriptionId);
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexNegativeOne()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("alert-content");
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Overlay_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var overlay = Page.GetByTestId("alert-overlay");
        await Expect(overlay).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Title_ShouldDisplay_CorrectText()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var title = Page.GetByTestId("alert-title");
        await Expect(title).ToHaveTextAsync("Confirm Action");
    }

    [Test]
    public async Task Description_ShouldDisplay_CorrectText()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var description = Page.GetByTestId("alert-description");
        await Expect(description).ToHaveTextAsync("Are you sure you want to continue?");
    }

    [Test]
    public async Task CancelButton_ShouldHave_TypeButton()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var cancel = Page.GetByTestId("alert-cancel");
        await Expect(cancel).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task ConfirmButton_ShouldHave_TypeButton()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var confirm = Page.GetByTestId("alert-confirm");
        await Expect(confirm).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task CustomAlert_ShouldDisplay_CustomButtonText()
    {
        var trigger = Page.GetByTestId("custom-trigger");
        await trigger.ClickAsync();

        var cancel = Page.GetByTestId("alert-cancel");
        var confirm = Page.GetByTestId("alert-confirm");

        await Expect(cancel).ToHaveTextAsync("Discard");
        await Expect(confirm).ToHaveTextAsync("Save");
    }
}
