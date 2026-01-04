using Dominus.Application.Interfaces.IRepository;
using Dominus.Domain.Enums;
using Dominus.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/payment")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IOrderRepository _orders;
    private readonly IConfiguration _config;

    public PaymentWebhookController(IOrderRepository orders, IConfiguration config)
    {
        _orders = orders;
        _config = config;
    }

    // 🔹 UroPay does a GET ping first → return OK
    [HttpGet("uropay-webhook")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok("Webhook alive");
    }

    // 🔥 REAL PAYMENT WEBHOOK
    [HttpPost("uropay-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] JsonElement payload)
    {
        var body = payload.GetRawText();
        Console.WriteLine("WEBHOOK BODY => " + body);

        var receivedHash = Request.Headers["x-verify"].ToString();
        var storedHash = _config["UroPay:WebhookSecretHashed"];

        if (!string.Equals(receivedHash, storedHash, StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Invalid signature");

        var uroPayOrderId = payload.GetProperty("uroPayOrderId").GetString();
        var status = payload.GetProperty("status").GetString();
        var refId = payload.GetProperty("referenceNumber").GetString();

        var order = await _orders.Query()
            .FirstOrDefaultAsync(x => x.UroPayOrderId == uroPayOrderId);

        if (order == null)
            return NotFound();

        if (string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
        {
            order.Status = OrderStatus.Paid;
            order.PaymentReference = refId;
            order.PaidOn = DateTime.UtcNow;
            await _orders.SaveChangesAsync();
        }

        return Ok("Webhook processed");
    }
}

