using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for analyzing and managing SQL Server configuration settings
/// </summary>
public interface IServerConfigurationService
{
    /// <summary>
    /// Gets all server configuration settings
    /// </summary>
    Task<List<ServerConfiguration>> GetAllConfigurationsAsync();

    /// <summary>
    /// Gets specific configuration setting by name
    /// </summary>
    Task<ServerConfiguration?> GetConfigurationAsync(string name);

    /// <summary>
    /// Analyzes current configuration and provides recommendations
    /// </summary>
    Task<List<ConfigurationRecommendation>> AnalyzeConfigurationAsync();

    /// <summary>
    /// Gets server memory information
    /// </summary>
    Task<ServerMemoryInfo> GetServerMemoryInfoAsync();

    /// <summary>
    /// Applies a configuration change
    /// </summary>
    /// <param name="recommendation">The recommendation to apply</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ApplyRecommendationAsync(ConfigurationRecommendation recommendation);

    /// <summary>
    /// Generates a script to apply a recommendation
    /// </summary>
    string GenerateApplyScript(ConfigurationRecommendation recommendation);
}

/// <summary>
/// Information about server memory
/// </summary>
public class ServerMemoryInfo
{
    /// <summary>
    /// Total physical memory on the server (MB)
    /// </summary>
    public long TotalPhysicalMemoryMB { get; set; }

    /// <summary>
    /// Available physical memory (MB)
    /// </summary>
    public long AvailablePhysicalMemoryMB { get; set; }

    /// <summary>
    /// SQL Server committed memory (MB)
    /// </summary>
    public long SqlServerCommittedMB { get; set; }

    /// <summary>
    /// Max server memory configuration (MB)
    /// </summary>
    public int MaxServerMemoryMB { get; set; }

    /// <summary>
    /// Min server memory configuration (MB)
    /// </summary>
    public int MinServerMemoryMB { get; set; }

    /// <summary>
    /// Recommended max server memory (MB)
    /// </summary>
    public long RecommendedMaxServerMemoryMB { get; set; }
}
