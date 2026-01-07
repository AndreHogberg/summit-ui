using Microsoft.Playwright;

using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DropdownMenu;

public class DropdownMenuSelectionTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dropdown-menu/selection";

    [Test]
    public async Task CheckboxItem_ShouldToggleState()
    {
        await Page.GetByTestId("checkbox-trigger").ClickAsync();

        var checkbox = Page.GetByTestId("checkbox-item-toolbar");
        await Expect(checkbox).ToHaveAttributeAsync("role", "menuitemcheckbox");
        await Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");

        await checkbox.ClickAsync();

        // Checkbox items should NOT close the menu automatically
        await Expect(Page.GetByTestId("checkbox-content")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("checkbox-status")).ToHaveTextAsync("Toolbar: Off");
    }

    [Test]
    public async Task RadioItem_ShouldChangeSelection()
    {
        await Page.GetByTestId("radio-trigger").ClickAsync();

        var group = Page.GetByTestId("radio-group");
        await Expect(group).ToHaveAttributeAsync("role", "group");
        await Expect(group).ToHaveAttributeAsync("aria-label", "Theme selection");

        var lightRadio = Page.GetByTestId("radio-item-light");
        var systemRadio = Page.GetByTestId("radio-item-system");

        await Expect(lightRadio).ToHaveAttributeAsync("role", "menuitemradio");
        await Expect(lightRadio).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(systemRadio).ToHaveAttributeAsync("aria-checked", "true");

        await lightRadio.ClickAsync();

        // Menu closes
        await Expect(Page.GetByTestId("radio-content")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByTestId("radio-status")).ToHaveTextAsync("Theme: light");
    }

    [Test]
    public async Task Group_ShouldHaveProperLabeling()
    {
        await Page.GetByTestId("group-trigger").ClickAsync();

        var group = Page.GetByTestId("group-1");
        var label = Page.GetByTestId("group-label-1");

        await Expect(group).ToHaveAttributeAsync("role", "group");
        var labelId = await label.GetAttributeAsync("id");
        await Expect(group).ToHaveAttributeAsync("aria-labelledby", labelId!);
    }
}
