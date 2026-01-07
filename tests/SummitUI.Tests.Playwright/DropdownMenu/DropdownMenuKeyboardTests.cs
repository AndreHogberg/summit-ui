using Microsoft.Playwright;

using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DropdownMenu;

public class DropdownMenuKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dropdown-menu/keyboard";

    [Test]
    public async Task Menu_ShouldOpen_OnKeyboardKeys()
    {
        var trigger = Page.GetByTestId("keyboard-trigger");

        // Enter
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page.GetByTestId("keyboard-content")).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("keyboard-content")).Not.ToBeVisibleAsync();

        // Space
        await Page.Keyboard.PressAsync(" ");
        await Expect(Page.GetByTestId("keyboard-content")).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("keyboard-content")).Not.ToBeVisibleAsync();

        // ArrowDown
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("keyboard-content")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ArrowKeys_ShouldNavigateItems()
    {
        await Page.GetByTestId("keyboard-trigger").ClickAsync();

        await Expect(Page.GetByTestId("item-apple")).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("item-banana")).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("ArrowUp");
        await Expect(Page.GetByTestId("item-apple")).ToBeFocusedAsync();
    }

    [Test]
    public async Task Navigation_ShouldSkipDisabledItems()
    {
        await Page.GetByTestId("keyboard-trigger").ClickAsync();

        // Focus Cherry (item 4)
        await Page.Keyboard.PressAsync("ArrowDown"); // Banana
        await Page.Keyboard.PressAsync("ArrowDown"); // Blueberry
        await Page.Keyboard.PressAsync("ArrowDown"); // Cherry
        await Expect(Page.GetByTestId("item-cherry")).ToBeFocusedAsync();

        // Next is Date (Disabled), so it should jump to Elderberry
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("item-elderberry")).ToBeFocusedAsync();

        // Prev from Elderberry should skip Date and go to Cherry
        await Page.Keyboard.PressAsync("ArrowUp");
        await Expect(Page.GetByTestId("item-cherry")).ToBeFocusedAsync();
    }

    [Test]
    public async Task HomeEnd_ShouldNavigateToFirstLast()
    {
        await Page.GetByTestId("keyboard-trigger").ClickAsync();

        await Page.Keyboard.PressAsync("End");
        await Expect(Page.GetByTestId("item-elderberry")).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Home");
        await Expect(Page.GetByTestId("item-apple")).ToBeFocusedAsync();
    }

    [Test]
    public async Task Typeahead_ShouldFocusMatchingItems()
    {

        await Page.GetByTestId("keyboard-trigger").ClickAsync();

        await Page.Keyboard.TypeAsync("blu");
        await Expect(Page.GetByTestId("item-blueberry")).ToBeFocusedAsync();

        await Task.Delay(500); // Wait for typeahead timeout

        await Page.Keyboard.TypeAsync("c");
        await Expect(Page.GetByTestId("item-cherry")).ToBeFocusedAsync();
    }

    [Test]
    public async Task Escape_ShouldCloseMenu_AndReturnFocus()
    {
        var trigger = Page.GetByTestId("keyboard-trigger");
        await trigger.ClickAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("keyboard-content")).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Selection_ShouldCloseMenu()
    {
        await Page.GetByTestId("keyboard-trigger").ClickAsync();

        // Select Apple with Enter
        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page.GetByTestId("keyboard-content")).Not.ToBeVisibleAsync();

        await Page.GetByTestId("keyboard-trigger").ClickAsync();

        // Select Apple with Space
        await Page.Keyboard.PressAsync(" ");
        await Expect(Page.GetByTestId("keyboard-content")).Not.ToBeVisibleAsync();
    }
}
