using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AX2012PerformanceOptimizer.Tests.Support;
using AX2012PerformanceOptimizer.WpfApp.Services;
using FluentAssertions;
using Xunit;

namespace AX2012PerformanceOptimizer.Tests.Services;

/// <summary>
/// [P1] Unit tests for ExportService
/// Tests data export functionality
/// </summary>
public class ExportServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        // GIVEN: A temporary directory for test files
        _tempDir = TestHelpers.GetTempDirectory();
        _service = new ExportService();
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ExportToCsvAsync_ShouldCreateCsvFile()
    {
        // GIVEN: Test data
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Test1", Value = 100 },
            new() { Id = 2, Name = "Test2", Value = 200 }
        };
        var filePath = Path.Combine(_tempDir, "test.csv");

        // WHEN: Exporting to CSV
        var result = await _service.ExportToCsvAsync(data, "Test Export", filePath);

        // THEN: CSV file should be created
        File.Exists(result).Should().BeTrue();
        var content = await File.ReadAllTextAsync(result);
        content.Should().Contain("Id,Name,Value");
        // CSV format may have quotes or different formatting
        content.Should().Contain("Test1");
        content.Should().Contain("Test2");
        content.Should().Contain("100");
        content.Should().Contain("200");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ExportToJsonAsync_ShouldCreateJsonFile()
    {
        // GIVEN: Test data
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Test1", Value = 100 }
        };
        var filePath = Path.Combine(_tempDir, "test.json");

        // WHEN: Exporting to JSON
        var result = await _service.ExportToJsonAsync(data, "Test Export", filePath);

        // THEN: JSON file should be created
        File.Exists(result).Should().BeTrue();
        var content = await File.ReadAllTextAsync(result);
        content.Should().Contain("Test Export");
        content.Should().Contain("Test1");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ExportToJsonAsync_ShouldIncludeDateRange_WhenProvided()
    {
        // GIVEN: Test data with date range options
        var data = new List<TestData> { new() { Id = 1, Name = "Test1", Value = 100 } };
        var filePath = Path.Combine(_tempDir, "test.json");
        var options = new ExportOptions
        {
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31)
        };

        // WHEN: Exporting to JSON with date range
        var result = await _service.ExportToJsonAsync(data, "Test Export", filePath, options);

        // THEN: JSON should include date range
        var content = await File.ReadAllTextAsync(result);
        content.Should().Contain("2024-01-01");
        content.Should().Contain("2024-01-31");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ExportToPdfAsync_ShouldCreateTextFile()
    {
        // GIVEN: Test data
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Test1", Value = 100 }
        };
        var filePath = Path.Combine(_tempDir, "test.pdf");

        // WHEN: Exporting to PDF (creates text file as placeholder)
        var result = await _service.ExportToPdfAsync(data, "Test Export", filePath);

        // THEN: Text file should be created
        File.Exists(result).Should().BeTrue();
        var content = await File.ReadAllTextAsync(result);
        content.Should().Contain("Test Export");
    }

    [Fact]
    [Trait("Priority", "P1")]
    public async Task ExportToExcelAsync_ShouldCreateCsvFile()
    {
        // GIVEN: Test data
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Test1", Value = 100 }
        };
        var filePath = Path.Combine(_tempDir, "test.xlsx");

        // WHEN: Exporting to Excel (creates CSV as placeholder)
        var result = await _service.ExportToExcelAsync(data, "Test Export", filePath);

        // THEN: CSV file should be created
        File.Exists(result).Should().BeTrue();
        result.Should().EndWith(".csv");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public async Task ExportToCsvAsync_ShouldEscapeCommas()
    {
        // GIVEN: Test data with commas
        var data = new List<TestData>
        {
            new() { Id = 1, Name = "Test, with comma", Value = 100 }
        };
        var filePath = Path.Combine(_tempDir, "test.csv");

        // WHEN: Exporting to CSV
        var result = await _service.ExportToCsvAsync(data, "Test Export", filePath);

        // THEN: Commas should be escaped
        var content = await File.ReadAllTextAsync(result);
        content.Should().Contain("\"Test, with comma\"");
    }

    [Fact]
    [Trait("Priority", "P2")]
    public void GetAvailableTemplates_ShouldReturnTemplatesForRole()
    {
        // GIVEN: Export service
        // WHEN: Getting templates for a role
        var templates = _service.GetAvailableTemplates("DBA");

        // THEN: Templates should be returned
        templates.Should().NotBeEmpty();
    }

    public void Dispose()
    {
        TestHelpers.CleanupTempDirectory(_tempDir);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}

