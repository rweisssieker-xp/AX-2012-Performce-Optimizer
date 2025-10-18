using AX2012PerformanceOptimizer.Data.Models;

namespace AX2012PerformanceOptimizer.Data.Configuration;

public interface IConfigurationService
{
    Task<List<ConnectionProfile>> GetAllProfilesAsync();
    Task<ConnectionProfile?> GetProfileByIdAsync(string id);
    Task<ConnectionProfile?> GetDefaultProfileAsync();
    Task SaveProfileAsync(ConnectionProfile profile);
    Task DeleteProfileAsync(string id);
    Task<bool> TestConnectionAsync(ConnectionProfile profile);
    string EncryptPassword(string password);
    string DecryptPassword(string encryptedPassword);

    Task ExportProfilesAsync(string filePath);
    Task ImportProfilesAsync(string filePath, bool merge = true);

    // AI Configuration
    Task<AiConfiguration?> GetAiConfigurationAsync();
    Task SaveAiConfigurationAsync(AiConfiguration configuration);
}

