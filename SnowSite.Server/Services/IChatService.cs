namespace SnowSite.Server.Services;
using SnowSite.Server.Models;
using System.Net.WebSockets;

public interface IChatService
{
    Task<List<Dialog>> GetDialogsAsync();
    Task<List<Message>> GetMessagesAsync(int dialogId);
    Task<Message> SendMessageAsync(int dialogId, string text, IFormFile? attachment);
    Task<List<Message>> SearchMessagesAsync(string query);

    Task HandleWebSocket(WebSocket webSocket);

}