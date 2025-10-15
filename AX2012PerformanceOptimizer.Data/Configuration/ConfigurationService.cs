using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AX2012PerformanceOptimizer.Data.Models;
using Microsoft.Data.SqlClient;

namespace AX2012PerformanceOptimizer.Data.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private List<ConnectionProfile> _profiles = new();

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "AX2012PerformanceOptimizer");
        Directory.CreateDirectory(appFolder);
        _configFilePath = Path.Combine(appFolder, "profiles.json");
        LoadProfiles();
    }

    private void LoadProfiles()
    {
        if (File.Exists(_configFilePath))
        {
            var json = File.ReadAllText(_configFilePath);
            _profiles = JsonSerializer.Deserialize<List<ConnectionProfile>>(json) ?? new();
        }
    }

    private async Task SaveProfilesAsync()
    {
        var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(_configFilePath, json);
    }

    public Task<List<ConnectionProfile>> GetAllProfilesAsync()
    {
        return Task.FromResult(_profiles);
    }

    public Task<ConnectionProfile?> GetProfileByIdAsync(string id)
    {
        return Task.FromResult(_profiles.FirstOrDefault(p => p.Id == id));
    }

    public Task<ConnectionProfile?> GetDefaultProfileAsync()
    {
        return Task.FromResult(_profiles.FirstOrDefault(p => p.IsDefault));
    }

    public async Task SaveProfileAsync(ConnectionProfile profile)
    {
        var existing = _profiles.FirstOrDefault(p => p.Id == profile.Id);
        if (existing != null)
        {
            _profiles.Remove(existing);
        }

        if (profile.IsDefault)
        {
            // Remove default flag from other profiles
            foreach (var p in _profiles)
            {
                p.IsDefault = false;
            }
        }

        _profiles.Add(profile);
        await SaveProfilesAsync();
    }

    public async Task DeleteProfileAsync(string id)
    {
        var profile = _profiles.FirstOrDefault(p => p.Id == id);
        if (profile != null)
        {
            _profiles.Remove(profile);
            await SaveProfilesAsync();
        }
    }

    public async Task<bool> TestConnectionAsync(ConnectionProfile profile)
    {
        try
        {
            var connectionString = BuildConnectionString(profile);
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string EncryptPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        var data = Encoding.UTF8.GetBytes(password);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public string DecryptPassword(string encryptedPassword)
    {
        if (string.IsNullOrEmpty(encryptedPassword))
            return string.Empty;

        try
        {
            var data = Convert.FromBase64String(encryptedPassword);
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string BuildConnectionString(ConnectionProfile profile)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = profile.SqlServerName,
            InitialCatalog = profile.DatabaseName,
            IntegratedSecurity = profile.UseWindowsAuthentication,
            TrustServerCertificate = true,
            ConnectTimeout = 30
        };

        if (!profile.UseWindowsAuthentication)
        {
            builder.UserID = profile.Username;
            builder.Password = DecryptPassword(profile.EncryptedPassword);
        }

        return builder.ConnectionString;
    }
}

