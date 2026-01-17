using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SummitUI;
using SummitUI.Extensions;

namespace SummitUI.Tests.Unit.Localization;

/// <summary>
/// Tests for the dependency injection registration of <see cref="ISummitUILocalizer"/>.
/// Verifies that <see cref="TryAddSingleton"/> is used correctly so consumers can override.
/// </summary>
public class LocalizerDependencyInjectionTests
{
    private sealed class CustomLocalizer : ISummitUILocalizer
    {
        public string this[string key] => $"Custom:{key}";
        public string this[string key, params object[] arguments] => string.Format(this[key], arguments);
    }

    private sealed class AnotherCustomLocalizer : ISummitUILocalizer
    {
        public string this[string key] => $"Another:{key}";
        public string this[string key, params object[] arguments] => string.Format(this[key], arguments);
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(); // Required by AddLocalization
        return services;
    }

    [Test]
    public async Task AddSummitUI_RegistersDefaultLocalizer()
    {
        // Arrange
        var services = CreateServices();

        // Act
        services.AddSummitUI();
        var provider = services.BuildServiceProvider();
        var localizer = provider.GetService<ISummitUILocalizer>();

        // Assert
        await Assert.That(localizer).IsNotNull();
    }

    [Test]
    public async Task AddSummitUI_UsesTryAddSingleton_AllowsPreRegistration()
    {
        // Arrange
        var services = CreateServices();

        // Pre-register custom localizer
        services.AddSingleton<ISummitUILocalizer, CustomLocalizer>();

        // Act
        services.AddSummitUI(); // Should NOT overwrite the custom registration
        var provider = services.BuildServiceProvider();
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Assert - Custom registration takes precedence
        await Assert.That(localizer).IsTypeOf<CustomLocalizer>();
        await Assert.That(localizer["Test"]).IsEqualTo("Custom:Test");
    }

    [Test]
    public async Task AddSummitUI_MultipleCallsDoNotDuplicate()
    {
        // Arrange
        var services = CreateServices();

        // Act - Call AddSummitUI multiple times
        services.AddSummitUI();
        services.AddSummitUI();
        services.AddSummitUI();

        var provider = services.BuildServiceProvider();
        var localizers = provider.GetServices<ISummitUILocalizer>().ToList();

        // Assert - Only one localizer should be registered
        await Assert.That(localizers.Count).IsEqualTo(1);
    }

    [Test]
    public async Task CustomLocalizer_CanReplaceDefault_AfterAddSummitUI()
    {
        // Arrange
        var services = CreateServices();
        services.AddSummitUI();

        // Act - Replace the default with a custom one
        // Note: This replaces because AddSingleton adds a new registration
        services.AddSingleton<ISummitUILocalizer, CustomLocalizer>();
        var provider = services.BuildServiceProvider();

        // GetRequiredService returns the last registered service for single-instance resolution
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Assert - Custom takes precedence when registered after
        await Assert.That(localizer).IsTypeOf<CustomLocalizer>();
    }

    [Test]
    public async Task Localizer_IsSingleton()
    {
        // Arrange
        var services = CreateServices();
        services.AddSummitUI();
        var provider = services.BuildServiceProvider();

        // Act
        var localizer1 = provider.GetRequiredService<ISummitUILocalizer>();
        var localizer2 = provider.GetRequiredService<ISummitUILocalizer>();

        // Assert - Same instance
        await Assert.That(ReferenceEquals(localizer1, localizer2)).IsTrue();
    }

    [Test]
    public async Task Localizer_IsSingleton_AcrossScopes()
    {
        // Arrange
        var services = CreateServices();
        services.AddSummitUI();
        var provider = services.BuildServiceProvider();

        // Act
        ISummitUILocalizer? localizer1;
        ISummitUILocalizer? localizer2;

        using (var scope1 = provider.CreateScope())
        {
            localizer1 = scope1.ServiceProvider.GetRequiredService<ISummitUILocalizer>();
        }

        using (var scope2 = provider.CreateScope())
        {
            localizer2 = scope2.ServiceProvider.GetRequiredService<ISummitUILocalizer>();
        }

        // Assert - Same instance across scopes (singleton)
        await Assert.That(ReferenceEquals(localizer1, localizer2)).IsTrue();
    }

    [Test]
    public async Task TryAddSingleton_DoesNotOverwrite_ExistingRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // First registration
        services.AddSingleton<ISummitUILocalizer, CustomLocalizer>();

        // Try to add another - should be ignored
        services.TryAddSingleton<ISummitUILocalizer, AnotherCustomLocalizer>();

        var provider = services.BuildServiceProvider();

        // Act
        var localizer = provider.GetRequiredService<ISummitUILocalizer>();

        // Assert - First registration wins
        await Assert.That(localizer).IsTypeOf<CustomLocalizer>();
    }

    [Test]
    public async Task AddLocalization_IsCalledByAddSummitUI()
    {
        // Arrange
        var services = CreateServices();

        // Act
        services.AddSummitUI();
        var provider = services.BuildServiceProvider();

        // Assert - IStringLocalizerFactory should be available (registered by AddLocalization)
        var factory = provider.GetService<Microsoft.Extensions.Localization.IStringLocalizerFactory>();
        await Assert.That(factory).IsNotNull();
    }
}
