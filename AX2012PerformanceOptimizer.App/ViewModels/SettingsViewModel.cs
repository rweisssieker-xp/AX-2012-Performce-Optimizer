using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AX2012PerformanceOptimizer.Data.Models;
using AX2012PerformanceOptimizer.Data.Configuration;
using AX2012PerformanceOptimizer.Data.SqlServer;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.App.ViewModels;

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

    public SettingsViewModel(
        IConfigurationService configService,
        ISqlConnectionManager sqlConnectionManager)
    {
        _configService = configService;
        _sqlConnectionManager = sqlConnectionManager;
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
            Name = "New Profile"
        };
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (SelectedProfile != null)
        {
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
}

