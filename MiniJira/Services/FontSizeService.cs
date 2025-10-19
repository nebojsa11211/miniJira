using Microsoft.JSInterop;
using System.Globalization;

namespace MiniJira.Services;

/// <summary>
/// Implementation of the font size service.
/// Manages font size settings and provides functionality to change font sizes across the application.
/// </summary>
public class FontSizeService : IFontSizeService
{
    private readonly IJSRuntime _jsRuntime;
    private decimal _currentFontSizeMultiplier = 1.0m;
    private readonly FontSizeOption[] _availableOptions;
    private const string StorageKey = "fontSize";

    public FontSizeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _availableOptions = new[]
        {
            new FontSizeOption(1.0m, "FontSize.Normal"),
            new FontSizeOption(1.1m, "FontSize.Bigger10"),
            new FontSizeOption(1.2m, "FontSize.Bigger20"),
            new FontSizeOption(1.3m, "FontSize.Bigger30"),
            new FontSizeOption(1.4m, "FontSize.Bigger40"),
            new FontSizeOption(1.5m, "FontSize.Bigger50")
        };
    }

    public decimal CurrentFontSizeMultiplier => _currentFontSizeMultiplier;

    public FontSizeOption[] AvailableOptions => _availableOptions;

    public event EventHandler? FontSizeChanged;

    public async Task InitializeAsync()
    {
        try
        {
            var fontSizeStr = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(fontSizeStr) && decimal.TryParse(fontSizeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var fontSize))
            {
                if (_availableOptions.Any(o => o.Multiplier == fontSize))
                {
                    _currentFontSizeMultiplier = fontSize;
                }
            }
        }
        catch
        {
            // If loading fails, keep default font size
        }
    }

    public void SetFontSize(decimal multiplier)
    {
        if (!_availableOptions.Any(o => o.Multiplier == multiplier))
        {
            throw new ArgumentException($"Font size multiplier {multiplier} is not supported.", nameof(multiplier));
        }

        _currentFontSizeMultiplier = multiplier;
        FontSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, _currentFontSizeMultiplier.ToString(CultureInfo.InvariantCulture));
        }
        catch
        {
            // Silently fail if storage is not available
        }
    }
}
