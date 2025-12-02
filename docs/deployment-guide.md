# Deployment Guide - AX 2012 Performance Optimizer

## Deployment Overview

The application is deployed as a **single-file, self-contained executable** for Windows x64.

## Build Configuration

### Release Build Settings

- **Configuration**: Release
- **Runtime**: win-x64
- **Self-Contained**: true
- **Single File**: true
- **Publish ReadyToRun**: true
- **Publish Trimmed**: true (partial trim mode)

## Publishing

### Using PowerShell Script

```powershell
.\publish-release.ps1 -OutputPath "./publish" -Runtime "win-x64"
```

### Manual Publishing

```powershell
dotnet publish AX2012PerformanceOptimizer.WpfApp/AX2012PerformanceOptimizer.WpfApp.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output ./publish `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:EnableCompressionInSingleFile=true `
  /p:PublishReadyToRun=true `
  /p:ReadyToRunUseCrossgen2=true `
  /p:PublishTrimmed=true `
  /p:TrimMode=partial `
  /p:DebugType=None `
  /p:DebugSymbols=false
Output: `./publish/AX2012PerformanceOptimizer.WpfApp.exe`
```

## Deployment Package Contents

### Single Executable
- **AX2012PerformanceOptimizer.WpfApp.exe**: Single-file executable (~50-80MB self-contained)

### Configuration Files
- **profiles.json and ai-config.json are created at runtime in `%LocalAppData%LocalAppData%\AX2012PerformanceOptimizer\`.

## System Requirements

### Minimum Requirements
- **OS**: Windows 10 (Build 17763) or later / Windows 11
- **Architecture**: x64
- **RAM**: 4 GB minimum, 8 GB recommended
- **Disk Space**: 1 GB for application

### Network Requirements
- **SQL Server**: TCP 1433 (default) or configured port
- **AX Business Connector**: TCP 2712 (default AOS port)
- **AI Services** (optional): HTTPS 443 (OpenAI/Azure OpenAI)

### Permissions Required
- **SQL Server**: `db_datareader` role + `VIEW SERVER STATE` permission
- **File System**: Read/write to `%LocalAppData%\AX2012PerformanceOptimizer\`
- **Windows**: Standard user permissions (no admin required)

## Installation

### Zero-Install Deployment

1. **Copy executable** to target machine
2. **No installation required** - just run the .exe
3. **First run** creates configuration directory automatically

### Configuration Directory

Location: `%LocalAppData%\AX2012PerformanceOptimizer\AX2012PerformanceOptimizer\`

Contents:
- `profiles.json`: Connection profiles (connection profiles)
- `ai-config.json`rypted AI configuration

rypted)

## Distribution Methods

### 1. Direct Distribution
- Copy executable to target
- Share via file share or email
- Users download link

### 2. Enterprise Distribution
- Deploy via SCCM/
- Group Policy deployment
- Intune deployment

### 3. Portable Deployment
- Copy executable to USB drive deployment
- Run from any location
- No registry changes
- No system modifications

## Deployment Checklist

- [ ] Build Release configuration
- [ ] Test on clean Windows machine
- ] Verify SQL Server connectivity
- ] Verify AX Business Connector connectivity (optional)
- [ ] Test AI features
- [ ] Create deployment package
- [ ] Document deployment steps
- [ ] Provide user documentation

## Post-Deployment

### First-Time Setup
1. Launch application
2. Navigate to Settings tab
3. Create connection profile
4. Configure SQL Server connection
5. Test connection
6. Connect and start monitoring

### Updates
1. Stop application
2. Replace executable with new version
3. Configuration files are preserved
4. Restart application

## Troubleshooting Deployment

### Common Issues

**Issue**: Application won't start
- **Solution**: Check Windows version (Windows 10/11 required)
- **Solution**: Verify .NET 8 runtime (included in self-contained build)

**Issue**: SQL connection fails
- **Solution**: Verify firewall rules (TCP 1433)
- **Solution**: Check SQL Server permissions
- **Solution**: Verify connection string

**Issue**: Configuration not saved
- **Solution**: Check write permissions to `%LocalAppData%`
- **Solution**: Verify disk space available

## Security Considerations

1. **Read-Only Operations**: All database operations are read-only
2. **Encrypted Credentials**: Passwords encrypted using Windows DPAPI
3. **Local Storage**: Configuration stored locally, not on network
4. **No Admin Required**: Runs with standard user permissions
5. **Network Security**: Supports SSL/TLS for SQL connections

## Performance Considerations

- **Startup Time**: <2 seconds
- **Memory Usage**: 50-80 MB idle, 100-150 MB active
- **Disk I/O**: Minimal (configuration files only)
- **Network**: Only when connected to database

