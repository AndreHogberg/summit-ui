namespace SummitUI.Tests.Playwright.DatePicker;

/// <summary>
/// Tests for DatePicker ARIA attributes and accessibility compliance.
/// </summary>
public class DatePickerAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/datepicker/basic";

    #region Trigger ARIA Tests

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
    public async Task Trigger_ShouldHave_AriaLabel()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await Expect(trigger).ToHaveAttributeAsync("aria-label", "Open calendar");
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

    #endregion

    #region Disabled Trigger ARIA Tests

    [Test]
    public async Task DisabledTrigger_ShouldHave_DisabledAttribute()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await Expect(trigger).ToHaveAttributeAsync("disabled", "");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_AriaDisabledTrue()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await Expect(trigger).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledTrigger_ShouldHave_DataDisabled()
    {
        var trigger = Page.GetByTestId("disabled-trigger");
        await Expect(trigger).ToHaveAttributeAsync("data-disabled", "");
    }

    #endregion

    #region Content ARIA Tests

    [Test]
    public async Task Content_ShouldHave_RoleDialog()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("role", "dialog");
    }

    [Test]
    public async Task Content_ShouldHave_AriaModal()
    {
        // DatePicker content has aria-modal attribute set based on EffectiveTrapFocus
        // Default behavior uses Context.Modal which defaults to true for DatePicker
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("aria-modal", "true");
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
    public async Task Content_ShouldHave_TabIndexMinusOne()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Content_ShouldHave_DataSide()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        // Default side is bottom
        await Expect(content).ToHaveAttributeAsync("data-side", "bottom");
    }

    [Test]
    public async Task Content_ShouldHave_DataAlign()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        // Default align is start
        await Expect(content).ToHaveAttributeAsync("data-align", "start");
    }

    #endregion

    #region Field ARIA Tests

    [Test]
    public async Task DisabledField_ShouldHave_DataDisabled()
    {
        var field = Page.GetByTestId("disabled-field");
        await Expect(field).ToHaveAttributeAsync("data-disabled", "");
    }

    #endregion
}
