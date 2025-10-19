using MiniJira.Models;

namespace MiniJira.Services;

public interface IUserService
{
    System.Threading.Tasks.Task<List<User>> GetUsersAsync();
}
