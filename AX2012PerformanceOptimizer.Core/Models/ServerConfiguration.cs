namespace AX2012PerformanceOptimizer.Core.Models;

/// <summary>
/// Represents a SQL Server configuration setting from sp_configure
/// </summary>
public class ServerConfiguration
{
    /// <summary>
    /// Configuration option name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current configured value
    /// </summary>
    public int ConfiguredValue { get; set; }

    /// <summary>
    /// Current running value (value_in_use)
    /// </summary>
    public int RunningValue { get; set; }

    /// <summary>
    /// Minimum allowed value
    /// </summary>
    public int MinValue { get; set; }

    /// <summary>
    /// Maximum allowed value
    /// </summary>
    public int MaxValue { get; set; }

    /// <summary>
    /// Whether this is an advanced configuration option
    /// </summary>
    public bool IsAdvanced { get; set; }

    /// <summary>
    /// Whether this setting requires restart to take effect
    /// </summary>
    public bool IsDynamic { get; set; }

    /// <summary>
    /// Description of the configuration option
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether the configured value matches the running value
    /// </summary>
    public bool IsInSync => ConfiguredValue == RunningValue;
}
