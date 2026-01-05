using TUnit.Playwright;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// Accessibility tests for the DateField component.
/// Tests ARIA attributes, keyboard navigation, segment behavior, and focus management.
/// </summary>
public class DateFieldAccessibilityTests : PageTest
{
    private const string DateFieldDemoUrl = "datefield";

    [Before(Test)]
    public async Task NavigateToDateFieldDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + DateFieldDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    #region ARIA Attributes - Spinbutton Role

    [Test]
    public async Task Segment_ShouldHave_RoleSpinbutton()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("role", "spinbutton");
    }

    [Test]
    public async Task Segment_ShouldHave_AriaLabel()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        var ariaLabel = await daySegment.GetAttributeAsync("aria-label");
        
        await Assert.That(ariaLabel).IsNotNull();
        await Assert.That(ariaLabel!.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task DaySegment_ShouldHave_AriaValuemin()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("aria-valuemin", "1");
    }

    [Test]
    public async Task DaySegment_ShouldHave_AriaValuemax()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='day']");
        var maxValue = await segment.GetAttributeAsync("aria-valuemax");
        
        // Max depends on month (28-31), just verify it exists and is reasonable
        await Assert.That(maxValue).IsNotNull();
        var max = int.Parse(maxValue!);
        await Assert.That(max).IsGreaterThanOrEqualTo(28);
        await Assert.That(max).IsLessThanOrEqualTo(31);
    }

    [Test]
    public async Task MonthSegment_ShouldHave_AriaValuemin()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='month']");
        await Expect(segment).ToHaveAttributeAsync("aria-valuemin", "1");
    }

    [Test]
    public async Task MonthSegment_ShouldHave_AriaValuemax()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='month']");
        await Expect(segment).ToHaveAttributeAsync("aria-valuemax", "12");
    }

    [Test]
    public async Task YearSegment_ShouldHave_AriaValuemin()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='year']");
        await Expect(segment).ToHaveAttributeAsync("aria-valuemin", "1");
    }

    [Test]
    public async Task YearSegment_ShouldHave_AriaValuemax()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='year']");
        await Expect(segment).ToHaveAttributeAsync("aria-valuemax", "9999");
    }

    [Test]
    public async Task Segment_ShouldHave_AriaValuenow()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        var valueNow = await daySegment.GetAttributeAsync("aria-valuenow");
        
        await Assert.That(valueNow).IsNotNull();
        var value = int.Parse(valueNow!);
        await Assert.That(value).IsGreaterThanOrEqualTo(1);
        await Assert.That(value).IsLessThanOrEqualTo(31);
    }

    [Test]
    public async Task Segment_ShouldHave_AriaValuetext()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        var valueText = await daySegment.GetAttributeAsync("aria-valuetext");
        
        await Assert.That(valueText).IsNotNull();
        await Assert.That(valueText!.Length).IsGreaterThan(0);
    }

    #endregion

    #region ARIA Attributes - Literal Segments

    [Test]
    public async Task LiteralSegment_ShouldHave_AriaHidden()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var literal = section.Locator("[data-segment='literal']").First;
        await Expect(literal).ToHaveAttributeAsync("aria-hidden", "true");
    }

    #endregion

    #region Contenteditable Attributes

    [Test]
    public async Task Segment_ShouldHave_InputmodeNumeric()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("inputmode", "numeric");
    }

    [Test]
    public async Task Segment_ShouldHave_Tabindex0()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("tabindex", "0");
    }

    #endregion

    #region Time Segments

    [Test]
    public async Task HourSegment_ShouldHave_RoleSpinbutton()
    {
        var section = Page.Locator("[data-testid='datetime-section']");
        var segment = section.Locator("[data-segment='hour']");
        await Expect(segment).ToHaveAttributeAsync("role", "spinbutton");
    }

    [Test]
    public async Task MinuteSegment_ShouldHave_RoleSpinbutton()
    {
        var section = Page.Locator("[data-testid='datetime-section']");
        var segment = section.Locator("[data-segment='minute']");
        await Expect(segment).ToHaveAttributeAsync("role", "spinbutton");
    }

    [Test]
    public async Task MinuteSegment_ShouldHave_AriaValuemax59()
    {
        var section = Page.Locator("[data-testid='datetime-section']");
        var segment = section.Locator("[data-segment='minute']");
        await Expect(segment).ToHaveAttributeAsync("aria-valuemax", "59");
    }

    #endregion

    #region Disabled State

    [Test]
    public async Task DisabledSegment_ShouldHave_DataDisabled()
    {
        var section = Page.Locator("[data-testid='disabled-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledSegment_ShouldHave_TabindexNegative1()
    {
        var section = Page.Locator("[data-testid='disabled-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task DisabledSegment_ShouldNotBeFocusable_ViaTab()
    {
        var section = Page.Locator("[data-testid='disabled-section']");
        var segment = section.Locator("[data-segment='day']");
        
        // Try to tab into the disabled segment - it should be skipped due to tabindex=-1
        // Focus the page body first
        await Page.Locator("body").FocusAsync();
        
        // Tab multiple times and verify the disabled segment never gets focus
        for (var i = 0; i < 5; i++)
        {
            await Page.Keyboard.PressAsync("Tab");
            var focused = Page.Locator(":focus");
            var focusedTestId = await focused.GetAttributeAsync("data-testid");
            
            // If we're in the disabled section, check we didn't focus the segment
            if (focusedTestId?.Contains("disabled") == true)
            {
                await Expect(segment).Not.ToBeFocusedAsync();
            }
        }
    }

    #endregion

    #region ReadOnly State

    [Test]
    public async Task ReadOnlySegment_ShouldHave_DataReadonly()
    {
        var section = Page.Locator("[data-testid='readonly-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("data-readonly", "");
    }

    #endregion

    #region Invalid State

    [Test]
    public async Task InvalidSection_ShouldHave_DataInvalid()
    {
        var section = Page.Locator("[data-testid='invalid-section']");
        var root = section.Locator("[role='group']");
        await Expect(root).ToHaveAttributeAsync("data-invalid", "");
    }

    #endregion

    #region Placeholder State

    [Test]
    public async Task PlaceholderSegment_ShouldHave_DataPlaceholder()
    {
        var section = Page.Locator("[data-testid='placeholder-section']");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("data-placeholder", "");
    }

    [Test]
    public async Task PlaceholderSegment_ShouldNotHave_AriaValuenow()
    {
        var section = Page.Locator("[data-testid='placeholder-section']");
        var segment = section.Locator("[data-segment='day']");
        
        // When showing placeholder, aria-valuenow should be omitted
        var valueNow = await segment.GetAttributeAsync("aria-valuenow");
        await Assert.That(valueNow).IsNull();
    }

    #endregion

    #region Keyboard Navigation - Arrow Keys

    [Test]
    public async Task ArrowUp_ShouldIncrement_DaySegment()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='day']");
        
        var initialValue = await segment.GetAttributeAsync("aria-valuenow");
        
        await segment.FocusAsync();
        // Wait for JS initialization
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowUp");
        // Wait for Blazor to re-render
        await Page.WaitForTimeoutAsync(100);
        
        var newValue = await segment.GetAttributeAsync("aria-valuenow");
        await Assert.That(int.Parse(newValue!)).IsNotEqualTo(int.Parse(initialValue!));
    }

    [Test]
    public async Task ArrowDown_ShouldDecrement_DaySegment()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segment = section.Locator("[data-segment='day']");
        
        var initialValue = await segment.GetAttributeAsync("aria-valuenow");
        
        await segment.FocusAsync();
        // Wait for JS initialization
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("ArrowDown");
        // Wait for Blazor to re-render
        await Page.WaitForTimeoutAsync(100);
        
        var newValue = await segment.GetAttributeAsync("aria-valuenow");
        await Assert.That(int.Parse(newValue!)).IsNotEqualTo(int.Parse(initialValue!));
    }

    [Test]
    public async Task ArrowRight_ShouldFocus_NextSegment()
    {
        var section = Page.Locator("[data-testid='swedish-section']");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        
        await yearSegment.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        
        await Expect(monthSegment).ToBeFocusedAsync();
    }

    [Test]
    public async Task ArrowLeft_ShouldFocus_PreviousSegment()
    {
        var section = Page.Locator("[data-testid='swedish-section']");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        
        await monthSegment.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowLeft");
        
        await Expect(yearSegment).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tab_ShouldNavigate_BetweenSegments()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segments = section.Locator("[role='spinbutton']");
        
        var firstSegment = segments.First;
        await firstSegment.FocusAsync();
        await Expect(firstSegment).ToBeFocusedAsync();
        
        await Page.Keyboard.PressAsync("Tab");
        
        // The next focusable element should be focused (second segment or outside)
        await Expect(firstSegment).Not.ToBeFocusedAsync();
    }

    #endregion

    #region Keyboard Navigation - Numeric Input

    [Test]
    public async Task NumericInput_ShouldUpdate_SegmentValue()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("15");
        
        await Expect(daySegment).ToHaveAttributeAsync("aria-valuenow", "15");
    }

    [Test]
    public async Task NumericInput_ShouldAutoAdvance_OnCompleteValue()
    {
        var section = Page.Locator("[data-testid='swedish-section']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");
        
        await monthSegment.FocusAsync();
        // Typing "12" for month should auto-advance to next segment
        await Page.Keyboard.TypeAsync("12");
        
        // Wait a bit for focus to move
        await Page.WaitForTimeoutAsync(100);
        
        // Focus should move to day segment after complete 2-digit month
        await Expect(daySegment).ToBeFocusedAsync();
    }

    #endregion

    #region Keyboard Navigation - Backspace/Delete

    [Test]
    public async Task Backspace_ShouldClear_Segment()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        
        // First verify segment has a value
        var initialValue = await daySegment.GetAttributeAsync("aria-valuenow");
        await Assert.That(initialValue).IsNotNull();
        
        await daySegment.FocusAsync();
        await Page.Keyboard.PressAsync("Backspace");
        
        // After backspace, the segment should show placeholder state
        await Expect(daySegment).ToHaveAttributeAsync("data-placeholder", "");
    }

    [Test]
    public async Task Backspace_ShouldClearOnlyFocusedSegment()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        var monthSegment = section.Locator("[data-segment='month']");
        var yearSegment = section.Locator("[data-segment='year']");
        
        // Verify initial state - all segments have values (no placeholder attribute)
        await Expect(daySegment).Not.ToHaveAttributeAsync("data-placeholder", "");
        await Expect(monthSegment).Not.ToHaveAttributeAsync("data-placeholder", "");
        await Expect(yearSegment).Not.ToHaveAttributeAsync("data-placeholder", "");
        
        // Press backspace on day segment
        await daySegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("Backspace");
        await Page.WaitForTimeoutAsync(100);
        
        // Day should show placeholder, others should retain values
        await Expect(daySegment).ToHaveAttributeAsync("data-placeholder", "");
        await Expect(monthSegment).Not.ToHaveAttributeAsync("data-placeholder", "");
        await Expect(yearSegment).Not.ToHaveAttributeAsync("data-placeholder", "");
        
        // Day should show "dd" placeholder text
        var dayText = await daySegment.TextContentAsync();
        await Assert.That(dayText).IsEqualTo("dd");
    }

    [Test]
    public async Task Delete_ShouldClear_Segment()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        
        await daySegment.FocusAsync();
        await Page.Keyboard.PressAsync("Delete");
        
        // After delete, the segment should show placeholder state
        await Expect(daySegment).ToHaveAttributeAsync("data-placeholder", "");
    }

    #endregion

    #region Placeholder Format Display

    [Test]
    public async Task NullValue_ShouldShowPlaceholderFormat()
    {
        var section = Page.Locator("[data-testid='placeholder-section']");
        var daySegment = section.Locator("[data-segment='day']");
        var monthSegment = section.Locator("[data-segment='month']");
        var yearSegment = section.Locator("[data-segment='year']");
        
        // All segments should show placeholder format text
        await Assert.That(await daySegment.TextContentAsync()).IsEqualTo("dd");
        await Assert.That(await monthSegment.TextContentAsync()).IsEqualTo("mm");
        await Assert.That(await yearSegment.TextContentAsync()).IsEqualTo("yyyy");
    }

    [Test]
    public async Task PartialEntry_ShouldShowMixedState()
    {
        var section = Page.Locator("[data-testid='placeholder-section']");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");
        
        // Type year only
        await yearSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("2025");
        await Page.WaitForTimeoutAsync(100);
        
        // Year should have value, month and day should show placeholders
        await Assert.That(await yearSegment.TextContentAsync()).IsEqualTo("2025");
        await Assert.That(await monthSegment.TextContentAsync()).IsEqualTo("mm");
        await Assert.That(await daySegment.TextContentAsync()).IsEqualTo("dd");
        
        // Year should not have placeholder attribute, others should
        await Expect(yearSegment).Not.ToHaveAttributeAsync("data-placeholder", "");
        await Expect(monthSegment).ToHaveAttributeAsync("data-placeholder", "");
        await Expect(daySegment).ToHaveAttributeAsync("data-placeholder", "");
        
        // Bound value should still be null
        var valueDisplay = Page.Locator("[data-testid='placeholder-value']");
        await Expect(valueDisplay).ToContainTextAsync("None");
    }

    [Test]
    public async Task AllSegmentsFilled_ShouldSetBoundValue()
    {
        var section = Page.Locator("[data-testid='placeholder-section']");
        var yearSegment = section.Locator("[data-segment='year']");
        var monthSegment = section.Locator("[data-segment='month']");
        var daySegment = section.Locator("[data-segment='day']");
        
        // Fill all segments
        await yearSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("2025");
        await monthSegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("06");
        await daySegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.TypeAsync("15");
        
        await Page.WaitForTimeoutAsync(200);
        
        // Bound value should now be set
        var valueDisplay = Page.Locator("[data-testid='placeholder-value']");
        await Expect(valueDisplay).ToContainTextAsync("2025-06-15");
    }

    [Test]
    public async Task ClearedSegment_CanBeReentered()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        
        // Clear the segment
        await daySegment.FocusAsync();
        await Page.WaitForTimeoutAsync(100);
        await Page.Keyboard.PressAsync("Backspace");
        await Page.WaitForTimeoutAsync(100);
        
        // Should show placeholder
        await Expect(daySegment).ToHaveAttributeAsync("data-placeholder", "");
        await Assert.That(await daySegment.TextContentAsync()).IsEqualTo("dd");
        
        // Re-enter a value
        await Page.Keyboard.TypeAsync("25");
        await Page.WaitForTimeoutAsync(100);
        
        // Should now show the value
        await Expect(daySegment).Not.ToHaveAttributeAsync("data-placeholder", "");
        await Assert.That(await daySegment.TextContentAsync()).IsEqualTo("25");
    }

    #endregion

    #region Value Binding

    [Test]
    public async Task ValueChange_ShouldUpdate_DisplayedValue()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        
        await daySegment.FocusAsync();
        await Page.Keyboard.TypeAsync("25");
        
        // The display value should show 25
        await Page.WaitForTimeoutAsync(100);
        var displayedValue = Page.Locator("[data-testid='basic-value']");
        var text = await displayedValue.TextContentAsync();
        
        await Assert.That(text!).Contains("25");
    }

    #endregion

    #region Locale-Specific Tests

    [Test]
    public async Task SwedishLocale_ShouldShow_YearFirst()
    {
        var section = Page.Locator("[data-testid='swedish-section']");
        var input = section.Locator(".datefield-input");
        var segments = input.Locator("[role='spinbutton']");
        
        // Swedish format is yyyy-MM-dd, so year should be first
        var firstSegment = segments.First;
        await Expect(firstSegment).ToHaveAttributeAsync("data-segment", "year");
    }

    [Test]
    public async Task USLocale_ShouldShow_MonthFirst()
    {
        var section = Page.Locator("[data-testid='us-section']");
        var input = section.Locator(".datefield-input");
        var segments = input.Locator("[role='spinbutton']");
        
        // US format is M/d/yyyy, so month should be first
        var firstSegment = segments.First;
        await Expect(firstSegment).ToHaveAttributeAsync("data-segment", "month");
    }

    [Test]
    public async Task JapaneseLocale_ShouldShow_YearFirst()
    {
        var section = Page.Locator("[data-testid='japanese-section']");
        var input = section.Locator(".datefield-input");
        var segments = input.Locator("[role='spinbutton']");
        
        // Japanese format is yyyy/MM/dd, so year should be first
        var firstSegment = segments.First;
        await Expect(firstSegment).ToHaveAttributeAsync("data-segment", "year");
    }

    [Test]
    public async Task SwedishLocale_ShouldHave_SwedishLabel()
    {
        var section = Page.Locator("[data-testid='swedish-section']");
        var daySegment = section.Locator("[data-segment='day']");
        var ariaLabel = await daySegment.GetAttributeAsync("aria-label");
        
        // Swedish label for day should be "dag"
        await Assert.That(ariaLabel!.ToLowerInvariant()).IsEqualTo("dag");
    }

    #endregion

    #region Min/Max Validation

    [Test]
    public async Task MinMaxSection_ShouldShow_InvalidState_WhenOutOfRange()
    {
        var section = Page.Locator("[data-testid='minmax-section']");
        var yearSegment = section.Locator("[data-segment='year']");
        
        // Set year to 2024 which is before min date (2025-01-01)
        await yearSegment.FocusAsync();
        await Page.Keyboard.TypeAsync("2024");
        
        await Page.WaitForTimeoutAsync(100);
        
        // Error message should be visible
        var errorMessage = Page.Locator("[data-testid='minmax-error']");
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    #endregion

    #region Focus Management

    [Test]
    public async Task Click_ShouldFocus_Segment()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var daySegment = section.Locator("[data-segment='day']");
        
        await daySegment.ClickAsync();
        
        await Expect(daySegment).ToBeFocusedAsync();
    }

    [Test]
    public async Task AllSegments_ShouldBe_Focusable()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var segments = section.Locator("[role='spinbutton']");
        var count = await segments.CountAsync();
        
        for (var i = 0; i < count; i++)
        {
            var segment = segments.Nth(i);
            await segment.FocusAsync();
            await Expect(segment).ToBeFocusedAsync();
        }
    }

    #endregion

    #region Label Association

    [Test]
    public async Task DateFieldLabel_ShouldExist()
    {
        var section = Page.Locator("[data-testid='basic-section']");
        var label = section.Locator(".datefield-label");
        
        await Expect(label).ToBeVisibleAsync();
        await Expect(label).ToHaveTextAsync("Select a date");
    }

    #endregion
}
