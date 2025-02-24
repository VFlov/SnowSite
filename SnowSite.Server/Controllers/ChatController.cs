using Microsoft.AspNetCore.Mvc;
using SnowSite.Server.Models;
using SnowSite.Server.Services;

namespace SnowSite.Server.Controllers
{
    // Controllers/ChatController.cs
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("dialogs")]
        public async Task<ActionResult<List<Dialog>>> GetDialogs()
        {
            return Ok(await _chatService.GetDialogsAsync());
        }

        [HttpGet("messages/{dialogId}")]
        public async Task<ActionResult<List<Message>>> GetMessages(int dialogId)
        {
            return Ok(await _chatService.GetMessagesAsync(dialogId));
        }

        [HttpPost("messages/{dialogId}")]
        public async Task<ActionResult<Message>> SendMessage(int dialogId, [FromBody] SendMessageRequest request)
        {
            var message = await _chatService.SendMessageAsync(dialogId, request.Text);
            return CreatedAtAction(nameof(GetMessages), new { dialogId = message.Id }, message);
        }
    }

    public class SendMessageRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
