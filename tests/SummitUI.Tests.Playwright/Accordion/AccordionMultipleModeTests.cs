namespace SummitUI.Tests.Playwright.Accordion;

/// <summary>
/// Tests for Accordion multiple mode behavior.
/// Verifies that multiple items can be open simultaneously.
/// </summary>
public class AccordionMultipleModeTests : SummitTestBase
{
    protected override string TestPagePath => "tests/accordion/multiple";

    [Test]
    public async Task MultipleMode_ShouldAllowMultipleItemsOpen()
    {
        // Multiple mode has feature-1 and feature-2 open by default
        var triggers = Page.Locator("[data-summit-accordion-trigger][aria-expanded='true']");
        var expandedCount = await triggers.CountAsync();

        // Should have at least 2 expanded (from DefaultValues)
        await Assert.That(expandedCount).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task MultipleMode_ShouldHaveFirstTwoItemsOpen_ByDefault()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        var trigger2 = Page.GetByTestId("trigger-2");
        var trigger3 = Page.GetByTestId("trigger-3");

        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trigger3).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task MultipleMode_ShouldNotCloseOtherItems_WhenOpeningNew()
    {
        // Get initial count of expanded items
        var initialExpandedTriggers = Page.Locator("[data-summit-accordion-trigger][aria-expanded='true']");
        var initialCount = await initialExpandedTriggers.CountAsync();

        // Find the collapsed trigger and click it
        var collapsedTrigger = Page.GetByTestId("trigger-3");
        await Expect(collapsedTrigger).ToHaveAttributeAsync("aria-expanded", "false");

        await collapsedTrigger.ClickAsync();

        // Now should have one more expanded item
        var newExpandedTriggers = Page.Locator("[data-summit-accordion-trigger][aria-expanded='true']");
        var newCount = await newExpandedTriggers.CountAsync();

        await Assert.That(newCount).IsEqualTo(initialCount + 1);
    }

    [Test]
    public async Task MultipleMode_ShouldAllowClosingIndividualItems()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");

        // Close first item
        await trigger1.ClickAsync();

        // First item should be closed
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "false");

        // Second item should still be open
        var trigger2 = Page.GetByTestId("trigger-2");
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task MultipleMode_ShouldAllowAllItemsToBeOpen()
    {
        // Open the third item (first two are already open)
        var trigger3 = Page.GetByTestId("trigger-3");
        await trigger3.ClickAsync();

        // All three should now be open
        var trigger1 = Page.GetByTestId("trigger-1");
        var trigger2 = Page.GetByTestId("trigger-2");

        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trigger3).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task MultipleMode_ShouldAllowAllItemsToBeClosed()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        var trigger2 = Page.GetByTestId("trigger-2");

        // Close both open items
        await trigger1.ClickAsync();
        await trigger2.ClickAsync();

        // Both should be closed
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "false");
    }
}
