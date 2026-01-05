namespace SummitUI.Tests.Playwright.Accordion;

/// <summary>
/// Tests for Accordion keyboard navigation.
/// Verifies Enter/Space toggle, Arrow keys, Home/End navigation.
/// </summary>
public class AccordionKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/accordion/keyboard";

    #region Keyboard Navigation - Enter/Space

    [Test]
    public async Task Enter_ShouldToggleItem()
    {
        var trigger = Page.GetByTestId("trigger-2");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    public async Task Space_ShouldToggleItem()
    {
        var trigger = Page.GetByTestId("trigger-2");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
    }

    #endregion

    #region Keyboard Navigation - Arrow Keys

    [Test]
    public async Task ArrowDown_ShouldMoveFocusToNextTrigger()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var secondTrigger = Page.GetByTestId("trigger-2");

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(secondTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldMoveFocusToPreviousTrigger()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var secondTrigger = Page.GetByTestId("trigger-2");

        await secondTrigger.FocusAsync();
        await Expect(secondTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowDown_ShouldLoopToFirst_WhenAtLast()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var thirdTrigger = Page.GetByTestId("trigger-3");

        await thirdTrigger.FocusAsync();
        await Expect(thirdTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        // Should loop to the first trigger
        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldLoopToLast_WhenAtFirst()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var thirdTrigger = Page.GetByTestId("trigger-3");

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        // Should loop to the last trigger
        await Expect(thirdTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Home_ShouldMoveFocusToFirstTrigger()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var thirdTrigger = Page.GetByTestId("trigger-3");

        await thirdTrigger.FocusAsync();
        await Expect(thirdTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Home");

        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task End_ShouldMoveFocusToLastTrigger()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var thirdTrigger = Page.GetByTestId("trigger-3");

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("End");

        await Expect(thirdTrigger).ToBeFocusedAsync();
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Trigger_ShouldReceiveFocus_OnClick()
    {
        var trigger = Page.GetByTestId("trigger-1");
        await trigger.ClickAsync();

        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Trigger_ShouldRetainFocus_AfterToggle()
    {
        var trigger = Page.GetByTestId("trigger-1");
        await trigger.FocusAsync();
        await Expect(trigger).ToBeFocusedAsync();

        // Click to toggle (collapse)
        await trigger.ClickAsync();

        // Focus should remain on the trigger
        await Expect(trigger).ToBeFocusedAsync();

        // Click to toggle again (expand)
        await trigger.ClickAsync();

        // Focus should still be on the trigger
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tab_ShouldNavigateBetweenTriggers()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var secondTrigger = Page.GetByTestId("trigger-2");

        await firstTrigger.FocusAsync();
        await Expect(firstTrigger).ToBeFocusedAsync();

        // Tab to next focusable element
        await Page.Keyboard.PressAsync("Tab");

        // Second trigger should be focused
        await Expect(secondTrigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task ShiftTab_ShouldNavigateBackwards()
    {
        var firstTrigger = Page.GetByTestId("trigger-1");
        var secondTrigger = Page.GetByTestId("trigger-2");

        await secondTrigger.FocusAsync();
        await Expect(secondTrigger).ToBeFocusedAsync();

        // Shift+Tab to previous element
        await Page.Keyboard.PressAsync("Shift+Tab");

        // First trigger should be focused
        await Expect(firstTrigger).ToBeFocusedAsync();
    }

    #endregion
}
