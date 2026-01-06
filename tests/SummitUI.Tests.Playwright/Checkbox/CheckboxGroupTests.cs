namespace SummitUI.Tests.Playwright.Checkbox;

/// <summary>
/// Tests for Checkbox Group behavior.
/// Verifies group accessibility, value management, and disabled groups.
/// </summary>
public class CheckboxGroupTests : SummitTestBase
{
    protected override string TestPagePath => "tests/checkbox/group";

    #region Group Accessibility

    [Test]
    public async Task CheckboxGroup_ShouldHave_RoleGroup()
    {
        var group = Page.GetByTestId("basic-group");
        await Expect(group).ToHaveAttributeAsync("role", "group");
    }

    [Test]
    public async Task CheckboxGroup_ShouldHave_AriaLabelledby()
    {
        var group = Page.GetByTestId("basic-group");
        var ariaLabelledby = await group.GetAttributeAsync("aria-labelledby");

        await Assert.That(ariaLabelledby).IsNotNull();

        // Verify the label element exists with that ID
        var label = Page.Locator($"#{ariaLabelledby}");
        await Expect(label).ToHaveCountAsync(1);
    }

    [Test]
    public async Task CheckboxGroupLabel_ShouldHave_MatchingId()
    {
        var group = Page.GetByTestId("basic-group");
        var ariaLabelledby = await group.GetAttributeAsync("aria-labelledby");

        var label = Page.GetByTestId("group-label");
        var labelId = await label.GetAttributeAsync("id");

        await Assert.That(ariaLabelledby).IsEqualTo(labelId);
    }

    #endregion

    #region Group Value Management

    [Test]
    public async Task CheckboxGroup_ShouldHaveDefaultValues()
    {
        // Feature 1 and Feature 3 are checked by default
        var feature1Checkbox = Page.GetByTestId("feature-1-checkbox");
        var feature2Checkbox = Page.GetByTestId("feature-2-checkbox");
        var feature3Checkbox = Page.GetByTestId("feature-3-checkbox");

        await Expect(feature1Checkbox).ToHaveAttributeAsync("data-state", "checked");
        await Expect(feature2Checkbox).ToHaveAttributeAsync("data-state", "unchecked");
        await Expect(feature3Checkbox).ToHaveAttributeAsync("data-state", "checked");
    }

    [Test]
    public async Task CheckboxGroup_ShouldManageValues()
    {
        var feature1Checkbox = Page.GetByTestId("feature-1-checkbox");
        var feature2Checkbox = Page.GetByTestId("feature-2-checkbox");

        await Expect(feature1Checkbox).ToHaveAttributeAsync("data-state", "checked");
        await Expect(feature2Checkbox).ToHaveAttributeAsync("data-state", "unchecked");

        // Click feature 2 to check it
        await feature2Checkbox.ClickAsync();
        await Expect(feature2Checkbox).ToHaveAttributeAsync("data-state", "checked");

        // Click feature 1 to uncheck it
        await feature1Checkbox.ClickAsync();
        await Expect(feature1Checkbox).ToHaveAttributeAsync("data-state", "unchecked");
    }

    #endregion

    #region Disabled Group

    [Test]
    public async Task DisabledGroup_ShouldHave_AriaDisabled()
    {
        var group = Page.GetByTestId("disabled-group");
        await Expect(group).ToHaveAttributeAsync("aria-disabled", "true");
    }

    [Test]
    public async Task DisabledGroup_ShouldHave_DataDisabled()
    {
        var group = Page.GetByTestId("disabled-group");
        await Expect(group).ToHaveAttributeAsync("data-disabled", "");
    }

    [Test]
    public async Task DisabledGroup_ShouldDisable_AllCheckboxes()
    {
        var group = Page.GetByTestId("disabled-group");
        var checkboxes = group.Locator("[data-summit-checkbox]");
        var count = await checkboxes.CountAsync();

        for (var i = 0; i < count; i++)
        {
            await Expect(checkboxes.Nth(i)).ToBeDisabledAsync();
        }
    }

    [Test]
    public async Task DisabledGroup_Checkboxes_ShouldNotToggle()
    {
        var checkbox = Page.GetByTestId("setting-1-checkbox");
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");

        // Force click on disabled checkbox
        await checkbox.ClickAsync(new() { Force = true });

        // Should still be checked
        await Expect(checkbox).ToHaveAttributeAsync("data-state", "checked");
    }

    #endregion
}
