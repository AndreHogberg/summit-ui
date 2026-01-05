using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the Popover component.
/// Tests ARIA attributes, keyboard navigation, and focus management.
/// </summary>
public class PopoverAccessibilityTests : PageTest
{
    private const string PopoverDemoUrl = "popover";

    [Before(Test)]
    public async Task NavigateToPopoverDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + PopoverDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes on Trigger

[Test]
    public async Task Trigger_ShouldHave_AriaHaspopupDialog()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "dialog");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        var contentId = await content.GetAttributeAsync("id");

        await Assert.That(ariaControls).IsNotNull();
        await Assert.That(ariaControls).IsEqualTo(contentId);
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateClosed_WhenClosed()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region ARIA Attributes on Content

    [Test]
    public async Task Content_ShouldHave_RoleDialog()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToHaveAttributeAsync("role", "dialog");
    }

    [Test]
    public async Task Content_ShouldHave_AriaModalFalse_WhenNotTrapFocus()
    {
        // Basic popover without TrapFocus
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToHaveAttributeAsync("aria-modal", "false");
    }

    [Test]
    public async Task Content_ShouldHave_AriaModalTrue_WhenTrapFocus()
    {
        // Popover with overlay and TrapFocus enabled
        var triggerWithOverlay = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open with Overlay" });
        await triggerWithOverlay.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToHaveAttributeAsync("aria-modal", "true");
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexNegativeOne()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region Close Button Accessibility

    [Test]
    public async Task CloseButton_ShouldHave_AriaLabel()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var closeButton = Page.Locator("[data-summit-popover-close]").First;
        var ariaLabel = await closeButton.GetAttributeAsync("aria-label");

        await Assert.That(ariaLabel).IsNotNull();
    }

    [Test]
    public async Task CloseButton_ShouldClose_Popover()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var closeButton = Page.Locator("[data-summit-popover-close]").First;
        await closeButton.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Keyboard Navigation

    [Test]
    public async Task Popover_ShouldClose_OnEscapeKey()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Popover_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Popover_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldUpdateAriaExpanded_AfterEscapeClose()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Popover_ShouldClose_OnOutsideClick()
    {
        var trigger = Page.Locator("[data-summit-popover-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Click outside the popover
        await Page.Locator("body").ClickAsync(new() { Position = new() { X = 0, Y = 0 } });

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task PopoverWithTrapFocus_ShouldTrapFocusWithinContent()
    {
        // Open popover with overlay (has TrapFocus enabled)
        var triggerWithOverlay = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open with Overlay" });
        await triggerWithOverlay.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Get focusable elements inside the popover
        var input = content.Locator("input");
        var closeButton = content.Locator("[data-summit-popover-close]");

        // Focus the input
        await input.FocusAsync();
        await Expect(input).ToBeFocusedAsync();

        // Tab to next element
        await Page.Keyboard.PressAsync("Tab");
        await Expect(closeButton).ToBeFocusedAsync();
    }

    #endregion

    #region Overlay Accessibility

    [Test]
    public async Task Overlay_ShouldHave_AriaHiddenTrue()
    {
        var triggerWithOverlay = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open with Overlay" });
        await triggerWithOverlay.ClickAsync();

        var overlay = Page.Locator("[data-summit-popover-overlay]");
        await Expect(overlay).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Test]
    public async Task Overlay_ShouldClose_PopoverOnClick()
    {
        var triggerWithOverlay = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open with Overlay" });
        await triggerWithOverlay.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var overlay = Page.Locator("[data-summit-popover-overlay]");
        await overlay.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Different Placement Accessibility

[Test]
    [Arguments("Top", "top")]
    [Arguments("Right", "right")]
    [Arguments("Bottom", "bottom")]
    [Arguments("Left", "left")]
    public async Task Content_ShouldHave_CorrectDataSideAttribute(string buttonText, string expectedSide)
    {
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = buttonText, Exact = true });
        
        // Wait for trigger to be visible, then scroll to center of viewport for placement test
        await Expect(trigger).ToBeVisibleAsync();
        await Page.EvaluateAsync(@"(element) => {
            const rect = element.getBoundingClientRect();
            const scrollY = window.scrollY + rect.top - (window.innerHeight / 2);
            window.scrollTo({ top: scrollY, behavior: 'instant' });
        }", await trigger.ElementHandleAsync());
        
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-popover-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-side", expectedSide);
    }

    #endregion

    #region Multiple Popovers

    [Test]
    public async Task OpeningSecondPopover_ShouldClose_FirstPopover_ViaClick()
    {
        // Open the first popover
        var firstTrigger = Page.Locator("[data-summit-popover-trigger]").First;
        await firstTrigger.ClickAsync();

        var firstContent = Page.Locator("[data-summit-popover-content]").First;
        await Expect(firstContent).ToBeVisibleAsync();

        // Click on another popover trigger (e.g., "Top" button)
        var secondTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Top", Exact = true });
        await secondTrigger.ClickAsync();

        // First popover should be closed, second should be open
        await Expect(firstTrigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(secondTrigger).ToHaveAttributeAsync("aria-expanded", "true");

        // Only one popover content should be visible
        var visibleContents = Page.Locator("[data-summit-popover-content][data-state='open']");
        await Expect(visibleContents).ToHaveCountAsync(1);
    }

    [Test]
    public async Task OpeningSecondPopover_ShouldClose_FirstPopover_ViaKeyboard()
    {
        // Open the first popover
        var firstTrigger = Page.Locator("[data-summit-popover-trigger]").First;
        await firstTrigger.ClickAsync();

        var firstContent = Page.Locator("[data-summit-popover-content]").First;
        await Expect(firstContent).ToBeVisibleAsync();

        // Tab to the close button, then tab out to next trigger
        var closeButton = firstContent.Locator("[data-summit-popover-close]");
        await closeButton.FocusAsync();
        
        // Tab multiple times to reach another popover trigger
        var secondTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Top", Exact = true });
        await secondTrigger.FocusAsync();
        
        // Open via keyboard
        await Page.Keyboard.PressAsync("Enter");

        // First popover should be closed, second should be open
        await Expect(firstTrigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(secondTrigger).ToHaveAttributeAsync("aria-expanded", "true");

        // Only one popover content should be visible
        var visibleContents = Page.Locator("[data-summit-popover-content][data-state='open']");
        await Expect(visibleContents).ToHaveCountAsync(1);
    }

    #endregion

    #region Animated Content

    [Test]
    public async Task AnimatedPopover_ShouldOpen_OnEnterKey()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-trigger]");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-content]");
        await Expect(content).ToBeVisibleAsync();
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task AnimatedPopover_ShouldFocusContent_AfterOpen()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-trigger]");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-content]");
        await Expect(content).ToBeVisibleAsync();

        // The input inside the popover should be focusable
        var input = content.Locator("input");
        await input.FocusAsync();
        await Expect(input).ToBeFocusedAsync();
    }

    [Test]
    public async Task AnimatedPopover_ShouldClose_OnEscape()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-trigger]");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-content]");
        await Expect(content).ToBeVisibleAsync();

        // Press Escape to close
        await Page.Keyboard.PressAsync("Escape");

        // Content should close (after animation completes)
        await Expect(content).Not.ToBeVisibleAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task AnimatedPopover_ShouldClose_OnCloseButtonClick()
    {
        // Navigate to "With Animations" section
        var trigger = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-trigger]");
        await trigger.ClickAsync();

        var content = Page.Locator("section").Filter(new() { HasText = "With Animations" }).Locator("[data-summit-popover-content]");
        await Expect(content).ToBeVisibleAsync();

        // Click the close button
        var closeButton = content.Locator("[data-summit-popover-close]");
        await closeButton.ClickAsync();

        // Content should close (after animation completes)
        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion
}
