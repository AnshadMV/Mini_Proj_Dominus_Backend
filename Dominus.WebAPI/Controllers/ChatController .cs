using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Crmf;
using Dominus.Application.DTOs.ChatDTOs;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly GeminiChatBotService _chat;

    public ChatController(GeminiChatBotService chat)
    {
        _chat = chat;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        var reply = await _chat.AskAsync(request.Message);
        return Ok(new { reply });
    }
}
