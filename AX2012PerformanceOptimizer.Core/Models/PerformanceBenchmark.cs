namespace AX2012PerformanceOptimizer.Core.Models;

public class BenchmarkProfile
{
    public string IndustryType { get; set; } = string.Empty;
    public string UserCountRange { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string AXVersion { get; set; } = "2012 R3";
}

public class BenchmarkReport
{
    public string YourRanking { get; set; } = string.Empty;
    public Dictionary<string, BenchmarkMetric> Metrics { get; set; } = new();
    public List<BestPractice> BestPractices { get; set; } = new();
    public List<string> TrendingIssues { get; set; } = new();
    public int PeerCount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class BenchmarkMetric
{
    public string Name { get; set; } = string.Empty;
    public double YourValue { get; set; }
    public double MedianValue { get; set; }
    public double Top10PercentValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Above Average", "Below Average", "Top Performer"
}

public class BestPractice
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AdoptionRate { get; set; } // Percentage
    public double AverageImprovement { get; set; }
    public List<string> Steps { get; set; } = new();
}
