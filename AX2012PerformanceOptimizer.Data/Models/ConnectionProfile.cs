namespace AX2012PerformanceOptimizer.Data.Models;

public class ConnectionProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string SqlServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
    public bool UseWindowsAuthentication { get; set; } = true;
    public string AosServerName { get; set; } = string.Empty;
    public int AosPort { get; set; } = 2712;
    public string AxCompany { get; set; } = "DAT";
    public bool IsDefault { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedDate { get; set; }
}

