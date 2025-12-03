namespace AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

/// <summary>
/// Represents the different layers in the performance stack
/// </summary>
public enum LayerType
{
    /// <summary>
    /// Database layer (SQL Server)
    /// </summary>
    Database = 0,

    /// <summary>
    /// AOS Server layer (Application Object Server)
    /// </summary>
    AosServer = 1,

    /// <summary>
    /// Network layer (network communication)
    /// </summary>
    Network = 2,

    /// <summary>
    /// Client layer (end-user client application)
    /// </summary>
    Client = 3
}
