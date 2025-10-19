using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MiniJira.Resources;
using MiniJira.Services;

namespace MiniJira.Components.Shared;

public partial class ThemeToggle : IDisposable
{
    [Inject]
    private IThemeService ThemeService { get; set; } = null!;

    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        await ThemeService.InitializeAsync();
        ThemeService.OnThemeChanged += StateHasChanged;
    }

    private async System.Threading.Tasks.Task ToggleTheme()
    {
        await ThemeService.ToggleThemeAsync();
    }

    public void Dispose()
    {
        ThemeService.OnThemeChanged -= StateHasChanged;
    }
}
