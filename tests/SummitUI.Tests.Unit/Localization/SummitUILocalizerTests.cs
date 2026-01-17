using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

using SummitUI;
using SummitUI.Extensions;

namespace SummitUI.Tests.Unit.Localization;

/// <summary>
/// Tests for <see cref="ISummitUILocalizer"/> and the default <see cref="SummitUILocalizer"/> implementation.
/// </summary>
public class SummitUILocalizerTests
{
    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(); // Required by AddLocalization
        services.AddSummitUI();
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task Localizer_ReturnsDialogCloseLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Dialog_CloseLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Close dialog");
    }

    [Test]
    public async Task Localizer_ReturnsPopoverCloseLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Popover_CloseLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Close");
    }

    [Test]
    public async Task Localizer_ReturnsComboboxClearAllLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Combobox_ClearAllLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Clear all selections");
    }

    [Test]
    public async Task Localizer_ReturnsCalendarNextMonthLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Calendar_NextMonthLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Next month");
    }

    [Test]
    public async Task Localizer_ReturnsCalendarPreviousMonthLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Calendar_PreviousMonthLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Previous month");
    }

    [Test]
    public async Task Localizer_ReturnsCalendarGridInstructions()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Calendar_GridInstructions"];

        // Assert
        await Assert.That(result).IsEqualTo("Use arrow keys to navigate dates, Enter or Space to select");
    }

    [Test]
    public async Task Localizer_ReturnsDateFieldYearLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["DateField_YearLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Year");
    }

    [Test]
    public async Task Localizer_ReturnsDateFieldMonthLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["DateField_MonthLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Month");
    }

    [Test]
    public async Task Localizer_ReturnsDateFieldDayLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["DateField_DayLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Day");
    }

    [Test]
    public async Task Localizer_ReturnsDateFieldHourLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["DateField_HourLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Hour");
    }

    [Test]
    public async Task Localizer_ReturnsDateFieldMinuteLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["DateField_MinuteLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("Minute");
    }

    [Test]
    public async Task Localizer_ReturnsDateFieldDayPeriodLabel()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["DateField_DayPeriodLabel"];

        // Assert
        await Assert.That(result).IsEqualTo("AM/PM");
    }

    [Test]
    public async Task Localizer_FormatsCalendarDateSelectedAnnouncement()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Calendar_DateSelectedAnnouncement", "January 15, 2026"];

        // Assert
        await Assert.That(result).IsEqualTo("January 15, 2026 selected");
    }

    [Test]
    public async Task Localizer_ReturnsKeyForUnknownResource()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Act
        var result = localizer["Unknown_Key"];

        // Assert
        await Assert.That(result).IsEqualTo("Unknown_Key");
    }
}
