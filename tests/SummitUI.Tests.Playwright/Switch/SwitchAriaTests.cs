namespace SummitUI.Tests.Playwright.Switch;

/// <summary>
/// Tests for Switch ARIA attributes and accessibility.
/// </summary>
public class SwitchAriaTests : SummitTestBase
{
    protected override string TestPagePath => "tests/switch/basic";

    [Test]
    public async Task Switch_ShouldHave_RoleSwitch()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("role", "switch");
    }

    [Test]
    public async Task Switch_ShouldHave_TypeButton()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task Switch_ShouldHave_AriaCheckedFalse_WhenUnchecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Switch_ShouldHave_AriaCheckedTrue_WhenChecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        await switchEl.ClickAsync();
        
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Switch_ShouldHave_DataStateUnchecked_WhenUnchecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Switch_ShouldHave_DataStateChecked_WhenChecked()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        await switchEl.ClickAsync();
        
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "checked");
    }
}
