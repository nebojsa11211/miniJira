using MiniJira.Models;

namespace MiniJira.Services;

public class MockUserService : IUserService
{
    private static readonly List<User> _users = new()
    {
        new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "Tonci Harstić",
            Email = "tonci.harstic@example.com"
        },
        new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FullName = "Jura Hrastić",
            Email = "jura.hrastic@example.com"
        }
    };

    public System.Threading.Tasks.Task<List<User>> GetUsersAsync()
    {
        return System.Threading.Tasks.Task.FromResult(_users);
    }
}
