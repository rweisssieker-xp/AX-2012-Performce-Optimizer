# Deployment Guide - AX 2012 Performance Optimizer

## Building for Production

### Option 1: Visual Studio 2022 (Recommended)

1. **Open Solution**
   ```
   Open AX2012PerformanceOptimizer.sln in Visual Studio 2022
   ```

2. **Set Build Configuration**
   - Configuration: Release
   - Platform: x64

3. **Build Solution**
   - Build → Build Solution (Ctrl+Shift+B)

4. **Publish Application**
   - Right-click on AX2012PerformanceOptimizer.App project
   - Select "Publish"
   - Choose "Folder" as publish target
   - Configure publish settings:
     - Target Runtime: win-x64
     - Deployment Mode: Self-contained
     - Produce single file: Yes
     - Enable ReadyToRun compilation: Yes

### Option 2: Command Line (dotnet CLI)

```powershell
# Navigate to solution directory
cd C:\path\to\AX-2012-Performce-Optimizer

# Clean previous builds
dotnet clean AX2012PerformanceOptimizer.sln --configuration Release

# Restore packages
dotnet restore AX2012PerformanceOptimizer.sln

# Build for Release
dotnet build AX2012PerformanceOptimizer.sln --configuration Release --no-restore

# Publish as single-file executable
dotnet publish AX2012PerformanceOptimizer.App/AX2012PerformanceOptimizer.App.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output ./publish/win-x64 `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:EnableCompressionInSingleFile=true `
  /p:PublishTrimmed=false
```

## Deployment Package Structure

The published application should contain:

```
AX2012PerformanceOptimizer/
├── AX2012PerformanceOptimizer.exe    # Main executable (100-150 MB)
└── README.md                          # User documentation
```

## Distribution

### For Internal Use

1. **Copy to Network Share**
   ```
   \\server\share\Tools\AX2012PerformanceOptimizer\
   ```

2. **Create Shortcut** (Optional)
   - Create desktop shortcut for easy access
   - Set icon if custom icon is available

### For External Distribution

1. **Create ZIP Archive**
   ```powershell
   Compress-Archive -Path ./publish/win-x64/* -DestinationPath AX2012-Performance-Optimizer-v1.0.0.zip
   ```

2. **Generate Checksums**
   ```powershell
   Get-FileHash -Path AX2012-Performance-Optimizer-v1.0.0.zip -Algorithm SHA256 | `
     Format-List | Out-File checksums.txt
   ```

## Installation on Target Machines

### Requirements
- Windows 10 version 1809 (Build 17763) or later
- Windows 11 (any version)
- x64 processor architecture
- Approximately 200 MB disk space
- Network access to SQL Server

### Installation Steps

1. **No Installation Required**
   - Simply copy the executable to desired location
   - First run may take 10-15 seconds to initialize

2. **Recommended Locations**
   ```
   C:\Tools\AX2012PerformanceOptimizer\
   %USERPROFILE%\Desktop\AX2012PerformanceOptimizer\
   ```

3. **Configuration**
   - Settings stored in: `%LocalAppData%\AX2012PerformanceOptimizer\`
   - Portable: Can be run from USB drive

## SQL Server Permissions

### Minimum Required Permissions

```sql
-- Create login (if using SQL authentication)
CREATE LOGIN [AXMonitor] WITH PASSWORD = 'SecurePassword123!';

-- Create user in AX database
USE [MicrosoftDynamicsAX];
CREATE USER [AXMonitor] FOR LOGIN [AXMonitor];

-- Grant minimal permissions
ALTER ROLE db_datareader ADD MEMBER [AXMonitor];

-- Grant VIEW SERVER STATE for DMVs
USE master;
GRANT VIEW SERVER STATE TO [AXMonitor];
GRANT VIEW DATABASE STATE TO [AXMonitor];
```

### Read-Only Verification

```sql
-- Verify user has only read permissions
SELECT 
    dp.name AS UserName,
    dp.type_desc AS UserType,
    r.name AS RoleName
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members drm ON dp.principal_id = drm.member_principal_id
LEFT JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
WHERE dp.name = 'AXMonitor';
```

## Firewall Configuration

### SQL Server Port (Default: 1433)
```powershell
# Allow outbound to SQL Server
New-NetFirewallRule -DisplayName "AX Monitor - SQL Server" `
  -Direction Outbound `
  -Action Allow `
  -Protocol TCP `
  -RemotePort 1433
```

### AOS Port (Default: 2712)
```powershell
# Allow outbound to AOS
New-NetFirewallRule -DisplayName "AX Monitor - AOS" `
  -Direction Outbound `
  -Action Allow `
  -Protocol TCP `
  -RemotePort 2712
```

## Group Policy Deployment (Enterprise)

### Using SCCM/Intune

1. **Create Application Package**
   - Name: AX 2012 Performance Optimizer
   - Version: 1.0.0
   - Deployment Type: Script-based
   - Content Location: UNC path to executable

2. **Installation Script** (install.ps1)
   ```powershell
   $TargetPath = "C:\Program Files\AX2012PerformanceOptimizer"
   New-Item -Path $TargetPath -ItemType Directory -Force
   Copy-Item -Path ".\AX2012PerformanceOptimizer.exe" -Destination $TargetPath
   ```

3. **Detection Method**
   ```powershell
   Test-Path "C:\Program Files\AX2012PerformanceOptimizer\AX2012PerformanceOptimizer.exe"
   ```

## Update Procedure

### Manual Update
1. Close running application
2. Replace executable with new version
3. Configuration and profiles are preserved

### Automated Update
```powershell
# Stop application if running
Get-Process AX2012PerformanceOptimizer -ErrorAction SilentlyContinue | Stop-Process

# Backup current version
Copy-Item AX2012PerformanceOptimizer.exe AX2012PerformanceOptimizer.exe.backup

# Copy new version
Copy-Item \\server\share\updates\AX2012PerformanceOptimizer.exe .
```

## Troubleshooting Deployment Issues

### Issue: Application won't start

**Solution 1: Check .NET Runtime**
```powershell
# Verify .NET 8 is available (for non-self-contained builds)
dotnet --list-runtimes | Select-String "Microsoft.WindowsDesktop.App 8.0"
```

**Solution 2: Check Windows Version**
```powershell
[System.Environment]::OSVersion.Version
# Should be 10.0.17763 or higher
```

### Issue: Connection failures

**Solution: Test SQL connectivity**
```powershell
Test-NetConnection -ComputerName SQLSERVER -Port 1433
```

### Issue: Performance issues

**Solution: Check system resources**
```powershell
# Check available memory
Get-Counter '\Memory\Available MBytes'

# Check CPU usage
Get-Counter '\Processor(_Total)\% Processor Time'
```

## Security Hardening

### Code Signing (Recommended)
```powershell
# Sign executable with code signing certificate
Set-AuthenticodeSignature -FilePath AX2012PerformanceOptimizer.exe `
  -Certificate $cert `
  -TimestampServer http://timestamp.digicert.com
```

### Application Whitelisting
```powershell
# Add to AppLocker policy
$rule = New-AppLockerFilePathRule -Action Allow `
  -Path "C:\Program Files\AX2012PerformanceOptimizer\*" `
  -RuleType Publisher `
  -User Everyone
```

## Monitoring Deployment Success

### Log File Locations
- Application logs: `%LocalAppData%\AX2012PerformanceOptimizer\logs\`
- Configuration: `%LocalAppData%\AX2012PerformanceOptimizer\profiles.json`

### Health Check Script
```powershell
# Check if application is running
$Process = Get-Process AX2012PerformanceOptimizer -ErrorAction SilentlyContinue

if ($Process) {
    Write-Host "Application is running. PID: $($Process.Id)"
    Write-Host "Memory Usage: $([math]::Round($Process.WorkingSet64/1MB, 2)) MB"
} else {
    Write-Host "Application is not running"
}

# Check configuration exists
$ConfigPath = "$env:LOCALAPPDATA\AX2012PerformanceOptimizer\profiles.json"
if (Test-Path $ConfigPath) {
    Write-Host "Configuration file exists"
    $Config = Get-Content $ConfigPath | ConvertFrom-Json
    Write-Host "Number of profiles: $($Config.Count)"
} else {
    Write-Host "No configuration file found"
}
```

## Rollback Procedure

```powershell
# Stop application
Get-Process AX2012PerformanceOptimizer -ErrorAction SilentlyContinue | Stop-Process

# Restore previous version
Move-Item AX2012PerformanceOptimizer.exe.backup AX2012PerformanceOptimizer.exe -Force

# Restart application
Start-Process AX2012PerformanceOptimizer.exe
```

## Support and Contacts

For deployment issues:
- IT Support: itsupport@company.com
- AX Team: axsupport@company.com
- Tool Issues: GitHub Issues

---

**Document Version**: 1.0  
**Last Updated**: October 2025

