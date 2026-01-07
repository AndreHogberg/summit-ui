namespace SummitUI.Tests.Playwright.Switch;

/// <summary>
/// Tests for Switch form integration and keyboard interaction on disabled switches.
/// </summary>
public class SwitchFormTests : SummitTestBase
{
    protected override string TestPagePath => "tests/switch/form";

    #region Form Integration

    [Test]
    public async Task Switch_ShouldSupport_HiddenInput()
    {
        var formSwitch = Page.Locator("input[name='notifications']");
        await Expect(formSwitch).ToHaveCountAsync(1);
        await Expect(formSwitch).ToHaveAttributeAsync("type", "hidden");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHaveEmptyValue_WhenUnchecked()
    {
        var hiddenInput = Page.Locator("input[name='notifications']");
        await Expect(hiddenInput).ToHaveAttributeAsync("value", "");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHaveOnValue_WhenChecked()
    {
        var switchEl = Page.GetByTestId("notifications-switch");
        var hiddenInput = Page.Locator("input[name='notifications']");

        // Toggle the switch on
        await switchEl.ClickAsync();

        // Default value is "on" when no custom Value is set
        await Expect(hiddenInput).ToHaveAttributeAsync("value", "on");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHaveCustomValue_WhenChecked()
    {
        var switchEl = Page.GetByTestId("marketing-switch");
        var hiddenInput = Page.Locator("input[name='marketing']");

        // Toggle the switch on
        await switchEl.ClickAsync();

        // Marketing switch has Value="yes"
        await Expect(hiddenInput).ToHaveAttributeAsync("value", "yes");
    }

    [Test]
    public async Task Switch_HiddenInput_ShouldHave_RequiredAttribute()
    {
        var hiddenInput = Page.Locator("input[name='terms']");
        await Expect(hiddenInput).ToHaveAttributeAsync("required", "");
    }

    #endregion
}
