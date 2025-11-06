using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Services.AI;
using WinCheck.Core.Constants;

namespace WinCheck.Infrastructure.AI;

/// <summary>
/// Anthropic Claude provider implementation
/// </summary>
/// <remarks>
/// Uses static shared HttpClient to prevent socket exhaustion.
/// Supports Claude 3 (Sonnet, Opus, Haiku) models via messages API.
/// </remarks>
public class ClaudeProvider : IAIProvider
{
    // Shared static HttpClient to avoid socket exhaustion
    private static readonly HttpClient _sharedHttpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(AIProviderConstants.ApiTimeoutSeconds)
    };

    private readonly string _apiKey;
    private const string ApiEndpoint = AIProviderConstants.ClaudeApiEndpoint;

    public string ProviderName => "Claude";
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    public ClaudeProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> CompleteAsync(string prompt, AICompletionOptions? options = null)
    {
        options ??= new AICompletionOptions();

        var request = new
        {
            model = options.Model ?? AIProviderConstants.DefaultClaudeModel,
            max_tokens = options.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            system = options.SystemPrompt ?? AIProviderConstants.DefaultSystemPrompt,
            temperature = options.Temperature
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create request message with headers
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
        requestMessage.Content = content;
        requestMessage.Headers.Add("x-api-key", _apiKey);
        requestMessage.Headers.Add("anthropic-version", AIProviderConstants.ClaudeApiVersion);

        var response = await _sharedHttpClient.SendAsync(requestMessage);
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
