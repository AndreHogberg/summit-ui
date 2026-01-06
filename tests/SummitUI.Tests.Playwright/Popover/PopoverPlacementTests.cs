namespace SummitUI.Tests.Playwright.Popover;

/// <summary>
/// Tests for Popover placement attributes.
/// </summary>
public class PopoverPlacementTests : SummitTestBase
{
    protected override string TestPagePath => "tests/popover/placement";

    [Test]
    [Arguments("top")]
    [Arguments("right")]
    [Arguments("bottom")]
    [Arguments("left")]
    public async Task Content_ShouldHave_CorrectDataSideAttribute(string side)
    {
        var trigger = Page.GetByTestId($"trigger-{side}");
        await trigger.ClickAsync();

        var content = Page.GetByTestId($"content-{side}");
        await Expect(content).ToHaveAttributeAsync("data-side", side);
    }
}
