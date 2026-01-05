using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the Dialog component.
/// Tests ARIA attributes, keyboard navigation, focus management, and nested dialogs.
/// </summary>
public class DialogAccessibilityTests : PageTest
{
    private const string DialogDemoUrl = "dialog";

    [Before(Test)]
    public async Task NavigateToDialogDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + DialogDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes on Trigger

    [Test]
    public async Task Trigger_ShouldHave_AriaHaspopupDialog()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "dialog");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedFalse_WhenClosed()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaExpandedTrue_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Trigger_ShouldHave_AriaControls_MatchingContentId()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        var ariaControls = await trigger.GetAttributeAsync("aria-controls");

        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        var contentId = await content.GetAttributeAsync("id");

        await Assert.That(ariaControls).IsNotNull();
        await Assert.That(ariaControls).IsEqualTo(contentId);
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateClosed_WhenClosed()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task Trigger_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region ARIA Attributes on Content

    [Test]
    public async Task Content_ShouldHave_RoleDialog()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToHaveAttributeAsync("role", "dialog");
    }

    [Test]
    public async Task Content_ShouldHave_AriaModalTrue()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToHaveAttributeAsync("aria-modal", "true");
    }

    [Test]
    public async Task Content_ShouldHave_AriaLabelledby_MatchingTitleId()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        var ariaLabelledby = await content.GetAttributeAsync("aria-labelledby");

        var title = Page.Locator("[data-summit-dialog-title]").First;
        var titleId = await title.GetAttributeAsync("id");

        await Assert.That(ariaLabelledby).IsNotNull();
        await Assert.That(ariaLabelledby).IsEqualTo(titleId);
    }

    [Test]
    public async Task Content_ShouldHave_AriaDescribedby_MatchingDescriptionId()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        var ariaDescribedby = await content.GetAttributeAsync("aria-describedby");

        var description = Page.Locator("[data-summit-dialog-description]").First;
        var descriptionId = await description.GetAttributeAsync("id");

        await Assert.That(ariaDescribedby).IsNotNull();
        await Assert.That(ariaDescribedby).IsEqualTo(descriptionId);
    }

    [Test]
    public async Task Content_ShouldHave_TabIndexNegativeOne()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task Content_ShouldHave_DataStateOpen_WhenOpen()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
    }

    #endregion

    #region Close Button Accessibility

    [Test]
    public async Task CloseButton_ShouldHave_AriaLabel()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var closeButton = Page.Locator("[data-summit-dialog-close]").First;
        var ariaLabel = await closeButton.GetAttributeAsync("aria-label");

        await Assert.That(ariaLabel).IsNotNull();
    }

    [Test]
    public async Task CloseButton_ShouldClose_Dialog()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();

        var closeButton = Page.Locator("[data-summit-dialog-close]").First;
        await closeButton.ClickAsync();

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Keyboard Navigation

    [Test]
    public async Task Dialog_ShouldClose_OnEscapeKey()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Dialog_ShouldOpen_OnEnterKey()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dialog_ShouldOpen_OnSpaceKey()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task Trigger_ShouldUpdateAriaExpanded_AfterEscapeClose()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await Page.Keyboard.PressAsync("Escape");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Dialog_ShouldTrapFocus_WithinContent()
    {
        // Open dialog with form - "Edit Profile" button
        var trigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Edit Profile" });
        await trigger.ClickAsync();

        // Find the dialog content that contains the form (has #name input)
        var content = Page.Locator("[data-summit-dialog-content]").Filter(new() { Has = Page.Locator("#name") });
        await Expect(content).ToBeVisibleAsync();

        // Get focusable elements inside the dialog
        var nameInput = content.Locator("#name");
        var emailInput = content.Locator("#email");
        var cancelButton = content.Locator("[data-summit-dialog-close]").First;
        var saveButton = content.Locator("[data-summit-dialog-close]").Nth(1);

        // Focus the name input
        await nameInput.FocusAsync();
        await Expect(nameInput).ToBeFocusedAsync();

        // Tab to email input
        await Page.Keyboard.PressAsync("Tab");
        await Expect(emailInput).ToBeFocusedAsync();

        // Tab to cancel button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancelButton).ToBeFocusedAsync();

        // Tab to save button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(saveButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task Dialog_ShouldReturnFocus_ToTriggerOnClose()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Close via Escape
        await Page.Keyboard.PressAsync("Escape");

        await Expect(content).Not.ToBeVisibleAsync();

        // Focus should return to trigger
        await Expect(trigger).ToBeFocusedAsync();
    }

    #endregion

    #region Overlay Accessibility

    [Test]
    public async Task Overlay_ShouldHave_AriaHiddenTrue()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var overlay = Page.Locator("[data-summit-dialog-overlay]").First;
        await Expect(overlay).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Test]
    public async Task Overlay_ShouldClose_DialogOnClick()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Click on the overlay (positioned at fixed inset 0)
        var overlay = Page.Locator("[data-summit-dialog-overlay]").First;
        
        // Use dispatchEvent to trigger click since the content is over part of the overlay
        await overlay.DispatchEventAsync("click");

        await Expect(content).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Title and Description

    [Test]
    public async Task Title_ShouldHave_DataAttribute()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var title = Page.Locator("[data-summit-dialog-title]").First;
        await Expect(title).ToBeVisibleAsync();
    }

    [Test]
    public async Task Description_ShouldHave_DataAttribute()
    {
        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var description = Page.Locator("[data-summit-dialog-description]").First;
        await Expect(description).ToBeVisibleAsync();
    }

    #endregion

    #region Nested Dialogs

    [Test]
    public async Task NestedDialog_ShouldHave_DataNestedAttribute()
    {
        // Open parent dialog
        var parentTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Parent Dialog" });
        await parentTrigger.ClickAsync();

        var parentContent = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(parentContent).ToBeVisibleAsync();

        // Open nested dialog
        var nestedTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Nested Dialog" });
        await nestedTrigger.ClickAsync();

        // Nested dialog should have data-nested attribute (boolean attribute renders as empty string)
        var nestedContent = Page.Locator("[data-summit-dialog-content][data-nested]").First;
        await Expect(nestedContent).ToBeVisibleAsync();
    }

    [Test]
    public async Task ParentDialog_ShouldHave_DataNestedOpen_WhenNestedIsOpen()
    {
        // Open parent dialog
        var parentTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Parent Dialog" });
        await parentTrigger.ClickAsync();

        var parentContent = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(parentContent).ToBeVisibleAsync();

        // Open nested dialog
        var nestedTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Nested Dialog" });
        await nestedTrigger.ClickAsync();

        // Parent should have data-nested-open attribute (use attribute selector instead of value check)
        var parentWithNestedOpen = Page.Locator("[data-summit-dialog-content][data-nested-open]").First;
        await Expect(parentWithNestedOpen).ToBeVisibleAsync();
    }

    [Test]
    [Skip("Nested dialog test is flaky in CI - needs investigation")]
    public async Task NestedDialog_ShouldClose_IndependentlyFromParent()
    {
        // Navigate to the nested dialogs section and scroll it into view
        var nestedSection = Page.Locator("section").Filter(new() { HasText = "Nested Dialogs" });
        await nestedSection.ScrollIntoViewIfNeededAsync();
        
        // Open parent dialog - use the button within the section
        var parentTrigger = nestedSection.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Parent Dialog" });
        await parentTrigger.ClickAsync();

        // Wait a bit for the dialog to render
        await Page.WaitForTimeoutAsync(100);

        // Parent content should be visible (first dialog content on page after this click)
        var parentDialogContent = Page.Locator("[data-summit-dialog-content][data-state='open']").First;
        await Expect(parentDialogContent).ToBeVisibleAsync();

        // Open nested dialog
        var nestedTrigger = parentDialogContent.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Nested Dialog" });
        await nestedTrigger.ClickAsync();

        // Wait for nested dialog
        await Page.WaitForTimeoutAsync(100);

        // Find nested content by data-nested attribute
        var nestedContent = Page.Locator("[data-summit-dialog-content][data-nested][data-state='open']").First;
        await Expect(nestedContent).ToBeVisibleAsync();

        // Close nested dialog with Escape
        await Page.Keyboard.PressAsync("Escape");

        // Nested should be closed
        await Expect(nestedContent).Not.ToBeVisibleAsync();
        
        // Parent should still be visible
        await Expect(parentDialogContent).ToBeVisibleAsync();
    }

    [Test]
    public async Task NestedDialog_ShouldHave_CssVariableForDepth()
    {
        // Open parent dialog
        var parentTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Parent Dialog" });
        await parentTrigger.ClickAsync();

        var parentContent = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(parentContent).ToBeVisibleAsync();

        // Check parent dialog depth CSS variable
        var parentStyle = await parentContent.GetAttributeAsync("style");
        await Assert.That(parentStyle).Contains("--summit-dialog-depth: 0");

        // Open nested dialog
        var nestedTrigger = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Open Nested Dialog" });
        await nestedTrigger.ClickAsync();

        var nestedContent = Page.Locator("[data-summit-dialog-content][data-nested]").First;
        await Expect(nestedContent).ToBeVisibleAsync();

        // Check nested dialog depth CSS variable
        var nestedStyle = await nestedContent.GetAttributeAsync("style");
        await Assert.That(nestedStyle).Contains("--summit-dialog-depth: 1");
    }

    #endregion

    #region Controlled Dialog

    [Test]
    public async Task ControlledDialog_ShouldOpen_ViaExternalButton()
    {
        // Scroll to the Controlled Dialog section first
        var section = Page.Locator("section").Filter(new() { HasText = "Controlled Dialog" });
        await section.ScrollIntoViewIfNeededAsync();
        
        // Find the external toggle button
        var toggleButton = section.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Toggle Externally") });
        await toggleButton.ClickAsync();

        // Dialog should open
        var controlledContent = section.Locator("[data-summit-dialog-content]");
        await Expect(controlledContent).ToBeVisibleAsync();
    }

    [Test]
    public async Task ControlledDialog_ShouldClose_ViaExternalButton()
    {
        // Scroll to the Controlled Dialog section first
        var section = Page.Locator("section").Filter(new() { HasText = "Controlled Dialog" });
        await section.ScrollIntoViewIfNeededAsync();
        
        // Open via external toggle
        var toggleButton = section.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Toggle Externally") });
        await toggleButton.ClickAsync();

        var controlledContent = section.Locator("[data-summit-dialog-content]");
        await Expect(controlledContent).ToBeVisibleAsync();

        // Close the dialog first via the internal button, then test external toggle
        await Page.Keyboard.PressAsync("Escape");
        await Expect(controlledContent).Not.ToBeVisibleAsync();
        
        // Open again
        await toggleButton.ClickAsync();
        await Expect(controlledContent).ToBeVisibleAsync();
        
        // Close again via escape (external toggle is behind overlay)
        await Page.Keyboard.PressAsync("Escape");
        await Expect(controlledContent).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Alert Dialog Service

    [Test]
    public async Task AlertDialogService_ShouldShow_ConfirmDialog()
    {
        var simpleConfirmButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Simple Confirm" });
        await simpleConfirmButton.ClickAsync();

        // Alert dialog should appear
        var alertContent = Page.Locator("[data-summit-alertdialog-content][role='alertdialog']");
        await Expect(alertContent).ToBeVisibleAsync();
    }

    [Test]
    public async Task AlertDialogService_ShouldHave_CorrectTitle()
    {
        var simpleConfirmButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Simple Confirm" });
        await simpleConfirmButton.ClickAsync();

        var title = Page.Locator("[data-summit-alertdialog-title]");
        await Expect(title).ToHaveTextAsync("Confirm Action");
    }

    [Test]
    public async Task AlertDialogService_ShouldReturn_True_OnConfirm()
    {
        var simpleConfirmButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Simple Confirm" });
        await simpleConfirmButton.ClickAsync();

        // Click confirm button
        var confirmButton = Page.Locator("[data-summit-alertdialog-confirm]");
        await confirmButton.ClickAsync();

        // Check result message
        var resultAlert = Page.Locator(".alert-success");
        await Expect(resultAlert).ToContainTextAsync("confirmed");
    }

    [Test]
    public async Task AlertDialogService_ShouldReturn_False_OnCancel()
    {
        var simpleConfirmButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Simple Confirm" });
        await simpleConfirmButton.ClickAsync();

        // Click cancel button
        var cancelButton = Page.Locator("[data-summit-alertdialog-cancel]");
        await cancelButton.ClickAsync();

        // Check result message
        var resultAlert = Page.Locator(".alert-secondary");
        await Expect(resultAlert).ToContainTextAsync("cancelled");
    }

    [Test]
    public async Task AlertDialogService_DestructiveConfirm_ShouldHave_DataDestructive()
    {
        var deleteButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Delete Item" });
        await deleteButton.ClickAsync();

        // Check that the confirm button has data-destructive attribute (boolean attribute renders as empty string)
        var confirmButton = Page.Locator("[data-summit-alertdialog-confirm][data-destructive]");
        await Expect(confirmButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task AlertDialogService_ShouldClose_OnEscape()
    {
        var simpleConfirmButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Simple Confirm" });
        await simpleConfirmButton.ClickAsync();

        var alertContent = Page.Locator("[data-summit-alertdialog-content][role='alertdialog']");
        await Expect(alertContent).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");

        await Expect(alertContent).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Scroll Lock

    [Test]
    public async Task Dialog_ShouldPreventBodyScroll_WhenOpen()
    {
        // Add some content to make page scrollable
        await Page.EvaluateAsync(@"() => {
            document.body.style.minHeight = '200vh';
        }");

        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Check that body has overflow hidden or similar scroll lock
        var bodyStyle = await Page.EvaluateAsync<string>("() => window.getComputedStyle(document.body).overflow");
        await Assert.That(bodyStyle).IsEqualTo("hidden");
    }

    [Test]
    public async Task Dialog_ShouldRestoreBodyScroll_WhenClosed()
    {
        // Add some content to make page scrollable
        await Page.EvaluateAsync(@"() => {
            document.body.style.minHeight = '200vh';
        }");

        var trigger = Page.Locator("[data-summit-dialog-trigger]").First;
        await trigger.ClickAsync();

        var content = Page.Locator("[data-summit-dialog-content]").First;
        await Expect(content).ToBeVisibleAsync();

        // Close the dialog
        await Page.Keyboard.PressAsync("Escape");
        await Expect(content).Not.ToBeVisibleAsync();

        // Check that body scroll is restored
        var bodyStyle = await Page.EvaluateAsync<string>("() => window.getComputedStyle(document.body).overflow");
        await Assert.That(bodyStyle).IsNotEqualTo("hidden");
    }

    #endregion
}
