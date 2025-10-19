namespace MiniJira.Services;

/// <summary>
/// Service interface for managing font size preferences.
/// </summary>
public interface IFontSizeService
{
    /// <summary>
    /// Gets the current font size multiplier.
    /// </summary>
    decimal CurrentFontSizeMultiplier { get; }

    /// <summary>
    /// Gets the available font size options.
    /// </summary>
    FontSizeOption[] AvailableOptions { get; }

    /// <summary>
    /// Event raised when the font size changes.
    /// </summary>
    event EventHandler? FontSizeChanged;

    /// <summary>
    /// Initializes the service and loads saved font size from storage.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Sets the font size using the specified multiplier.
    /// </summary>
    /// <param name="multiplier">The font size multiplier (1.0 = normal, 1.1 = 10% bigger, 1.2 = 20% bigger)</param>
    void SetFontSize(decimal multiplier);

    /// <summary>
    /// Saves the current font size to persistent storage.
    /// </summary>
    Task SaveAsync();
}

/// <summary>
/// Represents a font size option.
/// </summary>
public record FontSizeOption(decimal Multiplier, string DisplayKey);
