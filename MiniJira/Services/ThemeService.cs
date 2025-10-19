using Microsoft.JSInterop;

namespace MiniJira.Services;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";

    public event Action? OnThemeChanged;
    public string CurrentTheme => _currentTheme;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme");
            _currentTheme = string.IsNullOrEmpty(theme) ? "light" : theme;
            await ApplyThemeAsync(_currentTheme);
        }
        catch
        {
            _currentTheme = "light";
        }
    }

    public async Task ToggleThemeAsync()
    {
        _currentTheme = _currentTheme == "light" ? "dark" : "light";
        await ApplyThemeAsync(_currentTheme);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", _currentTheme);
        OnThemeChanged?.Invoke();
    }

    public async Task SetThemeAsync(string theme)
    {
        if (theme != "light" && theme != "dark") return;

        _currentTheme = theme;
        await ApplyThemeAsync(_currentTheme);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", _currentTheme);
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync(string theme)
    {
        await _jsRuntime.InvokeVoidAsync("eval",
            $"document.documentElement.setAttribute('data-theme', '{theme}')");
    }
}
