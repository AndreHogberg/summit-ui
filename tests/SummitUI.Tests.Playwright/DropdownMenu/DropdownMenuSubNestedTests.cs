using Microsoft.Playwright;

using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DropdownMenu;

public class DropdownMenuSubNestedTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dropdown-menu/submenu";

    [Test]
    public async Task NestedSubmenu_ShouldOpen_ThreeLevelsDeep()
    {
        await Page.GetByTestId("nested-submenu-trigger").ClickAsync();
        
        // Open level 1
        await Page.GetByTestId("level-1-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        
        // Open level 2
        await Page.GetByTestId("level-2-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        
        // Open level 3
        await Page.GetByTestId("level-3-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-3-content")).ToBeVisibleAsync();
        
        // All levels should be visible
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-3-content")).ToBeVisibleAsync();
    }

    [Test]
    public async Task NestedSubmenu_Keyboard_ShouldNavigateDeep()
    {
        await Page.GetByTestId("nested-submenu-trigger").ClickAsync();
        
        // First item should be focused
        await Expect(Page.GetByTestId("nested-top-item")).ToBeFocusedAsync();
        
        // Navigate to level 1 trigger (only 2 items in root: nested-top-item, level-1-trigger)
        await Page.Keyboard.PressAsync("ArrowDown"); // nested-top-item -> level-1-trigger
        await Expect(Page.GetByTestId("level-1-trigger")).ToBeFocusedAsync();
        
        // Open level 1
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        
        // First item in level 1 should be focused (level-1-item)
        await Expect(Page.GetByTestId("level-1-item")).ToBeFocusedAsync();
        
        // Navigate to level 2 trigger
        await Page.Keyboard.PressAsync("ArrowDown"); // level-1-item -> level-2-trigger
        await Expect(Page.GetByTestId("level-2-trigger")).ToBeFocusedAsync();
        
        // Open level 2
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        
        // First item in level 2 should be focused (level-2-item-a)
        await Expect(Page.GetByTestId("level-2-item-a")).ToBeFocusedAsync();
        
        // Navigate to level 3 trigger
        await Page.Keyboard.PressAsync("ArrowDown"); // level-2-item-a -> level-3-trigger
        await Expect(Page.GetByTestId("level-3-trigger")).ToBeFocusedAsync();
        
        // Open level 3
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("level-3-content")).ToBeVisibleAsync();
        
        // Focus should be on first item in level 3
        await Expect(Page.GetByTestId("level-3-item-1")).ToBeFocusedAsync();
    }

    [Test]
    public async Task NestedSubmenu_ArrowLeft_ShouldCloseOneLevel()
    {
        await Page.GetByTestId("nested-submenu-trigger").ClickAsync();
        
        // First item should be focused
        await Expect(Page.GetByTestId("nested-top-item")).ToBeFocusedAsync();
        
        // Navigate deep via keyboard
        await Page.Keyboard.PressAsync("ArrowDown"); // nested-top-item -> level-1-trigger
        await Expect(Page.GetByTestId("level-1-trigger")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowRight"); // Open level 1
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-1-item")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowDown"); // level-1-item -> level-2-trigger
        await Expect(Page.GetByTestId("level-2-trigger")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowRight"); // Open level 2
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-2-item-a")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowDown"); // level-2-item-a -> level-3-trigger
        await Expect(Page.GetByTestId("level-3-trigger")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowRight"); // Open level 3
        await Expect(Page.GetByTestId("level-3-content")).ToBeVisibleAsync();
        
        // ArrowLeft should close level 3 only
        await Page.Keyboard.PressAsync("ArrowLeft");
        await Expect(Page.GetByTestId("level-3-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-3-trigger")).ToBeFocusedAsync();
        
        // ArrowLeft again should close level 2
        await Page.Keyboard.PressAsync("ArrowLeft");
        await Expect(Page.GetByTestId("level-2-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-2-trigger")).ToBeFocusedAsync();
    }

    [Test]
    public async Task NestedSubmenu_Escape_ShouldCloseOneLevel()
    {
        await Page.GetByTestId("nested-submenu-trigger").ClickAsync();
        
        // Open all levels via hover
        await Page.GetByTestId("level-1-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        
        await Page.GetByTestId("level-2-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        
        await Page.GetByTestId("level-3-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-3-content")).ToBeVisibleAsync();
        
        // Focus an item in level 3
        await Page.GetByTestId("level-3-item-1").FocusAsync();
        
        // Escape should close level 3 only
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("level-3-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        
        // Escape again should close level 2
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("level-2-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        
        // Escape again should close level 1
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("level-1-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("nested-submenu-content")).ToBeVisibleAsync();
        
        // Final Escape closes entire menu
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.GetByTestId("nested-submenu-content")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task NestedSubmenu_ClickItem_ShouldCloseEntireTree()
    {
        await Page.GetByTestId("nested-submenu-trigger").ClickAsync();
        
        // Open all levels via hover
        await Page.GetByTestId("level-1-trigger").HoverAsync();
        await Page.GetByTestId("level-2-trigger").HoverAsync();
        await Page.GetByTestId("level-3-trigger").HoverAsync();
        
        await Expect(Page.GetByTestId("level-3-content")).ToBeVisibleAsync();
        
        // Click an item in the deepest level
        await Page.GetByTestId("level-3-item-1").ClickAsync();
        
        // All menus should close
        await Expect(Page.GetByTestId("level-3-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-2-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-1-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("nested-submenu-content")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task NestedSubmenu_HoverBackToParent_ShouldCloseChild()
    {
        await Page.GetByTestId("nested-submenu-trigger").ClickAsync();
        
        // Open level 1 and 2
        await Page.GetByTestId("level-1-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        
        await Page.GetByTestId("level-2-trigger").HoverAsync();
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        
        // Hover back to level 1 item (not a sub trigger)
        await Page.GetByTestId("level-1-item").HoverAsync();
        
        // Level 2 should close
        await Expect(Page.GetByTestId("level-2-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
    }

    [Test]
    public async Task NestedSubmenu_ShouldMaintainFocusChain()
    {
        await Page.GetByTestId("nested-submenu-trigger").ClickAsync();
        
        // First item should be focused
        await Expect(Page.GetByTestId("nested-top-item")).ToBeFocusedAsync();
        
        // Navigate to level 1 trigger
        await Page.Keyboard.PressAsync("ArrowDown"); // nested-top-item -> level-1-trigger
        await Expect(Page.GetByTestId("level-1-trigger")).ToBeFocusedAsync();
        
        // Open level 1
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("level-1-content")).ToBeVisibleAsync();
        
        // First item in level 1 should be focused
        await Expect(Page.GetByTestId("level-1-item")).ToBeFocusedAsync();
        
        // Navigate to level 2 trigger
        await Page.Keyboard.PressAsync("ArrowDown"); // level-1-item -> level-2-trigger
        await Expect(Page.GetByTestId("level-2-trigger")).ToBeFocusedAsync();
        
        // Open level 2
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByTestId("level-2-content")).ToBeVisibleAsync();
        
        // Focus should be on first item of level 2
        await Expect(Page.GetByTestId("level-2-item-a")).ToBeFocusedAsync();
        
        // Navigate within level 2
        await Page.Keyboard.PressAsync("ArrowDown"); // level-2-item-a -> level-3-trigger
        await Expect(Page.GetByTestId("level-3-trigger")).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowDown"); // level-3-trigger -> level-2-item-b
        await Expect(Page.GetByTestId("level-2-item-b")).ToBeFocusedAsync();
    }
}
