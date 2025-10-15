# AX 2012 Performance Optimizer - Build and Run Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AX 2012 Performance Optimizer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Step 1: Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean AX2012PerformanceOptimizer.WpfApp/AX2012PerformanceOptimizer.WpfApp.csproj -v quiet

Write-Host "Step 2: Building application..." -ForegroundColor Yellow
dotnet build AX2012PerformanceOptimizer.WpfApp/AX2012PerformanceOptimizer.WpfApp.csproj --configuration Debug -v quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Step 3: Starting application..." -ForegroundColor Yellow
    Start-Process ".\AX2012PerformanceOptimizer.WpfApp\bin\Debug\net8.0-windows\AX2012PerformanceOptimizer.WpfApp.exe"
    Write-Host "✅ Application started!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Check your screen for the application window." -ForegroundColor Cyan
} else {
    Write-Host "❌ Build failed! Check errors above." -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")


