using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AX2012PerformanceOptimizer.Core.Services;

/// <summary>
/// Service for analyzing and managing SQL Server configuration settings
/// </summary>
public class ServerConfigurationService : IServerConfigurationService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<ServerConfigurationService> _logger;

    public ServerConfigurationService(
        ISqlConnectionManager connectionManager,
        ILogger<ServerConfigurationService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<List<ServerConfiguration>> GetAllConfigurationsAsync()
    {
        var configurations = new List<ServerConfiguration>();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();

            var query = @"
                SELECT
                    name,
                    CAST(value AS INT) as configured_value,
                    CAST(value_in_use AS INT) as running_value,
                    CAST(minimum AS INT) as min_value,
                    CAST(maximum AS INT) as max_value,
                    CAST(is_advanced AS BIT) as is_advanced,
                    CAST(is_dynamic AS BIT) as is_dynamic
                FROM sys.configurations
                ORDER BY name";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                configurations.Add(new ServerConfiguration
                {
                    Name = reader.GetString(0),
                    ConfiguredValue = reader.GetInt32(1),
                    RunningValue = reader.GetInt32(2),
                    MinValue = reader.GetInt32(3),
                    MaxValue = reader.GetInt32(4),
                    IsAdvanced = reader.GetBoolean(5),
                    IsDynamic = reader.GetBoolean(6),
                    Description = GetConfigurationDescription(reader.GetString(0))
                });
            }

            _logger.LogInformation($"Retrieved {configurations.Count} server configurations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server configurations");
            throw;
        }

        return configurations;
    }

    public async Task<ServerConfiguration?> GetConfigurationAsync(string name)
    {
        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();

            var query = @"
                SELECT
                    name,
                    CAST(value AS INT) as configured_value,
                    CAST(value_in_use AS INT) as running_value,
                    CAST(minimum AS INT) as min_value,
                    CAST(maximum AS INT) as max_value,
                    CAST(is_advanced AS BIT) as is_advanced,
                    CAST(is_dynamic AS BIT) as is_dynamic
                FROM sys.configurations
                WHERE name = @name";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ServerConfiguration
                {
                    Name = reader.GetString(0),
                    ConfiguredValue = reader.GetInt32(1),
                    RunningValue = reader.GetInt32(2),
                    MinValue = reader.GetInt32(3),
                    MaxValue = reader.GetInt32(4),
                    IsAdvanced = reader.GetBoolean(5),
                    IsDynamic = reader.GetBoolean(6),
                    Description = GetConfigurationDescription(reader.GetString(0))
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting configuration '{name}'");
        }

        return null;
    }

    public async Task<List<ConfigurationRecommendation>> AnalyzeConfigurationAsync()
    {
        var recommendations = new List<ConfigurationRecommendation>();

        try
        {
            var configs = await GetAllConfigurationsAsync();
            var memoryInfo = await GetServerMemoryInfoAsync();

            // Analyze Max Server Memory
            var maxMemConfig = configs.FirstOrDefault(c => c.Name == "max server memory (MB)");
            if (maxMemConfig != null)
            {
                recommendations.AddRange(AnalyzeMaxServerMemory(maxMemConfig, memoryInfo));
            }

            // Analyze MAXDOP
            var maxdopConfig = configs.FirstOrDefault(c => c.Name == "max degree of parallelism");
            if (maxdopConfig != null)
            {
                recommendations.AddRange(AnalyzeMaxDOP(maxdopConfig));
            }

            // Analyze Cost Threshold for Parallelism
            var costThresholdConfig = configs.FirstOrDefault(c => c.Name == "cost threshold for parallelism");
            if (costThresholdConfig != null)
            {
                recommendations.AddRange(AnalyzeCostThreshold(costThresholdConfig));
            }

            // Analyze Optimize for Ad Hoc Workloads
            var adHocConfig = configs.FirstOrDefault(c => c.Name == "optimize for ad hoc workloads");
            if (adHocConfig != null)
            {
                recommendations.AddRange(AnalyzeAdHocWorkloads(adHocConfig));
            }

            // Analyze Backup Compression
            var backupCompConfig = configs.FirstOrDefault(c => c.Name == "backup compression default");
            if (backupCompConfig != null)
            {
                recommendations.AddRange(AnalyzeBackupCompression(backupCompConfig));
            }

            _logger.LogInformation($"Generated {recommendations.Count} configuration recommendations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing server configuration");
            throw;
        }

        return recommendations.OrderByDescending(r => r.Priority).ThenByDescending(r => r.ImpactScore).ToList();
    }

    public async Task<ServerMemoryInfo> GetServerMemoryInfoAsync()
    {
        var memoryInfo = new ServerMemoryInfo();

        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();

            // Get physical memory
            var memoryQuery = @"
                SELECT
                    total_physical_memory_kb / 1024 as total_memory_mb,
                    available_physical_memory_kb / 1024 as available_memory_mb
                FROM sys.dm_os_sys_memory";

            using var memCommand = new SqlCommand(memoryQuery, connection);
            using var memReader = await memCommand.ExecuteReaderAsync();

            if (await memReader.ReadAsync())
            {
                memoryInfo.TotalPhysicalMemoryMB = memReader.GetInt64(0);
                memoryInfo.AvailablePhysicalMemoryMB = memReader.GetInt64(1);
            }
            memReader.Close();

            // Get SQL Server memory usage
            var sqlMemoryQuery = @"
                SELECT
                    committed_kb / 1024 as committed_mb
                FROM sys.dm_os_sys_info";

            using var sqlMemCommand = new SqlCommand(sqlMemoryQuery, connection);
            var committedMB = await sqlMemCommand.ExecuteScalarAsync();
            if (committedMB != null)
            {
                memoryInfo.SqlServerCommittedMB = Convert.ToInt64(committedMB);
            }

            // Get max/min server memory config
            var configQuery = @"
                SELECT
                    name,
                    CAST(value AS INT) as config_value
                FROM sys.configurations
                WHERE name IN ('max server memory (MB)', 'min server memory (MB)')";

            using var configCommand = new SqlCommand(configQuery, connection);
            using var configReader = await configCommand.ExecuteReaderAsync();

            while (await configReader.ReadAsync())
            {
                var name = configReader.GetString(0);
                var value = configReader.GetInt32(1);

                if (name == "max server memory (MB)")
                    memoryInfo.MaxServerMemoryMB = value;
                else if (name == "min server memory (MB)")
                    memoryInfo.MinServerMemoryMB = value;
            }

            // Calculate recommended max server memory
            // Leave 4GB for OS, or 20% of total memory, whichever is larger
            var reserveForOS = Math.Max(4096, memoryInfo.TotalPhysicalMemoryMB * 0.2);
            memoryInfo.RecommendedMaxServerMemoryMB = (long)(memoryInfo.TotalPhysicalMemoryMB - reserveForOS);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server memory info");
            throw;
        }

        return memoryInfo;
    }

    public async Task<bool> ApplyRecommendationAsync(ConfigurationRecommendation recommendation)
    {
        try
        {
            var script = GenerateApplyScript(recommendation);

            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(script, connection);
            command.CommandTimeout = 60;

            await command.ExecuteNonQueryAsync();

            _logger.LogInformation($"Successfully applied recommendation for '{recommendation.SettingName}'");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error applying recommendation for '{recommendation.SettingName}'");
            return false;
        }
    }

    public string GenerateApplyScript(ConfigurationRecommendation recommendation)
    {
        var script = $@"
-- Apply recommendation for {recommendation.SettingName}
-- Current Value: {recommendation.CurrentValue}
-- Recommended Value: {recommendation.RecommendedValue}
-- Reason: {recommendation.Reason}

";

        if (recommendation.SettingName == "max server memory (MB)" ||
            recommendation.SettingName == "min server memory (MB)")
        {
            script += $@"
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure '{recommendation.SettingName}', {recommendation.RecommendedValue};
RECONFIGURE;
";
        }
        else if (recommendation.SettingName == "max degree of parallelism" ||
                 recommendation.SettingName == "cost threshold for parallelism")
        {
            script += $@"
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure '{recommendation.SettingName}', {recommendation.RecommendedValue};
RECONFIGURE;
";
        }
        else if (recommendation.SettingName == "optimize for ad hoc workloads" ||
                 recommendation.SettingName == "backup compression default")
        {
            script += $@"
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure '{recommendation.SettingName}', {recommendation.RecommendedValue};
RECONFIGURE;
";
        }
        else
        {
            script += $@"
EXEC sp_configure '{recommendation.SettingName}', {recommendation.RecommendedValue};
RECONFIGURE;
";
        }

        if (recommendation.RequiresRestart)
        {
            script += @"
-- NOTE: This setting requires a SQL Server restart to take effect
";
        }

        return script;
    }

    #region Private Analysis Methods

    private List<ConfigurationRecommendation> AnalyzeMaxServerMemory(
        ServerConfiguration config,
        ServerMemoryInfo memoryInfo)
    {
        var recommendations = new List<ConfigurationRecommendation>();

        // Default is 2147483647 (essentially unlimited)
        if (config.ConfiguredValue >= 2147483647 || config.ConfiguredValue > memoryInfo.RecommendedMaxServerMemoryMB)
        {
            recommendations.Add(new ConfigurationRecommendation
            {
                SettingName = config.Name,
                CurrentValue = config.ConfiguredValue,
                RecommendedValue = (int)memoryInfo.RecommendedMaxServerMemoryMB,
                Priority = RecommendationPriority.High,
                Category = RecommendationCategory.Memory,
                Reason = "Max server memory is not properly configured. SQL Server may consume all available memory, starving the OS.",
                ExpectedImpact = "Prevents OS memory starvation, improves system stability",
                Details = $"Total Physical Memory: {memoryInfo.TotalPhysicalMemoryMB:N0} MB\n" +
                         $"Recommended Max: {memoryInfo.RecommendedMaxServerMemoryMB:N0} MB\n" +
                         $"This leaves sufficient memory for the OS and other processes.",
                RequiresRestart = false,
                ImpactScore = 85
            });
        }

        return recommendations;
    }

    private List<ConfigurationRecommendation> AnalyzeMaxDOP(ServerConfiguration config)
    {
        var recommendations = new List<ConfigurationRecommendation>();

        // Default is 0 (use all processors)
        if (config.ConfiguredValue == 0)
        {
            recommendations.Add(new ConfigurationRecommendation
            {
                SettingName = config.Name,
                CurrentValue = config.ConfiguredValue,
                RecommendedValue = 4, // Conservative default for AX 2012
                Priority = RecommendationPriority.High,
                Category = RecommendationCategory.Parallelism,
                Reason = "MAXDOP is set to 0 (unlimited). This can cause excessive parallelism and CXPACKET waits.",
                ExpectedImpact = "Reduces CXPACKET waits, improves query predictability",
                Details = "For AX 2012, MAXDOP = 4 is a good starting point.\n" +
                         "Monitor query performance and adjust as needed.\n" +
                         "Consider using Resource Governor for specific workloads.",
                RequiresRestart = false,
                ImpactScore = 75
            });
        }

        return recommendations;
    }

    private List<ConfigurationRecommendation> AnalyzeCostThreshold(ServerConfiguration config)
    {
        var recommendations = new List<ConfigurationRecommendation>();

        // Default is 5, which is too low
        if (config.ConfiguredValue < 30)
        {
            recommendations.Add(new ConfigurationRecommendation
            {
                SettingName = config.Name,
                CurrentValue = config.ConfiguredValue,
                RecommendedValue = 50,
                Priority = RecommendationPriority.Medium,
                Category = RecommendationCategory.Parallelism,
                Reason = $"Cost threshold ({config.ConfiguredValue}) is too low. Queries parallelize too aggressively.",
                ExpectedImpact = "Reduces unnecessary parallelism for small queries",
                Details = "Recommended value is 50 for OLTP workloads like AX 2012.\n" +
                         "Only queries with cost > 50 will use parallel execution.\n" +
                         "This prevents small queries from consuming parallel threads.",
                RequiresRestart = false,
                ImpactScore = 60
            });
        }

        return recommendations;
    }

    private List<ConfigurationRecommendation> AnalyzeAdHocWorkloads(ServerConfiguration config)
    {
        var recommendations = new List<ConfigurationRecommendation>();

        // Should be enabled (1) for AX 2012
        if (config.ConfiguredValue == 0)
        {
            recommendations.Add(new ConfigurationRecommendation
            {
                SettingName = config.Name,
                CurrentValue = config.ConfiguredValue,
                RecommendedValue = 1,
                Priority = RecommendationPriority.Medium,
                Category = RecommendationCategory.Performance,
                Reason = "Optimize for ad hoc workloads is disabled. Plan cache may be bloated with single-use plans.",
                ExpectedImpact = "Reduces plan cache bloat, frees up memory",
                Details = "AX 2012 generates many ad-hoc queries.\n" +
                         "Enabling this setting stores only a plan stub initially.\n" +
                         "Full plan is cached only on second execution.",
                RequiresRestart = false,
                ImpactScore = 50
            });
        }

        return recommendations;
    }

    private List<ConfigurationRecommendation> AnalyzeBackupCompression(ServerConfiguration config)
    {
        var recommendations = new List<ConfigurationRecommendation>();

        // Should be enabled (1) if SQL Server edition supports it
        if (config.ConfiguredValue == 0)
        {
            recommendations.Add(new ConfigurationRecommendation
            {
                SettingName = config.Name,
                CurrentValue = config.ConfiguredValue,
                RecommendedValue = 1,
                Priority = RecommendationPriority.Low,
                Category = RecommendationCategory.Backup,
                Reason = "Backup compression is disabled. Backups are larger and slower.",
                ExpectedImpact = "Faster backups, reduced backup storage requirements",
                Details = "Backup compression typically provides 50-70% space savings.\n" +
                         "CPU overhead is minimal on modern hardware.\n" +
                         "Note: Requires Enterprise or Standard Edition (SQL 2008 R2 SP1+).",
                RequiresRestart = false,
                ImpactScore = 40
            });
        }

        return recommendations;
    }

    private string GetConfigurationDescription(string name)
    {
        return name switch
        {
            "max server memory (MB)" => "Maximum memory SQL Server can allocate (MB)",
            "min server memory (MB)" => "Minimum memory SQL Server will maintain (MB)",
            "max degree of parallelism" => "Maximum number of processors for parallel query execution",
            "cost threshold for parallelism" => "Minimum query cost before parallel execution is considered",
            "optimize for ad hoc workloads" => "Reduces plan cache bloat from single-use ad-hoc queries",
            "backup compression default" => "Enables backup compression by default",
            "recovery interval (min)" => "Maximum time (minutes) for automatic recovery",
            "fill factor (%)" => "Default fill factor for indexes",
            _ => name
        };
    }

    #endregion
}
