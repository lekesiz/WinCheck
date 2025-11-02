using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Services.AI;

namespace WinCheck.Infrastructure.AI;

public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string ApiEndpoint = "https://api.openai.com/v1/chat/completions";

    public string ProviderName => "OpenAI";
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    public OpenAIProvider(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> CompleteAsync(string prompt, AICompletionOptions? options = null)
    {
        options ??= new AICompletionOptions();

        var request = new
        {
            model = options.Model ?? "gpt-4",
            messages = new[]
            {
                new { role = "system", content = options.SystemPrompt ?? "You are a helpful Windows system optimization assistant." },
                new { role = "user", content = prompt }
            },
            temperature = options.Temperature,
            max_tokens = options.MaxTokens
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiEndpoint, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

        return result?.choices?[0]?.message?.content ?? string.Empty;
    }

    public async Task<T> CompleteAsync<T>(string prompt, AICompletionOptions? options = null) where T : class
    {
        var jsonResponse = await CompleteAsync(prompt + "\n\nRespond with valid JSON only.", options);
        return JsonSerializer.Deserialize<T>(jsonResponse) ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await CompleteAsync("Hello", new AICompletionOptions { MaxTokens = 10 });
            return !string.IsNullOrEmpty(response);
        }
        catch
        {
            return false;
        }
    }

    private class OpenAIResponse
    {
        public Choice[]? choices { get; set; }
    }

    private class Choice
    {
        public Message? message { get; set; }
    }

    private class Message
    {
        public string? content { get; set; }
    }
}
