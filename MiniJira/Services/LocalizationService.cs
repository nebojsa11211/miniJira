using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace MiniJira.Services;

/// <summary>
/// Implementation of the localization service.
/// Manages culture settings and provides culture switching functionality for the application.
/// Uses cookies to store culture preference (compatible with ASP.NET Core request localization middleware).
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private CultureInfo _currentCulture;
    private readonly CultureInfo[] _supportedCultures;
    private static readonly string CultureCookieName = CookieRequestCultureProvider.DefaultCookieName; // ".AspNetCore.Culture"

    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("hr-HR")
        };

        // Initialize from current thread culture (set by middleware)
        _currentCulture = CultureInfo.CurrentUICulture;
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public CultureInfo[] SupportedCultures => _supportedCultures;

    public event EventHandler? CultureChanged;

    public Task InitializeAsync()
    {
        // No longer needed - culture is set by middleware before this service is created
        // The middleware reads the culture cookie on every request and sets CultureInfo.CurrentCulture/CurrentUICulture
        // This method is kept for backward compatibility but does nothing
        return Task.CompletedTask;
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

    public Task SaveAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Set the culture cookie in ASP.NET Core format: "c=CULTURE|uic=UI_CULTURE"
                var cookieValue = CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(_currentCulture, _currentCulture));

                httpContext.Response.Cookies.Append(
                    CultureCookieName,
                    cookieValue,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        Path = "/",
                        HttpOnly = false, // Allow JavaScript to read if needed
                        SameSite = SameSiteMode.Lax
                    });
            }
        }
        catch
        {
            // Silently fail if cookie cannot be set
        }

        return Task.CompletedTask;
    }
}
