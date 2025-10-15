namespace AX2012PerformanceOptimizer.Core.Models;

public class DatabaseMetric
{
    public string DatabaseName { get; set; } = string.Empty;
    public long TotalSizeMB { get; set; }
    public long DataSizeMB { get; set; }
    public long LogSizeMB { get; set; }
    public long UnallocatedSpaceMB { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public class TableMetric
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public long RowCount { get; set; }
    public long TotalSpaceKB { get; set; }
    public long UsedSpaceKB { get; set; }
    public long IndexSpaceKB { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public class IndexFragmentation
{
    public string DatabaseName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public double FragmentationPercent { get; set; }
    public long PageCount { get; set; }
    public string IndexType { get; set; } = string.Empty;
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

public class MissingIndex
{
    public string DatabaseName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string EqualityColumns { get; set; } = string.Empty;
    public string InequalityColumns { get; set; } = string.Empty;
    public string IncludedColumns { get; set; } = string.Empty;
    public double ImpactScore { get; set; }
    public long UserSeeks { get; set; }
    public long UserScans { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}

