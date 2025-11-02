using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Services.AI;

namespace WinCheck.Infrastructure.AI;

public class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string ApiEndpoint = "https://api.anthropic.com/v1/messages";

    public string ProviderName => "Claude";
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    public ClaudeProvider(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> CompleteAsync(string prompt, AICompletionOptions? options = null)
    {
        options ??= new AICompletionOptions();

        var request = new
        {
            model = options.Model ?? "claude-3-sonnet-20240229",
            max_tokens = options.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            system = options.SystemPrompt ?? "You are a helpful Windows system optimization assistant.",
            temperature = options.Temperature
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiEndpoint, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ClaudeResponse>(responseJson);

        return result?.content?[0]?.text ?? string.Empty;
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

    private class ClaudeResponse
    {
        public ContentBlock[]? content { get; set; }
    }

    private class ContentBlock
    {
        public string? text { get; set; }
    }
}
