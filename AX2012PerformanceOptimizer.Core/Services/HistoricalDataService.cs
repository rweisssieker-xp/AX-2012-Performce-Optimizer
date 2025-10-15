using AX2012PerformanceOptimizer.Core.Models;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace AX2012PerformanceOptimizer.Core.Services;

public class HistoricalDataService : IHistoricalDataService
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public HistoricalDataService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "AX2012PerformanceOptimizer");
        Directory.CreateDirectory(appFolder);
        _databasePath = Path.Combine(appFolder, "history.db");
        _connectionString = $"Data Source={_databasePath}";
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var createTables = @"
            CREATE TABLE IF NOT EXISTS DashboardSnapshots (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                ActiveUsers INTEGER NOT NULL,
                RunningJobs INTEGER NOT NULL,
                DatabaseSizeMB INTEGER NOT NULL,
                ExpensiveQueries INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS QueryPerformanceHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                QueryHash TEXT NOT NULL,
                AvgCpuTimeMs REAL NOT NULL,
                ExecutionCount INTEGER NOT NULL,
                AvgLogicalReads INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS AosMetricHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                ActiveSessions INTEGER NOT NULL,
                CpuPercent REAL NOT NULL,
                MemoryMB INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS BatchJobHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                RunningCount INTEGER NOT NULL,
                FailedCount INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS DatabaseSizeHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                TotalSizeMB INTEGER NOT NULL,
                DataSizeMB INTEGER NOT NULL,
                LogSizeMB INTEGER NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_dashboard_timestamp ON DashboardSnapshots(Timestamp);
            CREATE INDEX IF NOT EXISTS idx_query_timestamp ON QueryPerformanceHistory(Timestamp);
            CREATE INDEX IF NOT EXISTS idx_query_hash ON QueryPerformanceHistory(QueryHash);
            CREATE INDEX IF NOT EXISTS idx_aos_timestamp ON AosMetricHistory(Timestamp);
            CREATE INDEX IF NOT EXISTS idx_batch_timestamp ON BatchJobHistory(Timestamp);
            CREATE INDEX IF NOT EXISTS idx_dbsize_timestamp ON DatabaseSizeHistory(Timestamp);
        ";

        using var command = new SqliteCommand(createTables, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveDashboardMetricsAsync(int activeUsers, int runningJobs, long dbSizeMB, int expensiveQueries)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"INSERT INTO DashboardSnapshots (Timestamp, ActiveUsers, RunningJobs, DatabaseSizeMB, ExpensiveQueries)
                    VALUES (@timestamp, @activeUsers, @runningJobs, @dbSizeMB, @expensiveQueries)";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@activeUsers", activeUsers);
        command.Parameters.AddWithValue("@runningJobs", runningJobs);
        command.Parameters.AddWithValue("@dbSizeMB", dbSizeMB);
        command.Parameters.AddWithValue("@expensiveQueries", expensiveQueries);

        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveQueryMetricsAsync(IEnumerable<SqlQueryMetric> queries)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        var sql = @"INSERT INTO QueryPerformanceHistory (Timestamp, QueryHash, AvgCpuTimeMs, ExecutionCount, AvgLogicalReads)
                    VALUES (@timestamp, @queryHash, @avgCpuTimeMs, @executionCount, @avgLogicalReads)";

        foreach (var query in queries.Take(10)) // Top 10 only
        {
            var queryHash = ComputeHash(query.QueryText);

            using var command = new SqliteCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@queryHash", queryHash);
            command.Parameters.AddWithValue("@avgCpuTimeMs", query.AvgCpuTimeMs);
            command.Parameters.AddWithValue("@executionCount", query.ExecutionCount);
            command.Parameters.AddWithValue("@avgLogicalReads", query.AvgLogicalReads);

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task SaveAosMetricsAsync(AosMetric metrics)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"INSERT INTO AosMetricHistory (Timestamp, ActiveSessions, CpuPercent, MemoryMB)
                    VALUES (@timestamp, @activeSessions, @cpuPercent, @memoryMB)";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@activeSessions", metrics.ActiveUserSessions);
        command.Parameters.AddWithValue("@cpuPercent", metrics.CpuUsagePercent);
        command.Parameters.AddWithValue("@memoryMB", metrics.MemoryUsageMB);

        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveBatchJobMetricsAsync(int runningCount, int failedCount)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"INSERT INTO BatchJobHistory (Timestamp, RunningCount, FailedCount)
                    VALUES (@timestamp, @runningCount, @failedCount)";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@runningCount", runningCount);
        command.Parameters.AddWithValue("@failedCount", failedCount);

        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveDatabaseMetricsAsync(DatabaseMetric metrics)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"INSERT INTO DatabaseSizeHistory (Timestamp, TotalSizeMB, DataSizeMB, LogSizeMB)
                    VALUES (@timestamp, @totalSizeMB, @dataSizeMB, @logSizeMB)";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@totalSizeMB", metrics.TotalSizeMB);
        command.Parameters.AddWithValue("@dataSizeMB", metrics.DataSizeMB);
        command.Parameters.AddWithValue("@logSizeMB", metrics.LogSizeMB);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<DashboardSnapshot>> GetDashboardHistoryAsync(DateTime from, DateTime to)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT Timestamp, ActiveUsers, RunningJobs, DatabaseSizeMB, ExpensiveQueries
                    FROM DashboardSnapshots
                    WHERE Timestamp >= @from AND Timestamp <= @to
                    ORDER BY Timestamp";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@from", from.ToString("O"));
        command.Parameters.AddWithValue("@to", to.ToString("O"));

        var results = new List<DashboardSnapshot>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new DashboardSnapshot
            {
                Timestamp = DateTime.Parse(reader.GetString(0)),
                ActiveUsers = reader.GetInt32(1),
                RunningJobs = reader.GetInt32(2),
                DatabaseSizeMB = reader.GetInt32(3),
                ExpensiveQueries = reader.GetInt32(4)
            });
        }

        return results;
    }

    public async Task<List<QueryPerformanceHistory>> GetQueryPerformanceHistoryAsync(string queryHash, DateTime from, DateTime to)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT Timestamp, QueryHash, AvgCpuTimeMs, ExecutionCount, AvgLogicalReads
                    FROM QueryPerformanceHistory
                    WHERE QueryHash = @queryHash AND Timestamp >= @from AND Timestamp <= @to
                    ORDER BY Timestamp";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@queryHash", queryHash);
        command.Parameters.AddWithValue("@from", from.ToString("O"));
        command.Parameters.AddWithValue("@to", to.ToString("O"));

        var results = new List<QueryPerformanceHistory>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new QueryPerformanceHistory
            {
                Timestamp = DateTime.Parse(reader.GetString(0)),
                QueryHash = reader.GetString(1),
                AvgCpuTimeMs = reader.GetDouble(2),
                ExecutionCount = reader.GetInt64(3),
                AvgLogicalReads = reader.GetInt64(4)
            });
        }

        return results;
    }

    public async Task<List<AosMetricHistory>> GetAosHistoryAsync(DateTime from, DateTime to)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT Timestamp, ActiveSessions, CpuPercent, MemoryMB
                    FROM AosMetricHistory
                    WHERE Timestamp >= @from AND Timestamp <= @to
                    ORDER BY Timestamp";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@from", from.ToString("O"));
        command.Parameters.AddWithValue("@to", to.ToString("O"));

        var results = new List<AosMetricHistory>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new AosMetricHistory
            {
                Timestamp = DateTime.Parse(reader.GetString(0)),
                ActiveSessions = reader.GetInt32(1),
                CpuPercent = reader.GetDouble(2),
                MemoryMB = reader.GetInt64(3)
            });
        }

        return results;
    }

    public async Task<List<BatchJobHistory>> GetBatchJobHistoryAsync(DateTime from, DateTime to)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT Timestamp, RunningCount, FailedCount
                    FROM BatchJobHistory
                    WHERE Timestamp >= @from AND Timestamp <= @to
                    ORDER BY Timestamp";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@from", from.ToString("O"));
        command.Parameters.AddWithValue("@to", to.ToString("O"));

        var results = new List<BatchJobHistory>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new BatchJobHistory
            {
                Timestamp = DateTime.Parse(reader.GetString(0)),
                RunningCount = reader.GetInt32(1),
                FailedCount = reader.GetInt32(2)
            });
        }

        return results;
    }

    public async Task<List<DatabaseSizeHistory>> GetDatabaseSizeHistoryAsync(DateTime from, DateTime to)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT Timestamp, TotalSizeMB, DataSizeMB, LogSizeMB
                    FROM DatabaseSizeHistory
                    WHERE Timestamp >= @from AND Timestamp <= @to
                    ORDER BY Timestamp";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@from", from.ToString("O"));
        command.Parameters.AddWithValue("@to", to.ToString("O"));

        var results = new List<DatabaseSizeHistory>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new DatabaseSizeHistory
            {
                Timestamp = DateTime.Parse(reader.GetString(0)),
                TotalSizeMB = reader.GetInt64(1),
                DataSizeMB = reader.GetInt64(2),
                LogSizeMB = reader.GetInt64(3)
            });
        }

        return results;
    }

    public async Task<TrendAnalysis> AnalyzeTrendAsync(string metricType, DateTime from, DateTime to)
    {
        var values = new List<double>();

        // Get values based on metric type
        if (metricType == "ActiveUsers")
        {
            var history = await GetDashboardHistoryAsync(from, to);
            values = history.Select(h => (double)h.ActiveUsers).ToList();
        }
        else if (metricType == "DatabaseSize")
        {
            var history = await GetDatabaseSizeHistoryAsync(from, to);
            values = history.Select(h => (double)h.TotalSizeMB).ToList();
        }

        if (values.Count == 0)
        {
            return new TrendAnalysis { MetricName = metricType };
        }

        var current = values.Last();
        var average = values.Average();
        var min = values.Min();
        var max = values.Max();

        // Calculate standard deviation
        var sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
        var stdDev = Math.Sqrt(sumOfSquares / values.Count);

        // Determine trend
        var firstHalf = values.Take(values.Count / 2).Average();
        var secondHalf = values.Skip(values.Count / 2).Average();
        var changePercent = ((secondHalf - firstHalf) / firstHalf) * 100;

        var trend = TrendDirection.Stable;
        if (Math.Abs(changePercent) < 5) trend = TrendDirection.Stable;
        else if (changePercent > 0) trend = TrendDirection.Increasing;
        else trend = TrendDirection.Decreasing;

        if (stdDev > average * 0.2) trend = TrendDirection.Volatile;

        return new TrendAnalysis
        {
            MetricName = metricType,
            CurrentValue = current,
            AverageValue = average,
            MinValue = min,
            MaxValue = max,
            StandardDeviation = stdDev,
            Trend = trend,
            ChangePercent = changePercent
        };
    }

    public async Task<Dictionary<string, double>> GetBaselineMetricsAsync()
    {
        var baseline = new Dictionary<string, double>();
        var from = DateTime.UtcNow.AddDays(-30);
        var to = DateTime.UtcNow;

        var dashboardHistory = await GetDashboardHistoryAsync(from, to);
        if (dashboardHistory.Count > 0)
        {
            baseline["ActiveUsers"] = dashboardHistory.Average(h => h.ActiveUsers);
            baseline["RunningJobs"] = dashboardHistory.Average(h => h.RunningJobs);
            baseline["ExpensiveQueries"] = dashboardHistory.Average(h => h.ExpensiveQueries);
        }

        var dbHistory = await GetDatabaseSizeHistoryAsync(from, to);
        if (dbHistory.Count > 0)
        {
            baseline["DatabaseSize"] = dbHistory.Average(h => h.TotalSizeMB);
        }

        return baseline;
    }

    public async Task CleanupOldDataAsync(int daysToKeep)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep).ToString("O");

        var tables = new[]
        {
            "DashboardSnapshots",
            "QueryPerformanceHistory",
            "AosMetricHistory",
            "BatchJobHistory",
            "DatabaseSizeHistory"
        };

        foreach (var table in tables)
        {
            var sql = $"DELETE FROM {table} WHERE Timestamp < @cutoffDate";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@cutoffDate", cutoffDate);
            await command.ExecuteNonQueryAsync();
        }

        // Vacuum to reclaim space
        using var vacuumCommand = new SqliteCommand("VACUUM", connection);
        await vacuumCommand.ExecuteNonQueryAsync();
    }

    private string ComputeHash(string text)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash)[..16]; // First 16 chars
    }
}
