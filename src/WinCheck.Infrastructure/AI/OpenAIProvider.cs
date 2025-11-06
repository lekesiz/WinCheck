using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Services.AI;
using WinCheck.Core.Constants;

namespace WinCheck.Infrastructure.AI;

/// <summary>
/// OpenAI GPT provider implementation
/// </summary>
/// <remarks>
/// Uses static shared HttpClient to prevent socket exhaustion.
/// Supports GPT-4 and other OpenAI models via chat completions API.
/// </remarks>
public class OpenAIProvider : IAIProvider
{
    // Shared static HttpClient to avoid socket exhaustion
    private static readonly HttpClient _sharedHttpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(AIProviderConstants.ApiTimeoutSeconds)
    };

    private readonly string _apiKey;
    private const string ApiEndpoint = AIProviderConstants.OpenAIApiEndpoint;

    public string ProviderName => "OpenAI";
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    public OpenAIProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> CompleteAsync(string prompt, AICompletionOptions? options = null)
    {
        options ??= new AICompletionOptions();

        var request = new
        {
            model = options.Model ?? AIProviderConstants.DefaultOpenAIModel,
            messages = new[]
            {
                new { role = "system", content = options.SystemPrompt ?? AIProviderConstants.DefaultSystemPrompt },
                new { role = "user", content = prompt }
            },
            temperature = options.Temperature,
            max_tokens = options.MaxTokens
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create request message with authorization header
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
        requestMessage.Content = content;
        requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _sharedHttpClient.SendAsync(requestMessage);
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
