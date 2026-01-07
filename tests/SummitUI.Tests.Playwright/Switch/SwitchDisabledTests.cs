namespace SummitUI.Tests.Playwright.Switch;

/// <summary>
/// Tests for disabled Switch behavior.
/// </summary>
public class SwitchDisabledTests : SummitTestBase
{
    protected override string TestPagePath => "tests/switch/disabled";

    [Test]
    public async Task DisabledSwitch_ShouldHave_DisabledAttribute()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToBeDisabledAsync();
    }

    [Test]
    public async Task DisabledSwitch_ShouldHave_AriaDisabled()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledSwitch_ShouldHave_DataDisabled()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledCheckedSwitch_ShouldNotToggle_OnClick()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "true");

        await disabledSwitch.ClickAsync(new() { Force = true });

        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task DisabledUncheckedSwitch_ShouldHave_DisabledAttributes()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");

        await Expect(disabledSwitch).ToBeDisabledAsync();
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabledSwitch).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledUncheckedSwitch_ShouldNotToggle_OnClick()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");

        await disabledSwitch.ClickAsync(new() { Force = true });

        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Enter_ShouldNotToggle_DisabledSwitch()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");

        await disabledSwitch.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Space_ShouldNotToggle_DisabledSwitch()
    {
        var disabledSwitch = Page.GetByTestId("disabled-unchecked-switch");
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");

        await disabledSwitch.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", "false");
    }
}
