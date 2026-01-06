namespace SummitUI.Tests.Playwright.Dialog;

/// <summary>
/// Tests for Dialog ARIA attributes and basic behavior.
/// </summary>
public class DialogAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dialog/basic";

    [Test]
    public async Task Trigger_ShouldHave_AriaHaspopupDialog()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "dialog");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateClosed_WhenClosed()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Content_ShouldHave_RoleDialog()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("role", "dialog");
    }

    [Test]
    public async Task Content_ShouldHave_AriaModalTrue()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("aria-modal", "true");
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTitleId()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        var title = Page.GetByTestId("basic-title");
        var titleId = await title.GetAttributeAsync("id");

        await Assert.That(ariaLabelledby).IsNotNull();
        await Assert.That(ariaLabelledby).IsEqualTo(titleId);
    }

    [Test]
    public async Task Content_ShouldHave_AriaDescribedby_MatchingDescriptionId()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        var ariaDescribedby = await content.GetAttributeAsync("aria-describedby");

        var description = Page.GetByTestId("basic-description");
        var descriptionId = await description.GetAttributeAsync("id");

        await Assert.That(ariaDescribedby).IsNotNull();
        await Assert.That(ariaDescribedby).IsEqualTo(descriptionId);
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexNegativeOne()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }
}
