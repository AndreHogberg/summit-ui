namespace SummitUI.Tests.Playwright.Accordion;

/// <summary>
/// Tests for Accordion non-collapsible mode behavior.
/// Verifies that at least one item must always remain open.
/// </summary>
public class AccordionNonCollapsibleTests : SummitTestBase
{
    protected override string TestPagePath => "tests/accordion/non-collapsible";

    [Test]
    public async Task NonCollapsible_ShouldHaveFirstItemOpen_ByDefault()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task NonCollapsible_ShouldNotCloseLastOpenItem()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");

        // Try to close it by clicking
        await trigger1.ClickAsync();

        // Should still be expanded (non-collapsible mode)
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task NonCollapsible_ShouldAllowSwitchingToAnotherItem()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        var trigger2 = Page.GetByTestId("trigger-2");

        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "false");

        // Click second item to switch
        await trigger2.ClickAsync();

        // Second should now be expanded
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");
        // First should now be collapsed
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task NonCollapsible_ShouldNotCloseSecondItem_WhenItIsTheOnlyOpen()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        var trigger2 = Page.GetByTestId("trigger-2");

        // Switch to second item
        await trigger2.ClickAsync();
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "false");

        // Try to close second item
        await trigger2.ClickAsync();

        // Second should still be expanded (can't close last item)
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task NonCollapsible_ShouldMaintainOneOpenItem_AfterMultipleSwitches()
    {
        var trigger1 = Page.GetByTestId("trigger-1");
        var trigger2 = Page.GetByTestId("trigger-2");

        // Switch multiple times
        await trigger2.ClickAsync();
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");

        await trigger1.ClickAsync();
        await Expect(trigger1).ToHaveAttributeAsync("aria-expanded", "true");

        await trigger2.ClickAsync();
        await Expect(trigger2).ToHaveAttributeAsync("aria-expanded", "true");

        // There should always be exactly one open
        var expandedCount = await Page.Locator("[data-summit-accordion-trigger][aria-expanded='true']").CountAsync();
        await Assert.That(expandedCount).IsEqualTo(1);
    }
}
