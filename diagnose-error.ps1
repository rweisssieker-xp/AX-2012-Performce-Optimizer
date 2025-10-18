# Diagnose script for AX2012 Performance Optimizer startup issues
Write-Host "=== AX2012 Performance Optimizer - Error Diagnosis ===" -ForegroundColor Cyan
Write-Host ""

# 1. Check if .NET 8 is installed
Write-Host "1. Checking .NET 8 installation..." -ForegroundColor Yellow
try {
    $dotnetInfo = & dotnet --list-runtimes | Select-String "Microsoft.WindowsDesktop.App 8"
    if ($dotnetInfo) {
        Write-Host "   ✓ .NET 8 Windows Desktop Runtime found" -ForegroundColor Green
        Write-Host "   $dotnetInfo" -ForegroundColor Gray
    } else {
        Write-Host "   ✗ .NET 8 Windows Desktop Runtime NOT found!" -ForegroundColor Red
        Write-Host "   Available runtimes:" -ForegroundColor Gray
        & dotnet --list-runtimes
    }
} catch {
    Write-Host "   ✗ Error checking .NET: $_" -ForegroundColor Red
}

Write-Host ""

# 2. Check if EXE exists
Write-Host "2. Checking if EXE exists..." -ForegroundColor Yellow
$exePath = "AX2012PerformanceOptimizer.WpfApp\bin\Debug\net8.0-windows\AX2012PerformanceOptimizer.WpfApp.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    Write-Host "   ✓ EXE found: $exePath" -ForegroundColor Green
    Write-Host "   Size: $($fileInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "   Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "   ✗ EXE not found at: $exePath" -ForegroundColor Red
}

Write-Host ""

# 3. Check dependencies
Write-Host "3. Checking dependencies..." -ForegroundColor Yellow
$dllPath = "AX2012PerformanceOptimizer.WpfApp\bin\Debug\net8.0-windows"
$requiredDlls = @(
    "AX2012PerformanceOptimizer.Core.dll",
    "AX2012PerformanceOptimizer.Data.dll"
)
foreach ($dll in $requiredDlls) {
    $fullPath = Join-Path $dllPath $dll
    if (Test-Path $fullPath) {
        Write-Host "   ✓ $dll" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $dll MISSING!" -ForegroundColor Red
    }
}

Write-Host ""

# 4. Try to start the app and capture any errors
Write-Host "4. Attempting to start application..." -ForegroundColor Yellow
Write-Host "   (This may take a few seconds...)" -ForegroundColor Gray
try {
    $pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $pinfo.FileName = $exePath
    $pinfo.RedirectStandardError = $true
    $pinfo.RedirectStandardOutput = $true
    $pinfo.UseShellExecute = $false
    $pinfo.CreateNoWindow = $true
    
    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $pinfo
    $p.Start() | Out-Null
    
    # Wait a bit for any startup errors
    Start-Sleep -Milliseconds 2000
    
    if ($p.HasExited) {
        Write-Host "   ✗ Application exited with code: $($p.ExitCode)" -ForegroundColor Red
        $stdout = $p.StandardOutput.ReadToEnd()
        $stderr = $p.StandardError.ReadToEnd()
        
        if ($stdout) {
            Write-Host "   Standard Output:" -ForegroundColor Yellow
            Write-Host "   $stdout" -ForegroundColor Gray
        }
        if ($stderr) {
            Write-Host "   Standard Error:" -ForegroundColor Red
            Write-Host "   $stderr" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ✓ Application started successfully (PID: $($p.Id))" -ForegroundColor Green
        Write-Host "   Stopping process for testing..." -ForegroundColor Gray
        $p.Kill()
    }
} catch {
    Write-Host "   ✗ Exception occurred: $_" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Diagnosis Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
