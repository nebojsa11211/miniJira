using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MiniJira.Resources;
using MiniJira.Services;

namespace MiniJira.Shared;

public partial class MainLayout : IAsyncDisposable
{
    private IJSObjectReference? _fontSizeModule;
    private IJSObjectReference? _colorPaletteModule;

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Inject]
    private ILocalizationService LocalizationService { get; set; } = null!;

    [Inject]
    private IFontSizeService FontSizeService { get; set; } = null!;

    [Inject]
    private IColorPaletteService ColorPaletteService { get; set; } = null!;

    [Inject]
    private IThemeService ThemeService { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    protected override void OnInitialized()
    {
        LocalizationService.CultureChanged += OnCultureChanged;
        FontSizeService.FontSizeChanged += OnFontSizeChanged;
        ColorPaletteService.ColorPaletteChanged += OnColorPaletteChanged;
        ThemeService.OnThemeChanged += OnThemeChangedHandler;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initialize services to load saved settings from localStorage
            await LocalizationService.InitializeAsync();
            await FontSizeService.InitializeAsync();
            await ThemeService.InitializeAsync();
            await ColorPaletteService.InitializeAsync();

            // Import the JavaScript modules
            _fontSizeModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/fontSizeManager.js");
            _colorPaletteModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/colorPaletteManager.js");

            // Set the initial font size
            await UpdateFontSizeInBrowser();

            // Set the initial color palette
            await UpdateColorPaletteInBrowser();

            // Trigger UI update after loading settings
            StateHasChanged();
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        // Use InvokeAsync to marshal the StateHasChanged call back to the UI thread
        // This is required because the CultureChanged event may be raised on a non-UI thread
        InvokeAsync(StateHasChanged);
    }

    private void OnFontSizeChanged(object? sender, EventArgs e)
    {
        // Update the CSS variable in the browser when font size changes
        InvokeAsync(async () =>
        {
            await UpdateFontSizeInBrowser();
            StateHasChanged();
        });
    }

    private async Task UpdateFontSizeInBrowser()
    {
        if (_fontSizeModule != null)
        {
            var multiplier = FontSizeService.CurrentFontSizeMultiplier.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            await _fontSizeModule.InvokeVoidAsync("setFontSizeMultiplier", multiplier);
        }
    }

    private void OnColorPaletteChanged(object? sender, EventArgs e)
    {
        // Update the color palette in the browser when it changes
        InvokeAsync(async () =>
        {
            await UpdateColorPaletteInBrowser();
            StateHasChanged();
        });
    }

    private void OnThemeChangedHandler()
    {
        // Update the color palette when theme changes (to apply correct palette variant)
        InvokeAsync(async () =>
        {
            await UpdateColorPaletteInBrowser();
            StateHasChanged();
        });
    }

    private async Task UpdateColorPaletteInBrowser()
    {
        if (_colorPaletteModule != null)
        {
            var palette = ColorPaletteService.CurrentPalette;
            var theme = ThemeService.CurrentTheme;
            await _colorPaletteModule.InvokeVoidAsync("setColorPalette", palette, theme);
        }
    }

    public async ValueTask DisposeAsync()
    {
        LocalizationService.CultureChanged -= OnCultureChanged;
        FontSizeService.FontSizeChanged -= OnFontSizeChanged;
        ColorPaletteService.ColorPaletteChanged -= OnColorPaletteChanged;
        ThemeService.OnThemeChanged -= OnThemeChangedHandler;

        if (_fontSizeModule != null)
        {
            await _fontSizeModule.DisposeAsync();
        }

        if (_colorPaletteModule != null)
        {
            await _colorPaletteModule.DisposeAsync();
        }
    }
}
