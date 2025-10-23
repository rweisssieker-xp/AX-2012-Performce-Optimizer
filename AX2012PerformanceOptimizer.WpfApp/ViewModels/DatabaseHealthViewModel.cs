using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Core.Services;
using AX2012PerformanceOptimizer.Core.Models;
using AX2012PerformanceOptimizer.Data.SqlServer;
using System.Collections.ObjectModel;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class DatabaseHealthViewModel : ObservableObject
{
    private readonly IDatabaseStatsService _databaseStats;
    private readonly ISqlConnectionManager _connectionManager;

    [ObservableProperty]
    private DatabaseMetric? currentMetrics;

    [ObservableProperty]
    private ObservableCollection<TableMetric> topTables = new();

    [ObservableProperty]
    private ObservableCollection<IndexFragmentation> fragmentedIndexes = new();

    [ObservableProperty]
    private ObservableCollection<MissingIndex> missingIndexes = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "No database connection - Please connect in Settings";

    [ObservableProperty]
    private bool isConnected;

    public DatabaseHealthViewModel(
        IDatabaseStatsService databaseStats,
        ISqlConnectionManager connectionManager)
    {
        _databaseStats = databaseStats;
        _connectionManager = connectionManager;
        _connectionManager.ConnectionChanged += OnConnectionChanged;

        // Check initial connection status
        IsConnected = _connectionManager.IsConnected;
        if (!IsConnected)
        {
            StatusMessage = "No database connection - Please connect in Settings";
        }
    }

    private async void OnConnectionChanged(object? sender, ConnectionChangedEventArgs e)
    {
        IsConnected = e.IsConnected;

        if (e.IsConnected)
        {
            StatusMessage = $"Connected to {e.DatabaseName} on {e.ServerName}";
            // Auto-load data when connection is established
            await LoadDataAsync();
        }
        else
        {
            StatusMessage = "Database connection lost - Please reconnect in Settings";
            // Clear data when connection is lost
            CurrentMetrics = null;
            TopTables.Clear();
            FragmentedIndexes.Clear();
            MissingIndexes.Clear();
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading database health metrics...";

        try
        {
            CurrentMetrics = await _databaseStats.GetDatabaseMetricsAsync();
            StatusMessage = "Loading top tables...";

            var tables = await _databaseStats.GetTopTablesBySize(20);
            TopTables.Clear();
            foreach (var table in tables)
            {
                TopTables.Add(table);
            }
            StatusMessage = $"Loaded {tables.Count} tables. Loading fragmented indexes...";

            var fragmented = await _databaseStats.GetFragmentedIndexesAsync(10);
            FragmentedIndexes.Clear();
            foreach (var index in fragmented)
            {
                FragmentedIndexes.Add(index);
            }
            StatusMessage = $"Loaded {fragmented.Count} fragmented indexes. Loading missing indexes...";

            var missing = await _databaseStats.GetMissingIndexesAsync();
            MissingIndexes.Clear();
            foreach (var index in missing)
            {
                MissingIndexes.Add(index);
            }
            StatusMessage = $"✅ Loaded: {tables.Count} tables, {fragmented.Count} fragmented indexes, {missing.Count} missing indexes";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error loading data: {ex.Message}";
            System.Windows.MessageBox.Show(
                $"Error loading database health data:\n\n{ex.Message}\n\nCheck the logs for more details.",
                "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task RebuildIndex(IndexFragmentation? index)
    {
        if (index == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Rebuild index '{index.IndexName}' on table '{index.TableName}'?\n\n" +
            $"Fragmentation: {index.FragmentationPercent:F2}%\n" +
            $"Pages: {index.PageCount:N0}\n\n" +
            $"This operation may take several minutes and will lock the table.",
            "Rebuild Index",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            var success = await _databaseStats.RebuildIndexAsync(index.SchemaName, index.TableName, index.IndexName);

            if (success)
            {
                System.Windows.MessageBox.Show(
                    $"Successfully rebuilt index '{index.IndexName}'!",
                    "Success",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                await RefreshAsync();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"Failed to rebuild index '{index.IndexName}'. Check logs for details.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReorganizeIndex(IndexFragmentation? index)
    {
        if (index == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Reorganize index '{index.IndexName}' on table '{index.TableName}'?\n\n" +
            $"Fragmentation: {index.FragmentationPercent:F2}%\n" +
            $"Pages: {index.PageCount:N0}\n\n" +
            $"This is a faster, online operation with less locking.",
            "Reorganize Index",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            var success = await _databaseStats.ReorganizeIndexAsync(index.SchemaName, index.TableName, index.IndexName);

            if (success)
            {
                System.Windows.MessageBox.Show(
                    $"Successfully reorganized index '{index.IndexName}'!",
                    "Success",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                await RefreshAsync();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"Failed to reorganize index '{index.IndexName}'. Check logs for details.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CopyIndexScript(MissingIndex? missingIndex)
    {
        if (missingIndex == null) return;

        var script = _databaseStats.GenerateCreateIndexScript(missingIndex);
        System.Windows.Clipboard.SetText(script);

        System.Windows.MessageBox.Show(
            "CREATE INDEX script copied to clipboard!",
            "Script Copied",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task CreateIndex(MissingIndex? missingIndex)
    {
        if (missingIndex == null) return;

        var script = _databaseStats.GenerateCreateIndexScript(missingIndex);

        var result = System.Windows.MessageBox.Show(
            $"Create index on table '{missingIndex.TableName}'?\n\n" +
            $"Impact Score: {missingIndex.ImpactScore:N0}\n" +
            $"Seeks: {missingIndex.UserSeeks:N0}, Scans: {missingIndex.UserScans:N0}\n\n" +
            $"The following script will be executed:\n\n{script}\n\n" +
            $"This may take several minutes.",
            "Create Index",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            var success = await _databaseStats.CreateIndexAsync(script);

            if (success)
            {
                System.Windows.MessageBox.Show(
                    $"Successfully created index!",
                    "Success",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                await RefreshAsync();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"Failed to create index. Check logs for details.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}


