using Microsoft.Extensions.DependencyInjection;

using SummitUI;
using SummitUI.Extensions;

namespace SummitUI.Tests.Unit.Localization;

/// <summary>
/// Tests for custom <see cref="ISummitUILocalizer"/> implementations.
/// Verifies that users can override the default localizer with their own translations.
/// </summary>
public class CustomLocalizerTests
{
    /// <summary>
    /// Example Swedish localizer implementation for testing.
    /// </summary>
    private sealed class SwedishLocalizer : ISummitUILocalizer
    {
        private readonly Dictionary<string, string> _translations = new()
        {
            ["Dialog_CloseLabel"] = "Stäng dialogruta",
            ["Popover_CloseLabel"] = "Stäng",
            ["Combobox_ClearAllLabel"] = "Rensa alla val",
            ["Calendar_NextMonthLabel"] = "Nästa månad",
            ["Calendar_PreviousMonthLabel"] = "Föregående månad",
            ["Calendar_GridInstructions"] = "Använd piltangenterna för att navigera, Enter eller Mellanslag för att välja",
            ["Calendar_DateSelectedAnnouncement"] = "{0} vald",
            ["DateField_YearLabel"] = "År",
            ["DateField_MonthLabel"] = "Månad",
            ["DateField_DayLabel"] = "Dag",
            ["DateField_HourLabel"] = "Timme",
            ["DateField_MinuteLabel"] = "Minut",
            ["DateField_DayPeriodLabel"] = "FM/EM"
        };

        public string this[string key] =>
            _translations.TryGetValue(key, out var value) ? value : key;

        public string this[string key, params object[] arguments] =>
            string.Format(this[key], arguments);
    }

    /// <summary>
    /// Partial localizer that only overrides some keys.
    /// </summary>
    private sealed class PartialLocalizer : ISummitUILocalizer
    {
        private readonly Dictionary<string, string> _translations = new()
        {
            ["Dialog_CloseLabel"] = "Custom Close"
            // Only overrides one key, others should fall back to key
        };

        public string this[string key] =>
            _translations.TryGetValue(key, out var value) ? value : key;

        public string this[string key, params object[] arguments] =>
            string.Format(this[key], arguments);
    }

    [Test]
    public async Task CustomLocalizer_CanBeRegisteredBeforeAddSummitUI()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register custom localizer BEFORE AddSummitUI
        services.AddSingleton<ISummitUILocalizer, SwedishLocalizer>();
        services.AddSummitUI();

        var provider = services.BuildServiceProvider();

        // Act
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();
        var result = localizer["Dialog_CloseLabel"];

        // Assert - Custom localizer takes precedence
        await Assert.That(localizer).IsTypeOf<SwedishLocalizer>();
        await Assert.That(result).IsEqualTo("Stäng dialogruta");
    }

    [Test]
    public async Task CustomLocalizer_CanBeRegisteredAfterAddSummitUI()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSummitUI();
        // Register custom localizer AFTER AddSummitUI (replaces default)
        services.AddSingleton<ISummitUILocalizer, SwedishLocalizer>();

        var provider = services.BuildServiceProvider();

        // Act
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();
        var result = localizer["Dialog_CloseLabel"];

        // Assert - Custom localizer takes precedence
        await Assert.That(localizer).IsTypeOf<SwedishLocalizer>();
        await Assert.That(result).IsEqualTo("Stäng dialogruta");
    }

    [Test]
    public async Task CustomLocalizer_ReturnsSwedishTranslations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISummitUILocalizer, SwedishLocalizer>();
        services.AddSummitUI();

        var provider = services.BuildServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act & Assert - Test all Swedish translations
        await Assert.That(localizer["Dialog_CloseLabel"]).IsEqualTo("Stäng dialogruta");
        await Assert.That(localizer["Popover_CloseLabel"]).IsEqualTo("Stäng");
        await Assert.That(localizer["Combobox_ClearAllLabel"]).IsEqualTo("Rensa alla val");
        await Assert.That(localizer["Calendar_NextMonthLabel"]).IsEqualTo("Nästa månad");
        await Assert.That(localizer["Calendar_PreviousMonthLabel"]).IsEqualTo("Föregående månad");
        await Assert.That(localizer["DateField_YearLabel"]).IsEqualTo("År");
        await Assert.That(localizer["DateField_MonthLabel"]).IsEqualTo("Månad");
        await Assert.That(localizer["DateField_DayLabel"]).IsEqualTo("Dag");
        await Assert.That(localizer["DateField_HourLabel"]).IsEqualTo("Timme");
        await Assert.That(localizer["DateField_MinuteLabel"]).IsEqualTo("Minut");
        await Assert.That(localizer["DateField_DayPeriodLabel"]).IsEqualTo("FM/EM");
    }

    [Test]
    public async Task CustomLocalizer_FormatsArguments()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISummitUILocalizer, SwedishLocalizer>();
        services.AddSummitUI();

        var provider = services.BuildServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Calendar_DateSelectedAnnouncement", "15 januari 2026"];

        // Assert
        await Assert.That(result).IsEqualTo("15 januari 2026 vald");
    }

    [Test]
    public async Task PartialLocalizer_ReturnsKeyForMissingTranslations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISummitUILocalizer, PartialLocalizer>();
        services.AddSummitUI();

        var provider = services.BuildServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act & Assert
        await Assert.That(localizer["Dialog_CloseLabel"]).IsEqualTo("Custom Close");
        await Assert.That(localizer["Popover_CloseLabel"]).IsEqualTo("Popover_CloseLabel"); // Falls back to key
        await Assert.That(localizer["Unknown_Key"]).IsEqualTo("Unknown_Key"); // Unknown also returns key
    }

    [Test]
    public async Task CustomLocalizer_ImplementsIndexerWithKey()
    {
        // Arrange
        ISummitUILocalizer localizer = new SwedishLocalizer();

        // Act
        var result = localizer["Dialog_CloseLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Stäng dialogruta");
    }

    [Test]
    public async Task CustomLocalizer_ImplementsIndexerWithKeyAndArguments()
    {
        // Arrange
        ISummitUILocalizer localizer = new SwedishLocalizer();

        // Act
        var result = localizer["Calendar_DateSelectedAnnouncement", "Idag"];

        // Assert
        await Assert.That(result).IsEqualTo("Idag vald");
    }
}
