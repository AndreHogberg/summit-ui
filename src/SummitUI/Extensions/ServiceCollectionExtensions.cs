using Microsoft.Extensions.DependencyInjection;

using SummitUI.Interop;
using SummitUI.Utilities;

namespace SummitUI.Extensions;

/// <summary>
/// Extension methods for registering SummitUI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SummitUI services to the dependency injection container.
    /// Call this in your Program.cs to enable SummitUI components.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSummitUI(this IServiceCollection services)
    {
        // Core utility services
        services.AddScoped<SummitUtilities>();
        services.AddScoped<FocusTrapJsInterop>();
        services.AddScoped<FloatingJsInterop>();

        // Component-specific services
        services.AddScoped<AccordionJsInterop>();
        services.AddScoped<DropdownMenuJsInterop>();
        services.AddScoped<PopoverJsInterop>();
        services.AddScoped<PopoverService>();
        services.AddScoped<SelectJsInterop>();
        services.AddScoped<DateFieldJsInterop>();
        services.AddScoped<DialogJsInterop>();
        services.AddScoped<CalendarJsInterop>();

        // Alert dialog service (singleton so it's shared across components)
        services.AddScoped<IAlertDialogService, AlertDialogService>();

        // Portal service for rendering content at the root of the DOM
        services.AddScoped<IPortalService, PortalService>();

        return services;
    }
}
