using MiniJira.Models;

namespace MiniJira.Services;

public class CurrentUserService : ICurrentUserService
{
    private User? _currentUser;

    public User? CurrentUser => _currentUser;

    public void SetCurrentUser(User user)
    {
        _currentUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    public bool IsUserSelected()
    {
        return _currentUser != null;
    }

    public void Logout()
    {
        _currentUser = null;
    }
}
