namespace SummitUI.Tests.Playwright.Toast;

/// <summary>
/// Tests for basic Toast functionality and user interactions.
/// </summary>
public class ToastBasicTests : SummitTestBase
{
    protected override string TestPagePath => "tests/toast/basic";

    [Test]
    public async Task SimpleToast_ShouldAppear_WhenButtonClicked()
    {
        await Page.GetByTestId("show-simple-toast").ClickAsync();
        
        // Wait for toast to appear in viewport
        var viewport = Page.GetByTestId("toast-viewport");
        var toast = viewport.Locator("[data-summit-toast-root]").First;
        await Expect(toast).ToBeVisibleAsync();
    }

    [Test]
    public async Task TitleToast_ShouldShow_TitleAndDescription()
    {
        await Page.GetByTestId("show-title-toast").ClickAsync();
        
        var viewport = Page.GetByTestId("toast-viewport");
        var toast = viewport.Locator("[data-summit-toast-root]").First;
        
        await Expect(toast.Locator("[data-summit-toast-title]")).ToHaveTextAsync("Success");
        await Expect(toast.Locator("[data-summit-toast-description]")).ToContainTextAsync("changes have been saved");
    }

    [Test]
    public async Task ActionToast_ShouldShow_ActionButton()
    {
        await Page.GetByTestId("show-action-toast").ClickAsync();
        
        var viewport = Page.GetByTestId("toast-viewport");
        var toast = viewport.Locator("[data-summit-toast-root]").First;
        
        var action = toast.Locator("[data-summit-toast-action]");
        await Expect(action).ToBeVisibleAsync();
        await Expect(action).ToHaveTextAsync("Undo");
    }

    [Test]
    public async Task CloseButton_ShouldDismiss_Toast()
    {
        await Page.GetByTestId("show-simple-toast").ClickAsync();
        
        var viewport = Page.GetByTestId("toast-viewport");
        var toast = viewport.Locator("[data-summit-toast-root]").First;
        await Expect(toast).ToBeVisibleAsync();
        
        var close = toast.Locator("[data-summit-toast-close]");
        await close.ClickAsync();
        
        await Expect(toast).ToBeHiddenAsync();
    }

    [Test]
    public async Task DismissAll_ShouldRemove_AllToasts()
    {
        // Show multiple toasts
        await Page.GetByTestId("show-simple-toast").ClickAsync();
        await Page.GetByTestId("show-title-toast").ClickAsync();
        
        var viewport = Page.GetByTestId("toast-viewport");
        var toasts = viewport.Locator("[data-summit-toast-root]");
        await Expect(toasts).ToHaveCountAsync(2);
        
        // Dismiss all
        await Page.GetByTestId("dismiss-all").ClickAsync();
        
        await Expect(toasts).ToHaveCountAsync(0);
    }

    [Test]
    public async Task DeclarativeToast_ShouldAppear_WhenShown()
    {
        await Page.GetByTestId("show-declarative").ClickAsync();
        
        var toast = Page.GetByTestId("declarative-toast");
        await Expect(toast).ToBeVisibleAsync();
    }

    [Test]
    public async Task DeclarativeToast_ShouldHave_TitleDescriptionActionClose()
    {
        await Page.GetByTestId("show-declarative").ClickAsync();
        
        await Expect(Page.GetByTestId("declarative-title")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("declarative-description")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("declarative-action")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("declarative-close")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ActionButton_ShouldClose_Toast_WhenClicked()
    {
        await Page.GetByTestId("show-declarative").ClickAsync();
        
        var action = Page.GetByTestId("declarative-action");
        await action.ClickAsync();
        
        var toast = Page.GetByTestId("declarative-toast");
        await Expect(toast).ToBeHiddenAsync();
    }

    [Test]
    public async Task Toast_ShouldHave_VariantAttribute()
    {
        await Page.GetByTestId("show-action-toast").ClickAsync();
        
        var viewport = Page.GetByTestId("toast-viewport");
        var toast = viewport.Locator("[data-summit-toast-root]").First;
        
        await Expect(toast).ToHaveAttributeAsync("data-variant", "warning");
    }
}
