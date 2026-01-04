using Dominus.Application.DTOs.Payment;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class UroPayService
{
    private readonly HttpClient _http;
    private readonly UroPaySettings _settings;

    public UroPayService(HttpClient http, IOptions<UroPaySettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    private string HashSecret()
    {
        using var sha = SHA512.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(_settings.Secret));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    public async Task<(string uroPayOrderId, string qr, string upi)> CreatePayment(decimal amount, string orderId, string email, string name)
    {
        var payload = new
        {
            vpa = _settings.VPA,
            vpaName = _settings.VPAName,
            amount = (int)(amount * 100), // amount in paise
            merchantOrderId = orderId,
            customerName = name,
            customerEmail = email,
            transactionNote = $"Payment for order #{orderId}"
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.uropay.me/order/generate");
        req.Headers.Add("X-API-KEY", _settings.ApiKey);
        req.Headers.Add("Authorization", $"Bearer {HashSecret()}");
        req.Headers.Add("Accept", "application/json");
        req.Content = JsonContent.Create(payload);

        var res = await _http.SendAsync(req);
        var raw = await res.Content.ReadAsStringAsync();
        Console.WriteLine("UROPAY RAW RESPONSE => " + raw);

        res.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(raw);
        var data = json.RootElement.GetProperty("data");

        return
        (
            data.GetProperty("uroPayOrderId").GetString()!,
            data.GetProperty("qrCode").GetString()!,
            data.GetProperty("upiString").GetString()!
        );
    }

    public async Task<bool> UpdatePayment(string uroPayOrderId, string referenceNumber)
    {
        var payload = new
        {
            uroPayOrderId,
            referenceNumber
        };

        var req = new HttpRequestMessage(HttpMethod.Patch, "https://api.uropay.me/order/update");
        req.Headers.Add("X-API-KEY", _settings.ApiKey);
        req.Headers.Add("Authorization", $"Bearer {HashSecret()}");
        req.Headers.Add("Accept", "application/json");
        req.Content = JsonContent.Create(payload);

        var res = await _http.SendAsync(req);
        return res.IsSuccessStatusCode;
    }
}
