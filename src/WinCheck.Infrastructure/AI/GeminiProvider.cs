using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Services.AI;
using WinCheck.Core.Constants;

namespace WinCheck.Infrastructure.AI;

/// <summary>
/// Google Gemini provider implementation
/// </summary>
/// <remarks>
/// Uses static shared HttpClient to prevent socket exhaustion.
/// Supports Gemini Pro and other Google AI models.
/// </remarks>
public class GeminiProvider : IAIProvider
{
    // Shared static HttpClient to avoid socket exhaustion
    private static readonly HttpClient _sharedHttpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(AIProviderConstants.ApiTimeoutSeconds)
    };

    private readonly string _apiKey;
    private const string ApiEndpointTemplate = AIProviderConstants.GeminiApiEndpointTemplate;

    public string ProviderName => "Gemini";
    public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

    public GeminiProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> CompleteAsync(string prompt, AICompletionOptions? options = null)
    {
        options ??= new AICompletionOptions();
        var model = options.Model ?? AIProviderConstants.DefaultGeminiModel;

        var endpoint = string.Format(ApiEndpointTemplate, model, _apiKey);

        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = options.Temperature,
                maxOutputTokens = options.MaxTokens
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _sharedHttpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

        return result?.candidates?[0]?.content?.parts?[0]?.text ?? string.Empty;
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

    private class GeminiResponse
    {
        public Candidate[]? candidates { get; set; }
    }

    private class Candidate
    {
        public Content? content { get; set; }
    }

    private class Content
    {
        public Part[]? parts { get; set; }
    }

    private class Part
    {
        public string? text { get; set; }
    }
}
