# State Management Patterns - AX 2012 Performance Optimizer

## Overview

The application uses **MVVM (Model-View-ViewModel)** pattern with **CommunityToolkit.Mvvm** for state management and data binding.

## State Management Framework

**CommunityToolkit.Mvvm 8.3.2**
- Provides `ObservableObject` base class
- `[ObservableProperty]` attribute for automatic property change notifications
- `[RelayCommand]` attribute for command pattern implementation
- Automatic INotifyPropertyChanged implementation

## State Management Patterns

### 1. ViewModel State

Each ViewModel inherits from `ObservableObject` and uses `[ObservableProperty]` attributes:

```csharp
public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private int activeUsers;
    
    [ObservableProperty]
    private bool isLoading;
    
    [ObservableProperty]
    private string statusMessage = "Ready";
}
```

**Pattern Benefits:**
- Automatic property change notifications
- Two-way data binding support
- Clean, declarative code

### 2. Collection State

Uses `ObservableCollection<T>` for collections that need to notify UI of changes:

```csharp
[ObservableProperty]
private ObservableCollection<SqlQueryMetric> queries = new();

[ObservableProperty]
private ObservableCollection<ConversationMessage> conversationHistory = new();
```

**Pattern Benefits:**
- Automatic UI updates when items added/removed
- Supports data binding to ListBox, DataGrid, etc.
- Thread-safe updates via Dispatcher

### 3. Command Pattern

Uses `[RelayCommand]` for UI commands:

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    IsLoading = true;
    // ... async operation
    IsLoading = false;
}
```

**Pattern Benefits:**
- Automatic CanExecute logic
- Async command support
- Clean separation of UI actions

### 4. Service-Based State

State is managed through injected services:

```csharp
private readonly ISqlQueryMonitorService _sqlMonitor;
private readonly IAosMonitorService _aosMonitor;
```

**Pattern Benefits:**
- Centralized state management
- Testable and mockable
- Separation of concerns

## State Flow

```
User Action (View)
    ↓
Command (ViewModel)
    ↓
Service Call (Business Logic)
    ↓
Data Update (Model/Service)
    ↓
Property Change Notification
    ↓
UI Update (View via Data Binding)
```

## Key State Management Features

1. **Reactive Properties**: All ViewModel properties use `[ObservableProperty]` for automatic change notifications
2. **Async State**: Proper async/await patterns with loading states
3. **Error State**: Status messages and error handling
4. **Demo Mode**: Fallback state when database not connected
5. **Connection State**: Tracks database connection status

## State Persistence

- **Configuration**: Stored in `%LocalAppData%\AX2012PerformanceOptimizer\profiles.json`
- **AI Configuration**: Stored in `%LocalAppData%\AX2012PerformanceOptimizer\ai-config.json`
- **Credentials**: Encrypted using Windows DPAPI
- **Session State**: In-memory only (not persisted)

## Thread Safety

- UI updates via `Dispatcher.Invoke` when needed
- Async operations properly awaited
- ObservableCollection updates on UI thread

