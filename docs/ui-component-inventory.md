# UI Component Inventory - AX 2012 Performance Optimizer

## Overview

The application uses **WPF (Windows Presentation Foundation)** with **XAML** for UI definition and **MVVM** pattern for data binding.

## Component Categories

### 1. Main Views (22 Views)

#### Core Monitoring Views
- **DashboardView**: System health overview
- **SqlPerformanceView**: SQL query performance analysis
- **AosMonitoringView**: AOS server monitoring
- **BatchJobsView**: Batch job monitoring
- **DatabaseHealthView**: Database health metrics
- **RecommendationsView**: Optimization recommendations
- **SettingsView**: Application settings and configuration

#### AI-Powered Views
- **NaturalLanguageAssistantView**: Natural language query assistant
- **AiInsightsDashboardView**: AI performance insights
- **AiHealthDashboardView**: AI health dashboard
- **NaturalLanguageToSqlView**: Natural language to SQL converter
- **QueryRiskScoringView**: Query risk scoring

#### Executive Views
- **ExecutiveDashboardView**: Executive dashboard
- **ExecutiveScorecardView**: Executive scorecard

#### Innovative Feature Views
- **PerformanceDnaView**: Performance DNA analysis
- **PerformanceCrystalBallView**: Performance Crystal Ball (scenarios)
- **PerformancePersonasView**: Performance Personas (AI experts)
- **PerformanceTimeMachineView**: Performance Time Machine (historical)
- **PerformanceCommunityView**: Performance Community (benchmarking)
- **PerformanceTherapistView**: Performance Therapist (interactive diagnosis)

#### Supporting Views
- **HistoricalTrendingView**: Historical trending analysis
- **ServerSettingsView**: Server configuration

### 2. Dialog Windows (4 Dialogs)

- **AiExplainerDialog**: AI query explanation popup
- **DetailsDialog**: Details display dialog
- **ErrorDialog**: Error message dialog
- **SummaryDialog**: Summary information dialog

### 3. Value Converters (10 Converters)

- **BooleanToVisibilityConverter**: Boolean to Visibility conversion
- **BoolToActiveInactiveConverter**: Boolean to Active/Inactive text
- **BoolToYesNoConverter**: Boolean to Yes/No text
- **CountToVisibilityConverter**: Count to Visibility (show if count > 0)
- **IntToVisibilityConverter**: Integer to Visibility
- **InverseBoolConverter**: Inverted boolean conversion
- **NullToBoolConverter**: Null to boolean conversion
- **NullToVisibilityConverter**: Null to Visibility conversion
- **ProfileIsActiveConverter**: Profile active status converter
- **StringToVisibilityConverter**: String to Visibility (show if not empty)

### 4. UI Libraries

- **LiveChartsCore.SkiaSharpView.WPF 2.0.0-rc4.3**: Charts and graphs
- **Microsoft.Xaml.Behaviors.Wpf 1.1.122**: XAML behaviors

## UI Patterns

### Data Binding
- Two-way binding for input controls
- One-way binding for display controls
- Collection binding to ListBox, DataGrid, ItemsControl

### Commands
- RelayCommand pattern for button actions
- Async commands for long-running operations
- CanExecute logic for command enabling/disabling

### Styles and Templates
- Consistent styling across views
- Resource dictionaries for shared styles
- Template-based controls

## Component Reusability

### Reusable Components
- **Value Converters**: Shared across all views
- **Dialog Service**: Centralized dialog management
- **Charts**: LiveCharts components reused across views

### View-Specific Components
- Each view has its own ViewModel
- Views are self-contained with their own XAML and code-behind
- No shared view components (each view is independent)

## UI Architecture

```
MainWindow (Shell)
    ├── Navigation (Tab Control)
    │   ├── Dashboard Tab → DashboardView
    │   ├── SQL Performance Tab → SqlPerformanceView
    │   ├── AOS Monitoring Tab → AosMonitoringView
    │   ├── Batch Jobs Tab → BatchJobsView
    │   ├── Database Health Tab → DatabaseHealthView
    │   ├── Recommendations Tab → RecommendationsView
    │   ├── Settings Tab → SettingsView
    │   └── [Additional AI/Innovative Tabs]
    │
    └── Dialogs (Modal Windows)
        ├── AiExplainerDialog
        ├── DetailsDialog
        ├── ErrorDialog
        └── SummaryDialog
```

## Design System

- **Color Scheme**: Modern, professional colors
- **Typography**: System fonts (Segoe UI)
- **Icons**: Unicode emoji icons (✅, 📊, 🔴, etc.)
- **Layout**: Responsive grid-based layouts
- **Spacing**: Consistent margins and padding

## Accessibility

- Keyboard navigation support
- Screen reader compatible (WPF accessibility)
- High contrast support (Windows theme)

