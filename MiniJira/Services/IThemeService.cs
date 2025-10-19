namespace MiniJira.Services;

public interface IThemeService
{
    event Action? OnThemeChanged;
    string CurrentTheme { get; }
    Task InitializeAsync();
    Task ToggleThemeAsync();
    Task SetThemeAsync(string theme);
}
