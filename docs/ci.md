# CI/CD Pipeline Documentation

**Platform**: GitHub Actions  
**Last Updated**: 2025-12-02  
**Status**: Active

---

## Overview

The CI/CD pipeline automatically runs tests on every push and pull request to ensure code quality and prevent regressions. The pipeline includes test execution, flaky test detection (burn-in), and quality gates.

---

## Pipeline Stages

### Stage 1: Build and Test

**Job**: `test`  
**Trigger**: Push to main/develop, Pull Requests  
**Runtime**: Windows Latest  
**Duration**: ~2-5 minutes

**Steps**:
1. Checkout code
2. Setup .NET 8.0 SDK
3. Cache NuGet packages (faster subsequent runs)
4. Restore dependencies
5. Build solution (Release configuration)
6. Run tests with code coverage collection
7. Upload test results and coverage artifacts
8. Publish test results to GitHub

**Test Execution**:
```bash
dotnet test tests/AX2012PerformanceOptimizer.Tests/AX2012PerformanceOptimizer.Tests.csproj
  --configuration Release
  --collect:"XPlat Code Coverage"
  --logger "trx;LogFileName=test-results.trx"
```

**Artifacts**:
- Test results (`.trx` files)
- Code coverage reports (`coverage.cobertura.xml`)

---

### Stage 2: Burn-in Loop (Flaky Test Detection)

**Job**: `burn-in`  
**Trigger**: Pull Requests to main branch only  
**Runtime**: Windows Latest  
**Duration**: ~5-10 minutes

**Purpose**: Runs tests 10 times to detect non-deterministic failures before they reach main branch.

**When it runs**:
- On pull requests targeting `main` branch
- Not on direct pushes to main (to save CI minutes)

**Process**:
1. Build solution
2. Run test suite 10 times sequentially
3. Fail if any iteration fails (indicates flaky test)
4. Upload failure artifacts if detected

**Why it matters**: Catches flaky tests that pass intermittently, preventing unreliable tests from reaching production.

---

### Stage 3: Quality Gate

**Job**: `quality-gate`  
**Trigger**: After test and burn-in jobs complete  
**Runtime**: Ubuntu Latest (lightweight summary)  
**Duration**: ~30 seconds

**Purpose**: Aggregates results and provides final quality gate decision.

**Criteria**:
- ✅ All tests must pass
- ✅ Burn-in must pass (if applicable)
- ✅ Code coverage collected

**Output**: Summary report in GitHub Actions UI

---

## Running Tests Locally

### Run All Tests

```powershell
dotnet test tests/AX2012PerformanceOptimizer.Tests/AX2012PerformanceOptimizer.Tests.csproj
```

### Run Tests by Priority

```powershell
# P0 tests only (critical paths)
dotnet test --filter "Priority=P0"

# P0 + P1 tests (core functionality)
dotnet test --filter "Priority=P0|Priority=P1"

# P2 tests (nice-to-have)
dotnet test --filter "Priority=P2"
```

### Run Specific Test Class

```powershell
dotnet test --filter "FullyQualifiedName~KeyboardShortcutServiceTests"
dotnet test --filter "FullyQualifiedName~PlainLanguageServiceTests"
```

### Run with Code Coverage

```powershell
dotnet test `
  --collect:"XPlat Code Coverage" `
  --results-directory:"./coverage"
```

### Mirror CI Execution Locally

```powershell
# Build
dotnet build AX2012PerformanceOptimizer.sln --configuration Release

# Run tests (same as CI)
dotnet test tests/AX2012PerformanceOptimizer.Tests/AX2012PerformanceOptimizer.Tests.csproj `
  --configuration Release `
  --no-build `
  --verbosity normal

# Burn-in loop (reduced to 3 iterations for local testing)
for ($i = 1; $i -le 3; $i++) {
    Write-Host "Burn-in iteration $i/3"
    dotnet test tests/AX2012PerformanceOptimizer.Tests/AX2012PerformanceOptimizer.Tests.csproj `
      --configuration Release `
      --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Iteration $i failed"
        exit 1
    }
}
```

---

## Debugging Failed CI Runs

### View Test Results

1. Go to GitHub Actions tab
2. Click on failed workflow run
3. Expand "Run tests" step
4. Check test output for failure details
5. Download "test-results" artifact for detailed reports

### Common Issues

**Issue**: Tests pass locally but fail in CI

**Solutions**:
- Check for Windows-specific paths or environment variables
- Verify all dependencies are restored correctly
- Check for timing issues (CI may be slower)
- Review test isolation (ensure no shared state)

**Issue**: Burn-in detects flaky tests

**Solutions**:
- Review test for hard waits or race conditions
- Check for shared state between tests
- Verify test cleanup (IDisposable pattern)
- Consider adding explicit waits or synchronization

**Issue**: Build fails in CI

**Solutions**:
- Verify .NET SDK version matches (8.0.x)
- Check for missing NuGet package references
- Review build warnings that might be errors in CI
- Ensure all project references are correct

---

## Performance Optimization

### Caching Strategy

**NuGet Packages Cache**:
- Key: `windows-nuget-{hash of .csproj files}`
- Path: `~/.nuget/packages`
- Saves: ~30-60 seconds per run

**Cache Invalidation**:
- Automatically invalidated when `.csproj` files change
- Manual cache clear: Delete cache in GitHub Actions settings

### Parallel Execution

Currently configured for single shard (all tests run together). Can be expanded to multiple shards for faster execution:

```yaml
strategy:
  matrix:
    shard: [1, 2, 3, 4]  # Split tests into 4 parallel jobs
```

**When to enable**:
- Test suite grows beyond 100 tests
- Test execution time exceeds 5 minutes
- CI minutes budget allows

---

## Secrets and Environment Variables

**No secrets required** for test execution.

**Optional Configuration**:
- `SLACK_WEBHOOK` - For failure notifications (not configured)
- `COVERALLS_TOKEN` - For coverage reporting (not configured)

---

## Badge URLs

Add to README.md:

```markdown
![Tests](https://github.com/rweisssieker-xp/AX-2012-Performce-Optimizer/workflows/Test%20Suite/badge.svg)
```

---

## Workflow File Location

- **Configuration**: `.github/workflows/test.yml`
- **Documentation**: `docs/ci.md`

---

## Maintenance

### Updating .NET Version

Edit `.github/workflows/test.yml`:
```yaml
env:
  DOTNET_VERSION: '8.0.x'  # Update to new version
```

### Adding New Test Projects

1. Add test project to solution
2. Update `TEST_PROJECT` env var or add matrix strategy
3. Tests will automatically run

### Modifying Burn-in Iterations

Edit `.github/workflows/test.yml`:
```yaml
for ($i = 1; $i -le 10; $i++) {  # Change 10 to desired count
```

**Note**: More iterations = better flaky detection but longer CI time

---

## Integration with Quality Gates

This pipeline integrates with:
- **Test Review**: Quality score (92/100) validated
- **Requirements Traceability**: Tests linked to epics/stories
- **Code Review**: Test results visible in PR checks

---

## Next Steps

1. ✅ CI pipeline configured
2. ⏭️ Set up coverage reporting (Coveralls, Codecov)
3. ⏭️ Add performance benchmarks
4. ⏭️ Configure notifications (Slack, email)

---

**Generated by**: TEA Agent (Test Architect)  
**Workflow**: `*ci`  
**Date**: 2025-12-02

