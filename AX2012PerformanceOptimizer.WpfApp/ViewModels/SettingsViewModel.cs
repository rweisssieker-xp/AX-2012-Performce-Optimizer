using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Data.Models;
using AX2012PerformanceOptimizer.Data.Configuration;
using AX2012PerformanceOptimizer.Data.SqlServer;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.WpfApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigurationService _configService;
    private readonly ISqlConnectionManager _sqlConnectionManager;

    [ObservableProperty]
    private ObservableCollection<ConnectionProfile> profiles = new();

    [ObservableProperty]
    private ConnectionProfile? selectedProfile;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string plainPassword = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> availableDatabases = new();

    // AI Configuration
    [ObservableProperty]
    private bool isAiEnabled;

    [ObservableProperty]
    private string aiProvider = "OpenAI";

    [ObservableProperty]
    private string aiApiKey = string.Empty;

    [ObservableProperty]
    private string aiEndpoint = "https://api.openai.com";

    [ObservableProperty]
    private string aiModel = "gpt-4o";

    [ObservableProperty]
    private string aiStatusMessage = string.Empty;

    public ObservableCollection<string> AiProviders { get; } = new() { "OpenAI", "AzureOpenAI" };
    public ObservableCollection<string> AiModels { get; } = new()
    {
        // ⭐ RECOMMENDED: Cost-Optimized Models
        "gpt-4o-mini",          // 💰 CHEAPEST - 80% cheaper than GPT-4 (0.15/1M tokens)
        "gpt-3.5-turbo",        // 💰 Ultra cheap - Legacy model (0.50/1M tokens)

        // 🌟 Latest High-Performance Models (GPT-4.5 / o1 Series)
        "gpt-4o",               // ⚡ Best balance - Fast & Smart (2.50/1M tokens)
        "o1",                   // 🧠 Full Reasoning Model - Production Ready (NEW!)
        "o1-mini",              // 🧠 Reasoning - Complex analysis (3.00/1M tokens)
        "o1-preview",           // 🧠 Advanced reasoning - Most capable (15.00/1M tokens)

        // 🔥 Power Models
        "gpt-4-turbo",          // Fast GPT-4 (10.00/1M tokens)
        "gpt-4",                // Original GPT-4 (30.00/1M tokens)
        "gpt-4-32k",            // Large context (60.00/1M tokens)

        // 📦 Legacy
        "gpt-3.5-turbo-16k"     // Larger context legacy
    };

    public SettingsViewModel(
        IConfigurationService configService,
        ISqlConnectionManager sqlConnectionManager)
    {
        _configService = configService;
        _sqlConnectionManager = sqlConnectionManager;

        // Load profiles on initialization
        _ = LoadProfilesAsync();
        _ = LoadAiConfigurationAsync();
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        var profiles = await _configService.GetAllProfilesAsync();
        Profiles.Clear();
        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }
    }

    [RelayCommand]
    private void NewProfile()
    {
        SelectedProfile = new ConnectionProfile
        {
            Name = "New Profile",
            UseWindowsAuthentication = true,
            AosPort = 2712,
            AxCompany = "DAT"
        };
        PlainPassword = string.Empty;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (SelectedProfile != null)
        {
            // Encrypt password if provided
            if (!string.IsNullOrEmpty(PlainPassword))
            {
                SelectedProfile.EncryptedPassword = _configService.EncryptPassword(PlainPassword);
                PlainPassword = string.Empty; // Clear after encrypting
            }

            await _configService.SaveProfileAsync(SelectedProfile);
            await LoadProfilesAsync();
            IsEditing = false;
            StatusMessage = "Profile saved successfully";
        }
    }

    [RelayCommand]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile != null)
        {
            await _configService.DeleteProfileAsync(SelectedProfile.Id);
            await LoadProfilesAsync();
            SelectedProfile = null;
            StatusMessage = "Profile deleted successfully";
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (SelectedProfile != null)
        {
            StatusMessage = "Testing connection...";
            var success = await _configService.TestConnectionAsync(SelectedProfile);
            StatusMessage = success ? "Connection successful!" : "Connection failed!";
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (SelectedProfile != null)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = SelectedProfile.SqlServerName,
                    InitialCatalog = SelectedProfile.DatabaseName,
                    IntegratedSecurity = SelectedProfile.UseWindowsAuthentication,
                    TrustServerCertificate = true
                };

                if (!SelectedProfile.UseWindowsAuthentication)
                {
                    builder.UserID = SelectedProfile.Username;
                    builder.Password = _configService.DecryptPassword(SelectedProfile.EncryptedPassword);
                }

                _sqlConnectionManager.SetConnectionString(builder.ConnectionString);
                
                SelectedProfile.LastUsedDate = DateTime.UtcNow;
                await _configService.SaveProfileAsync(SelectedProfile);

                StatusMessage = "Connected successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection error: {ex.Message}";
            }
        }
    }

    // AI Configuration Methods
    [RelayCommand]
    private async Task LoadAiConfigurationAsync()
    {
        try
        {
            var aiConfig = await _configService.GetAiConfigurationAsync();
            if (aiConfig != null)
            {
                IsAiEnabled = aiConfig.IsEnabled;
                AiProvider = aiConfig.Provider.ToString();
                AiEndpoint = aiConfig.Endpoint;
                AiModel = aiConfig.Model;

                // Decrypt API key for display (only if exists)
                if (!string.IsNullOrEmpty(aiConfig.EncryptedApiKey))
                {
                    AiApiKey = _configService.DecryptPassword(aiConfig.EncryptedApiKey);
                }
            }
        }
        catch (Exception ex)
        {
            AiStatusMessage = $"Error loading AI config: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAiConfigurationAsync()
    {
        try
        {
            var aiConfig = new AiConfiguration
            {
                IsEnabled = IsAiEnabled,
                Provider = AiProvider == "AzureOpenAI"
                    ? Data.Models.AiProvider.AzureOpenAI
                    : Data.Models.AiProvider.OpenAI,
                EncryptedApiKey = !string.IsNullOrEmpty(AiApiKey)
                    ? _configService.EncryptPassword(AiApiKey)
                    : string.Empty,
                Endpoint = AiEndpoint,
                Model = AiModel
            };

            await _configService.SaveAiConfigurationAsync(aiConfig);
            AiStatusMessage = "✅ AI Configuration saved successfully! Please restart the app for changes to take effect.";
        }
        catch (Exception ex)
        {
            AiStatusMessage = $"❌ Error saving AI config: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TestAiConfiguration()
    {
        if (string.IsNullOrEmpty(AiApiKey))
        {
            AiStatusMessage = "⚠️ Please enter an API Key";
            return;
        }

        if (string.IsNullOrEmpty(AiEndpoint))
        {
            AiStatusMessage = "⚠️ Please enter an Endpoint";
            return;
        }

        AiStatusMessage = "✅ Configuration looks valid. Save and restart the app to use AI features.";
    }

    [RelayCommand]
    private async Task LoadDatabasesAsync()
    {
        if (SelectedProfile == null)
        {
            StatusMessage = "⚠️ Please select or create a profile first";
            return;
        }

        if (string.IsNullOrEmpty(SelectedProfile.SqlServerName))
        {
            StatusMessage = "⚠️ Please enter SQL Server Name first";
            return;
        }

        try
        {
            StatusMessage = "Loading databases...";

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = SelectedProfile.SqlServerName,
                IntegratedSecurity = SelectedProfile.UseWindowsAuthentication,
                TrustServerCertificate = true,
                ConnectTimeout = 10
            };

            if (!SelectedProfile.UseWindowsAuthentication)
            {
                if (string.IsNullOrEmpty(SelectedProfile.Username))
                {
                    StatusMessage = "⚠️ Please enter Username for SQL Authentication";
                    return;
                }

                builder.UserID = SelectedProfile.Username;

                // Use plain password if available, otherwise decrypt stored password
                if (!string.IsNullOrEmpty(PlainPassword))
                {
                    builder.Password = PlainPassword;
                }
                else if (!string.IsNullOrEmpty(SelectedProfile.EncryptedPassword))
                {
                    builder.Password = _configService.DecryptPassword(SelectedProfile.EncryptedPassword);
                }
                else
                {
                    StatusMessage = "⚠️ Please enter Password for SQL Authentication";
                    return;
                }
            }

            using var conn = new SqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"SELECT name FROM sys.databases
                  WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
                  AND state_desc = 'ONLINE'
                  ORDER BY name",
                conn);

            using var reader = await cmd.ExecuteReaderAsync();

            AvailableDatabases.Clear();
            while (await reader.ReadAsync())
            {
                AvailableDatabases.Add(reader.GetString(0));
            }

            StatusMessage = $"✅ Loaded {AvailableDatabases.Count} databases";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error loading databases: {ex.Message}";
            AvailableDatabases.Clear();
        }
    }
}


