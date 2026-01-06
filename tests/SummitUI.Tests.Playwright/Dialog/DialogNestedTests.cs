namespace SummitUI.Tests.Playwright.Dialog;

/// <summary>
/// Tests for nested Dialog behavior.
/// </summary>
public class DialogNestedTests : SummitTestBase
{
    protected override string TestPagePath => "tests/dialog/nested";

    [Test]
    public async Task NestedDialog_ShouldHave_DataNestedAttribute()
    {
        await Page.GetByTestId("parent-trigger").ClickAsync();
        await Page.GetByTestId("nested-trigger").ClickAsync();

        var nestedContent = Page.GetByTestId("nested-content");
        await Expect(nestedContent).ToHaveAttributeAsync("data-nested", "");
    }

    [Test]
    public async Task ParentDialog_ShouldHave_DataNestedOpen_WhenNestedIsOpen()
    {
        await Page.GetByTestId("parent-trigger").ClickAsync();
        await Page.GetByTestId("nested-trigger").ClickAsync();

        var parentContent = Page.GetByTestId("parent-content");
        await Expect(parentContent).ToHaveAttributeAsync("data-nested-open", "");
    }

    [Test]
    public async Task NestedDialog_ShouldClose_IndependentlyFromParent()
    {
        await Page.GetByTestId("parent-trigger").ClickAsync();
        await Page.GetByTestId("nested-trigger").ClickAsync();

        var nestedContent = Page.GetByTestId("nested-content");
        var parentContent = Page.GetByTestId("parent-content");

        await Expect(nestedContent).ToBeVisibleAsync();

        // Close nested with Escape
        await Page.Keyboard.PressAsync("Escape");

        await Expect(nestedContent).Not.ToBeVisibleAsync();
        await Expect(parentContent).ToBeVisibleAsync();
    }

    [Test]
    public async Task NestedDialog_ShouldClose_AllDialogsSequentially_WithMultipleEscapePresses()
    {
        await Page.GetByTestId("parent-trigger").ClickAsync();
        await Page.GetByTestId("nested-trigger").ClickAsync();

        var nestedContent = Page.GetByTestId("nested-content");
        var parentContent = Page.GetByTestId("parent-content");

        // First Escape
        await Page.Keyboard.PressAsync("Escape");
        await Expect(nestedContent).Not.ToBeVisibleAsync();
        await Expect(parentContent).ToBeVisibleAsync();

        // Second Escape
        await Page.Keyboard.PressAsync("Escape");
        await Expect(parentContent).Not.ToBeVisibleAsync();
    }
}
