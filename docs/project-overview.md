# Project Overview - AX 2012 Performance Optimizer

## Project Information

- **Project Name**: AX 2012 Performance Optimizer
- **Project Type**: Desktop Application
- **Architecture**: 3-Layer Clean Architecture
- **Framework**: .NET 8 WPF
- **Status**: Production Ready (v2.1)
- **Build Status**: ✅ Stable (0 Errors, 21 Warnings)

## Purpose

A native Windows application for monitoring and optimizing Microsoft Dynamics AX 2012 R3 CU13 performance with SQL Server 2016. Provides real-time performance metrics, graphical visualizations, and intelligent optimization recommendations.

## Key Statistics

- **Project Size**: ~194 C# files, ~26 XAML files
- **Lines of Code**: ~10,100 total (~5,400 C#, ~2,800 XAML, ~1,900 docs)
- **Architecture**: Clean 3-Layer Architecture (App → Core → Data)
- **Build Time**: ~3 seconds
- **Memory Footprint**: 50-80 MB idle, 100-150 MB active
- **Startup Time**: <2 seconds

## Unique Selling Points (USPs)

1. **AX-Native Insights**: Uses AX-specific tables (SYSCLIENTSESSIONS, BATCHJOB, AIFGATEWAYQUEUE)
2. **Zero-Install Portable**: Single EXE, no installation required
3. **Dual Connection**: SQL Server DMVs + AX Business Connector
4. **AI-Powered**: OpenAI/Azure OpenAI integration for intelligent analysis
5. **Cost Analysis**: Translates performance metrics into business costs
6. **Predictive Analytics**: Forecasts performance trends and issues
7. **Self-Healing**: Autonomous query optimization with rollback
8. **Secure by Default**: Read-only operations, DPAPI encryption

## Core Features

### Monitoring Modules (7)
1. **Dashboard**: System health overview
2. **SQL Performance**: Query performance analysis
3. **AOS Monitoring**: AOS server health
4. **Batch Jobs**: Batch job monitoring
5. **Database Health**: Database metrics and index analysis
6. **Recommendations**: Optimization suggestions
7. **Settings**: Application settings

### AI Features (8)
1. Natural Language Query Assistant
2. AI Performance Insights Dashboard
3. Intelligent Query Rewriter
4. Query Auto-Fixer
5. Query Documentation Generator
6. AI Query Explainer
7. Performance Cost Calculator
8. Query Performance Forecasting

### Innovative Features (5 USP)
1. **Performance DNA**: Genetic algorithm-based optimization
2. **Performance Crystal Ball**: Business scenario predictions
3. **Performance Personas**: AI expert personas
4. **Performance Time Machine**: Historical analysis
5. **Performance Community**: Global benchmarking

## Technology Summary

- **Language**: C# 12
- **UI Framework**: WPF (.NET 8)
- **MVVM**: CommunityToolkit.Mvvm 8.3.2
- **Charts**: LiveChartsCore 2.0
- **DI**: Microsoft.Extensions.DependencyInjection 9.0.10
- **SQL**: Microsoft.Data.SqlClient 5.2.2
- **Security**: System.Security.Cryptography.ProtectedData 8.0.0

## Repository Structure

- **Monolith**: Single cohesive codebase
- **3 Projects**: WpfApp, Core, Data
- **Solution**: AX2012PerformanceOptimizer.sln

## Quick Reference

- **Entry Point**: `App.xaml.cs`
- **Main Window**: `MainWindow.xaml`
- **Configuration**: `%LocalAppData%\AX2012PerformanceOptimizer\`
- **Documentation**: See [index.md](./index.md)

## Getting Started

1. **Build**: `dotnet build AX2012PerformanceOptimizer.sln`
2. **Run**: `dotnet run --project AX2012PerformanceOptimizer.WpfApp`
3. **Publish**: `.\publish-release.ps1`

## Documentation Links

- [Architecture](./architecture.md) - Complete architecture documentation
- [Technology Stack](./technology-stack.md) - Technology details
- [Development Guide](./development-guide.md) - Development setup
- [Deployment Guide](./deployment-guide.md) - Deployment instructions
- [Source Tree](./source-tree-analysis.md) - Directory structure
- [UI Components](./ui-component-inventory.md) - UI component catalog
- [State Management](./state-management-patterns.md) - State management patterns

