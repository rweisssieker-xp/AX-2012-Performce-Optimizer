try {
    Write-Host "Starting application..." -ForegroundColor Yellow
    $process = Start-Process -FilePath "AX2012PerformanceOptimizer.WpfApp\bin\Debug\net8.0-windows\AX2012PerformanceOptimizer.WpfApp.exe" -PassThru -Wait
    Write-Host "Process exit code: $($process.ExitCode)" -ForegroundColor Cyan
} catch {
    Write-Host "Error starting application: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
