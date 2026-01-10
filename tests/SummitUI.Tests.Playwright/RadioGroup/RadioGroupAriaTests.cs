namespace SummitUI.Tests.Playwright.RadioGroup;

/// <summary>
/// Tests for RadioGroup ARIA attributes and accessibility.
/// </summary>
public class RadioGroupAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/radiogroup/basic";

    [Test]
    public async Task RadioGroup_ShouldHave_RoleRadiogroup()
    {
        var radioGroup = Page.GetByTestId("basic-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("role", "radiogroup");
    }

    [Test]
    public async Task RadioGroup_ShouldHave_AriaLabel()
    {
        var radioGroup = Page.GetByTestId("basic-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("aria-label", "Select a plan");
    }

    [Test]
    public async Task RadioGroup_ShouldHave_AriaOrientationVertical_ByDefault()
    {
        var radioGroup = Page.GetByTestId("basic-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("aria-orientation", "vertical");
    }

    [Test]
    public async Task RadioGroup_ShouldHave_AriaOrientationHorizontal_WhenSet()
    {
        var radioGroup = Page.GetByTestId("horizontal-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("aria-orientation", "horizontal");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_RoleRadio()
    {
        var radioItem = Page.GetByTestId("radio-starter");
        await Expect(radioItem).ToHaveAttributeAsync("role", "radio");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_TypeButton()
    {
        var radioItem = Page.GetByTestId("radio-starter");
        await Expect(radioItem).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_AriaCheckedFalse_WhenUnchecked()
    {
        var radioItem = Page.GetByTestId("radio-starter");
        await Expect(radioItem).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_AriaCheckedTrue_WhenSelected()
    {
        var radioItem = Page.GetByTestId("radio-starter");

        await radioItem.ClickAsync();

        await Expect(radioItem).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_DataStateUnchecked_WhenUnchecked()
    {
        var radioItem = Page.GetByTestId("radio-starter");
        await Expect(radioItem).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_DataStateChecked_WhenSelected()
    {
        var radioItem = Page.GetByTestId("radio-starter");

        await radioItem.ClickAsync();

        await Expect(radioItem).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_DataOrientationAttribute()
    {
        var radioItem = Page.GetByTestId("radio-starter");
        await Expect(radioItem).ToHaveAttributeAsync("data-orientation", "vertical");
    }

    [Test]
    public async Task RadioGroupItem_ShouldHave_DataSummitAttribute()
    {
        var radioItem = Page.GetByTestId("radio-starter");
        await Expect(radioItem).ToHaveAttributeAsync("data-summit-radio-group-item", "");
    }

    [Test]
    public async Task RadioGroup_ShouldHave_DataSummitAttribute()
    {
        var radioGroup = Page.GetByTestId("basic-radio-group");
        await Expect(radioGroup).ToHaveAttributeAsync("data-summit-radio-group", "");
    }
}
