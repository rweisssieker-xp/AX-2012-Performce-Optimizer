using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Query Clustering Service - Groups similar queries for bulk optimization
/// UNIQUE FEATURE: Advanced ML-based query clustering for mass optimization
/// </summary>
public interface IQueryClusteringService
{
    /// <summary>
    /// Cluster similar queries together for bulk optimization
    /// </summary>
    Task<ClusteringResult> ClusterQueriesAsync(
        List<SqlQueryMetric> queries,
        ClusteringOptions options);

    /// <summary>
    /// Find query templates and patterns
    /// </summary>
    Task<List<QueryTemplate>> FindQueryTemplatesAsync(
        List<SqlQueryMetric> queries);

    /// <summary>
    /// Optimize entire cluster at once
    /// </summary>
    Task<ClusterOptimizationResult> OptimizeClusterAsync(
        QueryCluster cluster);

    /// <summary>
    /// Detect duplicate or near-duplicate queries
    /// </summary>
    Task<List<DuplicateQueryGroup>> FindDuplicateQueriesAsync(
        List<SqlQueryMetric> queries);
}

/// <summary>
/// Clustering options
/// </summary>
public class ClusteringOptions
{
    public int MinClusterSize { get; set; } = 3;
    public double SimilarityThreshold { get; set; } = 0.75; // 0-1
    public string ClusteringMethod { get; set; } = "Similarity"; // Similarity, Performance, Table
}

/// <summary>
/// Clustering result
/// </summary>
public class ClusteringResult
{
    public DateTime ClusteringDate { get; set; }
    public int TotalQueries { get; set; }
    public int TotalClusters { get; set; }
    public List<QueryCluster> Clusters { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Query cluster
/// </summary>
public class QueryCluster
{
    public string ClusterId { get; set; } = string.Empty;
    public string ClusterName { get; set; } = string.Empty;
    public List<string> QueryHashes { get; set; } = new();
    public double AverageSimilarity { get; set; }
    public string CommonPattern { get; set; } = string.Empty;
    public double TotalExecutionTime { get; set; }
    public long TotalExecutions { get; set; }
}

/// <summary>
/// Query template
/// </summary>
public class QueryTemplate
{
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateSql { get; set; } = string.Empty;
    public int InstanceCount { get; set; }
    public List<string> Parameters { get; set; } = new();
}

/// <summary>
/// Cluster optimization result
/// </summary>
public class ClusterOptimizationResult
{
    public string ClusterId { get; set; } = string.Empty;
    public int QueriesOptimized { get; set; }
    public double TotalTimeSavings { get; set; }
    public List<string> Optimizations { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Duplicate query group
/// </summary>
public class DuplicateQueryGroup
{
    public string GroupId { get; set; } = string.Empty;
    public List<string> QueryHashes { get; set; } = new();
    public double SimilarityScore { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}
