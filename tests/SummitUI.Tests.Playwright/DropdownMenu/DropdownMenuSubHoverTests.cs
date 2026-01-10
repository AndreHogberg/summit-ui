using Microsoft.Playwright;

using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DropdownMenu;

public class DropdownMenuSubHoverTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dropdown-menu/submenu";

    [Test]
    public async Task Hover_ShouldOpenSubmenu_WithDelay()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        
        // Hover over submenu trigger
        await subTrigger.HoverAsync();
        
        // Should open after delay (100ms open delay)
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
    }

    [Test]
    public async Task HoverAway_ShouldCloseSubmenu_WithDelay()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.HoverAsync();
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // Hover away to a different item
        await Page.GetByTestId("item-1").HoverAsync();
        
        // Should close after delay (300ms close delay)
        await Expect(Page.GetByTestId("submenu-content-1")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task HoverIntoSubmenuContent_ShouldKeepSubmenuOpen()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.HoverAsync();
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
        
        // Hover into the submenu content
        await Page.GetByTestId("sub-item-2").HoverAsync();
        
        // Submenu should stay open
        await Expect(Page.GetByTestId("submenu-content-1")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ClickOnSubItem_ShouldCloseAllMenus()
    {
        await Page.GetByTestId("selection-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("selection-sub-trigger");
        await subTrigger.HoverAsync();
        await Expect(Page.GetByTestId("selection-sub-content")).ToBeVisibleAsync();
        
        // Click on submenu item
        await Page.GetByTestId("selection-sub-a").ClickAsync();
        
        // Both menus should close
        await Expect(Page.GetByTestId("selection-sub-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("selection-submenu-content")).Not.ToBeVisibleAsync();
        
        // Selection should have fired
        await Expect(Page.GetByTestId("selection-output")).ToHaveTextAsync("Sub Option A");
    }

    [Test]
    public async Task HoverOnSiblingSubmenu_ShouldCloseOtherSubmenu()
    {
        await Page.GetByTestId("multi-submenu-trigger").ClickAsync();
        
        // Hover over first submenu (Edit)
        await Page.GetByTestId("edit-submenu-trigger").HoverAsync();
        await Expect(Page.GetByTestId("edit-submenu-content")).ToBeVisibleAsync();
        
        // Hover over second submenu (View)
        await Page.GetByTestId("view-submenu-trigger").HoverAsync();
        await Expect(Page.GetByTestId("view-submenu-content")).ToBeVisibleAsync();
        
        // First submenu should be closed
        await Expect(Page.GetByTestId("edit-submenu-content")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task HoverOnThirdSubmenu_ShouldCloseSecondSubmenu()
    {
        await Page.GetByTestId("multi-submenu-trigger").ClickAsync();
        
        // Open second submenu
        await Page.GetByTestId("view-submenu-trigger").HoverAsync();
        await Expect(Page.GetByTestId("view-submenu-content")).ToBeVisibleAsync();
        
        // Hover over third submenu (Help)
        await Page.GetByTestId("help-submenu-trigger").HoverAsync();
        await Expect(Page.GetByTestId("help-submenu-content")).ToBeVisibleAsync();
        
        // Second submenu should be closed
        await Expect(Page.GetByTestId("view-submenu-content")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task DisabledSubTrigger_ShouldNotOpenOnHover()
    {
        await Page.GetByTestId("disabled-submenu-trigger").ClickAsync();
        
        // Hover over disabled submenu trigger
        await Page.GetByTestId("disabled-sub-trigger").HoverAsync();
        await Task.Delay(200); // Wait longer than open delay
        
        // Submenu should not open
        await Expect(Page.GetByTestId("disabled-sub-content")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task DisabledSubTrigger_EnabledSibling_ShouldOpenOnHover()
    {
        await Page.GetByTestId("disabled-submenu-trigger").ClickAsync();
        
        // Hover over enabled submenu trigger
        await Page.GetByTestId("enabled-sub-trigger").HoverAsync();
        
        // Submenu should open
        await Expect(Page.GetByTestId("enabled-sub-content")).ToBeVisibleAsync();
    }
}
