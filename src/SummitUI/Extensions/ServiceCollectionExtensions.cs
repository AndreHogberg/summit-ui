using Microsoft.Extensions.DependencyInjection;

using SummitUI.Interop;
using SummitUI.Services;
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
        services.AddScoped<MediaQueryJsInterop>();

        // Accessibility services
        services.AddScoped<ILiveAnnouncer, LiveAnnouncerService>();

        // Calendar formatting service (culture-based, no JS dependency)
        services.AddSingleton<CalendarFormatter>();

        // Component-specific services
        services.AddScoped<AccordionJsInterop>();
        services.AddScoped<DropdownMenuJsInterop>();
        services.AddScoped<PopoverJsInterop>();
        services.AddScoped<PopoverService>();
        services.AddScoped<SelectJsInterop>();
        services.AddScoped<DateFieldJsInterop>();
        services.AddScoped<DialogJsInterop>();
        services.AddScoped<CalendarJsInterop>();
        services.AddScoped<ToastJsInterop>();
        services.AddScoped<OtpJsInterop>();
        services.AddScoped<ScrollAreaJsInterop>();
        // Alert dialog service (singleton so it's shared across components)
        services.AddScoped<IAlertDialogService, AlertDialogService>();

        // Portal service for rendering content at the root of the DOM
        services.AddScoped<IPortalService, PortalService>();

        return services;
    }

    /// <summary>
    /// Adds a toast queue for the specified content type.
    /// </summary>
    /// <typeparam name="TContent">User-defined toast content type.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="maxVisibleToasts">Maximum number of toasts visible at once. Defaults to 5.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// // Define your toast content type
    /// public record MyToast(string Title, string? Description = null);
    /// 
    /// // Register in Program.cs
    /// builder.Services.AddToastQueue&lt;MyToast&gt;();
    /// 
    /// // Then inject IToastQueue&lt;MyToast&gt; in your components
    /// </code>
    /// </example>
    public static IServiceCollection AddToastQueue<TContent>(
        this IServiceCollection services,
        int maxVisibleToasts = 5)
    {
        var queue = new ToastQueue<TContent> { MaxVisibleToasts = maxVisibleToasts };
        services.AddSingleton<IToastQueue<TContent>>(queue);
        return services;
    }
}
