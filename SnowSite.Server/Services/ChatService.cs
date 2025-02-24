using Microsoft.EntityFrameworkCore;
using SnowSite.Server.Data;
using SnowSite.Server.Models;
using SnowSite.Server.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;

    public ChatService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Dialog>> GetDialogsAsync()
    {
        return await _context.Dialogs.ToListAsync();
    }

    public async Task<List<Message>> GetMessagesAsync(int dialogId)
    {
        return await _context.Messages
            .Where(m => m.Id == dialogId)
            .OrderBy(m => m.Time)
            .ToListAsync();
    }

    public async Task<Message> SendMessageAsync(int dialogId, string text)
    {
        var dialog = await _context.Dialogs.FindAsync(dialogId);
        if (dialog == null) throw new Exception("Dialog not found");

        var message = new Message
        {
            Id = dialogId,
            Sender = "sent", // Здесь можно добавить логику определения отправителя
            Text = text,
            Time = DateTime.Now
        };

        _context.Messages.Add(message);
        dialog.LastMessage = text;
        await _context.SaveChangesAsync();
        return message;
    }
}