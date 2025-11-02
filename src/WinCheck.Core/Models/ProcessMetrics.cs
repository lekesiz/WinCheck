namespace WinCheck.Core.Models;

public class ProcessMetrics
{
    public int ProcessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public double CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public long DiskReadBytes { get; set; }
    public long DiskWriteBytes { get; set; }
    public long NetworkSentBytes { get; set; }
    public long NetworkReceivedBytes { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public ProcessPriority Priority { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan TotalProcessorTime { get; set; }
    public bool IsSystemProcess { get; set; }
    public bool IsSigned { get; set; }
    public string? Publisher { get; set; }

    /// <summary>
    /// İşlemin sistem üzerindeki etki skoru (0-100)
    /// </summary>
    public double ImpactScore => CalculateImpactScore();

    private double CalculateImpactScore()
    {
        const long ONE_GB = 1024L * 1024L * 1024L;
        const long totalSystemMemory = 16L * ONE_GB; // Varsayılan 16GB

        // CPU (40%) + RAM (30%) + Disk I/O (20%) + Network (10%)
        var cpuScore = Math.Min(CpuUsage, 100) * 0.4;
        var memScore = (MemoryUsage / (double)totalSystemMemory) * 100 * 0.3;
        var diskScore = ((DiskReadBytes + DiskWriteBytes) / 1024.0 / 1024.0) * 0.2;
        var netScore = ((NetworkSentBytes + NetworkReceivedBytes) / 1024.0 / 1024.0) * 0.1;

        return Math.Min(cpuScore + memScore + diskScore + netScore, 100);
    }
}

public enum ProcessPriority
{
    Idle = 0,
    BelowNormal = 1,
    Normal = 2,
    AboveNormal = 3,
    High = 4,
    Realtime = 5
}

public class SuspiciousProcess
{
    public ProcessMetrics Metrics { get; set; } = new();
    public SuspicionLevel Level { get; set; }
    public List<SuspicionReason> Reasons { get; set; } = new();
    public RecommendedAction Action { get; set; }
    public string Description { get; set; } = string.Empty;
}

public enum SuspicionLevel
{
    Low,      // Performans düşüşü ama güvenli
    Medium,   // Şüpheli davranış
    High,     // Muhtemelen zararlı
    Critical  // Kesinlikle zararlı
}

public enum SuspicionReason
{
    HighCpuUsage,           // %80+ CPU kullanımı
    HighMemoryUsage,        // Sistem RAM'inin %50+
    ExcessiveDiskIO,        // Anormal disk aktivitesi
    UnknownPublisher,       // İmzasız executable
    HiddenProcess,          // Gizlenmeye çalışan işlem
    NetworkSpamming,        // Anormal network trafiği
    SuspiciousLocation,     // Temp, AppData gibi konumlar
    MultipleInstances,      // Aynı işlemden çok fazla
    SystemResourceAbuse,    // Handle leak, thread bombing
    KnownMalware           // Virustotal/signature match
}

public enum RecommendedAction
{
    Monitor,        // İzlemeye devam
    LowerPriority,  // Önceliği düşür
    Throttle,       // CPU/IO sınırla
    Terminate,      // Sonlandır
    Quarantine,     // Karantinaya al ve scan et
    Block           // Network bağlantısını engelle
}

public class SystemResourceUsage
{
    public double CpuUsagePercentage { get; set; }
    public long TotalMemoryBytes { get; set; }
    public long UsedMemoryBytes { get; set; }
    public long AvailableMemoryBytes { get; set; }
    public double MemoryUsagePercentage { get; set; }
    public List<DiskUsage> DiskUsages { get; set; } = new();
    public long NetworkReceiveRate { get; set; } // bytes/sec
    public long NetworkSendRate { get; set; } // bytes/sec
    public int ActiveProcessCount { get; set; }
    public int TotalThreadCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class DiskUsage
{
    public string DriveName { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public long UsedSize { get; set; }
    public long FreeSize { get; set; }
    public double UsagePercentage { get; set; }
    public long ReadRate { get; set; } // bytes/sec
    public long WriteRate { get; set; } // bytes/sec
    public bool IsSsd { get; set; }
}
