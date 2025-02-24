namespace SnowSite.Server.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        int GetCurrentUserId();
    }
}
