using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models;

namespace AX2012PerformanceOptimizer.Core.Services;

public class AlertService : IAlertService
{
    private readonly List<AlertRule> _rules = new();
    private readonly object _lock = new();

    public Task AddRuleAsync(AlertRule rule)
    {
        lock (_lock)
        {
            var idx = _rules.FindIndex(r => r.Id == rule.Id);
            if (idx >= 0)
            {
                _rules[idx] = rule;
            }
            else
            {
                _rules.Add(rule);
            }
        }
        return Task.CompletedTask;
    }

    public Task RemoveRuleAsync(string ruleId)
    {
        lock (_lock)
        {
            var existing = _rules.FirstOrDefault(r => r.Id == ruleId);
            if (existing != null)
            {
                _rules.Remove(existing);
            }
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AlertRule>> GetRulesAsync()
    {
        lock (_lock)
        {
            return Task.FromResult((IReadOnlyList<AlertRule>)_rules.ToList());
        }
    }

    public Task<IReadOnlyList<AlertRule>> EvaluateAsync(string metricName, double value, DateTime timestampUtc)
    {
        List<AlertRule> matches;
        lock (_lock)
        {
            matches = _rules.Where(r => r.MetricName == metricName &&
                ((r.Comparison == AlertComparison.GreaterThan && value > r.Threshold) ||
                 (r.Comparison == AlertComparison.LessThan && value < r.Threshold)))
                .ToList();
        }
        return Task.FromResult((IReadOnlyList<AlertRule>)matches);
    }
}
