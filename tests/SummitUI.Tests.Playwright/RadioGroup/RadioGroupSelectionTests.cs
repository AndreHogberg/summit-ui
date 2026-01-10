namespace SummitUI.Tests.Playwright.RadioGroup;

/// <summary>
/// Tests for RadioGroup selection behavior.
/// </summary>
public class RadioGroupSelectionTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/basic";

    [Test]
    public async Task Click_ShouldSelectItem()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var selectedText = Page.GetByTestId("selected-value");

        await Expect(selectedText).ToHaveTextAsync("Selected: none");

        await starterItem.ClickAsync();

        await Expect(selectedText).ToHaveTextAsync("Selected: starter");
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Click_ShouldDeselectPreviousItem()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var proItem = Page.GetByTestId("radio-pro");

        // Select starter first
        await starterItem.ClickAsync();
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "true");

        // Select pro
        await proItem.ClickAsync();

        // Starter should be deselected
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(proItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task OnlyOneItem_ShouldBeSelected_AtATime()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var proItem = Page.GetByTestId("radio-pro");
        var enterpriseItem = Page.GetByTestId("radio-enterprise");

        // Select starter
        await starterItem.ClickAsync();

        // Verify only starter is selected
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(proItem).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(enterpriseItem).ToHaveAttributeAsync("aria-checked", "false");

        // Select enterprise
        await enterpriseItem.ClickAsync();

        // Verify only enterprise is selected
        await Expect(starterItem).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(proItem).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(enterpriseItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task ControlledMode_ShouldSyncWithExternalState()
    {
        var option1 = Page.GetByTestId("controlled-option1");
        var option2 = Page.GetByTestId("controlled-option2");
        var option3 = Page.GetByTestId("controlled-option3");

        // Initially option1 should be selected (controlled value)
        await Expect(option1).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(option2).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(option3).ToHaveAttributeAsync("aria-checked", "false");

        // Click external button to select option2
        var selectOption2Button = Page.GetByText("Select Option 2");
        await selectOption2Button.ClickAsync();

        // Option2 should now be selected
        await Expect(option1).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(option2).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(option3).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task DefaultValue_ShouldBeSelected_Initially()
    {
        var smallItem = Page.GetByTestId("default-small");
        var mediumItem = Page.GetByTestId("default-medium");
        var largeItem = Page.GetByTestId("default-large");

        // Medium should be selected by default
        await Expect(smallItem).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(mediumItem).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(largeItem).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task ValueChanged_ShouldFireOnSelection()
    {
        var starterItem = Page.GetByTestId("radio-starter");
        var selectedText = Page.GetByTestId("selected-value");

        // Initially no selection
        await Expect(selectedText).ToHaveTextAsync("Selected: none");

        // Select starter
        await starterItem.ClickAsync();

        // Value should be updated
        await Expect(selectedText).ToHaveTextAsync("Selected: starter");
    }
}
