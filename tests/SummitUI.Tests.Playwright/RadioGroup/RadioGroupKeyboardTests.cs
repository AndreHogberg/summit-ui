namespace SummitUI.Tests.Playwright.RadioGroup;

/// <summary>
/// Tests for RadioGroup keyboard navigation.
/// </summary>
public class RadioGroupKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/basic";

    [Test]
    public async Task Tab_ShouldFocusFirstItem_WhenNoneSelected()
    {
        // Get the first focusable element on the page
        var starterItem = Page.GetByTestId("radio-starter");
        
        // Click somewhere outside to ensure we start fresh
        await Page.Locator("h1").ClickAsync();
        
        // Tab into the radio group - first item should be focusable
        await Page.Keyboard.PressAsync("Tab");
        
        // Keep tabbing until we reach the radio group
        var maxTabs = 10;
        var currentTab = 0;
        while (currentTab < maxTabs)
        {
            var focused = Page.Locator(":focus");
            if (await focused.GetAttributeAsync("data-testid") == "radio-starter")
            {
                break;
            }
            await Page.Keyboard.PressAsync("Tab");
            currentTab++;
        }

        await Expect(starterItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tab_ShouldFocusSelectedItem_WhenOneIsSelected()
    {
        // Click on the second item to select it
        var proItem = Page.GetByTestId("radio-pro");
        await proItem.ClickAsync();

        // Tab away
        await Page.Keyboard.PressAsync("Tab");
        
        // Tab back - should focus the selected item
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(proItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task Space_ShouldSelectFocusedItem()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        await starterItem.FocusAsync();

        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "false");

        await Page.Keyboard.PressAsync(" ");

        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task ArrowDown_ShouldNavigateToNextItem_AndSelectIt()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var proItem = Page.GetByTestId("radio-pro");

        await starterItem.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        // Arrow down should move focus to next item AND select it
        await Expect(proItem).ToBeFocusedAsync();
        await Expect(proItem).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task ArrowUp_ShouldNavigateToPreviousItem_AndSelectIt()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var proItem = Page.GetByTestId("radio-pro");

        // First select and focus the pro item
        await proItem.ClickAsync();
        await proItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        // Arrow up should move focus to previous item AND select it
        await Expect(starterItem).ToBeFocusedAsync();
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(proItem).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task ArrowDown_ShouldWrapToFirst_WhenOnLastItem_WithLoopEnabled()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var enterpriseItem = Page.GetByTestId("radio-enterprise");

        // Focus the last item
        await enterpriseItem.ClickAsync();
        await enterpriseItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        // Should wrap to first item
        await Expect(starterItem).ToBeFocusedAsync();
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task ArrowUp_ShouldWrapToLast_WhenOnFirstItem_WithLoopEnabled()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var enterpriseItem = Page.GetByTestId("radio-enterprise");

        // Focus the first item
        await starterItem.ClickAsync();
        await starterItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        // Should wrap to last item
        await Expect(enterpriseItem).ToBeFocusedAsync();
        await Expect(enterpriseItem).ToHaveAttributeAsync("aria-checked", "true");
    }
}

/// <summary>
/// Tests for horizontal RadioGroup keyboard navigation.
/// </summary>
public class RadioGroupHorizontalKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/basic";

    [Test]
    public async Task ArrowRight_ShouldNavigateToNextItem_InHorizontalGroup()
    {
        var leftItem = Page.GetByTestId("horizontal-left");
        var centerItem = Page.GetByTestId("horizontal-center");

        await leftItem.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");

        await Expect(centerItem).ToBeFocusedAsync();
        await Expect(centerItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task ArrowLeft_ShouldNavigateToPreviousItem_InHorizontalGroup()
    {
        var leftItem = Page.GetByTestId("horizontal-left");
        var centerItem = Page.GetByTestId("horizontal-center");

        await centerItem.ClickAsync();
        await centerItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowLeft");

        await Expect(leftItem).ToBeFocusedAsync();
        await Expect(leftItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task ArrowDown_ShouldNotNavigate_InHorizontalGroup()
    {
        var leftItem = Page.GetByTestId("horizontal-left");

        await leftItem.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        // Should remain focused on the same item
        await Expect(leftItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowUp_ShouldNotNavigate_InHorizontalGroup()
    {
        var centerItem = Page.GetByTestId("horizontal-center");

        await centerItem.ClickAsync();
        await centerItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        // Should remain focused on the same item
        await Expect(centerItem).ToBeFocusedAsync();
    }
}

/// <summary>
/// Tests for RadioGroup navigation with Loop disabled.
/// </summary>
public class RadioGroupNoLoopKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/basic";

    [Test]
    public async Task ArrowDown_ShouldNotWrap_WhenLoopDisabled()
    {
        var thirdItem = Page.GetByTestId("noloop-third");

        // Select and focus the last item
        await thirdItem.ClickAsync();
        await thirdItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowDown");

        // Should stay on the last item
        await Expect(thirdItem).ToBeFocusedAsync();
        await Expect(thirdItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task ArrowUp_ShouldNotWrap_WhenLoopDisabled()
    {
        var firstItem = Page.GetByTestId("noloop-first");

        // Select and focus the first item
        await firstItem.ClickAsync();
        await firstItem.FocusAsync();

        await Page.Keyboard.PressAsync("ArrowUp");

        // Should stay on the first item
        await Expect(firstItem).ToBeFocusedAsync();
        await Expect(firstItem).ToHaveAttributeAsync("aria-checked", "true");
    }
}
