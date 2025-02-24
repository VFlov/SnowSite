namespace SnowSite.Server.Services;
using SnowSite.Server.Models;

public interface IChatService
{
    Task<List<Dialog>> GetDialogsAsync();
    Task<List<Message>> GetMessagesAsync(int dialogId);
    Task<Message> SendMessageAsync(int dialogId, string text);
}