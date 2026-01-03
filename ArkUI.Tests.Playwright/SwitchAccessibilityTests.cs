using TUnit.Playwright;

namespace ArkUI.Tests.Playwright;

public class SwitchAccessibilityTests : PageTest
{
    private const string SwitchDemoUrl = "switch";

    [Before(Test)]
    public async Task NavigateToSwitchDemo()
    {
        await Page.GotoAsync(Hooks.ServerUrl + SwitchDemoUrl);
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
    }

    [Test]
    public async Task Switch_ShouldHave_RoleSwitch()
    {
        var switchEl = Page.Locator(".switch-root").First;
        await Expect(switchEl).ToHaveAttributeAsync("role", "switch");
    }

    [Test]
    public async Task Switch_ShouldHave_TypeButton()
    {
        var switchEl = Page.Locator(".switch-root").First;
        await Expect(switchEl).ToHaveAttributeAsync("type", "button");
    }

    [Test]
    public async Task Switch_ShouldToggle_OnClick()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        // Initial state
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");

        // Toggle on
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "checked");

        // Toggle off
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
        await Expect(switchEl).ToHaveAttributeAsync("data-state", "unchecked");
    }

    [Test]
    public async Task Switch_ShouldHave_DataState_OnThumb()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        var thumb = switchEl.Locator(".switch-thumb");

        await Expect(thumb).ToHaveAttributeAsync("data-state", "unchecked");
        
        await switchEl.ClickAsync();
        
        await Expect(thumb).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task DisabledSwitch_ShouldHave_DisabledAttributes()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        
        await Expect(disabledSwitch).ToBeDisabledAsync();
        await Expect(disabledSwitch).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledSwitch_ShouldNotToggle()
    {
        var disabledSwitch = Page.GetByTestId("disabled-checked-switch");
        var initialState = await disabledSwitch.GetAttributeAsync("aria-checked");
        
        await disabledSwitch.ClickAsync(new() { Force = true });
        
        await Expect(disabledSwitch).ToHaveAttributeAsync("aria-checked", initialState!);
    }

    [Test]
    public async Task Switch_ShouldSupport_HiddenInput()
    {
        var formSwitch = Page.Locator("input[name='notifications']");
        await Expect(formSwitch).ToHaveCountAsync(1);
        await Expect(formSwitch).ToHaveAttributeAsync("type", "hidden");
    }
}
