using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MiniJira.Resources;
using MiniJira.Services;

namespace MiniJira.Components.Shared;

public partial class ColorPaletteSelector : IDisposable
{
    [Inject]
    private IColorPaletteService ColorPaletteService { get; set; } = null!;

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private string currentPalette = "classic-red";
    private string currentDescription = "Palette.ClassicRed.Desc";

    protected override void OnInitialized()
    {
        currentPalette = ColorPaletteService.CurrentPalette;
        UpdateCurrentDescription();
        ColorPaletteService.ColorPaletteChanged += OnPaletteChangedEvent;
    }

    private void OnPaletteChanged(ChangeEventArgs e)
    {
        var paletteId = e.Value?.ToString();
        if (!string.IsNullOrEmpty(paletteId))
        {
            ColorPaletteService.SetColorPalette(paletteId);
            currentPalette = paletteId;
            UpdateCurrentDescription();
        }
    }

    private void OnPaletteChangedEvent(object? sender, EventArgs e)
    {
        currentPalette = ColorPaletteService.CurrentPalette;
        UpdateCurrentDescription();
        InvokeAsync(StateHasChanged);
    }

    private void UpdateCurrentDescription()
    {
        var option = ColorPaletteService.AvailableOptions
            .FirstOrDefault(o => o.Id == currentPalette);

        if (option != null)
        {
            currentDescription = option.DescriptionKey;
        }
    }

    public void Dispose()
    {
        ColorPaletteService.ColorPaletteChanged -= OnPaletteChangedEvent;
    }
}
