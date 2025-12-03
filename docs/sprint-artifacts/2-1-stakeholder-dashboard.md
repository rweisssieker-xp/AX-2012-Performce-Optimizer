# Story: 2-1 Performance Stakeholder Dashboard

**Story Key:** 2-1-stakeholder-dashboard  
**Story Points:** 5  
**Status:** Ready for Dev  
**Sprint:** Sprint 2

---

## Story

**As a** [Executive/DBA/Developer/End-User]  
**I want** to see performance metrics tailored to my role  
**So that** I can focus on metrics relevant to my responsibilities

---

## Acceptance Criteria

### AC-2.1.1: Role Selector
- [ ] Role selector dropdown in dashboard header
- [ ] Available roles: Executive, DBA, Developer, End-User
- [ ] Current role clearly displayed
- [ ] Role selection persists across sessions
- [ ] Quick role switching (no page reload)

### AC-2.1.2: Executive View
- [ ] Business metrics: Cost impact, ROI, Budget impact
- [ ] High-level KPIs: System health score, User satisfaction
- [ ] Executive summary cards
- [ ] Trend visualizations (cost trends, performance trends)
- [ ] Action items for management
- [ ] Plain language explanations

### AC-2.1.3: DBA View
- [ ] Technical metrics: Query performance, Database health
- [ ] Detailed query analysis
- [ ] Index recommendations
- [ ] Performance optimization tools
- [ ] Technical diagnostics
- [ ] SQL scripts and execution plans

### AC-2.1.4: Developer View
- [ ] Code-level performance metrics
- [ ] Query details and execution plans
- [ ] Performance by module/class
- [ ] Code optimization suggestions
- [ ] Development-focused recommendations
- [ ] Integration with development workflow

### AC-2.1.5: End-User View
- [ ] User experience metrics: Response time, Error rate
- [ ] System availability
- [ ] User-facing performance indicators
- [ ] Simple status indicators (Good/Fair/Poor)
- [ ] Plain language status messages
- [ ] No technical jargon

### AC-2.1.6: Seamless Switching
- [ ] Instant role switching (<1 second)
- [ ] Context preserved (time range, filters)
- [ ] Smooth transitions
- [ ] No data loss

---

## Tasks/Subtasks

### Task 2.1.1: Create Models & Enums
- [x] Create `UserRole.cs` enum in `AX2012PerformanceOptimizer.Core/Models/StakeholderDashboard/`
- [x] Create `RoleBasedDashboardData.cs` model
- [x] Create `DashboardWidget.cs` model
- [x] Create `WidgetType.cs` enum
- [x] Create `RoleWidgetConfiguration.cs` model

### Task 2.1.2: Create Service Interface
- [x] Create `IRoleBasedDashboardService.cs` interface in `AX2012PerformanceOptimizer.Core/Services/StakeholderDashboard/`
- [x] Define `GetDashboardDataAsync(UserRole role, TimeRange timeRange)` method
- [x] Define `GetRoleSpecificWidgetsAsync(UserRole role)` method
- [x] Define `GetRoleConfigurationAsync(UserRole role)` method

### Task 2.1.3: Implement Service
- [x] Implement `RoleBasedDashboardService.cs`
- [x] Inject `IExecutiveScorecardService` for executive metrics
- [x] Inject `ISqlQueryMonitorService` for DBA metrics
- [x] Inject `IQueryAnalyzerService` for developer metrics
- [x] Inject `IAosMonitorService` for end-user metrics
- [x] Inject `IPerformanceCostCalculatorService` for cost calculations
- [x] Implement role-specific data filtering
- [x] Implement widget configuration logic

### Task 2.1.4: Extend DashboardViewModel
- [x] Open `DashboardViewModel.cs` in `AX2012PerformanceOptimizer.WpfApp/ViewModels/`
- [x] Add `UserRole SelectedRole` property (ObservableProperty)
- [x] Add `RoleBasedDashboardData? RoleData` property
- [x] Add `List<UserRole> AvailableRoles` property
- [x] Add `LoadRoleDataCommand` (RelayCommand)
- [x] Add `ChangeRoleCommand` (RelayCommand)
- [x] Implement role switching logic
- [x] Add role persistence (load/save) - simplified implementation

### Task 2.1.5: Update DashboardView
- [x] Open `DashboardView.xaml` in `AX2012PerformanceOptimizer.WpfApp/Views/`
- [x] Add role selector ComboBox in header
- [x] Add role-specific widget containers (ItemsControl)
- [x] Bind widgets to `RoleData.Widgets`
- [x] Add BooleanToVisibilityConverter for conditional display
- [x] Keep default dashboard when no role data

### Task 2.1.6: Create Role-Specific Widgets
- [ ] Create `ExecutiveWidget.xaml` UserControl (deferred - using generic widget display for now)
- [ ] Create `DBAWidget.xaml` UserControl (deferred - using generic widget display for now)
- [ ] Create `DeveloperWidget.xaml` UserControl (deferred - using generic widget display for now)
- [ ] Create `EndUserWidget.xaml` UserControl (deferred - using generic widget display for now)
- [x] Implement role-specific widget data generation in service

### Task 2.1.7: Register Services & ViewModel
- [x] Register `IRoleBasedDashboardService` → `RoleBasedDashboardService` in `App.xaml.cs` (AddSingleton)
- [x] Verify `DashboardViewModel` registration (already exists)
- [x] Update `DashboardViewModel` constructor to inject `IRoleBasedDashboardService`

### Task 2.1.8: Unit Tests
- [ ] Create `RoleBasedDashboardServiceTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/Services/`
- [ ] Test `GetDashboardDataAsync_ReturnsRoleSpecificData`
- [ ] Test `GetRoleSpecificWidgetsAsync_ReturnsCorrectWidgets`
- [ ] Update `DashboardViewModelTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/ViewModels/`
- [ ] Test `RoleChangedCommand_UpdatesDisplay`
- [ ] Test role persistence
- [ ] Achieve 80%+ code coverage

### Task 2.1.9: Integration Tests
- [ ] Test role switching works correctly
- [ ] Test role-specific widgets display
- [ ] Test role persistence across sessions
- [ ] Test context preservation during role switch

---

## Dev Notes

### Architecture Requirements
- Extends existing `DashboardViewModel` (no new ViewModel needed)
- Uses existing dashboard architecture
- Follow existing MVVM pattern (CommunityToolkit.Mvvm)
- Use dependency injection

### Technical Specifications
- **Service Location:** `AX2012PerformanceOptimizer.Core/Services/StakeholderDashboard/`
- **Model Location:** `AX2012PerformanceOptimizer.Core/Models/StakeholderDashboard/`
- **ViewModel Extension:** `AX2012PerformanceOptimizer.WpfApp/ViewModels/DashboardViewModel.cs`
- **View Extension:** `AX2012PerformanceOptimizer.WpfApp/Views/DashboardView.xaml`

### Dependencies
- `IExecutiveDashboardService` - Already exists, for executive metrics
- `ISqlQueryMonitorService` - Already exists, for DBA metrics
- `IQueryAnalyzerService` - Already exists, for developer metrics
- `IAosMonitorService` - Already exists, for end-user metrics
- `ISettingsService` - For role persistence (if available)

### Performance Considerations
- Role switching: Instant (<1 second)
- Widget loading: Lazy load widgets when role changes
- Data caching: Cache role-specific data
- Smooth transitions: Use WPF animations

---

## Dev Agent Record

### Implementation Plan
1. Created all models and enums for role-based dashboard
2. Implemented `IRoleBasedDashboardService` interface
3. Implemented `RoleBasedDashboardService` with role-specific data generation
4. Extended `DashboardViewModel` with role selection and data loading
5. Updated `DashboardView.xaml` with role selector and widget display
6. Registered service in DI container
7. Fixed build errors (cost calculator method calls, type conversions)

### Completion Notes
- Core functionality implemented: role selection, role-specific widgets, data loading
- Widget visualization uses generic template (role-specific widgets deferred)
- Role persistence uses simplified implementation (can be enhanced with ISettingsService)
- Service integrates with existing services: ExecutiveScorecardService, SqlQueryMonitorService, QueryAnalyzerService, AosMonitorService, PerformanceCostCalculatorService
- Build successful, ready for testing

### Debug Log
- Fixed: `CalculateDailyCostAsync` method not found → used `GenerateExecutiveSummaryAsync` instead
- Fixed: Type mismatch (double * decimal) → changed to double * double
- Fixed: XAML converter references → added BooleanToVisibilityConverter to resources

---

## File List
**Created:**
- `AX2012PerformanceOptimizer.Core/Models/StakeholderDashboard/UserRole.cs`
- `AX2012PerformanceOptimizer.Core/Models/StakeholderDashboard/WidgetType.cs`
- `AX2012PerformanceOptimizer.Core/Models/StakeholderDashboard/DashboardWidget.cs`
- `AX2012PerformanceOptimizer.Core/Models/StakeholderDashboard/RoleBasedDashboardData.cs`
- `AX2012PerformanceOptimizer.Core/Models/StakeholderDashboard/RoleWidgetConfiguration.cs`
- `AX2012PerformanceOptimizer.Core/Services/StakeholderDashboard/IRoleBasedDashboardService.cs`
- `AX2012PerformanceOptimizer.Core/Services/StakeholderDashboard/RoleBasedDashboardService.cs`

**Modified:**
- `AX2012PerformanceOptimizer.WpfApp/ViewModels/DashboardViewModel.cs` - Added role selection, role data loading
- `AX2012PerformanceOptimizer.WpfApp/Views/DashboardView.xaml` - Added role selector, widget display
- `AX2012PerformanceOptimizer.WpfApp/App.xaml.cs` - Registered IRoleBasedDashboardService

---

## Change Log
_To be updated with significant changes_

---

## Status
**Current Status:** In Progress (Core Implementation Complete, Testing Pending)  
**Last Updated:** 2025-12-03
