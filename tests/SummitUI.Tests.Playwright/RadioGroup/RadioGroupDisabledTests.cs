namespace SummitUI.Tests.Playwright.RadioGroup;

/// <summary>
/// Tests for RadioGroup disabled states.
/// </summary>
public class RadioGroupDisabledTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/disabled";

    [Test]
    public async Task DisabledGroup_ShouldHave_DataDisabledAttribute()
    {
        var radioGroup = Page.GetByTestId("disabled-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledGroup_Items_ShouldHave_AriaDisabled()
    {
        var option1 = Page.GetByTestId("disabled-group-option1");
        var option2 = Page.GetByTestId("disabled-group-option2");
        var option3 = Page.GetByTestId("disabled-group-option3");

        await Expect(option1).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(option2).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(option3).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledGroup_Items_ShouldHave_DisabledAttribute()
    {
        var option1 = Page.GetByTestId("disabled-group-option1");
        await Expect(option1).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledGroup_Click_ShouldNotSelectItem()
    {
        var option2 = Page.GetByTestId("disabled-group-option2");
        var valueText = Page.GetByTestId("disabled-group-value");

        // Initial value is option1
        await Expect(valueText).ToHaveTextAsync("Value: option1");

        // Try to click option2
        await option2.ClickAsync(new() { Force = true });

        // Value should not change
        await Expect(valueText).ToHaveTextAsync("Value: option1");
    }

    [Test]
    public async Task IndividualDisabledItem_ShouldHave_AriaDisabled()
    {
        var disabledItem = Page.GetByTestId("disabled1");
        await Expect(disabledItem).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task IndividualDisabledItem_ShouldHave_DataDisabledAttribute()
    {
        var disabledItem = Page.GetByTestId("disabled1");
        await Expect(disabledItem).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task IndividualDisabledItem_Click_ShouldNotSelectItem()
    {
        var disabledItem = Page.GetByTestId("disabled1");
        var enabledItem = Page.GetByTestId("enabled1");
        var valueText = Page.GetByTestId("disabled-items-value");

        // First select enabled1
        await enabledItem.ClickAsync();
        await Expect(valueText).ToHaveTextAsync("Selected: enabled1");

        // Try to click disabled item
        await disabledItem.ClickAsync(new() { Force = true });

        // Value should not change
        await Expect(valueText).ToHaveTextAsync("Selected: enabled1");
    }

    [Test]
    public async Task KeyboardNavigation_ShouldSkipDisabledItems()
    {
        var enabled1 = Page.GetByTestId("enabled1");
        var enabled2 = Page.GetByTestId("enabled2");
        var disabled1 = Page.GetByTestId("disabled1");

        // Focus and select enabled1
        await enabled1.ClickAsync();
        await enabled1.FocusAsync();

        // Press ArrowDown - should skip disabled1 and go to enabled2
        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(enabled2).ToBeFocusedAsync();
        await Expect(enabled2).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(disabled1).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task FirstDisabledItem_TabShouldFocusSecondItem()
    {
        // Navigate to the first-disabled section
        await Page.GotoAsync(Hooks.ServerUrl + "tests/radiogroup/disabled");
        await WaitForBlazorReady();

        var firstDisabledSection = Page.GetByTestId("first-disabled-section");
        var secondItem = Page.GetByTestId("first-disabled-second");

        // Focus something in the section and tab
        await firstDisabledSection.Locator("h2").ClickAsync();
        await Page.Keyboard.PressAsync("Tab");

        // Second item should receive focus since first is disabled
        await Expect(secondItem).ToBeFocusedAsync();
    }

    [Test]
    public async Task ToggleableGroup_ShouldUpdateDisabledState()
    {
        var optionA = Page.GetByTestId("toggleable-a");
        var toggleButton = Page.GetByTestId("toggle-disabled-btn");

        // Initially enabled
        await Expect(optionA).Not.ToBeDisabledAsync();

        // Toggle disabled
        await toggleButton.ClickAsync();

        // Now disabled
        await Expect(optionA).ToBeDisabledAsync();
        await Expect(optionA).ToHaveAttributeAsync("aria-disabled", "true");

        // Toggle back
        await toggleButton.ClickAsync();

        // Enabled again
        await Expect(optionA).Not.ToBeDisabledAsync();
    }
}
