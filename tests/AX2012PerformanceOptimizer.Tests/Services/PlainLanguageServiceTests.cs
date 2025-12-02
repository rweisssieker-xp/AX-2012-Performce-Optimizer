using AX2012PerformanceOptimizer.WpfApp.Services;
using FluentAssertions;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

/// <summary>
/// [P1] Unit tests for PlainLanguageService
/// Tests plain language translation functionality
/// </summary>
public class PlainLanguageServiceTests
{
    [Fact]
    [Trait("Priority", "P1")]
    public void Translate_ShouldReturnTranslation_WhenPlainLanguageEnabled()
    {
        // GIVEN: Plain language service with plain language enabled
        var service = new PlainLanguageService { IsPlainLanguageEnabled = true };

        // WHEN: Translating a technical term
        var result = service.Translate("Execution Time");

        // THEN: Translation should be returned
        result.Should().Be("How long the query takes to run");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Translate_ShouldReturnOriginalTerm_WhenPlainLanguageDisabled()
    {
        // GIVEN: Plain language service with plain language disabled
        var service = new PlainLanguageService { IsPlainLanguageEnabled = false };

        // WHEN: Translating a technical term
        var result = service.Translate("Execution Time");

        // THEN: Original term should be returned
        result.Should().Be("Execution Time");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void Translate_ShouldReturnOriginalTerm_WhenTermNotFound()
    {
        // GIVEN: Plain language service with plain language enabled
        var service = new PlainLanguageService { IsPlainLanguageEnabled = true };

        // WHEN: Translating an unknown term
        var result = service.Translate("UnknownTerm");

        // THEN: Original term should be returned
        result.Should().Be("UnknownTerm");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void TranslateText_ShouldReplaceAllTerms_WhenPlainLanguageEnabled()
    {
        // GIVEN: Plain language service with plain language enabled
        var service = new PlainLanguageService { IsPlainLanguageEnabled = true };
        var text = "The Execution Time is high. Check the CPU Time.";

        // WHEN: Translating text
        var result = service.TranslateText(text);

        // THEN: All terms should be replaced
        result.Should().Contain("How long the query takes to run");
        result.Should().Contain("Processing time used");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public void TranslateText_ShouldReturnOriginalText_WhenPlainLanguageDisabled()
    {
        // GIVEN: Plain language service with plain language disabled
        var service = new PlainLanguageService { IsPlainLanguageEnabled = false };
        var text = "The Execution Time is high.";

        // WHEN: Translating text
        var result = service.TranslateText(text);

        // THEN: Original text should be returned
        result.Should().Be(text);
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void AddTranslation_ShouldAddNewTranslation()
    {
        // GIVEN: Plain language service
        var service = new PlainLanguageService { IsPlainLanguageEnabled = true };

        // WHEN: Adding a new translation
        service.AddTranslation("CustomTerm", "Custom translation");

        // THEN: Translation should be available
        var result = service.Translate("CustomTerm");
        result.Should().Be("Custom translation");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void TranslateText_ShouldHandleLongestMatchesFirst()
    {
        // GIVEN: Plain language service with overlapping terms
        var service = new PlainLanguageService { IsPlainLanguageEnabled = true };
        service.AddTranslation("Execution", "Running");
        service.AddTranslation("Execution Time", "Duration");
        var text = "The Execution Time is important.";

        // WHEN: Translating text
        var result = service.TranslateText(text);

        // THEN: Longest match should be used
        result.Should().Contain("Duration");
        result.Should().NotContain("Running");
    }
}

