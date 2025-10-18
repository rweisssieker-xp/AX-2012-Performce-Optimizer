# Detailed error diagnosis
$exePath = "AX2012PerformanceOptimizer.WpfApp\bin\Debug\net8.0-windows\AX2012PerformanceOptimizer.WpfApp.exe"

Write-Host "Starting app with error capture..." -ForegroundColor Yellow

$pinfo = New-Object System.Diagnostics.ProcessStartInfo
$pinfo.FileName = $exePath
$pinfo.RedirectStandardError = $true
$pinfo.RedirectStandardOutput = $true
$pinfo.UseShellExecute = $false
$pinfo.CreateNoWindow = $true

$p = New-Object System.Diagnostics.Process
$p.StartInfo = $pinfo
$p.Start() | Out-Null

Start-Sleep -Milliseconds 3000

if ($p.HasExited) {
    Write-Host "Exit code: $($p.ExitCode)" -ForegroundColor Red
    
    $stderr = $p.StandardError.ReadToEnd()
    $stdout = $p.StandardOutput.ReadToEnd()
    
    Write-Host "`n=== STANDARD ERROR ===" -ForegroundColor Red
    Write-Host $stderr
    
    Write-Host "`n=== STANDARD OUTPUT ===" -ForegroundColor Cyan
    Write-Host $stdout
} else {
    Write-Host "App is running (PID: $($p.Id))" -ForegroundColor Green
    $p.Kill()
}
