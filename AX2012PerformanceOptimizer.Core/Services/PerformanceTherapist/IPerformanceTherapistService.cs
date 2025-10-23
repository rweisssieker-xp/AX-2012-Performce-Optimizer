using AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;

namespace AX2012PerformanceOptimizer.Core.Services.PerformanceTherapist;

/// <summary>
/// AI-powered interactive performance therapy service
/// </summary>
public interface IPerformanceTherapistService
{
    /// <summary>
    /// Start a new therapy session
    /// </summary>
    Task<TherapyResponse> StartSessionAsync(
        string initialProblem = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Continue an existing therapy session
    /// </summary>
    Task<TherapyResponse> ContinueSessionAsync(
        TherapyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active sessions
    /// </summary>
    Task<List<TherapySession>> GetActiveSessionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get session by ID
    /// </summary>
    Task<TherapySession?> GetSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// End session and get final diagnosis
    /// </summary>
    Task<DiagnosisResult> EndSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect system symptoms automatically
    /// </summary>
    Task<List<SystemSymptom>> DetectSymptomsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get suggested questions for current session phase
    /// </summary>
    Task<List<string>> GetSuggestedQuestionsAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}
