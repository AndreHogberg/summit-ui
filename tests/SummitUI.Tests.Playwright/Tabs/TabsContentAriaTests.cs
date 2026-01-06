namespace SummitUI.Tests.Playwright.Tabs;

/// <summary>
/// Tests for TabsContent ARIA attributes.
/// </summary>
public class TabsContentAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/tabs/basic";

    [Test]
    public async Task Content_ShouldHave_RoleTabpanel()
    {
        var content = Page.GetByTestId("content-account");
        await Expect(content).ToHaveAttributeAsync("role", "tabpanel");
    }

    [Test]
    public async Task Content_ShouldHave_UniqueId()
    {
        // Click different tabs to verify each content has unique ID
        var trigger1 = Page.GetByTestId("trigger-account");
        var trigger2 = Page.GetByTestId("trigger-password");

        // Get first content ID
        var content1 = Page.GetByTestId("content-account");
        var id1 = await content1.GetAttributeAsync("id");
        await Assert.That(id1).IsNotNull();

        // Click second tab
        await trigger2.ClickAsync();
        var content2 = Page.GetByTestId("content-password");
        var id2 = await content2.GetAttributeAsync("id");
        await Assert.That(id2).IsNotNull();

        // Verify IDs are unique
        await Assert.That(id1).IsNotEqualTo(id2);
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTriggerId()
    {
        var content = Page.GetByTestId("content-account");
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsNotNull();

        // Verify the trigger with that ID exists
        var trigger = Page.Locator($"[data-summit-tabs-trigger]#{ariaLabelledby}");
        await Expect(trigger).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexZero()
    {
        var content = Page.GetByTestId("content-account");
        await Expect(content).ToHaveAttributeAsync("tabindex", "0");
    }

    [Test]
    public async Task Content_ShouldHave_DataStateActive_WhenVisible()
    {
        var content = Page.GetByTestId("content-account");
        await Expect(content).ToHaveAttributeAsync("data-state", "active");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Content_ShouldHave_DataOrientation()
    {
        var content = Page.GetByTestId("content-account");
        await Expect(content).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    [Test]
    public async Task Content_ShouldHave_DataSummitTabsContentAttribute()
    {
        var content = Page.GetByTestId("content-account");
        // Check attribute exists
        await Expect(content).ToHaveAttributeAsync("data-summit-tabs-content", "");
    }

    [Test]
    public async Task OnlyActiveContent_ShouldBeVisible()
    {
        // Account content should be visible (default active)
        var activeContent = Page.GetByTestId("content-account");
        await Expect(activeContent).ToBeVisibleAsync();

        // Within the basic section, only one content panel should be rendered at a time
        var basicSection = Page.GetByTestId("basic-section");
        var allContents = basicSection.Locator("[data-summit-tabs-content]");
        await Expect(allContents).ToHaveCountAsync(1);
    }

    [Test]
    public async Task ContentVisibility_ShouldUpdate_OnTabChange()
    {
        // Initially account content is visible
        var accountContent = Page.GetByTestId("content-account");
        await Expect(accountContent).ToBeVisibleAsync();

        // Click password tab
        var passwordTrigger = Page.GetByTestId("trigger-password");
        await passwordTrigger.ClickAsync();

        // Account content should no longer be in the DOM
        await Expect(accountContent).ToHaveCountAsync(0);

        // Password content should now be visible
        var passwordContent = Page.GetByTestId("content-password");
        await Expect(passwordContent).ToBeVisibleAsync();
    }

    [Test]
    public async Task TriggerAndContent_ShouldHave_MatchingIdRelationship()
    {
        // Get the trigger
        var trigger = Page.GetByTestId("trigger-account");
        var triggerId = await trigger.GetAttributeAsync("id");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Get the corresponding content
        var content = Page.Locator($"#{ariaControls}");
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        // The content's aria-labelledby should point back to the trigger
        await Assert.That(ariaLabelledby).IsEqualTo(triggerId);
    }
}
