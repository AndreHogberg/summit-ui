namespace SummitUI.Tests.Playwright.Tabs;

/// <summary>
/// Tests for TabsList ARIA attributes.
/// </summary>
public class TabsListAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/tabs/basic";

    [Test]
    public async Task TabsList_ShouldHave_RoleTablist()
    {
        var tabsList = Page.GetByTestId("basic-tabs-list");
        await Expect(tabsList).ToHaveAttributeAsync("role", "tablist");
    }

    [Test]
    public async Task TabsList_ShouldHave_AriaOrientationHorizontal_ByDefault()
    {
        var tabsList = Page.GetByTestId("basic-tabs-list");
        await Expect(tabsList).ToHaveAttributeAsync("aria-orientation", "horizontal");
    }

    [Test]
    public async Task TabsList_ShouldHave_DataOrientationHorizontal_ByDefault()
    {
        var tabsList = Page.GetByTestId("basic-tabs-list");
        await Expect(tabsList).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    [Test]
    public async Task TabsList_ShouldHave_DataSummitTabsListAttribute()
    {
        var tabsList = Page.GetByTestId("basic-tabs-list");
        await Expect(tabsList).ToHaveAttributeAsync("data-summit-tabs-list", "");
    }
}
