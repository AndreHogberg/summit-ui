namespace SummitUI.Tests.Playwright.Toast;

/// <summary>
/// Tests for Toast ARIA attributes and accessibility compliance.
/// Verifies adherence to WCAG standards and WAI-ARIA live region requirements.
/// </summary>
public class ToastAccessibilityTests : SummitTestBase
{
    protected override string TestPagePath => "tests/toast/accessibility";

    [Test]
    public async Task Viewport_ShouldHave_RoleRegion()
    {
        var viewport = Page.GetByTestId("toast-viewport");
        await Expect(viewport).ToHaveAttributeAsync("role", "region");
    }

    [Test]
    public async Task Viewport_ShouldHave_AriaLabel()
    {
        var viewport = Page.GetByTestId("toast-viewport");
        await Expect(viewport).ToHaveAttributeAsync("aria-label", "Test notifications (F8)");
    }

    [Test]
    public async Task Viewport_ShouldBe_Focusable()
    {
        var viewport = Page.GetByTestId("toast-viewport");
        await Expect(viewport).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task ForegroundToast_ShouldHave_RoleStatus()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToHaveAttributeAsync("role", "status");
    }

    [Test]
    public async Task ForegroundToast_ShouldHave_AriaLiveAssertive()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToHaveAttributeAsync("aria-live", "assertive");
    }

    [Test]
    public async Task BackgroundToast_ShouldHave_AriaLivePolite()
    {
        await Page.GetByTestId("show-background").ClickAsync();
        
        var toast = Page.GetByTestId("background-toast");
        await Expect(toast).ToHaveAttributeAsync("aria-live", "polite");
    }

    [Test]
    public async Task Toast_ShouldHave_AriaAtomic()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToHaveAttributeAsync("aria-atomic", "true");
    }

    [Test]
    public async Task Toast_ShouldHave_AriaLabelledby_MatchingTitleId()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        var title = Page.GetByTestId("foreground-title");
        
        var ariaLabelledby = await toast.GetAttributeAsync("aria-labelledby");
        var titleId = await title.GetAttributeAsync("id");
        
        await Assert.That(ariaLabelledby).IsEqualTo(titleId);
    }

    [Test]
    public async Task Toast_ShouldHave_AriaDescribedby_MatchingDescriptionId()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        var description = Page.GetByTestId("foreground-description");
        
        var ariaDescribedby = await toast.GetAttributeAsync("aria-describedby");
        var descriptionId = await description.GetAttributeAsync("id");
        
        await Assert.That(ariaDescribedby).IsEqualTo(descriptionId);
    }

    [Test]
    public async Task Toast_ShouldBe_Focusable()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToHaveAttributeAsync("tabindex", "0");
    }

    [Test]
    public async Task CloseButton_ShouldHave_AriaLabel()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var closeButton = Page.GetByTestId("foreground-close");
        await Expect(closeButton).ToHaveAttributeAsync("aria-label", "Dismiss notification");
    }

    [Test]
    public async Task ActionButton_ShouldHave_AltTextData()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var actionButton = Page.GetByTestId("foreground-action");
        await Expect(actionButton).ToHaveAttributeAsync("data-summit-toast-announce-alt", "Press Enter to retry the operation");
    }

    [Test]
    public async Task Toast_ShouldHave_DataState_Open()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task Toast_ShouldHave_DataSwipeDirection()
    {
        await Page.GetByTestId("show-foreground").ClickAsync();
        
        var toast = Page.GetByTestId("foreground-toast");
        await Expect(toast).ToHaveAttributeAsync("data-swipe-direction", "right");
    }
}
