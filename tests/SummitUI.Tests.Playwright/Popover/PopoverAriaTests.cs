namespace SummitUI.Tests.Playwright.Popover;

/// <summary>
/// Tests for Popover ARIA attributes and basic behavior.
/// </summary>
public class PopoverAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/popover/basic";

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
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");
        await Assert.That(ariaControls).IsNotNull();

        await trigger.ClickAsync();
        var content = Page.GetByTestId("basic-content");
        var contentId = await content.GetAttributeAsync("id");

        await Assert.That(ariaControls).IsEqualTo(contentId);
    }
}
