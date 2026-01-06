using Microsoft.Playwright;
using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DateField;

public class DateFieldKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/date-field/keyboard";

    [Test]
    public async Task ArrowKeys_ShouldChangeValue()
    {
        var section = Page.GetByTestId("keyboard-section");
        var daySegment = section.Locator("[data-segment='day']");
        
        await daySegment.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowUp");
        await Expect(daySegment).ToHaveAttributeAsync("aria-valuenow", "16");
        
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(daySegment).ToHaveAttributeAsync("aria-valuenow", "15");
    }

    [Test]
    public async Task ArrowKeys_ShouldNavigateBetweenSegments()
    {
        var section = Page.GetByTestId("keyboard-section");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        
        await yearSegment.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(monthSegment).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("ArrowLeft");
        await Expect(yearSegment).ToBeFocusedAsync();
    }

    [Test]
    public async Task NumericInput_ShouldUpdateAndAutoAdvance()
    {
        var section = Page.GetByTestId("keyboard-section");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");
        
        await monthSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("12"); // Dec
        
        await Expect(monthSegment).ToHaveAttributeAsync("aria-valuenow", "12");
        await Expect(daySegment).ToBeFocusedAsync();
    }

    [Test]
    public async Task Backspace_ShouldClearSegment()
    {
        var section = Page.GetByTestId("keyboard-section");
        var daySegment = section.Locator("[data-segment='day']");
        
        await daySegment.FocusAsync();
        await Page.Keyboard.PressAsync("Backspace");
        
        await Expect(daySegment).ToHaveAttributeAsync("data-placeholder", "");
        await Expect(daySegment).ToHaveTextAsync("dd");
    }

    [Test]
    public async Task MinMaxValidation_ShouldShowError()
    {
        var section = Page.GetByTestId("minmax-section");
        var yearSegment = section.Locator("[data-segment='year']");
        
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2024"); // Out of range (min 2025)
        
        await Expect(Page.GetByTestId("minmax-error")).ToBeVisibleAsync();
    }
}
