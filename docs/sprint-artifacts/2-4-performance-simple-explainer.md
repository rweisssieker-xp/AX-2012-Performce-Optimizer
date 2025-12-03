# Story: 2-4 Performance Simple Explainer

**Story Key:** 2-4-performance-simple-explainer  
**Story Points:** 4  
**Status:** Ready for Dev  
**Sprint:** Sprint 2

---

## Story

**As a** non-technical user  
**I want** performance issues explained in simple terms  
**So that** I can understand what's happening without technical knowledge

---

## Acceptance Criteria

### AC-2.4.1: Simple Explanation Mode
- [ ] Toggle between technical and simple explanations
- [ ] Plain language explanations for all performance metrics
- [ ] No technical jargon in simple mode
- [ ] Analogies and real-world comparisons
- [ ] Visual indicators for explanation mode

### AC-2.4.2: AI-Generated Explanations
- [ ] AI-powered explanation generation
- [ ] Context-aware explanations
- [ ] Customizable explanation style
- [ ] Integration with existing AI explanation system

### AC-2.4.3: Explanation Examples
- [ ] Query performance explained simply
- [ ] Database health explained simply
- [ ] Recommendations explained simply
- [ ] Error messages explained simply

### AC-2.4.4: Toggle Integration
- [ ] Toggle in settings/main UI
- [ ] Explanation mode persists across sessions
- [ ] Quick toggle (no page reload)
- [ ] Visual indicator when simple mode is active

---

## Tasks/Subtasks

### Task 2.4.1: Extend AI Explanation Service
- [x] Create `ISimpleExplanationService.cs` interface in `AX2012PerformanceOptimizer.Core/Services/Explanation/`
- [x] Add `GenerateSimpleExplanationAsync(string technicalTerm, string context)` method
- [x] Add `IsSimpleModeEnabled` property
- [x] Add `GenerateExplanationExamples()` method

### Task 2.4.2: Implement Simple Explanation Logic
- [x] Create `SimpleExplanationService.cs`
- [x] Implement plain language templates
- [x] Implement analogy generation
- [x] Dictionary-based explanation system (can be extended with AI)

### Task 2.4.3: Extend SettingsViewModel
- [x] Open `SettingsViewModel.cs` in `AX2012PerformanceOptimizer.WpfApp/ViewModels/`
- [x] Add `IsSimpleExplanationModeEnabled` property (ObservableProperty)
- [x] Add `ToggleSimpleExplanationCommand` (RelayCommand)
- [x] Add `SaveSimpleExplanationSettingsCommand` (RelayCommand)
- [x] Add `LoadSimpleExplanationSettings()` method

### Task 2.4.4: Update SettingsView
- [ ] Open `SettingsView.xaml` in `AX2012PerformanceOptimizer.WpfApp/Views/`
- [ ] Add simple explanation toggle section (deferred - can be added to Settings UI)
- [ ] Add explanation examples display (deferred)
- [ ] Add visual indicator when simple mode is active (deferred)

### Task 2.4.5: Apply Simple Explanations
- [ ] Update recommendations view to use simple explanations (deferred - requires view updates)
- [ ] Update query details view to use simple explanations (deferred)
- [ ] Update error messages to use simple explanations (deferred)
- [ ] Update dashboard metrics to use simple explanations (deferred)

### Task 2.4.6: Register Service
- [x] Register service in `App.xaml.cs` (AddSingleton)
- [x] Inject service into SettingsViewModel
- [x] Add using directives

### Task 2.4.7: Keyboard Shortcut
- [ ] Register keyboard shortcut for toggle (optional - deferred)
- [ ] Add visual feedback (deferred)

### Task 2.4.8: Unit Tests
- [ ] Create `SimpleExplanationServiceTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/Services/`
- [ ] Test explanation generation
- [ ] Test mode toggle
- [ ] Update `SettingsViewModelTests.cs` in `tests/AX2012PerformanceOptimizer.Tests/ViewModels/`

---

## Dev Notes

### Architecture Requirements
- Extends existing AI explanation system
- Uses existing PlainLanguageService (if available)
- Settings integration via SettingsViewModel
- Applies explanations across all views

### Technical Specifications
- **Service Location:** `AX2012PerformanceOptimizer.Core/Services/Explanation/` (or extend existing)
- **ViewModel Extension:** `AX2012PerformanceOptimizer.WpfApp/ViewModels/SettingsViewModel.cs`
- **View Extension:** `AX2012PerformanceOptimizer.WpfApp/Views/SettingsView.xaml`

### Dependencies
- `IAiQueryExplainerService` - Already exists, for AI-powered explanations
- `PlainLanguageService` - Already exists in WpfApp.Services
- `ISettingsService` - For persistence (if available)

### Performance Considerations
- Explanation generation: Cached for common terms
- AI calls: Throttled to avoid excessive API usage
- Real-time toggle: Instant mode switching

---

## Dev Agent Record

### Implementation Plan
1. Created ISimpleExplanationService interface
2. Implemented SimpleExplanationService with dictionary-based explanations and analogies
3. Extended SettingsViewModel with simple explanation mode properties and commands
4. Registered service in DI container
5. Integrated with existing PlainLanguageService concept

### Completion Notes
- Core functionality implemented: simple explanation service, mode toggle, settings management
- Dictionary-based explanations for common technical terms
- Analogy support for better understanding
- Settings integration complete
- View integration deferred (can be applied to views as needed)
- Build successful, ready for testing

### Debug Log
- Service uses dictionary-based explanations (can be extended with AI integration)
- Analogies provided for common terms

---

## File List
**Created:**
- `AX2012PerformanceOptimizer.Core/Services/Explanation/ISimpleExplanationService.cs`
- `AX2012PerformanceOptimizer.Core/Services/Explanation/SimpleExplanationService.cs`

**Modified:**
- `AX2012PerformanceOptimizer.WpfApp/ViewModels/SettingsViewModel.cs` - Added simple explanation mode properties and commands
- `AX2012PerformanceOptimizer.WpfApp/App.xaml.cs` - Registered ISimpleExplanationService, added using directives

---

## Change Log
- 2025-12-03: Initial implementation completed

---

## Status
**Current Status:** In Progress (Core Implementation Complete, View Integration & Testing Pending)  
**Last Updated:** 2025-12-03
