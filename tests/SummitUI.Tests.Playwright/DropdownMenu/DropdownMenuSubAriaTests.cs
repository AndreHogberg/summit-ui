using Microsoft.Playwright;

using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DropdownMenu;

public class DropdownMenuSubAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dropdown-menu/submenu";

    [Test]
    public async Task SubTrigger_ShouldHave_AriaHasPopup()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await Expect(subTrigger).ToHaveAttributeAsync("aria-haspopup", "menu");
    }

    [Test]
    public async Task SubTrigger_ShouldHave_AriaExpanded_False_WhenClosed()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await Expect(subTrigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(subTrigger).ToHaveAttributeAsync("data-state", "closed");
    }

    [Test]
    public async Task SubTrigger_ShouldHave_AriaExpanded_True_WhenOpen()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        
        // Open submenu via keyboard
        await subTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        await Expect(subTrigger).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(subTrigger).ToHaveAttributeAsync("data-state", "open");
    }

    [Test]
    public async Task SubTrigger_ShouldHave_AriaControls_WhenOpen()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        var subContent = Page.GetByTestId("submenu-content-1");
        var contentId = await subContent.GetAttributeAsync("id");
        
        await Expect(subTrigger).ToHaveAttributeAsync("aria-controls", contentId!);
    }

    [Test]
    public async Task SubTrigger_ShouldHave_MenuItemRole()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await Expect(subTrigger).ToHaveAttributeAsync("role", "menuitem");
    }

    [Test]
    public async Task SubContent_ShouldHave_MenuRole()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // Open submenu
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        var subContent = Page.GetByTestId("submenu-content-1");
        await Expect(subContent).ToHaveAttributeAsync("role", "menu");
    }

    [Test]
    public async Task SubContent_ShouldHave_AriaOrientation()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        var subContent = Page.GetByTestId("submenu-content-1");
        await Expect(subContent).ToHaveAttributeAsync("aria-orientation", "vertical");
    }

    [Test]
    public async Task SubContent_ShouldHave_AriaLabelledBy()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        var subContent = Page.GetByTestId("submenu-content-1");
        var triggerId = await subTrigger.GetAttributeAsync("id");
        
        await Expect(subContent).ToHaveAttributeAsync("aria-labelledby", triggerId!);
    }

    [Test]
    public async Task SubMenuItem_ShouldHave_MenuItemRole()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        var subItem = Page.GetByTestId("sub-item-1");
        await Expect(subItem).ToHaveAttributeAsync("role", "menuitem");
        await Expect(subItem).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task DisabledSubTrigger_ShouldHave_AriaDisabled()
    {
        await Page.GetByTestId("disabled-submenu-trigger").ClickAsync();
        
        var disabledTrigger = Page.GetByTestId("disabled-sub-trigger");
        await Expect(disabledTrigger).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabledTrigger).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task SubTrigger_ShouldHave_DataHighlighted_WhenFocused()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        // Navigate to submenu trigger via keyboard (this properly sets highlighted state)
        await Page.Keyboard.PressAsync("ArrowDown"); // item-1 -> item-2
        await Page.Keyboard.PressAsync("ArrowDown"); // item-2 -> submenu-trigger-1
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await Expect(subTrigger).ToBeFocusedAsync();
        await Expect(subTrigger).ToHaveAttributeAsync("data-highlighted", "");
    }

    [Test]
    public async Task SubContent_ShouldHave_DataState_Open()
    {
        await Page.GetByTestId("basic-submenu-trigger").ClickAsync();
        
        var subTrigger = Page.GetByTestId("submenu-trigger-1");
        await subTrigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        var subContent = Page.GetByTestId("submenu-content-1");
        await Expect(subContent).ToHaveAttributeAsync("data-state", "open");
    }
}
