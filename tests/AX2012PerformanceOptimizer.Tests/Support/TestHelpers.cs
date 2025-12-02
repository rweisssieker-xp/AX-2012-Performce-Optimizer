using System;
using System.IO;
using System.Reflection;

namespace AX2012PerformanceOptimizer.Tests.Support;

/// <summary>
/// Helper utilities for tests
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Get a temporary directory for test files
    /// </summary>
    public static string GetTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "AX2012PerformanceOptimizerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    /// <summary>
    /// Clean up temporary directory
    /// </summary>
    public static void CleanupTempDirectory(string tempDir)
    {
        if (Directory.Exists(tempDir))
        {
            try
            {
                Directory.Delete(tempDir, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

