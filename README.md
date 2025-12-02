# AX 2012 Performance Optimizer

![Tests](https://github.com/rweisssieker-xp/AX-2012-Performce-Optimizer/workflows/Test%20Suite/badge.svg)

A native Windows application for monitoring and optimizing Microsoft Dynamics AX 2012 R3 CU13 performance with SQL Server 2016. This tool provides real-time performance metrics, graphical visualizations, and intelligent optimization recommendations.

## 📚 Documentation

- **[CORE_DOCUMENTATION.md](CORE_DOCUMENTATION.md)** - Complete core features, architecture, development setup, and deployment
- **[AI_FEATURES.md](AI_FEATURES.md)** - All AI-powered features: Natural Language Assistant, Auto-Fixer, Query Rewriter, etc.
- **[INNOVATIVE_FEATURES.md](INNOVATIVE_FEATURES.md)** - Unique USP features: Performance DNA, Crystal Ball, Personas, Time Machine, Community Benchmarking

> **Quick Start**: New users should start with [CORE_DOCUMENTATION.md](CORE_DOCUMENTATION.md) for setup instructions.

## Features

### 🎯 Core Capabilities

- **Real-time Performance Monitoring**: Track SQL queries, AOS health, batch jobs, and database metrics
- **Graphical Visualizations**: Beautiful charts and graphs showing performance trends
- **Intelligent Recommendations**: AI-powered analysis provides actionable optimization suggestions
- **Dual Connection**: Connects to both SQL Server (DMVs) and AX Business Connector
- **Portable Application**: Single executable with no installation required
- **Secure Credentials**: Encrypted password storage using Windows DPAPI
 - **Demo Mode**: Immediate value without a DB connection (auto demo data on Dashboard)

### 🌟 Why it's different (USPs)

- **AX-native insights**: Uses AX tables like `SYSCLIENTSESSIONS`, `BATCHJOB`, `AIFGATEWAYQUEUE` for targeted diagnostics.
- **Governed Auto-Fixer**: Two-step preview, safety levels, concrete “Why” and impact estimates, audit-ready `.sql` output.
- **What-if predictions**: Before/after CPU, I/O, and duration with contributing factors and confidence.
- **Cost-optimized AI**: Tiered model selection + response caching for up to 98–99% cost reduction.
- **Zero-install, secure-by-default**: Portable EXE, DPAPI-encrypted secrets, read-only access, least privilege.

### 📊 Monitoring Modules

#### 1. Dashboard
- System health overview
- Active user count
- Running batch jobs
- Database size metrics
- Critical alerts and warnings
 - Demo data when disconnected

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
- Index fragmentation analysis (lowered threshold: 10%, min 100 pages)
- Missing index recommendations with impact scoring
- Statistics freshness
- Real-time status messages and loading indicators
- Empty-state messages when no issues found
- One-click index rebuild/reorganize
- Copy-to-clipboard CREATE INDEX scripts

#### 6. Recommendations
- Categorized optimization suggestions
- Priority-based recommendations (Critical, High, Medium, Low)
- One-click SQL script generation
- Impact analysis for each recommendation
- Track implemented recommendations

## Technical Stack

### Architecture
- **Framework**: .NET 8 WPF
- **UI Pattern**: MVVM with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Data Access**: Microsoft.Data.SqlClient + AX Business Connector
- **Charting**: LiveChartsCore.SkiaSharpView.WPF
- **Configuration**: JSON-based with encrypted credentials

### Project Structure

```
AX2012PerformanceOptimizer/
├── AX2012PerformanceOptimizer.WpfApp/       # WPF Application Layer
│   ├── Views/                               # XAML Views
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
    └── LiveCharts WPF Configuration
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
   AX2012PerformanceOptimizer.WpfApp.exe
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
   - If not connected, Dashboard shows demo data by default

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
  - Windows Desktop Development
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
dotnet publish AX2012PerformanceOptimizer.WpfApp/AX2012PerformanceOptimizer.WpfApp.csproj `
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

## Recent Updates

### Version 2.1 (October 2025)

#### UI/UX Improvements
- ✅ **Fixed Performance Crystal Ball**: Missing DataContext issue resolved - scenarios now load correctly
- ✅ **Database Health UX**: Added status messages, loading indicators, and empty-state displays
- ✅ **Fragmented Indexes**: Lowered detection threshold (30% → 10%) and page minimum (1000 → 100 pages)
- ✅ **Empty-State Messages**: User-friendly messages when no issues detected
  - "✅ No Fragmented Indexes Found - All indexes are healthy!"
  - "✅ No Missing Indexes Detected - Database schema appears well-optimized!"
- ✅ **Fixed 5 Innovative Views**: DataContext binding fixed for Crystal Ball, DNA, Personas, Time Machine, and Community

#### Technical Fixes
- Fixed missing ViewModel injection in 5 innovative feature views
- Improved data loading feedback with real-time status updates
- Enhanced empty collection handling with informative UI
- Better error handling and user feedback

## Roadmap

### Planned Features
- [ ] Export reports to PDF/Excel
- [ ] Historical trend analysis with Performance Time Machine
- [ ] Email alerts for critical issues
- [ ] Multi-instance monitoring
- [ ] Custom dashboard widgets
- [ ] Query execution plan visualization
- [ ] Automated maintenance scripts

### Future Enhancements
- [ ] Support for AX 2012 R2 and earlier
- [ ] Cloud database monitoring (Azure SQL)
- [ ] Mobile companion app
- [ ] REST API for integration with other tools
- [ ] Slack/Teams notifications
- [ ] Advanced anomaly detection with ML

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

- Built with **WPF (.NET 8)**
- Charts powered by **LiveChartsCore.SkiaSharpView.WPF**
- MVVM with [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

## Disclaimer

This tool is provided as-is for monitoring purposes only. Always test in a non-production environment first. The authors are not responsible for any performance issues or data loss resulting from the use of this tool.

---

**Version**: 2.1.0
**Last Updated**: October 23, 2025
**Target Platform**: Microsoft Dynamics AX 2012 R3 CU13 + SQL Server 2016+
**Build Status**: ✅ Stable (0 errors, 21 warnings)
**Production Ready**: Yes

