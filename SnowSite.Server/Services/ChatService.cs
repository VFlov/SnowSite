using Microsoft.EntityFrameworkCore;
using SnowSite.Server.Data;
using SnowSite.Server.Models;
using SnowSite.Server.Services;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly List<WebSocket> _sockets = new();

    public ChatService(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<List<Dialog>> GetDialogsAsync()
    {
        var userId = _authService.GetCurrentUserId();
        return await _context.Dialogs
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Message>> GetMessagesAsync(int dialogId)
    {
        return await _context.Messages
            .Where(m => m.DialogId == dialogId)
            .OrderBy(m => m.Time)
            .ToListAsync();
    }

    public async Task<Message> SendMessageAsync(int dialogId, string text, IFormFile? attachment)
    {
        var dialog = await _context.Dialogs.FindAsync(dialogId);
        if (dialog == null) throw new Exception("Dialog not found");

        var message = new Message
        {
            DialogId = dialogId,
            Sender = _context.Users.Find(_authService.GetCurrentUserId())?.Username ?? "Unknown",
            Text = text,
            Time = DateTime.Now,
            AttachmentUrl = attachment != null ? await SaveAttachment(attachment) : null
        };

        _context.Messages.Add(message);
        dialog.LastMessage = text;
        dialog.UnreadCount++;
        await _context.SaveChangesAsync();

        await NotifyClients(message);
        return message;
    }

    public async Task<List<Message>> SearchMessagesAsync(string query)
    {
        return await _context.Messages
            .Where(m => m.Text.Contains(query))
            .OrderBy(m => m.Time)
            .Take(50)
            .ToListAsync();
    }

    public async Task HandleWebSocket(WebSocket webSocket)
    {
        _sockets.Add(webSocket);
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            // Здесь можно добавить обработку входящих сообщений через WebSocket
        }

        _sockets.Remove(webSocket);
    }

    private async Task NotifyClients(Message message)
    {
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);

        foreach (var socket in _sockets.Where(s => s.State == WebSocketState.Open))
        {
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async Task<string> SaveAttachment(IFormFile file)
    {
        var path = Path.Combine("wwwroot/uploads", Guid.NewGuid() + Path.GetExtension(file.FileName));
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{Path.GetFileName(path)}";
    }
}