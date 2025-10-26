using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MiniJira.Resources;
using MiniJira.Services;

namespace MiniJira.Components.Shared;

public partial class LanguageSelector : IDisposable
{
    [Inject]
    private ILocalizationService LocalizationService { get; set; } = null!;

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private string currentCulture = "en-US";

    protected override void OnInitialized()
    {
        currentCulture = LocalizationService.CurrentCulture.Name;
        LocalizationService.CultureChanged += OnCultureChanged;
    }

    private void OnLanguageChanged(ChangeEventArgs e)
    {
        var cultureName = e.Value?.ToString();
        if (!string.IsNullOrEmpty(cultureName))
        {
            // Navigate to the culture controller endpoint which will:
            // 1. Set the culture cookie via HTTP response (not SignalR)
            // 2. Redirect back to the current page
            // This is required because Blazor Server uses SignalR and cannot set cookies directly
            var uri = new Uri(NavigationManager.Uri);
            var redirectUri = uri.PathAndQuery;
            NavigationManager.NavigateTo($"/Culture/Set?culture={Uri.EscapeDataString(cultureName)}&redirectUri={Uri.EscapeDataString(redirectUri)}", forceLoad: true);
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        currentCulture = LocalizationService.CurrentCulture.Name;
        // Use InvokeAsync to marshal the StateHasChanged call back to the UI thread
        // This is required because the CultureChanged event may be raised on a non-UI thread
        InvokeAsync(StateHasChanged);
    }

    private string GetLanguageName(CultureInfo culture)
    {
        return culture.Name switch
        {
            "en-US" => Localizer["Language.English"],
            "hr-HR" => Localizer["Language.Croatian"],
            _ => culture.NativeName
        };
    }

    public void Dispose()
    {
        LocalizationService.CultureChanged -= OnCultureChanged;
    }
}
