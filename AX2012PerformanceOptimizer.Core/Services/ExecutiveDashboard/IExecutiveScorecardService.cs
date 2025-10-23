using AX2012PerformanceOptimizer.Core.Models.ExecutiveDashboard;

namespace AX2012PerformanceOptimizer.Core.Services.ExecutiveDashboard;

public interface IExecutiveScorecardService
{
    /// <summary>
    /// Generates current executive scorecard with all metrics
    /// </summary>
    Task<ExecutiveScorecard> GenerateScorecardAsync();

    /// <summary>
    /// Generates monthly business review report
    /// </summary>
    Task<MonthlyBusinessReview> GenerateMonthlyReviewAsync(DateTime month);

    /// <summary>
    /// Gets quarterly business review
    /// </summary>
    Task<MonthlyBusinessReview> GenerateQuarterlyReviewAsync(int year, int quarter);

    /// <summary>
    /// Calculates performance grade from score
    /// </summary>
    PerformanceGrade CalculateGrade(int score);

    /// <summary>
    /// Exports scorecard to PowerPoint format
    /// </summary>
    Task<byte[]> ExportToPowerPointAsync(ExecutiveScorecard scorecard);

    /// <summary>
    /// Exports scorecard to PDF format
    /// </summary>
    Task<byte[]> ExportToPdfAsync(ExecutiveScorecard scorecard);

    /// <summary>
    /// Gets historical scorecards for trending
    /// </summary>
    Task<List<ExecutiveScorecard>> GetHistoricalScorecardsAsync(DateTime startDate, DateTime endDate);
}
