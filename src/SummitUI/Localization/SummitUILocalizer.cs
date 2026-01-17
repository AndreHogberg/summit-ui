using Microsoft.Extensions.Localization;

namespace SummitUI;

/// <summary>
/// Default implementation of <see cref="ISummitUILocalizer"/> that uses .NET's
/// <see cref="IStringLocalizer{T}"/> for resource-based localization.
/// </summary>
/// <remarks>
/// This implementation wraps <see cref="IStringLocalizer{SummitUIResources}"/> to provide
/// localized strings from the embedded SummitUIResources.resx file. The .NET resource
/// system automatically handles culture fallback.
/// </remarks>
internal sealed class SummitUILocalizer(IStringLocalizer<SummitUIResources> localizer) : ISummitUILocalizer
{
    /// <inheritdoc />
    public string this[string key] => localizer[key];

    /// <inheritdoc />
    public string this[string key, params object[] arguments] => localizer[key, arguments];
}
