using System.Globalization;
using Microsoft.JSInterop;

namespace MiniJira.Services;

/// <summary>
/// Implementation of the localization service.
/// Manages culture settings and provides culture switching functionality for the application.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IJSRuntime _jsRuntime;
    private CultureInfo _currentCulture;
    private readonly CultureInfo[] _supportedCultures;
    private const string StorageKey = "culture";

    public LocalizationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("hr-HR")
        };

        // Default to English
        _currentCulture = _supportedCultures[0];
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public CultureInfo[] SupportedCultures => _supportedCultures;

    public event EventHandler? CultureChanged;

    public async Task InitializeAsync()
    {
        try
        {
            var cultureName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(cultureName))
            {
                var culture = _supportedCultures.FirstOrDefault(c => c.Name == cultureName);
                if (culture != null)
                {
                    _currentCulture = culture;
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;

                    // Trigger CultureChanged event to notify all subscribers
                    CultureChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch
        {
            // If loading fails, keep default culture
        }
    }

    public void SetCulture(CultureInfo culture)
    {
        if (!_supportedCultures.Any(c => c.Name == culture.Name))
        {
            throw new ArgumentException($"Culture {culture.Name} is not supported.", nameof(culture));
        }

        _currentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetCultureAsync(CultureInfo culture)
    {
        if (!_supportedCultures.Any(c => c.Name == culture.Name))
        {
            throw new ArgumentException($"Culture {culture.Name} is not supported.", nameof(culture));
        }

        _currentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        CultureChanged?.Invoke(this, EventArgs.Empty);

        // Automatically persist the culture change
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, _currentCulture.Name);
        }
        catch
        {
            // Silently fail if storage is not available
        }
    }
}
