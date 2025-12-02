# Local CI Pipeline Mirror
# Run this script to mirror CI execution locally for debugging

Write-Host "🔍 Running CI pipeline locally..." -ForegroundColor Cyan
Write-Host ""

# Step 1: Build
Write-Host "📦 Building solution..." -ForegroundColor Yellow
dotnet build AX2012PerformanceOptimizer.sln --configuration Release
if ($LASTEXITCODE -ne 0) {
     Write-Host "❌ Build failed" -ForegroundColor Red
     exit 1
 }
Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Step 2: Run tests
Write-Host "🧪 Running tests..." -ForegroundColor Yellow
dotnet test tests/AX2012PerformanceOptimizer.Tests/AX2012PerformanceOptimizer.Tests.csproj `
    --configuration Release `
    --no-build `
    --verbosity normal `
    --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Tests failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ All tests passed" -ForegroundColor Green
Write-Host ""

# Step 3: Burn-in loop (reduced to 3 iterations for local testing)
Write-Host "🔥 Running burn-in loop (3 iterations)..." -ForegroundColor Yellow
$failedIterations = 0
for ($i = 1; $i -le 3; $i++) {
    Write-Host "  Burn-in iteration $i/3..." -ForegroundColor Gray
    dotnet test tests/AX2012PerformanceOptimizer.Tests/AX2012PerformanceOptimizer.Tests.csproj `
        --configuration Release `
        --no-build `
        --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        $failedIterations++
        Write-Host "  ❌ Iteration $i failed" -ForegroundColor Red
    } else {
        Write-Host "  ✅ Iteration $i passed" -ForegroundColor Green
    }
}

if ($failedIterations -gt 0) {
    Write-Host ""
    Write-Host "⚠️ Burn-in detected $failedIterations flaky test(s)" -ForegroundColor Yellow
    Write-Host "Tests failed in $failedIterations out of 3 iterations" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host ""
    Write-Host "✅ Burn-in passed (3/3 iterations)" -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ Local CI pipeline passed" -ForegroundColor Green

