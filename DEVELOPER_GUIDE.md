# Developer Guide - AX 2012 Performance Optimizer

## Development Environment Setup

### Prerequisites

1. **Visual Studio 2022** (Version 17.8 or later)
   - Workloads:
     - .NET Desktop Development
     - Universal Windows Platform development
   - Individual Components:
     - Windows App SDK C# Templates
     - .NET 8.0 Runtime

2. **Windows SDK**
   - Windows 10 SDK (10.0.19041.0 or later)
   - Windows 11 SDK (recommended)

3. **Git** (for version control)

### Getting Started

```powershell
# Clone repository
git clone https://github.com/yourusername/AX-2012-Performance-Optimizer.git
cd AX-2012-Performance-Optimizer

# Open in Visual Studio
start AX2012PerformanceOptimizer.sln

# Or use VS Code (limited WinUI 3 support)
code .
```

## Project Structure Deep Dive

### Layer Dependencies
```
App Layer (WinUI 3)
    ↓ depends on
Core Layer (Business Logic)
    ↓ depends on
Data Layer (Data Access)
    ↓ depends on
External (SQL Server, AX)
```

### Key Design Patterns

#### 1. MVVM (Model-View-ViewModel)
```csharp
// View
public partial class DashboardView : Page
{
    public DashboardViewModel ViewModel { get; }
}

// ViewModel
public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private int activeUsers;
    
    [RelayCommand]
    private async Task LoadDataAsync() { }
}

// Model
public class AosMetric
{
    public int ActiveUserSessions { get; set; }
}
```

#### 2. Dependency Injection
```csharp
// Registration in App.xaml.cs
services.AddSingleton<ISqlConnectionManager, SqlConnectionManager>();
services.AddTransient<DashboardViewModel>();

// Usage in ViewModels
public DashboardViewModel(
    ISqlQueryMonitorService sqlMonitor,
    IAosMonitorService aosMonitor)
{
    _sqlMonitor = sqlMonitor;
    _aosMonitor = aosMonitor;
}
```

#### 3. Service Pattern
```csharp
public interface ISqlQueryMonitorService
{
    Task<List<SqlQueryMetric>> GetTopExpensiveQueriesAsync(int topCount);
    Task StartMonitoringAsync(CancellationToken cancellationToken);
    event EventHandler<SqlQueryMetric>? NewMetricCollected;
}
```

## Adding New Features

### 1. Adding a New Monitoring Service

**Step 1: Create Model** (Core/Models/)
```csharp
namespace AX2012PerformanceOptimizer.Core.Models;

public class CustomMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}
```

**Step 2: Create Interface** (Core/Services/)
```csharp
namespace AX2012PerformanceOptimizer.Core.Services;

public interface ICustomMonitorService
{
    Task<List<CustomMetric>> GetMetricsAsync();
    Task StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync();
    event EventHandler<CustomMetric>? NewMetricCollected;
}
```

**Step 3: Implement Service** (Core/Services/)
```csharp
public class CustomMonitorService : ICustomMonitorService
{
    private readonly ISqlConnectionManager _connectionManager;
    private readonly ILogger<CustomMonitorService> _logger;

    public event EventHandler<CustomMetric>? NewMetricCollected;

    public CustomMonitorService(
        ISqlConnectionManager connectionManager,
        ILogger<CustomMonitorService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<List<CustomMetric>> GetMetricsAsync()
    {
        var metrics = new List<CustomMetric>();
        
        try
        {
            using var connection = await _connectionManager.GetConnectionAsync();
            using var command = new SqlCommand(@"
                SELECT Name, Value FROM CustomTable", connection);
                
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                metrics.Add(new CustomMetric
                {
                    Name = reader.GetString(0),
                    Value = reader.GetDouble(1)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom metrics");
        }
        
        return metrics;
    }
    
    public Task StartMonitoringAsync(CancellationToken cancellationToken = default)
    {
        // Implement background monitoring
        return Task.CompletedTask;
    }
    
    public Task StopMonitoringAsync()
    {
        return Task.CompletedTask;
    }
}
```

**Step 4: Register Service** (App.xaml.cs)
```csharp
services.AddSingleton<ICustomMonitorService, CustomMonitorService>();
```

**Step 5: Create ViewModel** (App/ViewModels/)
```csharp
public partial class CustomMonitorViewModel : ObservableObject
{
    private readonly ICustomMonitorService _customMonitor;
    
    [ObservableProperty]
    private ObservableCollection<CustomMetric> metrics = new();
    
    public CustomMonitorViewModel(ICustomMonitorService customMonitor)
    {
        _customMonitor = customMonitor;
    }
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var data = await _customMonitor.GetMetricsAsync();
        Metrics.Clear();
        foreach (var metric in data)
        {
            Metrics.Add(metric);
        }
    }
}
```

**Step 6: Create View** (App/Views/)
```xaml
<Page x:Class="AX2012PerformanceOptimizer.App.Views.CustomMonitorView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid Padding="24">
        <ListView ItemsSource="{x:Bind ViewModel.Metrics, Mode=OneWay}">
            <ListView.ItemTemplate>
                <DataTemplate>
                    <StackPanel>
                        <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                        <TextBlock Text="{Binding Value}"/>
                    </StackPanel>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>
    </Grid>
</Page>
```

### 2. Adding a New Recommendation Type

```csharp
// In RecommendationEngine.cs

private async Task AnalyzeCustomMetricsAsync()
{
    var metrics = await _customMonitor.GetMetricsAsync();
    
    foreach (var metric in metrics.Where(m => m.Value > 100))
    {
        _recommendations.Add(new Recommendation
        {
            Title = $"High Value Detected: {metric.Name}",
            Description = $"Value is {metric.Value}, which exceeds threshold.",
            Category = RecommendationCategory.CustomCategory,
            Priority = RecommendationPriority.Medium,
            ImpactAnalysis = "This could impact performance.",
            ActionScript = "-- Review and adjust settings",
            RelatedObjects = new List<string> { metric.Name }
        });
    }
}

// Call from GenerateRecommendationsAsync
public async Task<List<Recommendation>> GenerateRecommendationsAsync()
{
    _recommendations.Clear();
    
    await AnalyzeSqlQueriesAsync();
    await AnalyzeIndexFragmentationAsync();
    await AnalyzeCustomMetricsAsync(); // New
    
    return _recommendations;
}
```

## Debugging Tips

### 1. SQL Query Debugging

```csharp
// Add logging to services
_logger.LogDebug("Executing query: {Query}", queryText);

// In appsettings.json (if using configuration)
{
  "Logging": {
    "LogLevel": {
      "AX2012PerformanceOptimizer": "Debug"
    }
  }
}
```

### 2. ViewModel Debugging

```csharp
// Use INotifyPropertyChanged validation
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsDataLoaded))]
private bool isLoading;

public bool IsDataLoaded => !IsLoading;

// Add breakpoints in RelayCommand methods
[RelayCommand]
private async Task LoadDataAsync()
{
    IsLoading = true; // Breakpoint here
    try
    {
        var data = await _service.GetDataAsync();
        // Process data
    }
    finally
    {
        IsLoading = false;
    }
}
```

### 3. Connection Issues

```csharp
// Test connection before queries
public async Task<bool> TestConnectionAsync()
{
    try
    {
        using var connection = await _connectionManager.GetConnectionAsync();
        using var command = new SqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync();
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Connection test failed");
        return false;
    }
}
```

## Common Issues and Solutions

### Issue 1: WinUI 3 Build Errors

**Problem**: `Microsoft.Build.Packaging.Pri.Tasks.dll not found`

**Solution**:
1. Install Windows App SDK from Visual Studio Installer
2. Ensure you're using VS 2022 (not 2019)
3. Clean and rebuild solution
4. If persists, install Windows SDK manually

### Issue 2: XAML Designer Errors

**Problem**: Designer won't load XAML files

**Solution**:
1. Close and reopen Visual Studio
2. Build solution first (F6)
3. Use XAML Hot Reload instead (Alt+F10)
4. Check for XAML syntax errors

### Issue 3: Circular Dependency

**Problem**: Projects reference each other

**Solution**:
- ✅ Allowed: App → Core → Data
- ❌ Forbidden: Data → Core, Core → App
- Move shared models to appropriate layer

### Issue 4: Async/Await Issues

**Problem**: UI freezing or deadlocks

**Solution**:
```csharp
// ❌ Bad - blocks UI thread
public void LoadData()
{
    var data = _service.GetDataAsync().Result; // NEVER DO THIS
}

// ✅ Good - truly async
public async Task LoadDataAsync()
{
    var data = await _service.GetDataAsync();
}
```

## Testing Strategies

### Unit Testing (Recommended Setup)

```csharp
// Install NuGet packages:
// - Microsoft.NET.Test.Sdk
// - xunit
// - xunit.runner.visualstudio
// - Moq

using Moq;
using Xunit;

public class SqlQueryMonitorServiceTests
{
    [Fact]
    public async Task GetTopExpensiveQueries_ReturnsData()
    {
        // Arrange
        var mockConnection = new Mock<ISqlConnectionManager>();
        var mockLogger = new Mock<ILogger<SqlQueryMonitorService>>();
        var service = new SqlQueryMonitorService(
            mockConnection.Object, 
            mockLogger.Object);
        
        // Act
        var results = await service.GetTopExpensiveQueriesAsync(10);
        
        // Assert
        Assert.NotNull(results);
    }
}
```

### Integration Testing

```csharp
// Test with real database (use test connection string)
[Fact]
public async Task IntegrationTest_RealDatabase()
{
    var connectionString = "Server=test-server;Database=test-db;...";
    var manager = new SqlConnectionManager();
    manager.SetConnectionString(connectionString);
    
    var service = new SqlQueryMonitorService(manager, logger);
    var results = await service.GetTopExpensiveQueriesAsync(5);
    
    Assert.NotEmpty(results);
}
```

## Code Style Guidelines

### Naming Conventions
- **Interfaces**: `IServiceName`
- **Classes**: `PascalCase`
- **Private fields**: `_camelCase`
- **Properties**: `PascalCase`
- **Methods**: `PascalCaseAsync` (if async)
- **Constants**: `PascalCase`

### File Organization
```
ServiceName.cs
├── Private fields
├── Constructor
├── Public properties
├── Public methods
├── Private methods
└── Event handlers
```

### Documentation
```csharp
/// <summary>
/// Monitors SQL query performance using DMVs.
/// </summary>
/// <remarks>
/// This service collects metrics from sys.dm_exec_query_stats
/// and provides real-time monitoring capabilities.
/// </remarks>
public class SqlQueryMonitorService : ISqlQueryMonitorService
{
    /// <summary>
    /// Gets the top expensive queries by CPU time.
    /// </summary>
    /// <param name="topCount">Number of queries to return</param>
    /// <returns>List of query metrics</returns>
    public async Task<List<SqlQueryMetric>> GetTopExpensiveQueriesAsync(int topCount)
    {
        // Implementation
    }
}
```

## Performance Optimization Tips

### 1. Use ConfigureAwait(false) for Library Code
```csharp
// In services and data access
var result = await command.ExecuteReaderAsync().ConfigureAwait(false);
```

### 2. Dispose Resources Properly
```csharp
// Use using statements
using var connection = await _connectionManager.GetConnectionAsync();
using var command = new SqlCommand(query, connection);
```

### 3. Batch Operations
```csharp
// ❌ Bad - multiple round trips
foreach (var id in ids)
{
    await GetDataByIdAsync(id);
}

// ✅ Good - single query
var data = await GetDataByIdsAsync(ids);
```

### 4. Cache Expensive Operations
```csharp
private List<TableMetric>? _cachedTables;
private DateTime _cacheExpiry;

public async Task<List<TableMetric>> GetTopTablesAsync()
{
    if (_cachedTables != null && DateTime.UtcNow < _cacheExpiry)
    {
        return _cachedTables;
    }
    
    _cachedTables = await FetchTablesFromDatabaseAsync();
    _cacheExpiry = DateTime.UtcNow.AddMinutes(5);
    return _cachedTables;
}
```

## Useful Resources

### Documentation
- [WinUI 3 Docs](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/)
- [.NET 8 Docs](https://docs.microsoft.com/en-us/dotnet/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [SQL Server DMVs](https://docs.microsoft.com/en-us/sql/relational-databases/system-dynamic-management-views/)

### Community
- Stack Overflow: `[winui-3]`, `[dynamics-ax]`
- GitHub Discussions
- Microsoft Q&A

### Tools
- **LINQPad**: Test SQL queries
- **SQL Server Profiler**: Monitor SQL activity
- **Visual Studio Performance Profiler**: Analyze application performance

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Pull Request Guidelines
- Include tests for new features
- Update documentation
- Follow existing code style
- Keep PRs focused and small
- Write descriptive commit messages

---

**Happy Coding!** 🚀

For questions or support, open an issue on GitHub or contact the maintainers.

