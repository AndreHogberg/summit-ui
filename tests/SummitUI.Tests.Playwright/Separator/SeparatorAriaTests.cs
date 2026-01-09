namespace SummitUI.Tests.Playwright.Separator;

/// <summary>
/// Tests for ARIA attributes on Separator components.
/// Verifies proper accessibility attributes for semantic and decorative separators.
/// </summary>
public class SeparatorAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/separator/basic";

    #region Semantic Separator (Default)

    [Test]
    public async Task Separator_ShouldHave_RoleSeparator()
    {
        var separator = Page.GetByTestId("separator-horizontal");
        await Expect(separator).ToHaveAttributeAsync("role", "separator");
    }

    [Test]
    public async Task Separator_ShouldHave_AriaOrientationHorizontal_WhenHorizontal()
    {
        var separator = Page.GetByTestId("separator-horizontal");
        await Expect(separator).ToHaveAttributeAsync("aria-orientation", "horizontal");
    }

    [Test]
    public async Task Separator_ShouldHave_AriaOrientationVertical_WhenVertical()
    {
        var separator = Page.GetByTestId("separator-vertical");
        await Expect(separator).ToHaveAttributeAsync("aria-orientation", "vertical");
    }

    [Test]
    public async Task Separator_ShouldHave_DataOrientationHorizontal_WhenHorizontal()
    {
        var separator = Page.GetByTestId("separator-horizontal");
        await Expect(separator).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    [Test]
    public async Task Separator_ShouldHave_DataOrientationVertical_WhenVertical()
    {
        var separator = Page.GetByTestId("separator-vertical");
        await Expect(separator).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion

    #region Decorative Separator

    [Test]
    public async Task DecorativeSeparator_ShouldHave_RoleNone()
    {
        var separator = Page.GetByTestId("separator-decorative");
        await Expect(separator).ToHaveAttributeAsync("role", "none");
    }

    [Test]
    public async Task DecorativeSeparator_ShouldNotHave_AriaOrientation()
    {
        var separator = Page.GetByTestId("separator-decorative");
        var ariaOrientation = await separator.GetAttributeAsync("aria-orientation");
        await Assert.That(ariaOrientation).IsNull();
    }

    [Test]
    public async Task DecorativeSeparator_ShouldStillHave_DataOrientation()
    {
        var separator = Page.GetByTestId("separator-decorative");
        await Expect(separator).ToHaveAttributeAsync("data-orientation", "horizontal");
    }

    [Test]
    public async Task DecorativeVerticalSeparator_ShouldHave_RoleNone()
    {
        var separator = Page.GetByTestId("separator-decorative-vertical");
        await Expect(separator).ToHaveAttributeAsync("role", "none");
    }

    [Test]
    public async Task DecorativeVerticalSeparator_ShouldHave_DataOrientationVertical()
    {
        var separator = Page.GetByTestId("separator-decorative-vertical");
        await Expect(separator).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    #endregion
}
