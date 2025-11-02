namespace WinCheck.Core.Models;

public class ServiceOptimizationPlan
{
    public List<ServiceRecommendation> Recommendations { get; set; } = new();
    public int TotalServices { get; set; }
    public int SuggestedToDisable { get; set; }
    public long EstimatedMemorySavings { get; set; }
    public TimeSpan EstimatedBootTimeReduction { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string BackupId { get; set; } = Guid.NewGuid().ToString();
}

public class ServiceRecommendation
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceStartMode CurrentStartMode { get; set; }
    public ServiceStartMode RecommendedStartMode { get; set; }
    public ServiceCategory Category { get; set; }
    public ServiceSafetyLevel SafetyLevel { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public List<string> DependentServices { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    public long EstimatedMemorySavingBytes { get; set; }
    public bool IsSelected { get; set; }
}

public enum ServiceStartMode
{
    Automatic = 2,
    Manual = 3,
    Disabled = 4,
    AutomaticDelayed = 5
}

public enum ServiceCategory
{
    SystemCritical,      // Windows core services
    Hardware,            // Donanım sürücüleri
    Network,             // Ağ servisleri
    Security,            // Güvenlik
    Performance,         // Performans/telemetry
    Multimedia,          // Ses, video
    Printing,            // Yazıcı servisleri
    RemoteAccess,        // Uzak masaüstü
    Microsoft,           // Microsoft uygulamaları
    ThirdParty,          // 3. parti yazılımlar
    Unnecessary          // Gereksiz
}

public enum ServiceSafetyLevel
{
    Critical,   // Asla dokunma
    Safe,       // Güvenle değiştirilebilir
    Conditional,// Kullanıma bağlı
    Recommended // Devre dışı bırakılması önerilir
}
