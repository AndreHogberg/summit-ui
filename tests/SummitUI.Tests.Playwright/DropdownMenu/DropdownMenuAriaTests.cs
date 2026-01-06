using Microsoft.Playwright;
using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DropdownMenu;

public class DropdownMenuAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dropdown-menu/basic";

    [Test]
    public async Task Trigger_ShouldHave_AriaAttributes()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        
        await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "menu");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "closed");

        await trigger.ClickAsync();
        
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(trigger).ToHaveAttributeAsync("data-state", "open");
        
        var content = Page.GetByTestId("basic-content");
        var contentId = await content.GetAttributeAsync("id");
        await Expect(trigger).ToHaveAttributeAsync("aria-controls", contentId!);
    }

    [Test]
    public async Task Content_ShouldHave_AriaAttributes()
    {
        var trigger = Page.GetByTestId("basic-trigger");
        await trigger.ClickAsync();

        var content = Page.GetByTestId("basic-content");
        await Expect(content).ToHaveAttributeAsync("role", "menu");
        await Expect(content).ToHaveAttributeAsync("aria-orientation", "vertical");
        await Expect(content).ToHaveAttributeAsync("data-state", "open");
        
        var triggerId = await trigger.GetAttributeAsync("id");
        await Expect(content).ToHaveAttributeAsync("aria-labelledby", triggerId!);
    }

    [Test]
    public async Task MenuItem_ShouldHave_AriaAttributes()
    {
        await Page.GetByTestId("basic-trigger").ClickAsync();

        var item = Page.GetByTestId("item-1");
        await Expect(item).ToHaveAttributeAsync("role", "menuitem");
        await Expect(item).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task DisabledMenuItem_ShouldHave_AriaAttributes()
    {
        await Page.GetByTestId("disabled-trigger").ClickAsync();

        var disabledItem = Page.GetByTestId("disabled-item-1");
        await Expect(disabledItem).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabledItem).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task Separator_ShouldHave_AriaAttributes()
    {
        await Page.GetByTestId("basic-trigger").ClickAsync();

        var separator = Page.GetByTestId("basic-separator");
        await Expect(separator).ToHaveAttributeAsync("role", "separator");
        await Expect(separator).ToHaveAttributeAsync("aria-orientation", "horizontal");
    }

    [Test]
    public async Task ControlledMenu_ShouldSyncWithExternalState()
    {
        var trigger = Page.GetByTestId("controlled-trigger");
        var externalToggle = Page.GetByTestId("external-toggle");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");

        await externalToggle.ClickAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");

        await trigger.ClickAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }
}
