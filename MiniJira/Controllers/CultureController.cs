using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MiniJira.Controllers;

/// <summary>
/// Controller for handling culture/language switching.
/// Required for Blazor Server because cookies cannot be set during SignalR circuits.
/// </summary>
[Route("[controller]/[action]")]
public class CultureController : Controller
{
    /// <summary>
    /// Sets the culture cookie and redirects back to the referring page or home.
    /// </summary>
    /// <param name="culture">The culture code (e.g., "en-US", "hr-HR")</param>
    /// <param name="redirectUri">Optional URI to redirect to after setting culture</param>
    [HttpGet]
    public IActionResult Set(string culture, string? redirectUri = null)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return BadRequest("Culture parameter is required");
        }

        // Set the culture cookie using ASP.NET Core's standard format
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax
            }
        );

        // Redirect to the specified URI or back to the referrer or home
        if (!string.IsNullOrWhiteSpace(redirectUri))
        {
            return LocalRedirect(redirectUri);
        }

        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer))
        {
            return Redirect(referer);
        }

        return LocalRedirect("~/");
    }
}
