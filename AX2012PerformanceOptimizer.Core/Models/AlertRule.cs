namespace AX2012PerformanceOptimizer.Core.Models;

public enum AlertComparison
{
    GreaterThan,
    LessThan
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public class AlertRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MetricName { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public AlertComparison Comparison { get; set; } = AlertComparison.GreaterThan;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;
    public string Message { get; set; } = string.Empty;
}
