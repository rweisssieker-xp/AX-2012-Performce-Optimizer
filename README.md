# AX 2012 Performance Optimizer

A native Windows application for monitoring and optimizing Microsoft Dynamics AX 2012 R3 CU13 performance with SQL Server 2016. This tool provides real-time performance metrics, graphical visualizations, and intelligent optimization recommendations.

## Features

### 🎯 Core Capabilities

- **Real-time Performance Monitoring**: Track SQL queries, AOS health, batch jobs, and database metrics
- **Graphical Visualizations**: Beautiful charts and graphs showing performance trends
- **Intelligent Recommendations**: AI-powered analysis provides actionable optimization suggestions
- **Dual Connection**: Connects to both SQL Server (DMVs) and AX Business Connector
- **Portable Application**: Single executable with no installation required
- **Secure Credentials**: Encrypted password storage using Windows DPAPI

### 📊 Monitoring Modules

#### 1. Dashboard
- System health overview
- Active user count
- Running batch jobs
- Database size metrics
- Critical alerts and warnings

#### 2. SQL Performance
- Top expensive queries by CPU, I/O, and execution time
- Query execution statistics
- Real-time query monitoring
- Detailed query analysis

#### 3. AOS Monitoring
- AOS server health status
- Active user sessions
- Memory and CPU utilization
- Client connection details

#### 4. Batch Jobs
- Running batch jobs visualization
- Failed jobs analysis
- Execution history and trends
- Duration statistics

#### 5. Database Health
- Database size metrics (data, log, unallocated space)
- Top tables by size
- Index fragmentation analysis
- Missing index recommendations
- Statistics freshness

#### 6. Recommendations
- Categorized optimization suggestions
- Priority-based recommendations (Critical, High, Medium, Low)
- One-click SQL script generation
- Impact analysis for each recommendation
- Track implemented recommendations

## Technical Stack

### Architecture
- **Framework**: .NET 8 with WinUI 3
- **UI Pattern**: MVVM with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Data Access**: Microsoft.Data.SqlClient + AX Business Connector
- **Charting**: LiveChartsCore for WinUI 3
- **Configuration**: JSON-based with encrypted credentials

### Project Structure

```
AX2012PerformanceOptimizer/
├── AX2012PerformanceOptimizer.App/          # WinUI 3 Application Layer
│   ├── Views/                                # XAML Views
│   ├── ViewModels/                          # MVVM ViewModels
│   └── App.xaml.cs                          # DI Container Setup
│
├── AX2012PerformanceOptimizer.Core/         # Business Logic Layer
│   ├── Models/                              # Domain Models
│   ├── Services/                            # Monitoring Services
│   │   ├── SqlQueryMonitorService.cs
│   │   ├── AosMonitorService.cs
│   │   ├── BatchJobMonitorService.cs
│   │   ├── DatabaseStatsService.cs
│   │   ├── AifMonitorService.cs
│   │   ├── SsrsMonitorService.cs
│   │   └── RecommendationEngine.cs
│   └── Interfaces/                          # Service Contracts
│
├── AX2012PerformanceOptimizer.Data/         # Data Access Layer
│   ├── SqlServer/                           # SQL Server Access
│   │   ├── SqlConnectionManager.cs
│   │   └── DMV Queries
│   ├── AxConnector/                         # AX Business Connector
│   │   └── AxConnectorService.cs
│   ├── Configuration/                       # Settings Management
│   │   └── ConfigurationService.cs
│   └── Models/                              # Data Models
│
└── AX2012PerformanceOptimizer.Charts/       # Charting Components
    └── LiveCharts Configuration
```

## Installation & Setup

### Prerequisites

1. **Windows 10/11** (x64)
2. **.NET 8 Runtime** (included in self-contained build)
3. **SQL Server Access**: Read permissions on target SQL Server
4. **AX 2012 R3 CU13**: (Optional) Business Connector for extended monitoring

### First-Time Setup

1. **Launch the Application**
   ```
   AX2012PerformanceOptimizer.exe
   ```

2. **Configure Connection Profile**
   - Navigate to Settings (gear icon)
   - Click "New Profile"
   - Enter connection details:
     - **Profile Name**: Descriptive name (e.g., "Production AX")
     - **SQL Server Name**: Server\Instance
     - **Database Name**: Your AX database name
     - **Authentication**: Windows or SQL Server
     - **AOS Server Name**: AOS server hostname
     - **AOS Port**: Default 2712
     - **Company**: Default "DAT"

3. **Test Connection**
   - Click "Test Connection" button
   - Verify successful connection
   - Click "Save" to store profile

4. **Connect**
   - Click "Connect" to activate the profile
   - Navigate to Dashboard to start monitoring

## SQL Server DMVs Used

The application queries the following SQL Server Dynamic Management Views (DMVs):

### Query Performance
- `sys.dm_exec_query_stats` - Query execution statistics
- `sys.dm_exec_sql_text` - Query text retrieval
- `sys.dm_exec_requests` - Currently executing requests

### Index Analysis
- `sys.dm_db_index_physical_stats` - Index fragmentation
- `sys.dm_db_missing_index_details` - Missing indexes
- `sys.dm_db_missing_index_groups` - Missing index groups
- `sys.dm_db_missing_index_group_stats` - Missing index impact

### Performance Counters
- `sys.dm_os_performance_counters` - SQL Server performance counters
- `sys.dm_os_wait_stats` - Wait statistics

### Database Information
- `sys.database_files` - Database file sizes
- `sys.tables`, `sys.indexes`, `sys.partitions` - Table and index information

## AX-Specific Tables

The application accesses these AX 2012 tables:

- `SYSCLIENTSESSIONS` - Active user sessions
- `BATCHJOB` - Current batch jobs
- `BATCHJOBHISTORY` - Historical batch job data
- `AIFGATEWAYQUEUE` - AIF message queue
- `SRSREPORTEXECUTIONLOG` - SSRS report execution (if available)

## Recommendation Categories

The recommendation engine analyzes performance data and generates suggestions in these categories:

1. **SQL Query Optimization**: High-CPU queries, inefficient query patterns
2. **Index Management**: Fragmentation, rebuild/reorganize suggestions
3. **Statistics Update**: Outdated statistics detection
4. **Batch Job Scheduling**: Failed jobs, long-running jobs
5. **AOS Configuration**: Server health issues, resource constraints
6. **Database Maintenance**: Size management, archival suggestions
7. **Memory Optimization**: Memory pressure indicators
8. **Storage Optimization**: File growth recommendations

## Security Considerations

- **Read-Only Operations**: All queries are read-only; no data modification
- **Encrypted Credentials**: Passwords encrypted using Windows DPAPI
- **Local Storage**: Configuration stored in `%LocalAppData%\AX2012PerformanceOptimizer`
- **Secure Connections**: Supports SSL/TLS for SQL Server connections
- **Minimal Permissions**: Only requires db_datareader role on AX database

## Building from Source

### Requirements
- Visual Studio 2022 (v17.8+) with:
  - .NET 8 SDK
  - Windows App SDK
  - C# 12
- Or: .NET 8 SDK + Windows SDK 10.0.19041.0+

### Build Steps

```bash
# Clone the repository
git clone https://github.com/yourusername/AX-2012-Performance-Optimizer.git
cd AX-2012-Performance-Optimizer

# Restore NuGet packages
dotnet restore AX2012PerformanceOptimizer.sln

# Build solution
dotnet build AX2012PerformanceOptimizer.sln --configuration Release

# Publish as single-file executable
dotnet publish AX2012PerformanceOptimizer.App/AX2012PerformanceOptimizer.App.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output ./publish `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true
```

## Configuration Files

Configuration is stored in JSON format at:
```
%LocalAppData%\AX2012PerformanceOptimizer\profiles.json
```

Example profile structure:
```json
[
  {
    "Id": "guid",
    "Name": "Production AX",
    "SqlServerName": "SQLSERVER\\AX2012",
    "DatabaseName": "MicrosoftDynamicsAX",
    "UseWindowsAuthentication": true,
    "AosServerName": "AOSSERVER",
    "AosPort": 2712,
    "AxCompany": "DAT",
    "IsDefault": true
  }
]
```

## Troubleshooting

### Common Issues

**Connection Failures**
- Verify SQL Server is accessible
- Check firewall rules (TCP 1433)
- Confirm user has db_datareader role
- Test connection string manually

**Missing Data**
- Some AX tables may not exist in all environments
- Business Connector requires AX client components
- DMVs require VIEW SERVER STATE permission

**Performance Impact**
- Queries are optimized for minimal impact
- Monitoring intervals are configurable
- Most queries have TOP N limits
- Background monitoring can be paused

## Roadmap

### Planned Features
- [ ] Export reports to PDF/Excel
- [ ] Historical trend analysis
- [ ] Email alerts for critical issues
- [ ] Multi-instance monitoring
- [ ] Custom dashboard widgets
- [ ] Query execution plan analysis
- [ ] Automated maintenance scripts

### Future Enhancements
- [ ] Support for AX 2012 R2 and earlier
- [ ] Cloud database monitoring
- [ ] Mobile companion app
- [ ] API for integration with other tools

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or suggestions:
- Create an issue on GitHub
- Contact: [your-email@example.com]

## Acknowledgments

- Built with [WinUI 3](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/)
- Charts powered by [LiveCharts2](https://livecharts.dev/)
- MVVM with [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

## Disclaimer

This tool is provided as-is for monitoring purposes only. Always test in a non-production environment first. The authors are not responsible for any performance issues or data loss resulting from the use of this tool.

---

**Version**: 1.0.0  
**Last Updated**: October 2025  
**Target Platform**: Microsoft Dynamics AX 2012 R3 CU13 + SQL Server 2016
