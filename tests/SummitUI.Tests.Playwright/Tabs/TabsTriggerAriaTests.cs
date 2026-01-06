namespace SummitUI.Tests.Playwright.Tabs;

/// <summary>
/// Tests for TabsTrigger ARIA attributes.
/// </summary>
public class TabsTriggerAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/tabs/basic";

    [Test]
    public async Task Trigger_ShouldHave_RoleTab()
    {
        var trigger = Page.GetByTestId("trigger-account");
        await Expect(trigger).ToHaveAttributeAsync("role", "tab");
    }

    [Test]
    public async Task Trigger_ShouldHave_UniqueId()
    {
        var triggers = Page.Locator("[data-summit-tabs-trigger]");
        var count = await triggers.CountAsync();

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await triggers.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaSelectedTrue_WhenActive()
    {
        var trigger = Page.GetByTestId("trigger-account");
        await Expect(trigger).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaSelectedFalse_WhenInactive()
    {
        var trigger = Page.GetByTestId("trigger-password");
        await Expect(trigger).ToHaveAttributeAsync("aria-selected", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.GetByTestId("trigger-account");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await Assert.That(ariaControls).IsNotNull();

        // Verify the content panel with that ID exists
        var content = Page.Locator($"[data-summit-tabs-content]#{ariaControls}");
        await Expect(content).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Trigger_ShouldHave_TabIndexZero_WhenActive()
    {
        var trigger = Page.GetByTestId("trigger-account");
        await Expect(trigger).ToHaveAttributeAsync("tabindex", "0");
    }

    [Test]
    public async Task Trigger_ShouldHave_TabIndexNegativeOne_WhenInactive()
    {
        var trigger = Page.GetByTestId("trigger-password");
        await Expect(trigger).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateActive_WhenSelected()
    {
        var trigger = Page.GetByTestId("trigger-account");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "active");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateInactive_WhenNotSelected()
    {
        var trigger = Page.GetByTestId("trigger-password");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "inactive");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataValue()
    {
        var trigger = Page.GetByTestId("trigger-account");
        await Expect(trigger).ToHaveAttributeAsync("data-value", "account");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataOrientation()
    {
        var trigger = Page.GetByTestId("trigger-account");
        await Expect(trigger).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataSummitTabsTriggerAttribute()
    {
        var trigger = Page.GetByTestId("trigger-account");
        await Expect(trigger).ToHaveAttributeAsync("data-summit-tabs-trigger", "");
    }
}
