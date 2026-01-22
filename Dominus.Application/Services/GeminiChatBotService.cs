using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class GeminiChatBotService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GeminiChatBotService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> AskAsync(string message)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:Model"];

        var url =
            $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = message }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(body);

        var response = await _http.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseText);
        }

        using var doc = JsonDocument.Parse(responseText);

        return doc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()!;
    }
}
