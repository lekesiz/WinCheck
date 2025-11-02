using System.Threading.Tasks;

namespace WinCheck.Core.Services.AI;

/// <summary>
/// AI Provider abstraction for multiple AI services
/// </summary>
public interface IAIProvider
{
    string ProviderName { get; }
    bool IsConfigured { get; }

    Task<string> CompleteAsync(string prompt, AICompletionOptions? options = null);
    Task<T> CompleteAsync<T>(string prompt, AICompletionOptions? options = null) where T : class;
    Task<bool> TestConnectionAsync();
}

public class AICompletionOptions
{
    public string? Model { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2000;
    public string? SystemPrompt { get; set; }
}
