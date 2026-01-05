namespace SummitUI.Tests.Playwright.Accordion;

/// <summary>
/// Tests for Accordion toggle behavior (click to expand/collapse).
/// Verifies content visibility and state updates.
/// </summary>
public class AccordionToggleTests : SummitTestBase
{
    protected override string TestPagePath => "tests/accordion/basic";

    #region Toggle Behavior (Click)

    [Test]
    public async Task Click_ShouldExpandCollapsedItem()
    {
        var trigger = Page.GetByTestId("trigger-2");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Click_ShouldCollapseExpandedItem()
    {
        var trigger = Page.GetByTestId("trigger-1");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Click_ShouldShowContent_WhenExpanded()
    {
        var trigger = Page.GetByTestId("trigger-2");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await trigger.ClickAsync();

        // Content should now be visible
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Click_ShouldHideContent_WhenCollapsed()
    {
        var trigger = Page.GetByTestId("trigger-1");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Content should be visible initially
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToBeVisibleAsync();

        await trigger.ClickAsync();

        // Content should be removed from DOM
        await Expect(content).ToHaveCountAsync(0);
    }

    [Test]
    public async Task Click_ShouldUpdateDataState_OnItem()
    {
        var item = Page.GetByTestId("item-1");
        var trigger = Page.GetByTestId("trigger-1");

        await Expect(item).ToHaveAttributeAsync("data-state", "open");

        await trigger.ClickAsync();

        await Expect(item).ToHaveAttributeAsync("data-state", "closed");
    }

    #endregion

    #region Content Visibility

    [Test]
    public async Task ExpandedContent_ShouldBeVisible()
    {
        var content = Page.Locator("[data-summit-accordion-content][data-state='open']").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task CollapsedContent_ShouldNotBeInDOM()
    {
        // Get a collapsed trigger's aria-controls
        var collapsedTrigger = Page.GetByTestId("trigger-2");
        var ariaControls = await collapsedTrigger.GetAttributeAsync("aria-controls");

        // The content should not be in the DOM (component doesn't render when collapsed)
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToHaveCountAsync(0);
    }

    [Test]
    public async Task ContentVisibility_ShouldUpdate_OnToggle()
    {
        var trigger = Page.GetByTestId("trigger-2");
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        // Content should not exist initially
        var content = Page.Locator($"#{ariaControls}");
        await Expect(content).ToHaveCountAsync(0);

        // Click to expand
        await trigger.ClickAsync();

        // Content should now be visible
        await Expect(content).ToBeVisibleAsync();
        await Expect(content).ToHaveAttributeAsync("data-state", "open");

        // Click to collapse
        await trigger.ClickAsync();

        // Content should be removed from DOM
        await Expect(content).ToHaveCountAsync(0);
    }

    #endregion
}
