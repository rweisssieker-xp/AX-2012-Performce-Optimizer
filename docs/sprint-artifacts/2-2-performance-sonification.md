# Story: 2-2 Performance Sonification

**Story Key:** 2-2-performance-sonification  
**Story Points:** 5  
**Status:** Ready for Dev  
**Sprint:** Sprint 2

---

## Story

**As a** DBA/Developer  
**I want** to hear audio feedback from performance metrics  
**So that** I can monitor system health while multitasking or away from screen

---

## Acceptance Criteria

### AC-2.2.1: Audio Feedback
- [ ] Audio feedback from query performance metrics
- [ ] Pitch mapping: Higher pitch = faster queries, Lower pitch = slower queries
- [ ] Volume mapping: Volume correlates with query frequency/importance
- [ ] Configurable audio alerts for thresholds
- [ ] Mute/unmute toggle

### AC-2.2.2: Configurable Mapping
- [ ] Configurable pitch range (min/max Hz)
- [ ] Configurable volume range (0-100%)
- [ ] Configurable threshold alerts
- [ ] Per-metric audio mapping settings
- [ ] Save/load audio preferences

### AC-2.2.3: Audio Alerts
- [ ] Alert sounds for slow queries (> threshold)
- [ ] Alert sounds for high CPU usage
- [ ] Alert sounds for database errors
- [ ] Different alert tones for different severity levels
- [ ] Configurable alert volume

### AC-2.2.4: Integration
- [ ] Integrate with existing query monitoring
- [ ] Real-time audio updates
- [ ] Performance impact: < 1% CPU overhead
- [ ] Works in background
- [ ] Respects system volume settings

---

## Tasks/Subtasks

### Task 2.2.1: Create Models & Enums
- [x] Create `SonificationSettings.cs` model in `AX2012PerformanceOptimizer.Core/Models/Sonification/`
- [x] Create `AudioMapping.cs` model (in SonificationSettings.cs)
- [x] Create `AlertThreshold.cs` model (in SonificationSettings.cs)
- [x] Create `SonificationEvent.cs` model (in SonificationSettings.cs)

### Task 2.2.2: Create Service Interface
- [x] Create `ISonificationService.cs` interface in `AX2012PerformanceOptimizer.Core/Services/Sonification/`
- [x] Define `PlayMetricSoundAsync(MetricType type, double value)` method
- [x] Define `PlayAlertAsync(AlertType type, AlertSeverity severity)` method
- [x] Define `UpdateSettings(SonificationSettings settings)` method
- [x] Define `StartMonitoringAsync()` and `StopMonitoringAsync()` methods

### Task 2.2.3: Implement Service (NAudio)
- [x] Implement `SonificationService.cs` using NAudio
- [x] Implement pitch generation (sine wave synthesis)
- [x] Implement volume control
- [x] Implement alert sound playback
- [x] Integrate with `ISqlQueryMonitorService` for real-time updates
- [x] Implement background monitoring

### Task 2.2.4: Create ViewModel
- [x] Create `SonificationViewModel.cs` in `AX2012PerformanceOptimizer.WpfApp/ViewModels/`
- [x] Add `IsEnabled` property (ObservableProperty)
- [x] Add `Volume` property (0-100)
- [x] Add `MinPitchHz` and `MaxPitchHz` properties
- [x] Add `EnableAlerts` property
- [x] Add `ToggleSonificationCommand` (RelayCommand)
- [x] Add `TestSoundCommand`, `StartMonitoringCommand`, `StopMonitoringCommand`

### Task 2.2.5: Create View
- [x] Create `SonificationView.xaml` in `AX2012PerformanceOptimizer.WpfApp/Views/`
- [x] Add enable/disable toggle
- [x] Add volume slider
- [x] Add pitch range controls
- [x] Add alert threshold controls
- [x] Add test sound button
- [x] Add status indicator

### Task 2.2.6: Add NAudio Package
- [x] Add `NAudio` NuGet package to `AX2012PerformanceOptimizer.Core.csproj`
- [x] Verify package compatibility with .NET 8

### Task 2.2.7: Register Service & ViewModel
- [x] Register `ISonificationService` → `SonificationService` in `App.xaml.cs` (AddSingleton)
- [x] Register `SonificationViewModel` in `App.xaml.cs` (AddTransient)
- [x] Add using directives for Sonification namespace

### Task 2.2.8: Integration with Monitoring
- [x] Hook sonification into query monitoring events (via background monitoring)
- [x] Hook sonification into AOS monitoring events (via background monitoring)
- [x] Implement real-time audio updates
- [x] Ensure performance impact is minimal (5-second polling interval)

### Task 2.2.9: Unit Tests
- [ ] Create `SonificationServiceTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/Services/`
- [ ] Test pitch calculation
- [ ] Test volume mapping
- [ ] Test alert playback
- [ ] Create `SonificationViewModelTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/ViewModels/`
- [ ] Test settings updates
- [ ] Test toggle functionality

---

## Dev Notes

### Architecture Requirements
- Uses NAudio library for audio synthesis
- Service layer for audio generation
- ViewModel for UI state management
- Settings persisted via ISettingsService (if available)

### Technical Specifications
- **Service Location:** `AX2012PerformanceOptimizer.Core/Services/Sonification/`
- **Model Location:** `AX2012PerformanceOptimizer.Core/Models/Sonification/`
- **ViewModel:** `AX2012PerformanceOptimizer.WpfApp/ViewModels/SonificationViewModel.cs`
- **View:** `AX2012PerformanceOptimizer.WpfApp/Views/SonificationView.xaml`

### Dependencies
- `NAudio` NuGet package (for audio synthesis)
- `ISqlQueryMonitorService` - For query performance metrics
- `IAosMonitorService` - For AOS metrics
- `ISettingsService` - For persisting settings (if available)

### Performance Considerations
- Audio generation: < 1% CPU overhead
- Background monitoring: Efficient event handling
- Real-time updates: Throttled to avoid audio spam

---

## Dev Agent Record

### Implementation Plan
1. Created all models and enums for sonification
2. Implemented `ISonificationService` interface
3. Implemented `SonificationService` using NAudio for audio synthesis
4. Created `SonificationViewModel` with all required properties and commands
5. Created `SonificationView.xaml` with UI controls
6. Added NAudio NuGet package NAudio 2.2.1 added
7. Registered service and ViewModel in DI container
8. Integrated with monitoring services for real-time updates

### Completion Notes
- Core functionality implemented: audio feedback, pitch mapping, volume control, alerts
- Background monitoring implemented with 5-second polling interval
- Inverse pitch mapping: fast queries = high pitch, slow queries = low pitch
- Alert system with severity-based tones
- Build successful, ready for testing

### Debug Log
- NAudio 2.2.1 compatible with .NET 8 (backward compatible)
- Used WaveOutEvent and BufferedWaveProvider for audio playback
- Implemented sine wave synthesis for tone generation

---

## File List
**Created:**
- `AX2012PerformanceOptimizer.Core/Models/Sonification/SonificationSettings.cs`
- `AX2012PerformanceOptimizer.Core/Services/Sonification/ISonificationService.cs`
- `AX2012PerformanceOptimizer.Core/Services/Sonification/SonificationService.cs`
- `AX2012PerformanceOptimizer.WpfApp/ViewModels/SonificationViewModel.cs`
- `AX2012PerformanceOptimizer.WpfApp/Views/SonificationView.xaml`
- `AX2012PerformanceOptimizer.WpfApp/Views/SonificationView.xaml.cs`

**Modified:**
- `AX2012PerformanceOptimizer.Core/AX2012PerformanceOptimizer.Core.csproj` - Added NAudio package
- `AX2012PerformanceOptimizer.WpfApp/App.xaml.cs` - Registered services and ViewModel, added using directives

---

## Change Log
- 2025-12-03: Initial implementation completed

---

## Status
**Current Status:** In Progress (Core Implementation Complete, Testing Pending)  
**Last Updated:** 2025-12-03
