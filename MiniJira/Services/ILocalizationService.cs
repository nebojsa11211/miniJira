using System.Globalization;

namespace MiniJira.Services;

/// <summary>
/// Service for managing application localization and culture.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current culture.
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Gets all supported cultures.
    /// </summary>
    CultureInfo[] SupportedCultures { get; }

    /// <summary>
    /// Initializes the service and loads saved culture from storage.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Sets the current culture.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    void SetCulture(CultureInfo culture);

    /// <summary>
    /// Sets the current culture and saves it to persistent storage.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    Task SetCultureAsync(CultureInfo culture);

    /// <summary>
    /// Saves the current culture to persistent storage.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Event raised when the culture changes.
    /// </summary>
    event EventHandler? CultureChanged;
}
