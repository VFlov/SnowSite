using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SnowSite.Server.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnowSite.Server.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Клиент подключился: ConnectionId={Context.ConnectionId}, UserId={userId}, {Context.User}");
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            else
            {
                Console.WriteLine("Пользователь не авторизован");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int dialogId, string text)
        {
            var message = await _chatService.SendMessageAsync(dialogId, text, null);

            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await _chatService.NotifyClients(message);
            }
        }
    }
}