namespace MiniJira.Services;

public interface IMobileMenuStateService
{
    bool IsMenuOpen { get; }
    event EventHandler? MenuStateChanged;
    void ToggleMenu();
    void CloseMenu();
}

public class MobileMenuStateService : IMobileMenuStateService
{
    public bool IsMenuOpen { get; private set; }

    public event EventHandler? MenuStateChanged;

    public void ToggleMenu()
    {
        IsMenuOpen = !IsMenuOpen;
        MenuStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CloseMenu()
    {
        if (IsMenuOpen)
        {
            IsMenuOpen = false;
            MenuStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
