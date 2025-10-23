namespace AX2012PerformanceOptimizer.Core.Models;

public class PerformanceHealthScorecard
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int OverallScore { get; set; }  // 0-100
    public string OverallGrade { get; set; } = string.Empty;  // "Excellent", "Good", "Acceptable", "Poor"
    public string IndustryRanking { get; set; } = string.Empty;  // "Top 15%", etc.

    // Detailed Metrics (50+ metrics grouped into categories)
    public List<HealthMetricCategory> Categories { get; set; } = new();

    // Industry Benchmarks
    public BenchmarkComparison IndustryBenchmark { get; set; } = new();

    // Compliance
    public ComplianceStatus Compliance { get; set; } = new();

    // Historical Trending
    public List<HistoricalScore> HistoricalScores { get; set; } = new();
}

public class HealthMetricCategory
{
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryScore { get; set; }  // 0-100
    public string Status { get; set; } = string.Empty;  // "Excellent", "Good", "Acceptable", "Poor", "Critical"
    public double Weight { get; set; }  // Weight in overall score calculation
    public List<HealthMetric> Metrics { get; set; } = new();
}

public class HealthMetric
{
    public string MetricName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double TargetValue { get; set; }
    public double IndustryAverage { get; set; }
    public double IndustryTop10Percent { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Score { get; set; }  // 0-100
    public string Status { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
}

public class BenchmarkComparison
{
    public string Industry { get; set; } = string.Empty;  // "Manufacturing", "Retail", etc.
    public string CompanySize { get; set; } = string.Empty;  // "Small", "Medium", "Large", "Enterprise"
    public string Region { get; set; } = string.Empty;  // "EMEA", "Americas", "APAC"

    // Your position
    public int YourScore { get; set; }
    public double IndustryAverage { get; set; }
    public double Top10PercentThreshold { get; set; }
    public double Top25PercentThreshold { get; set; }

    // Ranking
    public string PercentileRank { get; set; } = string.Empty;  // "Top 15%"
    public int TotalCompaniesInComparison { get; set; }
    public int YourRank { get; set; }

    // Category Comparisons
    public List<CategoryBenchmark> CategoryBenchmarks { get; set; } = new();
}

public class CategoryBenchmark
{
    public string CategoryName { get; set; } = string.Empty;
    public int YourScore { get; set; }
    public double IndustryAverage { get; set; }
    public double Top10Percent { get; set; }
    public string RelativePosition { get; set; } = string.Empty;  // "Above Average", "Below Average", "Top Performer"
    public int GapToTopPerformer { get; set; }  // Points to reach top 10%
}

public class ComplianceStatus
{
    public bool OverallCompliant { get; set; }
    public List<ComplianceRequirement> Requirements { get; set; } = new();
}

public class ComplianceRequirement
{
    public string RequirementName { get; set; } = string.Empty;  // "SOX", "GDPR", "ISO 27001", "Internal SLA"
    public bool IsMet { get; set; }
    public string Status { get; set; } = string.Empty;  // "Met", "Not Met", "Partial"
    public List<string> MissingCriteria { get; set; } = new();
    public string AuditTrailReference { get; set; } = string.Empty;
    public DateTime LastAuditDate { get; set; }
    public DateTime NextAuditDate { get; set; }
}

public class HistoricalScore
{
    public DateTime Date { get; set; }
    public int OverallScore { get; set; }
    public Dictionary<string, int> CategoryScores { get; set; } = new();
}

public class DetailedMetricsReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public PerformanceHealthScorecard Scorecard { get; set; } = new();

    // Export formats
    public string PdfFilePath { get; set; } = string.Empty;
    public string ExcelFilePath { get; set; } = string.Empty;
    public byte[]? PdfBytes { get; set; }
    public byte[]? ExcelBytes { get; set; }
}

// Predefined Metric Categories
public static class HealthMetricCategories
{
    public const string QueryPerformance = "Query Performance";
    public const string DatabaseHealth = "Database Health";
    public const string AosPerformance = "AOS Performance";
    public const string BatchJobSuccess = "Batch Job Success";
    public const string ResourceUtilization = "Resource Utilization";
    public const string IndexMaintenance = "Index Maintenance";
    public const string StatisticsFreshness = "Statistics Freshness";
    public const string BackupRecovery = "Backup & Recovery";
    public const string SecurityCompliance = "Security & Compliance";
    public const string HighAvailability = "High Availability";
}

// Industry Standards
public static class IndustryStandards
{
    public static class QueryPerformance
    {
        public const double ExcellentAvgResponseTimeMs = 100;
        public const double GoodAvgResponseTimeMs = 200;
        public const double AcceptableAvgResponseTimeMs = 500;
    }

    public static class BatchJobs
    {
        public const double ExcellentSuccessRate = 98.0;
        public const double GoodSuccessRate = 95.0;
        public const double AcceptableSuccessRate = 90.0;
    }

    public static class IndexHealth
    {
        public const double ExcellentFragmentationPercent = 10.0;
        public const double GoodFragmentationPercent = 20.0;
        public const double AcceptableFragmentationPercent = 30.0;
    }
}
