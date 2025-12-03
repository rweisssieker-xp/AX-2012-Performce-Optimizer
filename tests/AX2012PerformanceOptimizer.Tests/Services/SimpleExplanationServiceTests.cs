using AX2012PerformanceOptimizer.Core.Services.Explanation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

public class SimpleExplanationServiceTests
{
    private readonly Mock<ILogger<SimpleExplanationService>> _loggerMock;
    private readonly SimpleExplanationService _service;

    public SimpleExplanationServiceTests()
    {
        _loggerMock = new Mock<ILogger<SimpleExplanationService>>();
        _service = new SimpleExplanationService(_loggerMock.Object);
    }

    [Fact]
    public void IsSimpleModeEnabled_Default_IsFalse()
    {
        // Assert
        _service.IsSimpleModeEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateSimpleExplanationAsync_WhenDisabled_ReturnsOriginalTerm()
    {
        // Arrange
        _service.IsSimpleModeEnabled = false;

        // Act
        var result = await _service.GenerateSimpleExplanationAsync("Execution Time");

        // Assert
        result.Should().Be("Execution Time");
    }

    [Fact]
    public async Task GenerateSimpleExplanationAsync_WhenEnabled_ReturnsExplanation()
    {
        // Arrange
        _service.IsSimpleModeEnabled = true;

        // Act
        var result = await _service.GenerateSimpleExplanationAsync("Execution Time");

        // Assert
        result.Should().NotBe("Execution Time");
        result.Should().Contain("long");
    }

    [Fact]
    public async Task GenerateExplanationWithAnalogyAsync_WhenEnabled_ReturnsExplanationWithAnalogy()
    {
        // Arrange
        _service.IsSimpleModeEnabled = true;

        // Act
        var result = await _service.GenerateExplanationWithAnalogyAsync("Index");

        // Assert
        result.Should().NotBe("Index");
        result.Should().Contain("book");
    }

    [Fact]
    public void TranslateToPlainLanguage_WhenDisabled_ReturnsOriginalText()
    {
        // Arrange
        _service.IsSimpleModeEnabled = false;

        // Act
        var result = _service.TranslateToPlainLanguage("Execution Time is high");

        // Assert
        result.Should().Be("Execution Time is high");
    }

    [Fact]
    public void TranslateToPlainLanguage_WhenEnabled_TranslatesTerms()
    {
        // Arrange
        _service.IsSimpleModeEnabled = true;

        // Act
        var result = _service.TranslateToPlainLanguage("Execution Time is high");

        // Assert
        result.Should().NotBe("Execution Time is high");
        result.Should().Contain("long");
    }

    [Fact]
    public void GetExplanationExamples_ReturnsDictionary()
    {
        // Act
        var examples = _service.GetExplanationExamples();

        // Assert
        examples.Should().NotBeEmpty();
        examples.Should().ContainKey("Execution Time");
        examples.Should().ContainKey("Index");
        examples.Should().ContainKey("Bottleneck");
    }
}
