namespace MiniJira.Services;

public interface ISidebarStateService
{
    bool IsSidebarCollapsed { get; }
    event EventHandler? SidebarStateChanged;
    void ToggleSidebar();
    void SetSidebarState(bool isCollapsed);
}

public class SidebarStateService : ISidebarStateService
{
    private bool _isSidebarCollapsed = false;

    public bool IsSidebarCollapsed => _isSidebarCollapsed;

    public event EventHandler? SidebarStateChanged;

    public void ToggleSidebar()
    {
        _isSidebarCollapsed = !_isSidebarCollapsed;
        SidebarStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSidebarState(bool isCollapsed)
    {
        if (_isSidebarCollapsed != isCollapsed)
        {
            _isSidebarCollapsed = isCollapsed;
            SidebarStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
