namespace SummitUI.Tests.Playwright.Accordion;

/// <summary>
/// Tests for ARIA attributes on Accordion components.
/// Verifies proper accessibility attributes on Item, Header, Trigger, and Content.
/// </summary>
public class AccordionAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/accordion/basic";

    #region ARIA Attributes on AccordionItem

    [Test]
    public async Task Item_ShouldHave_DataStateOpen_WhenExpanded()
    {
        var item = Page.GetByTestId("item-1");
        await Expect(item).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Item_ShouldHave_DataStateClosed_WhenCollapsed()
    {
        var item = Page.GetByTestId("item-2");
        await Expect(item).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Item_ShouldHave_DataOrientation()
    {
        var item = Page.GetByTestId("item-1");
        await Expect(item).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region ARIA Attributes on AccordionHeader

    [Test]
    public async Task Header_ShouldBe_SemanticHeadingElement()
    {
        var header = Page.Locator("[data-summit-accordion-header]").First;
        var tagName = await header.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
        // Should be a semantic heading element (h1-h6), default is h3
        await Assert.That(tagName).IsEqualTo("h3");
    }

    [Test]
    public async Task Header_ShouldHave_DataOrientation()
    {
        var header = Page.Locator("[data-summit-accordion-header]").First;
        await Expect(header).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region ARIA Attributes on AccordionTrigger

    [Test]
    public async Task Trigger_ShouldHave_TypeButton()
    {
        var trigger = Page.GetByTestId("trigger-1");
        await Expect(trigger).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task Trigger_ShouldHave_UniqueId()
    {
        var triggers = Page.Locator("[data-summit-accordion-trigger]");
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
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var trigger = Page.GetByTestId("trigger-1");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var trigger = Page.GetByTestId("trigger-2");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.GetByTestId("trigger-1");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await Assert.That(ariaControls).IsNotNull();

        // Verify the content panel with that ID exists
        var content = Page.Locator($"[data-summit-accordion-content]#{ariaControls}");
        await Expect(content).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenExpanded()
    {
        var trigger = Page.GetByTestId("trigger-1");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateClosed_WhenCollapsed()
    {
        var trigger = Page.GetByTestId("trigger-2");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataOrientation()
    {
        var trigger = Page.GetByTestId("trigger-1");
        await Expect(trigger).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region ARIA Attributes on AccordionContent

    [Test]
    public async Task Content_ShouldHave_RoleRegion()
    {
        var content = Page.Locator("[data-summit-accordion-content]").First;
        await Expect(content).ToHaveAttributeAsync("role", "region");
    }

    [Test]
    public async Task Content_ShouldHave_UniqueId()
    {
        // Expand all items to get all content panels
        await Page.GetByTestId("trigger-2").ClickAsync();
        await Page.GetByTestId("trigger-3").ClickAsync();

        var contents = Page.Locator("[data-summit-accordion-content]");
        var count = await contents.CountAsync();

        await Assert.That(count).IsGreaterThanOrEqualTo(1);

        var ids = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var id = await contents.Nth(i).GetAttributeAsync("id");
            await Assert.That(id).IsNotNull();
            ids.Add(id!);
        }

        // Verify all IDs are unique
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTriggerId()
    {
        var content = Page.Locator("[data-summit-accordion-content]").First;
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsNotNull();

        // Verify the trigger with that ID exists
        var trigger = Page.Locator($"[data-summit-accordion-trigger]#{ariaLabelledby}");
        await Expect(trigger).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenVisible()
    {
        var content = Page.Locator("[data-summit-accordion-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Content_ShouldHave_DataOrientation()
    {
        var content = Page.Locator("[data-summit-accordion-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region ID Relationships

    [Test]
    public async Task TriggerAndContent_ShouldHave_MatchingIdRelationship()
    {
        var trigger = Page.GetByTestId("trigger-1");
        var triggerId = await trigger.GetAttributeAsync("id");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Get the corresponding content
        var content = Page.Locator($"#{ariaControls}");
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        // The content's aria-labelledby should point back to the trigger
        await Assert.That(ariaLabelledby).IsEqualTo(triggerId);
    }

    [Test]
    public async Task AllExpandedTriggers_ShouldHave_ValidAriaControls()
    {
        // Get all expanded triggers
        var triggers = Page.Locator("[data-summit-accordion-trigger][aria-expanded='true']");
        var count = await triggers.CountAsync();

        for (var i = 0; i < count; i++)
        {
            var trigger = triggers.Nth(i);
            var ariaControls = await trigger.GetAttributeAsync("aria-controls");
            await Assert.That(ariaControls).IsNotNull();

            // The content panel should exist when this accordion item is expanded
            var referencedContent = Page.Locator($"[data-summit-accordion-content][id='{ariaControls}']");
            await Expect(referencedContent).ToHaveCountAsync(1);
        }
    }

    #endregion
}
