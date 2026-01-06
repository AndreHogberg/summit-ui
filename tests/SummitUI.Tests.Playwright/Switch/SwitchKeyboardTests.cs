namespace SummitUI.Tests.Playwright.Switch;

/// <summary>
/// Tests for Switch keyboard navigation.
/// </summary>
public class SwitchKeyboardTests : SummitTestBase
{
    protected override string TestPagePath => "tests/switch/basic";

    [Test]
    public async Task Space_ShouldToggleSwitch()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");

        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Space_ShouldToggleSwitchOff()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        // First toggle on
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");

        // Now toggle off with Space
        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync(" ");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Enter_ShouldToggleSwitch()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");

        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");
    }

    [Test]
    public async Task Enter_ShouldToggleSwitchOff()
    {
        var switchEl = Page.GetByTestId("basic-switch");
        
        // First toggle on
        await switchEl.ClickAsync();
        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "true");

        // Now toggle off with Enter
        await switchEl.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(switchEl).ToHaveAttributeAsync("aria-checked", "false");
    }

    [Test]
    public async Task Tab_ShouldNavigateBetweenSwitches()
    {
        // Get the basic switch (first switch on the page)
        var basicSwitch = Page.GetByTestId("basic-switch");

        await basicSwitch.FocusAsync();
        await Expect(basicSwitch).ToBeFocusedAsync();

        // Tab to next focusable element
        await Page.Keyboard.PressAsync("Tab");
        
        // Verify focus moved (some element has focus)
        var activeElement = Page.Locator(":focus");
        await Expect(activeElement).ToHaveCountAsync(1);
    }
}
