namespace AX2012PerformanceOptimizer.Data.Models;

/// <summary>
/// Configuration for AI-powered query optimization
/// </summary>
public class AiConfiguration
{
    /// <summary>
    /// Whether AI features are enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// AI Provider (OpenAI or AzureOpenAI)
    /// </summary>
    public AiProvider Provider { get; set; } = AiProvider.OpenAI;

    /// <summary>
    /// API Key (encrypted)
    /// </summary>
    public string EncryptedApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API Endpoint URL
    /// </summary>
    public string Endpoint { get; set; } = "https://api.openai.com";

    /// <summary>
    /// Model name (e.g., gpt-4o, gpt-4, gpt-3.5-turbo)
    /// </summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>
    /// Azure OpenAI Deployment name (for Azure only)
    /// </summary>
    public string? AzureDeploymentName { get; set; }

    /// <summary>
    /// When configuration was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public enum AiProvider
{
    OpenAI,
    AzureOpenAI
}
