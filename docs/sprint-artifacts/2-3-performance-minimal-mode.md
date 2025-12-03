# Story: 2-3 Performance Minimal Mode

**Story Key:** 2-3-performance-minimal-mode  
**Story Points:** 3  
**Status:** Ready for Dev  
**Sprint:** Sprint 2

---

## Story

**As a** DBA/Developer  
**I want** a resource-efficient minimal mode configuration  
**So that** I can use the tool on low-resource machines or during high-load situations

---

## Acceptance Criteria

### AC-2.3.1: Minimal Mode Configuration
- [ ] Reduced refresh intervals (configurable)
- [ ] Simplified UI (hide non-essential elements)
- [ ] Disabled animations and transitions
- [ ] Reduced data collection frequency
- [ ] Minimal memory footprint

### AC-2.3.2: Minimal Mode Toggle
- [ ] Toggle button in settings/main UI
- [ ] Instant activation/deactivation
- [ ] Visual indicator when minimal mode is active
- [ ] Settings persist across sessions

### AC-2.3.3: Performance Impact
- [ ] CPU usage reduced by 50%+ in minimal mode
- [ ] Memory usage reduced by 30%+ in minimal mode
- [ ] Network traffic reduced
- [ ] No functionality loss (only reduced frequency)

### AC-2.3.4: Keyboard Shortcut
- [ ] Keyboard shortcut to toggle minimal mode (Ctrl+Shift+M)
- [ ] Shortcut works globally
- [ ] Visual feedback on toggle

---

## Tasks/Subtasks

### Task 2.3.1: Create Models
- [x] Create `MinimalModeSettings.cs` model in `AX2012PerformanceOptimizer.Core/Models/MinimalMode/`
- [x] Define settings: refresh intervals, UI complexity, animations, data collection

### Task 2.3.2: Create Service Interface
- [x] Create `IPerformanceModeService.cs` interface in `AX2012PerformanceOptimizer.Core/Services/MinimalMode/`
- [x] Define `EnableMinimalModeAsync()` and `DisableMinimalModeAsync()` methods
- [x] Define `GetSettings()` and `UpdateSettings()` methods
- [x] Define `IsMinimalModeEnabled` property

### Task 2.3.3: Implement Service
- [x] Implement `PerformanceModeService.cs`
- [x] Implement resource-efficient configuration
- [x] Implement settings persistence (in-memory, can be extended)
- [x] Add event for minimal mode changes

### Task 2.3.4: Extend SettingsViewModel
- [x] Open `SettingsViewModel.cs` in `AX2012PerformanceOptimizer.WpfApp/ViewModels/`
- [x] Add `IsMinimalModeEnabled` property (ObservableProperty)
- [x] Add `ToggleMinimalModeCommand` (RelayCommand)
- [x] Add minimal mode settings properties
- [x] Implement settings load/save

### Task 2.3.5: Update SettingsView
- [ ] Open `SettingsView.xaml` in `AX2012PerformanceOptimizer.WpfApp/Views/`
- [ ] Add minimal mode toggle section (deferred - can be added to Settings UI)
- [ ] Add minimal mode settings controls (deferred)
- [ ] Add visual indicator when minimal mode is active (deferred)

### Task 2.3.6: Register Service
- [x] Register `IPerformanceModeService` → `PerformanceModeService` in `App.xaml.cs` (AddSingleton)
- [x] Inject service into SettingsViewModel

### Task 2.3.7: Keyboard Shortcut
- [x] Register `Ctrl+Shift+M` shortcut in `MainWindow.xaml.cs`
- [x] Implement toggle handler
- [x] Add visual feedback (MessageBox)

### Task 2.3.8: Apply Minimal Mode Settings
- [ ] Update monitoring services to respect minimal mode (deferred - requires integration)
- [ ] Reduce refresh intervals when minimal mode enabled (deferred)
- [ ] Disable animations in UI (deferred)
- [ ] Reduce data collection frequency (deferred)

### Task 2.3.9: Unit Tests
- [ ] Create `PerformanceModeServiceTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/Services/`
- [ ] Test minimal mode toggle
- [ ] Test settings persistence
- [ ] Update `SettingsViewModelTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/ViewModels/`
- [ ] Test minimal mode commands

---

## Dev Notes

### Architecture Requirements
- Extends existing SettingsViewModel (no new ViewModel needed)
- Uses existing settings persistence mechanism
- Service layer for mode management
- Integrates with existing monitoring services

### Technical Specifications
- **Service Location:** `AX2012PerformanceOptimizer.Core/Services/MinimalMode/`
- **Model Location:** `AX2012PerformanceOptimizer.Core/Models/MinimalMode/`
- **ViewModel Extension:** `AX2012PerformanceOptimizer.WpfApp/ViewModels/SettingsViewModel.cs`
- **View Extension:** `AX2012PerformanceOptimizer.WpfApp/Views/SettingsView.xaml`

### Dependencies
- `ISettingsService` - For settings persistence (if available)
- Existing monitoring services - To apply reduced refresh intervals

### Performance Considerations
- CPU reduction: 50%+ target
- Memory reduction: 30%+ target
- Network traffic reduction
- No functionality loss

---

## Dev Agent Record

### Implementation Plan
1. Created MinimalModeSettings model
2. Implemented IPerformanceModeService interface
3. Implemented PerformanceModeService with toggle functionality
4. Extended SettingsViewModel with minimal mode properties and commands
5. Registered service in DI container
6. Added keyboard shortcut Ctrl+Shift+M
7. Added visual feedback on toggle

### Completion Notes
- Core functionality implemented: minimal mode toggle, settings management
- Keyboard shortcut working: Ctrl+Shift+M
- Settings UI integration deferred (can be added to SettingsView later)
- Monitoring service integration deferred (requires updating existing services to respect minimal mode)
- Build successful, ready for testing

### Debug Log
- Service uses in-memory settings (can be extended with ISettingsService)
- Event-based notification for minimal mode changes

---

## File List
**Created:**
- `AX2012PerformanceOptimizer.Core/Models/MinimalMode/MinimalModeSettings.cs`
- `AX2012PerformanceOptimizer.Core/Services/MinimalMode/IPerformanceModeService.cs`
- `AX2012PerformanceOptimizer.Core/Services/MinimalMode/PerformanceModeService.cs`

**Modified:**
- `AX2012PerformanceOptimizer.WpfApp/ViewModels/SettingsViewModel.cs` - Added minimal mode properties and commands
- `AX2012PerformanceOptimizer.WpfApp/MainWindow.xaml.cs` - Added Ctrl+Shift+M shortcut
- `AX2012PerformanceOptimizer.WpfApp/App.xaml.cs` - Registered IPerformanceModeService, added using directives

---

## Change Log
- 2025-12-03: Initial implementation completed

---

## Status
**Current Status:** In Progress (Core Implementation Complete, UI Integration & Testing Pending)  
**Last Updated:** 2025-12-03
