using SnowSite.Server.Models;
namespace SnowSite.Server.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        int GetCurrentUserId();
        Task<List<User>> SearchUsersAsync(string query);
    }
}
