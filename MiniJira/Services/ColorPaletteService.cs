using Microsoft.JSInterop;

namespace MiniJira.Services;

/// <summary>
/// Implementation of the color palette service.
/// Manages color palette settings and provides functionality to change palettes across the application.
/// </summary>
public class ColorPaletteService : IColorPaletteService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IThemeService _themeService;
    private string _currentPalette = "classic-red";
    private readonly ColorPaletteOption[] _availableOptions;
    private const string StorageKey = "colorPalette";

    public ColorPaletteService(IJSRuntime jsRuntime, IThemeService themeService)
    {
        _jsRuntime = jsRuntime;
        _themeService = themeService;
        _availableOptions = new[]
        {
            // Light mode palettes
            new ColorPaletteOption(
                "classic-red",
                "Palette.ClassicRed",
                "Palette.ClassicRed.Desc",
                new[] { "light", "dark" }
            ),
            new ColorPaletteOption(
                "ocean-blue",
                "Palette.OceanBlue",
                "Palette.OceanBlue.Desc",
                new[] { "light", "dark" }
            ),
            new ColorPaletteOption(
                "forest-green",
                "Palette.ForestGreen",
                "Palette.ForestGreen.Desc",
                new[] { "light", "dark" }
            )
        };

        // Subscribe to theme changes to reapply palette
        _themeService.OnThemeChanged += async () => await ApplyPaletteAsync();
    }

    public string CurrentPalette => _currentPalette;

    public ColorPaletteOption[] AvailableOptions => _availableOptions;

    public event EventHandler? ColorPaletteChanged;

    public async Task InitializeAsync()
    {
        try
        {
            var palette = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(palette) && _availableOptions.Any(o => o.Id == palette))
            {
                _currentPalette = palette;
            }
            await ApplyPaletteAsync();
        }
        catch
        {
            // If loading fails, keep default palette
            _currentPalette = "classic-red";
        }
    }

    public void SetColorPalette(string paletteId)
    {
        if (!_availableOptions.Any(o => o.Id == paletteId))
        {
            throw new ArgumentException($"Color palette '{paletteId}' is not supported.", nameof(paletteId));
        }

        _currentPalette = paletteId;
        ColorPaletteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, _currentPalette);
            await ApplyPaletteAsync();
        }
        catch
        {
            // Silently fail if storage is not available
        }
    }

    public async Task ApplyPaletteAsync(string? theme = null)
    {
        try
        {
            // Get current theme if not provided
            var currentTheme = theme ?? _themeService.CurrentTheme;

            // Apply the color palette via JavaScript
            await _jsRuntime.InvokeVoidAsync("eval",
                $"document.documentElement.setAttribute('data-color-palette', '{_currentPalette}')");
        }
        catch
        {
            // Silently fail if JS is not available yet
        }
    }
}
