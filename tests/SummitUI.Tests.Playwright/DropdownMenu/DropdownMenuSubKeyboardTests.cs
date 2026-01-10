using Microsoft.Playwright;

using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DropdownMenu;

public class DropdownMenuSubKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dropdown-menu/submenu";

    [Test]
    public async Task ArrowRight_ShouldOpenSubmenu_WhenOnSubTrigger()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // Menu opens, verify first item is focused
        await Expect(Page.GetByTestId("item-1")).ToBeFocusedAsync();
        
        // Navigate to second item
        await Page.Keyboard.PressAsync("ArrowDown"); // item-1 -> item-2
        await Expect(Page.GetByTestId("item-2")).ToBeFocusedAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown"); // item-2 -> submenu-trigger-1
        await Expect(Page.GetByTestId("submenu-trigger-1")).ToBeFocusedAsync();
        
        // ArrowRight should open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // First item in submenu should be focused
        await Expect(Page.GetByTestId("sub-item-1")).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowLeft_ShouldCloseSubmenu_AndReturnFocus()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("submenu-trigger-1")).ToBeFocusedAsync();
        
        // Open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // Wait for first item to be focused (ensures SubContent.OnAfterRenderAsync completed)
        await Expect(Page.GetByTestId("sub-item-1")).ToBeFocusedAsync();
        
        // ArrowLeft should close submenu and return focus to trigger
        await Page.Keyboard.PressAsync("ArrowLeft");
        
        await Expect(Page.GetByTestId("submenu-content-1")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("submenu-trigger-1")).ToBeFocusedAsync();
    }

    [Test]
    public async Task Escape_ShouldCloseSubmenuOnly_NotParentMenu()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("submenu-trigger-1")).ToBeFocusedAsync();
        
        // Open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // Escape should close only the submenu
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("submenu-content-1")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("basic-submenu-content")).ToBeVisibleAsync();
        
        // Second Escape should close parent menu
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("basic-submenu-content")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task ArrowUpDown_ShouldNavigateWithinSubmenu()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("submenu-trigger-1")).ToBeFocusedAsync();
        
        // Open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // Navigate within submenu
        await Expect(Page.GetByTestId("sub-item-1")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("sub-item-2")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("sub-item-3")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowUp");
        await Expect(Page.GetByTestId("sub-item-2")).ToBeFocusedAsync();
    }

    [Test]
    public async Task Enter_ShouldSelectSubmenuItem_AndCloseAllMenus()
    {
        await Page.GetByTestId("selection-submenu-trigger").ClickAsync();
        
        // First item should be focused after opening
        await Expect(Page.GetByTestId("selection-main-item")).ToBeFocusedAsync();
        
        // Navigate to submenu trigger (one ArrowDown since we're on first item)
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("selection-sub-trigger")).ToBeFocusedAsync();
        
        // Open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("selection-sub-content")).ToBeVisibleAsync();
        
        // First submenu item should be focused
        await Expect(Page.GetByTestId("selection-sub-a")).ToBeFocusedAsync();
        
        // Select item with Enter
        await Page.Keyboard.PressAsync("Enter");
        
        // Both menus should close
        await Expect(Page.GetByTestId("selection-sub-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("selection-submenu-content")).Not.ToBeVisibleAsync();
        
        // Selection should have fired
        await Expect(Page.GetByTestId("selection-output")).ToHaveTextAsync("Sub Option A");
    }

    [Test]
    public async Task Space_ShouldSelectSubmenuItem_AndCloseAllMenus()
    {
        await Page.GetByTestId("selection-submenu-trigger").ClickAsync();
        
        // First item should be focused after opening
        await Expect(Page.GetByTestId("selection-main-item")).ToBeFocusedAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("selection-sub-trigger")).ToBeFocusedAsync();
        
        // Open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("selection-sub-content")).ToBeVisibleAsync();
        
        // First item focused, navigate to second item
        await Expect(Page.GetByTestId("selection-sub-a")).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("ArrowDown"); // sub-b
        await Expect(Page.GetByTestId("selection-sub-b")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync(" ");
        
        await Expect(Page.GetByTestId("selection-sub-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("selection-submenu-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("selection-output")).ToHaveTextAsync("Sub Option B");
    }

    [Test]
    public async Task RTL_ArrowLeft_ShouldOpenSubmenu()
    {
        await Page.GetByTestId("rtl-submenu-trigger").ClickAsync();
        
        // First item should be focused
        await Expect(Page.GetByTestId("rtl-item-1")).ToBeFocusedAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("rtl-sub-trigger")).ToBeFocusedAsync();
        
        // In RTL, ArrowLeft should open (opposite of LTR)
        await Page.Keyboard.PressAsync("ArrowLeft");
        await Expect(Page.GetByTestId("rtl-sub-content")).ToBeVisibleAsync();
        
        // First submenu item should be focused
        await Expect(Page.GetByTestId("rtl-sub-item-1")).ToBeFocusedAsync();
    }

    [Test]
    public async Task RTL_ArrowRight_ShouldCloseSubmenu()
    {
        await Page.GetByTestId("rtl-submenu-trigger").ClickAsync();
        
        // First item should be focused
        await Expect(Page.GetByTestId("rtl-item-1")).ToBeFocusedAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("rtl-sub-trigger")).ToBeFocusedAsync();
        
        // Open submenu (ArrowLeft in RTL)
        await Page.Keyboard.PressAsync("ArrowLeft");
        await Expect(Page.GetByTestId("rtl-sub-content")).ToBeVisibleAsync();
        
        // First submenu item should be focused
        await Expect(Page.GetByTestId("rtl-sub-item-1")).ToBeFocusedAsync();
        
        // In RTL, ArrowRight should close (opposite of LTR)
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("rtl-sub-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("rtl-sub-trigger")).ToBeFocusedAsync();
    }

    [Test]
    public async Task DisabledSubTrigger_ShouldNotOpenOnArrowRight()
    {
        await Page.GetByTestId("disabled-submenu-trigger").ClickAsync();
        
        // Navigate to disabled submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown"); // active-item
        await Page.Keyboard.PressAsync("ArrowDown"); // disabled-sub-trigger (skipped)
        
        // If keyboard navigation properly skips disabled items, we'd be on enabled-sub-trigger
        // If not, let's focus it directly for this test
        await Page.GetByTestId("disabled-sub-trigger").FocusAsync();
        
        // ArrowRight should not open disabled submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("disabled-sub-content")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Typeahead_ShouldWorkInSubmenu()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // First item focused
        await Expect(Page.GetByTestId("item-1")).ToBeFocusedAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("submenu-trigger-1")).ToBeFocusedAsync();
        
        // Open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // First submenu item should be focused
        await Expect(Page.GetByTestId("sub-item-1")).ToBeFocusedAsync();
        
        // Type to search within submenu
        // Items are: "Sub Item 1", "Sub Item 2", "Sub Item 3"
        // Typing "s" should keep focus on first matching item (sub-item-1)
        await Page.Keyboard.PressAsync("s");
        await Expect(Page.GetByTestId("sub-item-1")).ToBeFocusedAsync();
        
        // Submenu should still be open (typeahead doesn't close menu)
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HomeEnd_ShouldWorkInSubmenu()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // Navigate to submenu trigger
        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.GetByTestId("submenu-trigger-1")).ToBeFocusedAsync();
        
        // Open submenu
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // End should go to last item
        await Page.Keyboard.PressAsync("End");
        await Expect(Page.GetByTestId("sub-item-3")).ToBeFocusedAsync();
        
        // Home should go to first item
        await Page.Keyboard.PressAsync("Home");
        await Expect(Page.GetByTestId("sub-item-1")).ToBeFocusedAsync();
    }
}
