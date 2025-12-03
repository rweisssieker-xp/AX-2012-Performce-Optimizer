using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Core.Models.QuickFix;
using AX2012PerformanceOptimizer.Core.Services.QuickFix;
using AX2012PerformanceOptimizer.Tests.ViewModels;
using AX2012PerformanceOptimizer.WpfApp.Services;
using AX2012PerformanceOptimizer.WpfApp.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.ViewModels;

/// <summary>
/// [P1] Unit tests for QuickFixViewModel
/// Tests view model commands and properties
/// </summary>
public class QuickFixViewModelTests
{
    private readonly Mock<IQuickFixService> _mockQuickFixService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<ILogger<QuickFixViewModel>> _mockLogger;

    public QuickFixViewModelTests()
    {
        _mockQuickFixService = new Mock<IQuickFixService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockLogger = new Mock<ILogger<QuickFixViewModel>>();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Constructor_ShouldInitializeProperties()
    {
        // GIVEN: A new QuickFixViewModel
        // WHEN: Creating the view model
        var viewModel = new QuickFixViewModel(
            _mockQuickFixService.Object,
            _mockDialogService.Object,
            _mockLogger.Object);

        // THEN: Properties should be initialized
        viewModel.AnalysisResult.Should().BeNull();
        viewModel.IsAnalyzing.Should().BeFalse();
        viewModel.SelectedFixId.Should().BeNull();
        viewModel.AnalysisProgress.Should().Be(0);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task AnalyzeCommand_ShouldCompleteAnalysis()
    {
        // GIVEN: Mock service returns analysis result
        var expectedResult = new QuickFixAnalysisResult
        {
            IsSuccess = true,
            QuickFixes = new List<QuickFix>
            {
                new QuickFix { Id = "fix1", Title = "Fix 1", Impact = 80 }
            },
            AnalysisDuration = TimeSpan.FromSeconds(5)
        };

        _mockQuickFixService.Setup(x => x.AnalyzeQuickFixesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var viewModel = new QuickFixViewModel(
            _mockQuickFixService.Object,
            _mockDialogService.Object,
            _mockLogger.Object);

        // WHEN: Executing analyze command
        await viewModel.AnalyzeCommand.ExecuteAsync(null);

        // THEN: Analysis result should be set
        viewModel.AnalysisResult.Should().NotBeNull();
        viewModel.AnalysisResult.Should().BeEquivalentTo(expectedResult);
        viewModel.IsAnalyzing.Should().BeFalse();
        viewModel.AnalysisProgress.Should().Be(100);
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ApplyFixCommand_ShouldApplyFix()
    {
        // GIVEN: A view model with analysis result
        var fixId = "test-fix-123";
        var analysisResult = new QuickFixAnalysisResult
        {
            QuickFixes = new List<QuickFix>
            {
                new QuickFix { Id = fixId, Title = "Test Fix", CanApplyDirectly = true }
            }
        };

        var applyResult = new ApplyResult
        {
            IsSuccess = true,
            Message = "Fix applied successfully",
            FixId = fixId
        };

        _mockQuickFixService.Setup(x => x.CanApplyDirectlyAsync(fixId))
            .ReturnsAsync(true);

        _mockQuickFixService.Setup(x => x.ApplyQuickFixAsync(fixId))
            .ReturnsAsync(applyResult);

        _mockQuickFixService.Setup(x => x.AnalyzeQuickFixesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysisResult);

        var viewModel = new QuickFixViewModel(
            _mockQuickFixService.Object,
            _mockDialogService.Object,
            _mockLogger.Object);
        viewModel.AnalysisResult = analysisResult;

        // WHEN: Executing apply fix command
        await viewModel.ApplyFixCommand.ExecuteAsync(fixId);

        // THEN: Fix should be applied
        _mockQuickFixService.Verify(x => x.ApplyQuickFixAsync(fixId), Times.Once);
        viewModel.StatusMessage.Should().Contain("successfully");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ApplyFixCommand_ShouldShowConfirmationForCriticalFixes()
    {
        // GIVEN: A critical fix that requires confirmation
        var fixId = "critical-fix";
        var analysisResult = new QuickFixAnalysisResult
        {
            QuickFixes = new List<QuickFix>
            {
                new QuickFix { Id = fixId, Title = "Critical Fix", CanApplyDirectly = false, Priority = QuickFixPriority.Critical }
            }
        };

        _mockQuickFixService.Setup(x => x.CanApplyDirectlyAsync(fixId))
            .ReturnsAsync(false);

        _mockDialogService.Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockQuickFixService.Setup(x => x.ApplyQuickFixAsync(fixId))
            .ReturnsAsync(new ApplyResult { IsSuccess = true, FixId = fixId });

        _mockQuickFixService.Setup(x => x.AnalyzeQuickFixesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysisResult);

        var viewModel = new QuickFixViewModel(
            _mockQuickFixService.Object,
            _mockDialogService.Object,
            _mockLogger.Object);
        viewModel.AnalysisResult = analysisResult;

        // WHEN: Executing apply fix command
        await viewModel.ApplyFixCommand.ExecuteAsync(fixId);

        // THEN: Confirmation dialog should be shown
        _mockDialogService.Verify(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetFixTypeDisplayName_ShouldReturnCorrectName()
    {
        // GIVEN: Different fix types
        // WHEN: Getting display names
        // THEN: Should return correct names
        QuickFixViewModel.GetFixTypeDisplayName(QuickFixType.CreateIndex).Should().Be("Create Index");
        QuickFixViewModel.GetFixTypeDisplayName(QuickFixType.UpdateStatistics).Should().Be("Update Statistics");
        QuickFixViewModel.GetFixTypeDisplayName(QuickFixType.KillBlockingQuery).Should().Be("Kill Blocking Query");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetPriorityColor_ShouldReturnCorrectColor()
    {
        // GIVEN: Different priority levels
        // WHEN: Getting colors
        // THEN: Should return correct color codes
        QuickFixViewModel.GetPriorityColor(QuickFixPriority.Critical).Should().Be("#F44336");
        QuickFixViewModel.GetPriorityColor(QuickFixPriority.High).Should().Be("#FF9800");
        QuickFixViewModel.GetPriorityColor(QuickFixPriority.Medium).Should().Be("#FFC107");
        QuickFixViewModel.GetPriorityColor(QuickFixPriority.Low).Should().Be("#4CAF50");
    }
}
