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

    [Inject]
    private ISidebarStateService SidebarStateService { get; set; } = null!;

    [Inject]
    private IMobileMenuStateService MobileMenuStateService { get; set; } = null!;

    private bool collapseNavMenu = true;

    private string? NavMenuCssClass => collapseNavMenu ? "nav-menu-collapsed" : "nav-menu-expanded";
    private string? SidebarCssClass => SidebarStateService.IsSidebarCollapsed ? "sidebar-collapsed" : "";

    protected override void OnInitialized()
    {
        LocalizationService.CultureChanged += OnCultureChanged;
        SidebarStateService.SidebarStateChanged += OnSidebarStateChanged;
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        // Use InvokeAsync to marshal the StateHasChanged call back to the UI thread
        // This is required because the CultureChanged event may be raised on a non-UI thread
        InvokeAsync(StateHasChanged);
    }

    private void OnSidebarStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    private void ToggleSidebarCollapse()
    {
        SidebarStateService.ToggleSidebar();
    }

    private void OnNavItemClick()
    {
        // Close mobile menu when navigation item is clicked
        MobileMenuStateService.CloseMenu();
    }

    public void Dispose()
    {
        LocalizationService.CultureChanged -= OnCultureChanged;
        SidebarStateService.SidebarStateChanged -= OnSidebarStateChanged;
    }
}
