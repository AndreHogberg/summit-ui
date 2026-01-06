using Microsoft.Playwright;
using TUnit.Playwright;

namespace SummitUI.Tests.Playwright.DateField;

public class DateFieldAccessibilityTests : SummitTestBase
{
    protected override string TestPagePath => "tests/date-field/basic";

    [Test]
    public async Task Segment_ShouldHave_AriaAttributes()
    {
        var section = Page.GetByTestId("basic-section");
        var daySegment = section.Locator("[data-segment='day']");
        
        await Expect(daySegment).ToHaveAttributeAsync("role", "spinbutton");
        await Expect(daySegment).ToHaveAttributeAsync("aria-valuemin", "1");
        
        var ariaLabel = await daySegment.GetAttributeAsync("aria-label");
        await Assert.That(ariaLabel).IsNotNull();
        await Assert.That(ariaLabel!.Length).IsGreaterThan(0);
        
        var valueNow = await daySegment.GetAttributeAsync("aria-valuenow");
        await Assert.That(valueNow).IsEqualTo("15");
    }

    [Test]
    public async Task LiteralSegment_ShouldHave_AriaHidden()
    {
        var section = Page.GetByTestId("basic-section");
        var literal = section.Locator("[data-segment='literal']").First;
        await Expect(literal).ToHaveAttributeAsync("aria-hidden", "true");
    }

    [Test]
    public async Task TimeSegments_ShouldHave_ProperAttributes()
    {
        var section = Page.GetByTestId("datetime-section");
        var hourSegment = section.Locator("[data-segment='hour']");
        var minuteSegment = section.Locator("[data-segment='minute']");
        
        await Expect(hourSegment).ToHaveAttributeAsync("role", "spinbutton");
        await Expect(minuteSegment).ToHaveAttributeAsync("role", "spinbutton");
        await Expect(minuteSegment).ToHaveAttributeAsync("aria-valuemax", "59");
    }

    [Test]
    public async Task DisabledState_ShouldHave_ProperAttributes()
    {
        var section = Page.GetByTestId("disabled-section");
        var segment = section.Locator("[data-segment='day']");
        
        await Expect(segment).ToHaveAttributeAsync("data-disabled", "");
        await Expect(segment).ToHaveAttributeAsync("tabindex", "-1");
    }

    [Test]
    public async Task ReadOnlyState_ShouldHave_ProperAttributes()
    {
        var section = Page.GetByTestId("readonly-section");
        var segment = section.Locator("[data-segment='day']");
        await Expect(segment).ToHaveAttributeAsync("data-readonly", "");
    }

    [Test]
    public async Task PlaceholderState_ShouldHave_ProperAttributes()
    {
        var section = Page.GetByTestId("placeholder-section");
        var segment = section.Locator("[data-segment='day']");
        
        await Expect(segment).ToHaveAttributeAsync("data-placeholder", "");
        var valueNow = await segment.GetAttributeAsync("aria-valuenow");
        await Assert.That(valueNow).IsNull();
        await Assert.That(await segment.TextContentAsync()).IsEqualTo("dd");
    }
}
