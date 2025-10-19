using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MiniJira.Resources;
using MiniJira.Services;

namespace MiniJira.Shared;

public partial class NavMenu : IDisposable
{
    [Inject]
    private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Inject]
    private ILocalizationService LocalizationService { get; set; } = null!;

    private bool collapseNavMenu = true;
    private bool isSidebarCollapsed = false;

    private string? NavMenuCssClass => collapseNavMenu ? "nav-menu-collapsed" : "nav-menu-expanded";
    private string? SidebarCssClass => isSidebarCollapsed ? "sidebar-collapsed" : "";

    protected override void OnInitialized()
    {
        LocalizationService.CultureChanged += OnCultureChanged;
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        // Use InvokeAsync to marshal the StateHasChanged call back to the UI thread
        // This is required because the CultureChanged event may be raised on a non-UI thread
        InvokeAsync(StateHasChanged);
    }

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    private void ToggleSidebarCollapse()
    {
        isSidebarCollapsed = !isSidebarCollapsed;
    }

    public void Dispose()
    {
        LocalizationService.CultureChanged -= OnCultureChanged;
    }
}
