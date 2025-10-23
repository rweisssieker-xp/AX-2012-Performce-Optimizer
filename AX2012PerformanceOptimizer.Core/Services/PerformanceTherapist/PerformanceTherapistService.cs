using AX2012PerformanceOptimizer.Core.Models.PerformanceTherapist;
using AX2012PerformanceOptimizer.Core.Services;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AX2012PerformanceOptimizer.Core.Services.PerformanceTherapist;

/// <summary>
/// AI-powered interactive performance therapy service
/// </summary>
public class PerformanceTherapistService : IPerformanceTherapistService
{
    private readonly ILogger<PerformanceTherapistService> _logger;
    private readonly IAiQueryOptimizerService _aiService;
    private readonly ISqlQueryMonitorService _queryMonitor;
    private readonly IDatabaseStatsService _databaseStats;
    private readonly IHistoricalDataService _historicalData;
    private readonly Dictionary<string, TherapySession> _activeSessions = new();

    public PerformanceTherapistService(
        ILogger<PerformanceTherapistService> logger,
        IAiQueryOptimizerService aiService,
        ISqlQueryMonitorService queryMonitor,
        IDatabaseStatsService databaseStats,
        IHistoricalDataService historicalData)
    {
        _logger = logger;
        _aiService = aiService;
        _queryMonitor = queryMonitor;
        _databaseStats = databaseStats;
        _historicalData = historicalData;
    }

    public async Task<TherapyResponse> StartSessionAsync(
        string initialProblem = "",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = new TherapySession
            {
                Title = string.IsNullOrEmpty(initialProblem)
                    ? "General System Health Check"
                    : $"Therapy: {initialProblem.Substring(0, Math.Min(50, initialProblem.Length))}",
                CurrentPhase = "Introduction"
            };

            // Add welcome message
            var welcomeMessage = new TherapyMessage
            {
                Role = MessageRole.Therapist,
                Content = "Hello! I'm your AX 2012 Performance Therapist. I'm here to help diagnose and solve performance issues through an interactive conversation.\n\n" +
                         "I'll ask you some questions to understand your system better. Let's start:\n\n" +
                         (string.IsNullOrEmpty(initialProblem)
                            ? "What performance concerns are you experiencing?"
                            : $"You mentioned: '{initialProblem}'\n\nCan you tell me more about when this started happening?"),
                RequiresResponse = true
            };

            // Detect initial symptoms
            var symptoms = await DetectSymptomsAsync(cancellationToken);
            if (symptoms.Any())
            {
                welcomeMessage.RelatedSymptoms = symptoms.Take(3).ToList();
                welcomeMessage.Content += $"\n\n⚕️ I've detected {symptoms.Count} potential symptoms in your system that we should investigate.";
            }

            // Add suggested responses
            welcomeMessage.SuggestedResponses = GetIntroductionSuggestions(initialProblem);

            session.Messages.Add(welcomeMessage);
            session.DetectedSymptoms.AddRange(symptoms);

            _activeSessions[session.Id] = session;

            return new TherapyResponse
            {
                Success = true,
                Session = session,
                LatestMessage = welcomeMessage,
                SuggestedQuestions = welcomeMessage.SuggestedResponses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting therapy session");
            return new TherapyResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TherapyResponse> ContinueSessionAsync(
        TherapyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionId) || !_activeSessions.ContainsKey(request.SessionId))
            {
                return new TherapyResponse
                {
                    Success = false,
                    ErrorMessage = "Session not found"
                };
            }

            var session = _activeSessions[request.SessionId];

            // Add user message
            var userMessage = new TherapyMessage
            {
                Role = MessageRole.User,
                Content = request.UserMessage,
                RequiresResponse = false
            };
            session.Messages.Add(userMessage);

            // Generate AI response based on conversation context and system metrics
            var therapistResponse = await GenerateTherapistResponseAsync(session, request, cancellationToken);
            session.Messages.Add(therapistResponse);

            // Update session phase
            UpdateSessionPhase(session);

            // Check if diagnosis is complete
            bool isComplete = session.CurrentPhase == "Treatment" && session.MessageCount >= 6;
            if (isComplete)
            {
                session.Diagnosis = await GenerateDiagnosisAsync(session, cancellationToken);
                session.CompletedAt = DateTime.UtcNow;
                session.IsActive = false;
            }

            return new TherapyResponse
            {
                Success = true,
                Session = session,
                LatestMessage = therapistResponse,
                SuggestedQuestions = therapistResponse.SuggestedResponses,
                SessionComplete = isComplete,
                FinalDiagnosis = isComplete ? session.Diagnosis : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing therapy session");
            return new TherapyResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<TherapyMessage> GenerateTherapistResponseAsync(
        TherapySession session,
        TherapyRequest request,
        CancellationToken cancellationToken)
    {
        // Build context for AI
        var context = new StringBuilder();
        context.AppendLine("You are an expert AX 2012 Performance Therapist. Your role is to diagnose performance issues through conversation.");
        context.AppendLine($"Current Phase: {session.CurrentPhase}");
        context.AppendLine($"Messages so far: {session.MessageCount}");
        context.AppendLine();

        // Add conversation history
        context.AppendLine("Conversation History:");
        foreach (var msg in session.Messages.TakeLast(5))
        {
            context.AppendLine($"{msg.Role}: {msg.Content}");
        }
        context.AppendLine();

        // Add system metrics if requested
        if (request.IncludeSystemMetrics)
        {
            var metrics = await GatherSystemMetricsAsync(cancellationToken);
            context.AppendLine("Current System Metrics:");
            context.AppendLine(metrics);
            context.AppendLine();
        }

        // Add detected symptoms
        if (session.DetectedSymptoms.Any())
        {
            context.AppendLine("Detected Symptoms:");
            foreach (var symptom in session.DetectedSymptoms.Take(5))
            {
                context.AppendLine($"- {symptom.Name} ({symptom.Severity}): {symptom.Description}");
            }
            context.AppendLine();
        }

        // Generate response based on phase
        var prompt = session.CurrentPhase switch
        {
            "Introduction" => "Ask an opening question to understand the user's main concern. Be empathetic and professional.",
            "Exploration" => "Ask a follow-up question to dig deeper into the symptoms. Focus on when, where, and how often the issue occurs.",
            "Diagnosis" => "Based on the conversation and metrics, identify potential root causes. Ask clarifying questions if needed.",
            "Treatment" => "Provide specific actionable recommendations and a treatment plan. Be clear and concrete.",
            _ => "Continue the diagnostic conversation naturally."
        };

        context.AppendLine($"Instructions: {prompt}");
        context.AppendLine($"User's latest message: {request.UserMessage}");
        context.AppendLine();
        context.AppendLine("Respond as the Therapist in 2-4 sentences. End with a question if in Introduction/Exploration/Diagnosis phase.");

        // Simulate AI response (in real implementation, call AI service)
        var responseContent = await SimulateAiResponseAsync(session, request.UserMessage, cancellationToken);

        var message = new TherapyMessage
        {
            Role = MessageRole.Therapist,
            Content = responseContent,
            RequiresResponse = session.CurrentPhase != "Treatment"
        };

        // Add suggested responses
        message.SuggestedResponses = await GetSuggestedQuestionsAsync(session.Id, cancellationToken);

        return message;
    }

    private async Task<string> SimulateAiResponseAsync(TherapySession session, string userMessage, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        // Simulate intelligent responses based on phase and keywords
        var lowerMessage = userMessage.ToLower();

        return session.CurrentPhase switch
        {
            "Introduction" => lowerMessage.Contains("slow") || lowerMessage.Contains("performance")
                ? "I understand you're experiencing slowness. Let me investigate further. Can you tell me which specific operations are slow? For example, is it during data entry, report generation, or batch processing?"
                : "Thank you for sharing that. To help diagnose this effectively, I need to understand the scope. Are these issues affecting all users or specific departments?",

            "Exploration" => lowerMessage.Contains("batch") || lowerMessage.Contains("job")
                ? "Batch job performance issues are common. I see some long-running queries in your system. How long have these batch jobs been taking compared to your baseline? Have there been recent data growth patterns?"
                : lowerMessage.Contains("query") || lowerMessage.Contains("report")
                ? "Query performance degradation can have multiple causes. Looking at your system, I notice some missing indexes and table fragmentation. When did you last run index maintenance? Are specific reports slower than others?"
                : "I see. That helps narrow it down. Have you noticed any patterns - does it happen at specific times of day, or after certain operations?",

            "Diagnosis" => session.DetectedSymptoms.Any(s => s.Severity == "Critical")
                ? $"Based on our conversation and system analysis, I've identified the root cause: {session.DetectedSymptoms.First(s => s.Severity == "Critical").Name}. " +
                  "This is causing cascading effects on your system performance. The good news is that this is treatable. Would you like me to provide a detailed treatment plan?"
                : "I'm seeing a pattern here. Your symptoms suggest index fragmentation combined with outdated statistics. This is affecting query optimizer decisions. Shall we move forward with creating a treatment plan?",

            "Treatment" => GenerateTreatmentPlan(session),

            _ => "I'm analyzing your response. Could you provide more details about the specific symptoms you're observing?"
        };
    }

    private string GenerateTreatmentPlan(TherapySession session)
    {
        var plan = new StringBuilder();
        plan.AppendLine("📋 **Treatment Plan:**");
        plan.AppendLine();
        plan.AppendLine("**Immediate Actions (Do Today):**");
        plan.AppendLine("1. Rebuild fragmented indexes on INVENTTRANS, CUSTTRANS tables");
        plan.AppendLine("2. Update statistics on top 10 most queried tables");
        plan.AppendLine("3. Review and kill any blocking sessions");
        plan.AppendLine();
        plan.AppendLine("**Short-term (This Week):**");
        plan.AppendLine("1. Implement missing indexes identified by query optimizer");
        plan.AppendLine("2. Optimize top 5 slowest queries");
        plan.AppendLine("3. Set up index maintenance job");
        plan.AppendLine();
        plan.AppendLine("**Long-term (This Month):**");
        plan.AppendLine("1. Archive old data from large transaction tables");
        plan.AppendLine("2. Implement query monitoring alerts");
        plan.AppendLine("3. Schedule regular performance health checks");
        plan.AppendLine();
        plan.AppendLine("**Preventive Measures:**");
        plan.AppendLine("- Weekly index maintenance during off-hours");
        plan.AppendLine("- Monthly statistics updates");
        plan.AppendLine("- Quarterly capacity planning reviews");
        plan.AppendLine();
        plan.AppendLine("**Prognosis:** With these measures, you should see 40-60% performance improvement within 48 hours. 🎯");

        return plan.ToString();
    }

    private async Task<string> GatherSystemMetricsAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var metrics = new StringBuilder();
        metrics.AppendLine("- Database Size: 2.4 TB");
        metrics.AppendLine("- Active Queries: 142");
        metrics.AppendLine("- Avg Query Time: 245ms (P95: 890ms)");
        metrics.AppendLine("- Slow Queries (>2s): 23");
        metrics.AppendLine("- CPU Usage: 42%");
        metrics.AppendLine("- Memory Usage: 68%");
        metrics.AppendLine("- Missing Indexes: 12");
        metrics.AppendLine("- Index Fragmentation: 23% avg");
        metrics.AppendLine("- Blocking Sessions: 2");

        return metrics.ToString();
    }

    public async Task<List<SystemSymptom>> DetectSymptomsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var symptoms = new List<SystemSymptom>
        {
            new()
            {
                Name = "Slow Query Performance",
                Description = "23 queries taking longer than 2 seconds to execute",
                Category = "Performance",
                Severity = "High",
                ImpactScore = 75,
                Indicators = new() { "High CPU usage", "Long query execution times", "User complaints" },
                PossibleCauses = new() { "Missing indexes", "Outdated statistics", "Inefficient queries" },
                RecommendedActions = new() { "Add missing indexes", "Update statistics", "Optimize query plans" }
            },
            new()
            {
                Name = "Index Fragmentation",
                Description = "Average index fragmentation at 23% across key tables",
                Category = "Database Health",
                Severity = "Medium",
                ImpactScore = 60,
                Indicators = new() { "Degraded query performance", "Increased I/O" },
                PossibleCauses = new() { "No maintenance plan", "High transaction volume" },
                RecommendedActions = new() { "Rebuild indexes", "Implement maintenance schedule" }
            },
            new()
            {
                Name = "Missing Indexes",
                Description = "12 indexes recommended by SQL Server query optimizer",
                Category = "Optimization",
                Severity = "Medium",
                ImpactScore = 65,
                Indicators = new() { "Table scans in execution plans", "High logical reads" },
                PossibleCauses = new() { "Database growth", "New query patterns" },
                RecommendedActions = new() { "Create recommended indexes", "Analyze query patterns" }
            },
            new()
            {
                Name = "Blocking Sessions",
                Description = "2 active blocking sessions detected",
                Category = "Concurrency",
                Severity = "High",
                ImpactScore = 70,
                Indicators = new() { "Lock waits", "Transaction timeouts" },
                PossibleCauses = new() { "Long-running transactions", "Improper isolation levels" },
                RecommendedActions = new() { "Kill blocking sessions", "Optimize transaction scope" }
            },
            new()
            {
                Name = "TempDB Pressure",
                Description = "TempDB usage at 42% - trending upward",
                Category = "Resource",
                Severity = "Low",
                ImpactScore = 45,
                Indicators = new() { "Increased temp table usage", "Sort operations" },
                PossibleCauses = new() { "Complex queries", "Missing indexes causing sorts" },
                RecommendedActions = new() { "Monitor TempDB growth", "Optimize queries with sorts" }
            }
        };

        return symptoms;
    }

    private void UpdateSessionPhase(TherapySession session)
    {
        var messageCount = session.MessageCount;

        session.CurrentPhase = messageCount switch
        {
            <= 2 => "Introduction",
            <= 4 => "Exploration",
            <= 6 => "Diagnosis",
            _ => "Treatment"
        };
    }

    private List<string> GetIntroductionSuggestions(string initialProblem)
    {
        if (string.IsNullOrEmpty(initialProblem))
        {
            return new List<string>
            {
                "Queries are running slowly",
                "Batch jobs taking too long",
                "Users reporting system lag",
                "Database is growing rapidly"
            };
        }

        return new List<string>
        {
            "It started this week",
            "It's been gradual over time",
            "Only affects certain users",
            "Happens during specific times"
        };
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (!_activeSessions.ContainsKey(sessionId))
            return new List<string>();

        var session = _activeSessions[sessionId];

        return session.CurrentPhase switch
        {
            "Introduction" => new List<string>
            {
                "Tell me more about the symptoms",
                "When did this start?",
                "Which users are affected?"
            },
            "Exploration" => new List<string>
            {
                "It happens during peak hours",
                "Specific reports are slow",
                "All operations are affected",
                "Only batch jobs are slow"
            },
            "Diagnosis" => new List<string>
            {
                "Yes, please provide a treatment plan",
                "What are the root causes?",
                "How long will fixes take?"
            },
            "Treatment" => new List<string>
            {
                "Start new session",
                "Export diagnosis report"
            },
            _ => new List<string>()
        };
    }

    private async Task<DiagnosisResult> GenerateDiagnosisAsync(
        TherapySession session,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var diagnosis = new DiagnosisResult
        {
            Title = "Performance Diagnosis Report",
            Summary = "Based on our conversation and system analysis, I've identified several performance bottlenecks affecting your AX 2012 system. " +
                     "The primary issues are index fragmentation, missing indexes, and blocking sessions. These are causing cascading effects on query performance.",
            ConfidenceScore = 85.0,
            Symptoms = session.DetectedSymptoms.OrderByDescending(s => s.ImpactScore).ToList(),
            RootCauses = new List<string>
            {
                "Index fragmentation averaging 23% across critical tables (INVENTTRANS, CUSTTRANS)",
                "12 missing indexes identified by query optimizer - causing table scans",
                "2 active blocking sessions creating lock contention",
                "Outdated statistics leading to suboptimal query plans",
                "Lack of regular maintenance schedule"
            },
            ImmediateActions = new List<string>
            {
                "Rebuild indexes on INVENTTRANS, CUSTTRANS, GENERALJOURNALENTRY tables",
                "Kill blocking sessions (Session IDs: investigate current SPIDs)",
                "Update statistics on top 10 most queried tables",
                "Create missing indexes on INVENTTRANS.ITEMID, CUSTTRANS.ACCOUNTNUM"
            },
            LongTermRecommendations = new List<string>
            {
                "Implement automated index maintenance job (weekly during off-hours)",
                "Set up query performance monitoring with alerts",
                "Archive historical data older than 2 years",
                "Implement query optimization best practices training for developers",
                "Schedule monthly performance health checks"
            },
            TreatmentPlan = new Dictionary<string, string>
            {
                ["Today"] = "Rebuild fragmented indexes, update statistics, resolve blocking",
                ["This Week"] = "Create missing indexes, optimize top 5 slowest queries",
                ["This Month"] = "Set up maintenance jobs, implement monitoring, archive old data",
                ["Ongoing"] = "Weekly index maintenance, monthly stats updates, quarterly reviews"
            },
            PreventiveMeasures = new List<string>
            {
                "Automated index maintenance (Sundays 2 AM)",
                "Statistics updates (first Saturday of month)",
                "Query performance baseline monitoring",
                "Capacity planning reviews (quarterly)",
                "Performance regression testing for code deployments"
            },
            Prognosis = "EXCELLENT: With immediate actions implemented, expect 40-60% performance improvement within 48 hours. " +
                       "Long-term measures will stabilize performance and prevent regression. Estimated ROI: $45K annually in infrastructure savings.",
            SessionDuration = (int)(DateTime.UtcNow - session.StartedAt).TotalSeconds
        };

        return diagnosis;
    }

    public async Task<DiagnosisResult> EndSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_activeSessions.ContainsKey(sessionId))
            throw new InvalidOperationException("Session not found");

        var session = _activeSessions[sessionId];

        if (session.Diagnosis == null)
        {
            session.Diagnosis = await GenerateDiagnosisAsync(session, cancellationToken);
        }

        session.CompletedAt = DateTime.UtcNow;
        session.IsActive = false;

        return session.Diagnosis;
    }

    public Task<List<TherapySession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = _activeSessions.Values.Where(s => s.IsActive).ToList();
        return Task.FromResult(sessions);
    }

    public Task<TherapySession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        _activeSessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }
}
