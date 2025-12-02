# Development Guide - AX 2012 Performance Optimizer

## Prerequisites

### Required Software
1. **Visual Studio 2022** (17.8 or later)
   - .NET Desktop Development workload
   - .NET 8.0 SDK
   - Windows SDK 10.0.19041.0 or later

2. **Windows 10/11** (x64)
   - Required for WPF development

3. **Git** (optional, for version control)

### Optional Tools
- **LINQPad**: Test SQL queries
- **SQL Server Management Studio**: Database access
- **Fiddler**: Debug HTTP/AI service calls
- **Visual Studio Performance Profiler**: Performance analysis

## Getting Started

### 1. Clone Repository

```powershell
git clone <repository-url>
cd AX-2012-Performce-Optimizer
```

### 2. Open Solution

```powershell
# Option 1: Visual Studio
start AX2012PerformanceOptimizer.sln

# Option 2: VS Code
code .
```

### 3. Restore NuGet Packages

```powershell
dotnet restore AX2012PerformanceOptimizer.sln
```

### 4. Build Solution

```powershell
dotnet build AX2012PerformanceOptimizer.sln --configuration Debug
```

### 5. Run Application

```powershell
# Option 1: Using script
.\build-and-run.ps1

# Option 2: Direct run
dotnet run --project AX2012PerformanceOptimizer.WpfApp
```

## Project Structure

```
AX2012PerformanceOptimizer/
├── AX2012PerformanceOptimizer.WpfApp/    # WPF Application (Startup Project)
├── AX2012PerformanceOptimizer.Core/      # Business Logic
└── AX2012PerformanceOptimizer.Data/      # Data Access
```

## Development Workflow

### Adding a New Feature

1. **Create Model** (if needed)
   - Add to `Core/Models/`
   - Implement properties and validation

2. **Create Service Interface**
   - Add to `Core/Services/`
   - Define interface: `IServiceName.cs`

3. **Implement Service**
   - Add to `Core/Services/`
   - Implement interface: `ServiceName.cs`

4. **Register Service**
   - Add to `App.xaml.cs` dependency injection container

5. **Create ViewModel**
   - Add to `WpfApp/ViewModels/`
   - Inherit from `ObservableObject`
   - Use `[ObservableProperty]` and `[RelayCommand]`

6. **Create View**
   - Add XAML to `WpfApp/Views/`
   - Add code-behind (if needed)
   - Set DataContext to ViewModel

7. **Register ViewModel**
   - Add to `App.xaml.cs` as Transient

8. **Add Navigation**
   - Update `MainWindow.xaml` with new tab/view

## Build Commands

### Debug Build
```powershell
dotnet build AX2012PerformanceOptimizer.sln --configuration Debug
```

### Release Build
```powershell
dotnet build AX2012PerformanceOptimizer.sln --configuration Release
```

### Clean Build
```powershell
dotnet clean AX2012PerformanceOptimizer.sln
dotnet build AX2012PerformanceOptimizer.sln
```

## Testing

### Unit Testing
- Create test project: `AX2012PerformanceOptimizer.Tests`
- Test services and business logic
- Mock interfaces for dependencies

### Manual Testing
- Run application in Debug mode
- Test with demo data (no database connection)
- Test with real database connection

## Code Style

### C# Conventions
- Use `partial class` for ViewModels with `[ObservableProperty]`
- Use `async Task` for async methods
- Use `ObservableCollection<T>` for collections
- Use dependency injection for services

### Naming Conventions
- **Classes**: PascalCase (e.g., `DashboardViewModel`)
- **Properties**: PascalCase (e.g., `ActiveUsers`)
- **Methods**: PascalCase (e.g., `LoadDataAsync`)
- **Private fields**: camelCase with underscore (e.g., `_sqlMonitor`)
- **Interfaces**: Start with `I` (e.g., `ISqlQueryMonitorService`)

## Common Development Tasks

### Adding a New ViewModel

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class MyViewModel : ObservableObject
{
    private readonly IMyService _myService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public MyViewModel(IMyService myService)
    {
        _myService = myService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Your logic here
            StatusMessage = "Data loaded";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### Adding a New Service

```csharp
// Interface
public interface IMyService
{
    Task<MyModel> GetDataAsync();
}

// Implementation
public class MyService : IMyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task<MyModel> GetDataAsync()
    {
        // Implementation
    }
}
```

## Debugging

### Visual Studio Debugging
1. Set breakpoints in code
2. Press F5 to start debugging
3. Use Debug → Windows → Locals to inspect variables

### Logging
- Uses `ILogger<T>` for logging
- Log levels: Debug, Information, Warning, Error
- Logs to Visual Studio Output window

## Troubleshooting

### Build Errors
- **Missing NuGet packages**: Run `dotnet restore`
- **SDK not found**: Install .NET 8 SDK
- **WPF not available**: Install Desktop Development workload installed

### Runtime Errors
- **Connection errors**: Check SQL Server connection string
- **Missing dependencies**: Verify all NuGet packages restored
- **XAML errors**: Check XAML syntax and namespaces

## Performance Tips

1. **Use async/await** for all I/O operations
2. **Virtualize lists** for large collections
3. **Cache data** when appropriate
4. **Use background threads** for heavy computations
5. **Profile regularly** with Visual Studio Profiler

