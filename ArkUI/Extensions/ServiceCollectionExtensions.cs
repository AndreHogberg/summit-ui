using ArkUI.Interop;
using Microsoft.Extensions.DependencyInjection;

namespace ArkUI.Extensions;

/// <summary>
/// Extension methods for registering ArkUI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ArkUI services to the dependency injection container.
    /// Call this in your Program.cs to enable ArkUI components.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddArkUI(this IServiceCollection services)
    {
        // Core utility services
        services.AddScoped<FocusTrapJsInterop>();

        // Component-specific services
        services.AddScoped<AccordionJsInterop>();
        services.AddScoped<DropdownMenuJsInterop>();
        services.AddScoped<PopoverJsInterop>();
        services.AddScoped<SelectJsInterop>();
        services.AddScoped<TabsJsInterop>();

        return services;
    }
}
