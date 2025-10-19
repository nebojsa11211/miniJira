namespace MiniJira.Services;

/// <summary>
/// Service interface for managing color palette preferences.
/// </summary>
public interface IColorPaletteService
{
    /// <summary>
    /// Gets the current color palette identifier.
    /// </summary>
    string CurrentPalette { get; }

    /// <summary>
    /// Gets the available color palette options.
    /// </summary>
    ColorPaletteOption[] AvailableOptions { get; }

    /// <summary>
    /// Event raised when the color palette changes.
    /// </summary>
    event EventHandler? ColorPaletteChanged;

    /// <summary>
    /// Initializes the service and loads saved palette from storage.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Sets the color palette using the specified palette identifier.
    /// </summary>
    /// <param name="paletteId">The palette identifier (e.g., "classic-red", "ocean-blue")</param>
    void SetColorPalette(string paletteId);

    /// <summary>
    /// Saves the current color palette to persistent storage.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Gets the current theme from ThemeService to apply palette correctly.
    /// </summary>
    Task ApplyPaletteAsync(string? theme = null);
}

/// <summary>
/// Represents a color palette option.
/// </summary>
public record ColorPaletteOption(
    string Id,
    string NameKey,
    string DescriptionKey,
    string[] SupportedThemes
);
