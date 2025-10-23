using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public interface IPerformanceHealthScorecardService
{
    /// <summary>
    /// Generates comprehensive health scorecard with 50+ metrics
    /// </summary>
    Task<PerformanceHealthScorecard> GenerateHealthScorecardAsync();

    /// <summary>
    /// Gets detailed metrics report with all data
    /// </summary>
    Task<DetailedMetricsReport> GenerateDetailedReportAsync();

    /// <summary>
    /// Compares current performance against industry benchmarks
    /// </summary>
    Task<BenchmarkComparison> CompareToBenchmarksAsync(string industry, string companySize, string region);

    /// <summary>
    /// Checks compliance against requirements
    /// </summary>
    Task<ComplianceStatus> CheckComplianceAsync(List<string> requirements);

    /// <summary>
    /// Gets historical scorecard trend
    /// </summary>
    Task<List<HistoricalScore>> GetHistoricalTrendAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Exports health scorecard to PDF
    /// </summary>
    Task<byte[]> ExportToPdfAsync(PerformanceHealthScorecard scorecard);

    /// <summary>
    /// Exports health scorecard to Excel
    /// </summary>
    Task<byte[]> ExportToExcelAsync(PerformanceHealthScorecard scorecard);

    /// <summary>
    /// Sends health scorecard to auditor
    /// </summary>
    Task SendToAuditorAsync(string auditorEmail, PerformanceHealthScorecard scorecard);
}
