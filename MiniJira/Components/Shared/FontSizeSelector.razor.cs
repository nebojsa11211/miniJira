using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MiniJira.Resources;
using MiniJira.Services;

namespace MiniJira.Components.Shared;

public partial class FontSizeSelector : IDisposable
{
    [Inject]
    private IFontSizeService FontSizeService { get; set; } = null!;

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private decimal currentFontSize = 1.0m;

    protected override void OnInitialized()
    {
        currentFontSize = FontSizeService.CurrentFontSizeMultiplier;
        FontSizeService.FontSizeChanged += OnFontSizeChangedEvent;
    }

    private void OnFontSizeChanged(ChangeEventArgs e)
    {
        var multiplierStr = e.Value?.ToString();
        if (!string.IsNullOrEmpty(multiplierStr) && decimal.TryParse(multiplierStr, out var multiplier))
        {
            FontSizeService.SetFontSize(multiplier);
        }
    }

    private void OnFontSizeChangedEvent(object? sender, EventArgs e)
    {
        currentFontSize = FontSizeService.CurrentFontSizeMultiplier;
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        FontSizeService.FontSizeChanged -= OnFontSizeChangedEvent;
    }
}
