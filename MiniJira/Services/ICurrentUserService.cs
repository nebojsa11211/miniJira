using MiniJira.Models;

namespace MiniJira.Services;

public interface ICurrentUserService
{
    User? CurrentUser { get; }
    void SetCurrentUser(User user);
    bool IsUserSelected();
    void Logout();
}
