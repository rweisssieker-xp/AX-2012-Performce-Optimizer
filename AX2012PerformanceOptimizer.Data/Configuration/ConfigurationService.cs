using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AX2012PerformanceOptimizer.Data.Models;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AX2012PerformanceOptimizer.Data.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private readonly string _aiConfigFilePath;
    private List<ConnectionProfile> _profiles = new();
    private AiConfiguration? _aiConfiguration;

    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "AX2012PerformanceOptimizer");
        Directory.CreateDirectory(appFolder);
        _configFilePath = Path.Combine(appFolder, "profiles.json");
        _aiConfigFilePath = Path.Combine(appFolder, "ai-config.json");
        LoadProfiles();
        LoadAiConfiguration();
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

    public async Task ExportProfilesAsync(string filePath)
    {
        var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task ImportProfilesAsync(string filePath, bool merge = true)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var imported = JsonSerializer.Deserialize<List<ConnectionProfile>>(json) ?? new();

        if (merge)
        {
            var dict = _profiles.ToDictionary(p => p.Id);
            foreach (var p in imported)
            {
                dict[p.Id] = p;
            }
            _profiles = dict.Values.ToList();
        }
        else
        {
            _profiles = imported;
        }

        await SaveProfilesAsync();
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

    // AI Configuration Methods
    private void LoadAiConfiguration()
    {
        if (File.Exists(_aiConfigFilePath))
        {
            var json = File.ReadAllText(_aiConfigFilePath);
            _aiConfiguration = JsonSerializer.Deserialize<AiConfiguration>(json);
        }
    }

    private async Task SaveAiConfigurationAsync()
    {
        if (_aiConfiguration != null)
        {
            var json = JsonSerializer.Serialize(_aiConfiguration, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_aiConfigFilePath, json);
        }
    }

    public Task<AiConfiguration?> GetAiConfigurationAsync()
    {
        return Task.FromResult(_aiConfiguration);
    }

    public async Task SaveAiConfigurationAsync(AiConfiguration configuration)
    {
        configuration.LastUpdated = DateTime.UtcNow;
        _aiConfiguration = configuration;
        await SaveAiConfigurationAsync();
    }
}

