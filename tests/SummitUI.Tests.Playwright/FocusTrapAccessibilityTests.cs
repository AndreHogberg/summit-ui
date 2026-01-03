using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the FocusTrap component.
/// Tests focus management with varying numbers of focusable elements.
/// </summary>
public class FocusTrapAccessibilityTests : PageTest
{
    private const string FocusTrapDemoUrl = "focus-trap";

    [Before(Test)]
    public async Task NavigateToFocusTrapDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + FocusTrapDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region Zero Tabbable Items

    [Test]
    public async Task ZeroItems_ShouldTrapFocusInContainer_WhenNoFocusableElements()
    {
        var openButton = Page.GetByTestId("open-zero-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("zero-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // Wait for focus trap to activate - it may take a moment
        await Page.WaitForTimeoutAsync(200);

        // Verify the focus trap is active
        var focusTrapContainer = Page.Locator("[data-ark-focus-trap]");
        await Expect(focusTrapContainer).ToBeVisibleAsync();

        // Verify focus is somewhere within the dialog area
        // When there are no tabbable elements, focus trap focuses the container itself
        var isFocusInTrap = await focusTrapContainer.EvaluateAsync<bool>(
            "el => el.contains(document.activeElement) || el === document.activeElement");

        await Assert.That(isFocusInTrap).IsTrue();
    }

    [Test]
    public async Task ZeroItems_ShouldPreventTabFromLeavingDialog()
    {
        var openButton = Page.GetByTestId("open-zero-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("zero-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // Press Tab multiple times - focus should stay within the dialog
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Tab");

        // External button should not be focused
        var externalButton = Page.GetByTestId("external-button");
        await Expect(externalButton).Not.ToBeFocusedAsync();
    }

    [Test]
    public async Task ZeroItems_ShouldPreventShiftTabFromLeavingDialog()
    {
        var openButton = Page.GetByTestId("open-zero-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("zero-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // Press Shift+Tab multiple times - focus should stay within the dialog
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Page.Keyboard.PressAsync("Shift+Tab");

        // External button should not be focused
        var externalButton = Page.GetByTestId("external-button");
        await Expect(externalButton).Not.ToBeFocusedAsync();
    }

    [Test]
    public async Task ZeroItems_DialogCanReceiveEscapeKey_WhenProgrammaticallyFocused()
    {
        var openButton = Page.GetByTestId("open-zero-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("zero-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // Wait for focus trap to activate
        await Page.WaitForTimeoutAsync(200);

        // For zero-item dialogs, programmatically focus the dialog content
        // to allow it to receive keyboard events
        await dialog.FocusAsync();
        await Expect(dialog).ToBeFocusedAsync();

        // Now Escape key should work
        await Page.Keyboard.PressAsync("Escape");

        // Dialog should close
        await Expect(dialog).Not.ToBeVisibleAsync();
    }

    #endregion

    #region One Tabbable Item

    [Test]
    public async Task OneItem_ShouldAutoFocusTheOnlyButton()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("one-item-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // The close button should be focused automatically
        var closeButton = Page.GetByTestId("one-item-close");
        await Expect(closeButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task OneItem_ShouldCycleBackToSameButton_OnTab()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await Expect(closeButton).ToBeFocusedAsync();

        // Tab should cycle back to the same button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(closeButton).ToBeFocusedAsync();

        // Tab again - still the same button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(closeButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task OneItem_ShouldCycleBackToSameButton_OnShiftTab()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await Expect(closeButton).ToBeFocusedAsync();

        // Shift+Tab should cycle back to the same button
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(closeButton).ToBeFocusedAsync();

        // Shift+Tab again - still the same button
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(closeButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task OneItem_ShouldPreventFocusFromLeavingDialog()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await Expect(closeButton).ToBeFocusedAsync();

        // Press Tab multiple times
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Tab");

        // External button should not be focused
        var externalButton = Page.GetByTestId("external-button");
        await Expect(externalButton).Not.ToBeFocusedAsync();

        // Close button should still be focused
        await Expect(closeButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task OneItem_ShouldCloseDialog_WhenButtonClicked()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("one-item-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await closeButton.ClickAsync();

        await Expect(dialog).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Two Tabbable Items

    [Test]
    public async Task TwoItems_ShouldAutoFocusFirstButton()
    {
        var openButton = Page.GetByTestId("open-two-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("two-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // The first button (Cancel) should be focused automatically
        var cancelButton = Page.GetByTestId("two-items-cancel");
        await Expect(cancelButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task TwoItems_ShouldCycleBetweenButtons_OnTab()
    {
        var openButton = Page.GetByTestId("open-two-items");
        await openButton.ClickAsync();

        var cancelButton = Page.GetByTestId("two-items-cancel");
        var confirmButton = Page.GetByTestId("two-items-confirm");

        // First button should be focused initially
        await Expect(cancelButton).ToBeFocusedAsync();

        // Tab to second button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(confirmButton).ToBeFocusedAsync();

        // Tab wraps back to first button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancelButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task TwoItems_ShouldCycleBetweenButtons_OnShiftTab()
    {
        var openButton = Page.GetByTestId("open-two-items");
        await openButton.ClickAsync();

        var cancelButton = Page.GetByTestId("two-items-cancel");
        var confirmButton = Page.GetByTestId("two-items-confirm");

        // First button should be focused initially
        await Expect(cancelButton).ToBeFocusedAsync();

        // Shift+Tab wraps to last button
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(confirmButton).ToBeFocusedAsync();

        // Shift+Tab goes to first button
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(cancelButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task TwoItems_ShouldPreventFocusFromLeavingDialog()
    {
        var openButton = Page.GetByTestId("open-two-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("two-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // Press Tab many times
        for (int i = 0; i < 10; i++)
        {
            await Page.Keyboard.PressAsync("Tab");
        }

        // External button should not be focused
        var externalButton = Page.GetByTestId("external-button");
        await Expect(externalButton).Not.ToBeFocusedAsync();

        // Focus should still be within the dialog
        var cancelButton = Page.GetByTestId("two-items-cancel");
        var confirmButton = Page.GetByTestId("two-items-confirm");

        var cancelFocused = await cancelButton.EvaluateAsync<bool>("el => el === document.activeElement");
        var confirmFocused = await confirmButton.EvaluateAsync<bool>("el => el === document.activeElement");

        await Assert.That(cancelFocused || confirmFocused).IsTrue();
    }

    [Test]
    public async Task TwoItems_ShouldCloseDialog_WhenCancelClicked()
    {
        var openButton = Page.GetByTestId("open-two-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("two-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        var cancelButton = Page.GetByTestId("two-items-cancel");
        await cancelButton.ClickAsync();

        await Expect(dialog).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task TwoItems_ShouldCloseDialog_WhenConfirmClicked()
    {
        var openButton = Page.GetByTestId("open-two-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("two-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        var confirmButton = Page.GetByTestId("two-items-confirm");
        await confirmButton.ClickAsync();

        await Expect(dialog).Not.ToBeVisibleAsync();
    }

    #endregion

    #region Multiple Items (Form Dialog)

    [Test]
    public async Task MultipleItems_ShouldAutoFocusFirstInput()
    {
        var openButton = Page.GetByTestId("open-multiple-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("multiple-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // The first input should be focused automatically
        var nameInput = Page.GetByTestId("multiple-items-input");
        await Expect(nameInput).ToBeFocusedAsync();
    }

    [Test]
    public async Task MultipleItems_ShouldTabThroughAllElements()
    {
        var openButton = Page.GetByTestId("open-multiple-items");
        await openButton.ClickAsync();

        var nameInput = Page.GetByTestId("multiple-items-input");
        var emailInput = Page.GetByTestId("multiple-items-email");
        var cancelButton = Page.GetByTestId("multiple-items-cancel");
        var submitButton = Page.GetByTestId("multiple-items-submit");

        // First input focused initially
        await Expect(nameInput).ToBeFocusedAsync();

        // Tab to email input
        await Page.Keyboard.PressAsync("Tab");
        await Expect(emailInput).ToBeFocusedAsync();

        // Tab to cancel button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancelButton).ToBeFocusedAsync();

        // Tab to submit button
        await Page.Keyboard.PressAsync("Tab");
        await Expect(submitButton).ToBeFocusedAsync();

        // Tab wraps back to first input
        await Page.Keyboard.PressAsync("Tab");
        await Expect(nameInput).ToBeFocusedAsync();
    }

    [Test]
    public async Task MultipleItems_ShouldShiftTabThroughAllElements()
    {
        var openButton = Page.GetByTestId("open-multiple-items");
        await openButton.ClickAsync();

        var nameInput = Page.GetByTestId("multiple-items-input");
        var emailInput = Page.GetByTestId("multiple-items-email");
        var cancelButton = Page.GetByTestId("multiple-items-cancel");
        var submitButton = Page.GetByTestId("multiple-items-submit");

        // First input focused initially
        await Expect(nameInput).ToBeFocusedAsync();

        // Shift+Tab wraps to last button
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(submitButton).ToBeFocusedAsync();

        // Shift+Tab to cancel button
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(cancelButton).ToBeFocusedAsync();

        // Shift+Tab to email input
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(emailInput).ToBeFocusedAsync();

        // Shift+Tab to name input
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(nameInput).ToBeFocusedAsync();
    }

    [Test]
    public async Task MultipleItems_ShouldPreventFocusFromLeavingDialog()
    {
        var openButton = Page.GetByTestId("open-multiple-items");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("multiple-items-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        // Press Tab many times
        for (int i = 0; i < 20; i++)
        {
            await Page.Keyboard.PressAsync("Tab");
        }

        // External button should not be focused
        var externalButton = Page.GetByTestId("external-button");
        await Expect(externalButton).Not.ToBeFocusedAsync();
    }

    #endregion

    #region Focus Return

    [Test]
    public async Task FocusTrap_ShouldReturnFocusToTrigger_WhenClosed()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("one-item-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await closeButton.ClickAsync();

        await Expect(dialog).Not.ToBeVisibleAsync();

        // Focus should return to the trigger button
        await Expect(openButton).ToBeFocusedAsync();
    }

    [Test]
    public async Task FocusTrap_ShouldReturnFocusToTrigger_WhenClosedWithKeyboard()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.PressAsync("Enter");

        var dialog = Page.GetByTestId("one-item-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await closeButton.PressAsync("Enter");

        await Expect(dialog).Not.ToBeVisibleAsync();

        // Focus should return to the trigger button
        await Expect(openButton).ToBeFocusedAsync();
    }

    #endregion

    #region Click Outside Prevention

    [Test]
    public async Task FocusTrap_ShouldPreventFocusOnClickOutside()
    {
        var openButton = Page.GetByTestId("open-one-item");
        await openButton.ClickAsync();

        var dialog = Page.GetByTestId("one-item-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        var closeButton = Page.GetByTestId("one-item-close");
        await Expect(closeButton).ToBeFocusedAsync();

        // Try to click on external button
        var externalButton = Page.GetByTestId("external-button");
        await externalButton.ClickAsync(new() { Force = true });

        // Focus should return to the dialog, not stay on external button
        await Expect(externalButton).Not.ToBeFocusedAsync();
    }

    #endregion
}
