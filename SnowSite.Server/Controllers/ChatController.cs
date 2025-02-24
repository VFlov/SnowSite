using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnowSite.Server.Models;
using SnowSite.Server.Services;

namespace SnowSite.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<Message>> SendMessage(int dialogId, [FromForm] string text, IFormFile? attachment)
        {
            var message = await _chatService.SendMessageAsync(dialogId, text, attachment);
            return CreatedAtAction(nameof(GetMessages), new { dialogId = message.DialogId }, message);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<Message>>> SearchMessages([FromQuery] string query)
        {
            return Ok(await _chatService.SearchMessagesAsync(query));
        }
    }
}
