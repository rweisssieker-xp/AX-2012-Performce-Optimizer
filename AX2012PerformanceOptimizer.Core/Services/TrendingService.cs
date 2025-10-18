using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AX2012PerformanceOptimizer.Core.Services;

public class TrendingService : ITrendingService
{
    private readonly ConcurrentDictionary<string, List<(DateTime TimestampUtc, double Value)>> _store = new();

    public Task RecordAsync(string metricName, double value, DateTime timestampUtc)
    {
        var list = _store.GetOrAdd(metricName, _ => new List<(DateTime, double)>());
        lock (list)
        {
            list.Add((timestampUtc, value));
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<(DateTime TimestampUtc, double Value)>> QueryAsync(string metricName, DateTime fromUtc, DateTime toUtc)
    {
        if (!_store.TryGetValue(metricName, out var list))
        {
            return Task.FromResult((IReadOnlyList<(DateTime, double)>)Array.Empty<(DateTime, double)>());
        }
        List<(DateTime, double)> result;
        lock (list)
        {
            result = list.Where(x => x.TimestampUtc >= fromUtc && x.TimestampUtc <= toUtc)
                         .OrderBy(x => x.TimestampUtc)
                         .ToList();
        }
        return Task.FromResult((IReadOnlyList<(DateTime, double)>)result);
    }
}
