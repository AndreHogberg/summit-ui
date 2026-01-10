namespace SummitUI.Tests.Playwright.RadioGroup;

/// <summary>
/// Tests for RadioGroup RTL (Right-to-Left) navigation behavior.
/// </summary>
public class RadioGroupRtlTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/rtl";

    [Test]
    public async Task RTL_Horizontal_ArrowRight_ShouldNavigateToPrevious()
    {
        // In RTL, ArrowRight should go to the previous item (visually to the right, but logically previous)
        var secondItem = Page.GetByTestId("rtl-h-second");
        var firstItem = Page.GetByTestId("rtl-h-first");

        // Click and focus second item
        await secondItem.ClickAsync();
        await secondItem.FocusAsync();

        // Press ArrowRight - in RTL, this should go to "previous" (first)
        await Page.Keyboard.PressAsync("ArrowRight");

        await Expect(firstItem).ToBeFocusedAsync();
        await Expect(firstItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task RTL_Horizontal_ArrowLeft_ShouldNavigateToNext()
    {
        // In RTL, ArrowLeft should go to the next item (visually to the left, but logically next)
        var firstItem = Page.GetByTestId("rtl-h-first");
        var secondItem = Page.GetByTestId("rtl-h-second");

        // Click and focus first item
        await firstItem.ClickAsync();
        await firstItem.FocusAsync();

        // Press ArrowLeft - in RTL, this should go to "next" (second)
        await Page.Keyboard.PressAsync("ArrowLeft");

        await Expect(secondItem).ToBeFocusedAsync();
        await Expect(secondItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task RTL_Vertical_ArrowDown_ShouldNavigateToNext()
    {
        // Vertical navigation should be unaffected by RTL
        var topItem = Page.GetByTestId("rtl-v-top");
        var middleItem = Page.GetByTestId("rtl-v-middle");

        await topItem.ClickAsync();
        await topItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(middleItem).ToBeFocusedAsync();
        await Expect(middleItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task RTL_Vertical_ArrowUp_ShouldNavigateToPrevious()
    {
        // Vertical navigation should be unaffected by RTL
        var topItem = Page.GetByTestId("rtl-v-top");
        var middleItem = Page.GetByTestId("rtl-v-middle");

        await middleItem.ClickAsync();
        await middleItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        await Expect(topItem).ToBeFocusedAsync();
        await Expect(topItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task LTR_Horizontal_ArrowRight_ShouldNavigateToNext()
    {
        // In LTR, ArrowRight should go to the next item
        var firstItem = Page.GetByTestId("ltr-h-first");
        var secondItem = Page.GetByTestId("ltr-h-second");

        await firstItem.ClickAsync();
        await firstItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowRight");

        await Expect(secondItem).ToBeFocusedAsync();
        await Expect(secondItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task LTR_Horizontal_ArrowLeft_ShouldNavigateToPrevious()
    {
        // In LTR, ArrowLeft should go to the previous item
        var firstItem = Page.GetByTestId("ltr-h-first");
        var secondItem = Page.GetByTestId("ltr-h-second");

        await secondItem.ClickAsync();
        await secondItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowLeft");

        await Expect(firstItem).ToBeFocusedAsync();
        await Expect(firstItem).ToHaveAttributeAsync("aria-checked", "true");
    }
}
