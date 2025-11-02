# WinCheck - Teknik Mimari ve Tasarım Dokümanı

## İçindekiler
1. [Sistem Mimarisi](#sistem-mimarisi)
2. [Performans İzleme ve Optimizasyon Motoru](#performans-izleme-ve-optimizasyon-motoru)
3. [Servis ve İşlem Yönetimi](#servis-ve-işlem-yönetimi)
4. [Donanım Optimizasyonu](#donanım-optimizasyonu)
5. [Akıllı Öneri Sistemi](#akıllı-öneri-sistemi)
6. [Veri Akışı ve State Management](#veri-akışı-ve-state-management)
7. [API Referansları](#api-referansları)

---

## 1. Sistem Mimarisi

### 1.1 Genel Mimari

```
┌────────────────────────────────────────────────────────────────┐
│                      WinUI 3 Presentation                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐      │
│  │Dashboard │  │ Cleanup  │  │Optimizer │  │Settings  │      │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘      │
└───────┼─────────────┼─────────────┼─────────────┼─────────────┘
        │             │             │             │
        └─────────────┴─────────────┴─────────────┘
                        │
┌───────────────────────▼────────────────────────────────────────┐
│                  ViewModel Layer (MVVM)                        │
│  ┌──────────────────────────────────────────────────────┐     │
│  │  ObservableObject, RelayCommand, Validation          │     │
│  └──────────────────────────────────────────────────────┘     │
└────────────────────────────┬───────────────────────────────────┘
                             │
┌────────────────────────────▼───────────────────────────────────┐
│                    Service Layer                               │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐          │
│  │   Process    │ │  Hardware    │ │  Network     │          │
│  │   Monitor    │ │  Optimizer   │ │  Optimizer   │          │
│  └──────────────┘ └──────────────┘ └──────────────┘          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐          │
│  │   Service    │ │   Startup    │ │   Disk       │          │
│  │   Manager    │ │   Manager    │ │   Cleanup    │          │
│  └──────────────┘ └──────────────┘ └──────────────┘          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐          │
│  │   Registry   │ │ AI Analyzer  │ │   Backup     │          │
│  │   Service    │ │   Engine     │ │   Service    │          │
│  └──────────────┘ └──────────────┘ └──────────────┘          │
└────────────────────────────┬───────────────────────────────────┘
                             │
┌────────────────────────────▼───────────────────────────────────┐
│              Infrastructure Layer                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐      │
│  │   WMI    │  │ P/Invoke │  │   ETW    │  │ Perfmon  │      │
│  │ Provider │  │  Native  │  │ Tracing  │  │ Counters │      │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘      │
└────────────────────────────┬───────────────────────────────────┘
                             │
┌────────────────────────────▼───────────────────────────────────┐
│                Windows Kernel & APIs                           │
│  WMI │ Registry │ ETW │ Task Scheduler │ Services │ Drivers   │
└────────────────────────────────────────────────────────────────┘
```

---

## 2. Performans İzleme ve Optimizasyon Motoru

### 2.1 Real-time Process Monitor

**Amaç**: Sistemde çalışan tüm işlemleri izler, kaynak kullanımını analiz eder ve gereksiz işlemleri tespit eder.

#### İmplementasyon

```csharp
public interface IProcessMonitorService
{
    /// <summary>
    /// Tüm aktif işlemleri real-time izler
    /// </summary>
    IObservable<ProcessMetrics> MonitorProcesses();

    /// <summary>
    /// Gereksiz/zararlı işlemleri tespit eder
    /// </summary>
    Task<IEnumerable<SuspiciousProcess>> DetectSuspiciousProcessesAsync();

    /// <summary>
    /// İşlem önceliğini optimize eder
    /// </summary>
    Task OptimizeProcessPriorityAsync(int processId, ProcessPriority priority);

    /// <summary>
    /// İşlemi güvenli şekilde sonlandırır
    /// </summary>
    Task<bool> TerminateProcessAsync(int processId, bool force = false);
}

public class ProcessMetrics
{
    public int ProcessId { get; set; }
    public string Name { get; set; }
    public string ExecutablePath { get; set; }
    public double CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public long DiskReadBytes { get; set; }
    public long DiskWriteBytes { get; set; }
    public long NetworkSentBytes { get; set; }
    public long NetworkReceivedBytes { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public ProcessPriorityClass Priority { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan TotalProcessorTime { get; set; }

    /// <summary>
    /// İşlemin sistem üzerindeki etki skoru (0-100)
    /// </summary>
    public double ImpactScore => CalculateImpactScore();

    private double CalculateImpactScore()
    {
        // CPU (40%) + RAM (30%) + Disk I/O (20%) + Network (10%)
        var cpuScore = Math.Min(CpuUsage, 100) * 0.4;
        var memScore = (MemoryUsage / (double)TotalSystemMemory) * 100 * 0.3;
        var diskScore = ((DiskReadBytes + DiskWriteBytes) / 1024.0 / 1024.0) * 0.2;
        var netScore = ((NetworkSentBytes + NetworkReceivedBytes) / 1024.0 / 1024.0) * 0.1;

        return cpuScore + memScore + diskScore + netScore;
    }
}

public class SuspiciousProcess
{
    public ProcessMetrics Metrics { get; set; }
    public SuspicionLevel Level { get; set; }
    public List<SuspicionReason> Reasons { get; set; }
    public RecommendedAction Action { get; set; }
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
    Quarantine      // Karantinaya al ve scan et
}
```

#### Gerçek Zamanlı İzleme (ETW - Event Tracing for Windows)

```csharp
public class EtwProcessMonitor : IDisposable
{
    private TraceEventSession _session;
    private readonly ConcurrentDictionary<int, ProcessMetrics> _metrics;

    public EtwProcessMonitor()
    {
        _metrics = new ConcurrentDictionary<int, ProcessMetrics>();
        _session = new TraceEventSession("WinCheckMonitorSession");
    }

    public void StartMonitoring()
    {
        // Kernel provider for process events
        _session.EnableKernelProvider(
            KernelTraceEventParser.Keywords.Process |
            KernelTraceEventParser.Keywords.Thread |
            KernelTraceEventParser.Keywords.ImageLoad |
            KernelTraceEventParser.Keywords.DiskIO |
            KernelTraceEventParser.Keywords.NetworkTCPIP
        );

        var parser = new KernelTraceEventParser(_session.Source);

        // CPU usage tracking
        parser.PerfInfoSample += data =>
        {
            UpdateCpuMetrics(data.ProcessID, data.ThreadID);
        };

        // Disk I/O tracking
        parser.DiskIORead += data =>
        {
            UpdateDiskMetrics(data.ProcessID, data.TransferSize, isRead: true);
        };

        parser.DiskIOWrite += data =>
        {
            UpdateDiskMetrics(data.ProcessID, data.TransferSize, isRead: false);
        };

        // Network tracking
        parser.TcpIpSend += data =>
        {
            UpdateNetworkMetrics(data.ProcessID, data.size, isSend: true);
        };

        // Process lifecycle
        parser.ProcessStart += data =>
        {
            OnProcessStarted(data.ProcessID, data.ImageFileName);
        };

        Task.Run(() => _session.Source.Process());
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
```

#### Performance Counters ile İzleme

```csharp
public class PerformanceCounterMonitor
{
    private readonly Dictionary<int, ProcessCounters> _counters;

    public class ProcessCounters
    {
        public PerformanceCounter CpuCounter { get; set; }
        public PerformanceCounter MemoryCounter { get; set; }
        public PerformanceCounter ThreadCounter { get; set; }
        public PerformanceCounter HandleCounter { get; set; }
    }

    public async Task<ProcessMetrics> GetProcessMetricsAsync(int processId)
    {
        var process = Process.GetProcessById(processId);

        if (!_counters.TryGetValue(processId, out var counters))
        {
            counters = CreateCounters(process.ProcessName);
            _counters[processId] = counters;
        }

        return new ProcessMetrics
        {
            ProcessId = processId,
            Name = process.ProcessName,
            ExecutablePath = GetProcessPath(processId),
            CpuUsage = counters.CpuCounter.NextValue(),
            MemoryUsage = process.WorkingSet64,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            Priority = process.PriorityClass,
            StartTime = process.StartTime,
            TotalProcessorTime = process.TotalProcessorTime
        };
    }

    [DllImport("kernel32.dll")]
    private static extern bool QueryFullProcessImageName(
        IntPtr hProcess,
        int dwFlags,
        StringBuilder lpExeName,
        ref int lpdwSize
    );

    private string GetProcessPath(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            var buffer = new StringBuilder(1024);
            var size = buffer.Capacity;

            if (QueryFullProcessImageName(process.Handle, 0, buffer, ref size))
            {
                return buffer.ToString();
            }
        }
        catch { }

        return string.Empty;
    }
}
```

### 2.2 Process Analyzer - AI Destekli Analiz

```csharp
public class ProcessAnalyzer
{
    private readonly IProcessMonitorService _monitor;
    private readonly IVirusTotalService _virusTotal;
    private readonly ISigCheckService _sigCheck;

    /// <summary>
    /// İşlemi çok boyutlu analiz eder
    /// </summary>
    public async Task<ProcessAnalysis> AnalyzeProcessAsync(ProcessMetrics process)
    {
        var analysis = new ProcessAnalysis { Process = process };

        // 1. Signature verification
        analysis.SignatureInfo = await VerifySignatureAsync(process.ExecutablePath);

        // 2. Location analysis
        analysis.LocationRisk = AnalyzeLocation(process.ExecutablePath);

        // 3. Behavior analysis
        analysis.BehaviorScore = AnalyzeBehavior(process);

        // 4. Hash-based reputation
        var hash = ComputeFileHash(process.ExecutablePath);
        analysis.ReputationInfo = await _virusTotal.CheckHashAsync(hash);

        // 5. Network connections
        analysis.NetworkConnections = GetProcessConnections(process.ProcessId);

        // 6. Child processes
        analysis.ChildProcesses = GetChildProcesses(process.ProcessId);

        // 7. DLL injection detection
        analysis.LoadedModules = GetLoadedModules(process.ProcessId);
        analysis.HasSuspiciousDlls = DetectSuspiciousDlls(analysis.LoadedModules);

        // 8. Registry modifications
        analysis.RegistryActivity = MonitorRegistryActivity(process.ProcessId);

        // 9. File system activity
        analysis.FileSystemActivity = MonitorFileActivity(process.ProcessId);

        // 10. Calculate final risk score
        analysis.RiskScore = CalculateRiskScore(analysis);

        return analysis;
    }

    private double CalculateRiskScore(ProcessAnalysis analysis)
    {
        double score = 0;

        // Signature (25 points)
        if (!analysis.SignatureInfo.IsValid) score += 25;
        else if (!analysis.SignatureInfo.IsMicrosoft) score += 10;

        // Location (15 points)
        score += analysis.LocationRisk * 15;

        // Behavior (20 points)
        score += analysis.BehaviorScore * 20;

        // Reputation (30 points)
        if (analysis.ReputationInfo != null)
        {
            score += (analysis.ReputationInfo.Malicious /
                     (double)analysis.ReputationInfo.Total) * 30;
        }

        // Suspicious DLLs (10 points)
        if (analysis.HasSuspiciousDlls) score += 10;

        return Math.Min(score, 100);
    }

    private double AnalyzeBehavior(ProcessMetrics process)
    {
        double score = 0;

        // CPU usage (0-0.3)
        if (process.CpuUsage > 80) score += 0.3;
        else if (process.CpuUsage > 50) score += 0.15;

        // Memory usage (0-0.3)
        var memPercentage = (process.MemoryUsage / (double)TotalSystemMemory) * 100;
        if (memPercentage > 50) score += 0.3;
        else if (memPercentage > 25) score += 0.15;

        // Thread count (0-0.2)
        if (process.ThreadCount > 100) score += 0.2;
        else if (process.ThreadCount > 50) score += 0.1;

        // Handle count (0-0.2)
        if (process.HandleCount > 10000) score += 0.2;
        else if (process.HandleCount > 5000) score += 0.1;

        return Math.Min(score, 1.0);
    }
}
```

---

## 3. Servis ve İşlem Yönetimi

### 3.1 Windows Services Optimizer

```csharp
public interface IServiceOptimizerService
{
    /// <summary>
    /// Tüm servisleri analiz eder ve optimizasyon önerileri sunar
    /// </summary>
    Task<ServiceOptimizationPlan> AnalyzeServicesAsync();

    /// <summary>
    /// Gereksiz servisleri güvenli şekilde devre dışı bırakır
    /// </summary>
    Task<bool> OptimizeServicesAsync(ServiceOptimizationPlan plan);

    /// <summary>
    /// Devre dışı bırakılan servisleri geri yükler
    /// </summary>
    Task<bool> RestoreServicesAsync();
}

public class ServiceOptimizationPlan
{
    public List<ServiceRecommendation> Recommendations { get; set; }
    public int TotalServices { get; set; }
    public int SuggestedToDisable { get; set; }
    public long EstimatedMemorySavings { get; set; }
    public TimeSpan EstimatedBootTimeReduction { get; set; }
}

public class ServiceRecommendation
{
    public string ServiceName { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public ServiceStartMode CurrentStartMode { get; set; }
    public ServiceStartMode RecommendedStartMode { get; set; }
    public ServiceCategory Category { get; set; }
    public ServiceSafetyLevel SafetyLevel { get; set; }
    public List<string> Dependencies { get; set; }
    public List<string> DependentServices { get; set; }
    public string Reason { get; set; }
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

public class ServiceOptimizerService : IServiceOptimizerService
{
    // Güvenli devre dışı bırakılabilir servisler listesi
    private static readonly Dictionary<string, ServiceRecommendation> SafeToDisableServices = new()
    {
        ["SysMain"] = new()
        {
            ServiceName = "SysMain",
            DisplayName = "Superfetch/SysMain",
            RecommendedStartMode = ServiceStartMode.Disabled,
            Category = ServiceCategory.Performance,
            SafetyLevel = ServiceSafetyLevel.Safe,
            Reason = "SSD kullanan sistemlerde gereksiz. %2-5 RAM tasarrufu sağlar."
        },
        ["WSearch"] = new()
        {
            ServiceName = "WSearch",
            DisplayName = "Windows Search",
            RecommendedStartMode = ServiceStartMode.Manual,
            Category = ServiceCategory.Performance,
            SafetyLevel = ServiceSafetyLevel.Conditional,
            Reason = "Indexing yoğun CPU kullanır. Manuel başlatma önerilir."
        },
        ["DiagTrack"] = new()
        {
            ServiceName = "DiagTrack",
            DisplayName = "Connected User Experiences and Telemetry",
            RecommendedStartMode = ServiceStartMode.Disabled,
            Category = ServiceCategory.Performance,
            SafetyLevel = ServiceSafetyLevel.Recommended,
            Reason = "Telemetry servisi. Gizlilik ve performans için kapatılabilir."
        },
        ["TabletInputService"] = new()
        {
            ServiceName = "TabletInputService",
            DisplayName = "Touch Keyboard and Handwriting Panel Service",
            RecommendedStartMode = ServiceStartMode.Manual,
            Category = ServiceCategory.Hardware,
            SafetyLevel = ServiceSafetyLevel.Conditional,
            Reason = "Tablet/dokunmatik ekran yoksa gereksiz."
        },
        ["Fax"] = new()
        {
            ServiceName = "Fax",
            DisplayName = "Fax",
            RecommendedStartMode = ServiceStartMode.Disabled,
            Category = ServiceCategory.Unnecessary,
            SafetyLevel = ServiceSafetyLevel.Safe,
            Reason = "Modern sistemlerde kullanılmıyor."
        },
        ["RemoteRegistry"] = new()
        {
            ServiceName = "RemoteRegistry",
            DisplayName = "Remote Registry",
            RecommendedStartMode = ServiceStartMode.Disabled,
            Category = ServiceCategory.Security,
            SafetyLevel = ServiceSafetyLevel.Recommended,
            Reason = "Güvenlik riski. Uzaktan registry erişimi nadiren gerekir."
        },
        ["MapsBroker"] = new()
        {
            ServiceName = "MapsBroker",
            DisplayName = "Downloaded Maps Manager",
            RecommendedStartMode = ServiceStartMode.Manual,
            Category = ServiceCategory.Microsoft,
            SafetyLevel = ServiceSafetyLevel.Safe,
            Reason = "Windows Maps kullanılmıyorsa gereksiz."
        },
        ["XblAuthManager"] = new()
        {
            ServiceName = "XblAuthManager",
            DisplayName = "Xbox Live Auth Manager",
            RecommendedStartMode = ServiceStartMode.Manual,
            Category = ServiceCategory.Microsoft,
            SafetyLevel = ServiceSafetyLevel.Conditional,
            Reason = "Xbox kullanılmıyorsa gereksiz."
        }
    };

    public async Task<ServiceOptimizationPlan> AnalyzeServicesAsync()
    {
        var services = ServiceController.GetServices();
        var plan = new ServiceOptimizationPlan
        {
            TotalServices = services.Length,
            Recommendations = new List<ServiceRecommendation>()
        };

        foreach (var service in services)
        {
            using (service)
            {
                var recommendation = await AnalyzeServiceAsync(service);
                if (recommendation != null)
                {
                    plan.Recommendations.Add(recommendation);
                }
            }
        }

        plan.SuggestedToDisable = plan.Recommendations
            .Count(r => r.RecommendedStartMode == ServiceStartMode.Disabled);

        // RAM tasarruf tahmini (her servis ~10-50MB)
        plan.EstimatedMemorySavings = plan.SuggestedToDisable * 25 * 1024 * 1024L;

        // Boot time tahmini (her servis ~100-500ms)
        plan.EstimatedBootTimeReduction = TimeSpan.FromMilliseconds(
            plan.SuggestedToDisable * 250
        );

        return plan;
    }

    private async Task<ServiceRecommendation> AnalyzeServiceAsync(ServiceController service)
    {
        // Known safe-to-disable listesine bak
        if (SafeToDisableServices.TryGetValue(service.ServiceName, out var known))
        {
            known.CurrentStartMode = GetStartMode(service);
            return known;
        }

        // WMI ile detaylı bilgi al
        using var searcher = new ManagementObjectSearcher(
            $"SELECT * FROM Win32_Service WHERE Name = '{service.ServiceName}'"
        );

        var wmiService = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
        if (wmiService == null) return null;

        var recommendation = new ServiceRecommendation
        {
            ServiceName = service.ServiceName,
            DisplayName = service.DisplayName,
            Description = wmiService["Description"]?.ToString(),
            CurrentStartMode = GetStartMode(service),
            Category = CategorizeService(service, wmiService),
            SafetyLevel = DetermineSafetyLevel(service, wmiService)
        };

        // Dependencies kontrol et
        recommendation.Dependencies = service.ServicesDependedOn
            .Select(s => s.ServiceName).ToList();

        // Only suggest changes if safe
        if (recommendation.SafetyLevel == ServiceSafetyLevel.Recommended)
        {
            recommendation.RecommendedStartMode = ServiceStartMode.Disabled;
            recommendation.Reason = DetermineReason(recommendation);
            return recommendation;
        }

        return null;
    }

    private ServiceStartMode GetStartMode(ServiceController service)
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            $@"SYSTEM\CurrentControlSet\Services\{service.ServiceName}"
        );

        var startValue = (int)(key?.GetValue("Start") ?? 2);
        return startValue switch
        {
            2 => ServiceStartMode.Automatic,
            3 => ServiceStartMode.Manual,
            4 => ServiceStartMode.Disabled,
            _ => ServiceStartMode.Manual
        };
    }
}
```

### 3.2 Startup Programs Manager - Gelişmiş

```csharp
public class StartupManagerService : IStartupManagerService
{
    public async Task<List<StartupItem>> GetAllStartupItemsAsync()
    {
        var items = new List<StartupItem>();

        // 1. Registry Run keys
        items.AddRange(await GetRegistryStartupItemsAsync());

        // 2. Startup folder
        items.AddRange(GetStartupFolderItems());

        // 3. Task Scheduler
        items.AddRange(await GetScheduledTasksAsync());

        // 4. Services (auto-start)
        items.AddRange(await GetAutoStartServicesAsync());

        // 5. WMI Event Consumers
        items.AddRange(await GetWmiStartupItemsAsync());

        // Impact skoru hesapla
        foreach (var item in items)
        {
            item.ImpactScore = await CalculateImpactScoreAsync(item);
        }

        return items.OrderByDescending(i => i.ImpactScore).ToList();
    }

    private async Task<double> CalculateImpactScoreAsync(StartupItem item)
    {
        try
        {
            // Dosya boyutunu kontrol et
            var fileInfo = new FileInfo(item.Path);
            var sizeScore = Math.Min(fileInfo.Length / (100.0 * 1024 * 1024), 1.0); // 100MB = max

            // İşlemi başlat ve performansını ölç
            var startInfo = new ProcessStartInfo
            {
                FileName = item.Path,
                Arguments = item.Arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var stopwatch = Stopwatch.StartNew();
            using var process = Process.Start(startInfo);

            await Task.Delay(3000); // 3 saniye bekle

            if (process != null && !process.HasExited)
            {
                var cpuUsage = GetProcessCpuUsage(process);
                var memoryUsage = process.WorkingSet64;

                process.Kill();

                var startupTime = stopwatch.ElapsedMilliseconds;

                // Composite score
                // Startup time (40%) + CPU (30%) + Memory (20%) + Size (10%)
                var timeScore = Math.Min(startupTime / 5000.0, 1.0); // 5 sec = max
                var cpuScore = Math.Min(cpuUsage / 50.0, 1.0); // 50% = max
                var memScore = Math.Min(memoryUsage / (500.0 * 1024 * 1024), 1.0); // 500MB = max

                return (timeScore * 0.4 + cpuScore * 0.3 + memScore * 0.2 + sizeScore * 0.1) * 100;
            }
        }
        catch { }

        return 50; // Default medium impact
    }
}
```

---

## 4. Donanım Optimizasyonu

### 4.1 CPU Optimizasyonu

```csharp
public class CpuOptimizer
{
    /// <summary>
    /// CPU power plan optimizasyonu
    /// </summary>
    public async Task OptimizePowerPlanAsync(PowerProfile profile)
    {
        var guid = profile switch
        {
            PowerProfile.MaxPerformance => GUID_MAX_POWER_SAVINGS,
            PowerProfile.Balanced => GUID_TYPICAL_POWER_SAVINGS,
            PowerProfile.PowerSaver => GUID_MIN_POWER_SAVINGS,
            _ => GUID_TYPICAL_POWER_SAVINGS
        };

        PowerSetActiveScheme(IntPtr.Zero, ref guid);

        // Processor power management ayarları
        await SetPowerSettingAsync(
            SUB_PROCESSOR,
            PROCTHROTTLEMIN,
            profile == PowerProfile.MaxPerformance ? 100 : 5
        );
    }

    /// <summary>
    /// CPU core parking'i devre dışı bırak (performance için)
    /// </summary>
    public void DisableCoreParkining()
    {
        // Registry: HKLM\SYSTEM\ControlSet001\Control\Power\PowerSettings
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\0cc5b647-c1df-4637-891a-dec35c318583",
            writable: true
        );

        key?.SetValue("ValueMax", 0); // Disable core parking
    }

    /// <summary>
    /// CPU affinity optimizasyonu - kritik işlemleri belirli core'lara ata
    /// </summary>
    public void OptimizeProcessAffinity(int processId, AffinityStrategy strategy)
    {
        var process = Process.GetProcessById(processId);
        var coreCount = Environment.ProcessorCount;

        switch (strategy)
        {
            case AffinityStrategy.PerformanceCores:
                // İlk yarı (genelde P-cores)
                process.ProcessorAffinity = (IntPtr)((1 << (coreCount / 2)) - 1);
                break;

            case AffinityStrategy.EfficiencyCores:
                // İkinci yarı (E-cores)
                var mask = ((1 << coreCount) - 1) ^ ((1 << (coreCount / 2)) - 1);
                process.ProcessorAffinity = (IntPtr)mask;
                break;

            case AffinityStrategy.SingleCore:
                process.ProcessorAffinity = (IntPtr)1;
                break;
        }
    }
}
```

### 4.2 RAM Optimizasyonu

```csharp
public class MemoryOptimizer
{
    /// <summary>
    /// Standby list'i temizle (Windows SuperFetch cache)
    /// </summary>
    [DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

    public void ClearStandbyList()
    {
        // EmptyStandbyList komutu (yüksek ayrıcalık gerektirir)
        var psi = new ProcessStartInfo
        {
            FileName = "RAMMap.exe", // Sysinternals RAMMap
            Arguments = "-Ew",  // Empty working sets
            UseShellExecute = true,
            Verb = "runas"
        };

        Process.Start(psi)?.WaitForExit();
    }

    /// <summary>
    /// Working set'leri optimize et
    /// </summary>
    public void OptimizeWorkingSets()
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                SetProcessWorkingSetSize(process.Handle, -1, -1);
            }
            catch { }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    /// <summary>
    /// Memory compression ayarları
    /// </summary>
    public void ConfigureMemoryCompression(bool enable)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = enable
                ? "Enable-MMAgent -MemoryCompression"
                : "Disable-MMAgent -MemoryCompression",
            UseShellExecute = false,
            CreateNoWindow = true,
            Verb = "runas"
        };

        Process.Start(psi)?.WaitForExit();
    }

    /// <summary>
    /// Sayfa dosyası (pagefile) optimizasyonu
    /// </summary>
    public void OptimizePageFile(PageFileConfig config)
    {
        // WMI ile pagefile ayarları
        using var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_PageFileSetting"
        );

        foreach (ManagementObject pageFile in searcher.Get())
        {
            if (config.SystemManaged)
            {
                pageFile.Delete();
            }
            else
            {
                pageFile["InitialSize"] = config.InitialSizeMB;
                pageFile["MaximumSize"] = config.MaximumSizeMB;
                pageFile.Put();
            }
        }
    }
}
```

### 4.3 Disk Optimizasyonu

```csharp
public class DiskOptimizer
{
    /// <summary>
    /// SSD TRIM komutu
    /// </summary>
    public async Task<bool> TrimSsdAsync(string driveLetter)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "defrag.exe",
            Arguments = $"{driveLetter}: /L", // TRIM
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }

    /// <summary>
    /// Prefetch ve Superfetch optimizasyonu
    /// </summary>
    public void OptimizePrefetch(PrefetchMode mode)
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters",
            writable: true
        );

        // 0 = Disabled, 1 = App prefetch, 2 = Boot prefetch, 3 = Both
        key?.SetValue("EnablePrefetcher", (int)mode);
        key?.SetValue("EnableSuperfetch", (int)mode);
    }

    /// <summary>
    /// Write caching optimizasyonu
    /// </summary>
    public void EnableWriteCaching(string deviceId)
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            $@"SYSTEM\CurrentControlSet\Enum\{deviceId}\Device Parameters\Disk",
            writable: true
        );

        key?.SetValue("CacheIsPowerProtected", 1);
        key?.SetValue("UserWriteCacheSetting", 1);
    }

    /// <summary>
    /// NTFS son erişim zamanı kaydını kapat (performans artışı)
    /// </summary>
    public void DisableNtfsLastAccessTime()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "fsutil.exe",
            Arguments = "behavior set disablelastaccess 1",
            UseShellExecute = false,
            Verb = "runas"
        };

        Process.Start(psi)?.WaitForExit();
    }
}
```

### 4.4 Network Optimizasyonu

```csharp
public class NetworkOptimizer
{
    /// <summary>
    /// TCP/IP stack optimizasyonu
    /// </summary>
    public void OptimizeTcpStack()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
            writable: true
        );

        // TCP window auto-tuning
        key?.SetValue("TcpWindowSize", 65535);

        // Disable Nagle's algorithm için opsiyonel
        key?.SetValue("TcpAckFrequency", 1);
        key?.SetValue("TCPNoDelay", 1);

        // TCP 1323 options (timestamps, window scaling)
        key?.SetValue("Tcp1323Opts", 3);

        // SYN retransmit azaltma
        key?.SetValue("TcpMaxDataRetransmissions", 3);

        // DNS cache süresi
        key?.SetValue("MaxCacheTtl", 86400); // 1 gün
    }

    /// <summary>
    /// Network adapter ayarları
    /// </summary>
    public async Task OptimizeNetworkAdapterAsync(string adapterName)
    {
        // PowerShell ile network adapter ayarları
        var commands = new[]
        {
            // RSS (Receive Side Scaling) aktif et
            $"Set-NetAdapterRss -Name '{adapterName}' -Enabled $true",

            // Large Send Offload
            $"Set-NetAdapterLso -Name '{adapterName}' -Enabled $true",

            // Checksum Offload
            $"Set-NetAdapterChecksumOffload -Name '{adapterName}' -Enabled $true",

            // Jumbo Frames (1GB+ network için)
            $"Set-NetAdapterAdvancedProperty -Name '{adapterName}' -DisplayName 'Jumbo Packet' -DisplayValue '9014 Bytes'"
        };

        foreach (var command in commands)
        {
            await ExecutePowerShellAsync(command);
        }
    }

    /// <summary>
    /// Gereksiz network protokollerini kapat
    /// </summary>
    public void DisableUnusedProtocols(string adapterName)
    {
        // IPv6 (kullanılmıyorsa)
        // NetBIOS over TCP/IP
        // Link-Layer Topology Discovery

        using var adapter = new ManagementClass("Win32_NetworkAdapterConfiguration");
        foreach (ManagementObject obj in adapter.GetInstances())
        {
            if (obj["Description"].ToString() == adapterName)
            {
                obj.InvokeMethod("DisableIPv6", null);
                obj.InvokeMethod("SetTcpipNetbios", new object[] { 2 }); // Disable NetBIOS
            }
        }
    }
}
```

---

## 5. Akıllı Öneri Sistemi

### 5.1 AI-Powered Optimization Engine

```csharp
public class OptimizationEngine
{
    private readonly IProcessMonitorService _processMonitor;
    private readonly IServiceOptimizerService _serviceOptimizer;
    private readonly List<OptimizationRule> _rules;

    public async Task<OptimizationReport> GenerateReportAsync()
    {
        var report = new OptimizationReport
        {
            GeneratedAt = DateTime.Now,
            SystemInfo = await GatherSystemInfoAsync(),
            Recommendations = new List<Recommendation>()
        };

        // 1. Process analysis
        var processes = await _processMonitor.DetectSuspiciousProcessesAsync();
        foreach (var process in processes)
        {
            report.Recommendations.Add(new Recommendation
            {
                Category = RecommendationCategory.Process,
                Priority = MapPriority(process.Level),
                Title = $"'{process.Metrics.Name}' yüksek kaynak kullanıyor",
                Description = $"CPU: {process.Metrics.CpuUsage:F1}%, RAM: {process.Metrics.MemoryUsage / 1024 / 1024}MB",
                Action = process.Action,
                ImpactScore = process.Metrics.ImpactScore,
                AutomationAvailable = true
            });
        }

        // 2. Service optimization
        var servicePlan = await _serviceOptimizer.AnalyzeServicesAsync();
        foreach (var service in servicePlan.Recommendations)
        {
            report.Recommendations.Add(new Recommendation
            {
                Category = RecommendationCategory.Service,
                Priority = Priority.Medium,
                Title = $"'{service.DisplayName}' servisi optimize edilebilir",
                Description = service.Reason,
                EstimatedMemorySavings = 25 * 1024 * 1024, // ~25MB
                AutomationAvailable = service.SafetyLevel == ServiceSafetyLevel.Safe
            });
        }

        // 3. Startup optimization
        // 4. Disk cleanup opportunities
        // 5. Registry optimization
        // 6. Network optimization

        // Önceliklendirme: Impact score * Priority
        report.Recommendations = report.Recommendations
            .OrderByDescending(r => r.ImpactScore * (int)r.Priority)
            .ToList();

        // Toplam potansiyel kazanımlar
        report.TotalPotentialMemorySavings = report.Recommendations
            .Sum(r => r.EstimatedMemorySavings);

        report.TotalPotentialBootTimeReduction = TimeSpan.FromMilliseconds(
            report.Recommendations.Count(r => r.Category == RecommendationCategory.Startup) * 250
        );

        return report;
    }
}

public class Recommendation
{
    public RecommendationCategory Category { get; set; }
    public Priority Priority { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string TechnicalDetails { get; set; }
    public RecommendedAction Action { get; set; }
    public double ImpactScore { get; set; }
    public long EstimatedMemorySavings { get; set; }
    public TimeSpan EstimatedPerformanceGain { get; set; }
    public bool AutomationAvailable { get; set; }
    public List<string> Risks { get; set; }
    public string LearnMoreUrl { get; set; }
}

public enum RecommendationCategory
{
    Process,
    Service,
    Startup,
    Disk,
    Registry,
    Network,
    Hardware,
    Security,
    Privacy
}

public enum Priority
{
    Critical = 4,
    High = 3,
    Medium = 2,
    Low = 1,
    Info = 0
}
```

### 5.2 Machine Learning Model (Basitleştirilmiş)

```csharp
public class OptimizationPredictor
{
    /// <summary>
    /// Kullanıcı davranışlarından öğrenme
    /// </summary>
    public async Task<List<Prediction>> PredictOptimizationsAsync()
    {
        // Geçmiş verileri analiz et
        var history = await LoadOptimizationHistoryAsync();

        // Pattern detection
        var patterns = DetectPatterns(history);

        // Predictions
        var predictions = new List<Prediction>();

        // Örnek: Kullanıcı her Pazartesi Photoshop kullanıyor
        if (patterns.WeeklyAppUsage.ContainsKey("Photoshop") &&
            DateTime.Now.DayOfWeek == DayOfWeek.Monday)
        {
            predictions.Add(new Prediction
            {
                Type = PredictionType.ResourceAllocation,
                Confidence = 0.85,
                Suggestion = "Photoshop için sistem kaynaklarını optimize ediyorum",
                Actions = new List<string>
                {
                    "Diğer ağır uygulamaları askıya al",
                    "Page file'ı büyüt",
                    "GPU acceleration etkinleştir"
                }
            });
        }

        return predictions;
    }

    private UsagePatterns DetectPatterns(List<OptimizationHistory> history)
    {
        var patterns = new UsagePatterns();

        // Hourly usage
        var hourlyGroups = history.GroupBy(h => h.Timestamp.Hour);
        patterns.PeakHours = hourlyGroups
            .OrderByDescending(g => g.Average(h => h.CpuUsage))
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        // Weekly app usage
        patterns.WeeklyAppUsage = history
            .GroupBy(h => h.TopProcess)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(h => h.Timestamp.DayOfWeek).ToDictionary(x => x.Key, x => x.Count())
            );

        return patterns;
    }
}
```

---

## 6. Veri Akışı ve State Management

### 6.1 Reactive Architecture (System.Reactive)

```csharp
public class SystemMetricsStream
{
    private readonly IObservable<SystemMetrics> _metricsStream;

    public SystemMetricsStream()
    {
        // Her 1 saniyede bir sistem metriklerini yayınla
        _metricsStream = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => GatherCurrentMetrics())
            .Publish()
            .RefCount();
    }

    public IObservable<SystemMetrics> GetMetricsStream() => _metricsStream;

    public IObservable<Alert> GetAlertStream()
    {
        return _metricsStream
            .Select(metrics => DetectAnomalies(metrics))
            .Where(alert => alert != null);
    }

    public IObservable<double> GetCpuUsageStream()
    {
        return _metricsStream
            .Select(m => m.CpuUsage)
            .DistinctUntilChanged();
    }
}

// ViewModel'de kullanım
public class DashboardViewModel : ObservableObject
{
    private readonly SystemMetricsStream _stream;
    private readonly CompositeDisposable _subscriptions;

    public DashboardViewModel(SystemMetricsStream stream)
    {
        _stream = stream;
        _subscriptions = new CompositeDisposable();

        // CPU usage binding
        _stream.GetCpuUsageStream()
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(cpu => CpuUsage = cpu)
            .DisposeWith(_subscriptions);

        // Alert notifications
        _stream.GetAlertStream()
            .Throttle(TimeSpan.FromSeconds(5)) // Spam önleme
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(alert => ShowNotification(alert))
            .DisposeWith(_subscriptions);
    }

    public void Dispose()
    {
        _subscriptions?.Dispose();
    }
}
```

---

## 7. API Referansları

### 7.1 Native Windows APIs

```csharp
public static class NativeApi
{
    // Performance Monitoring
    [DllImport("pdh.dll")]
    public static extern int PdhOpenQuery(string szDataSource, IntPtr dwUserData, out IntPtr phQuery);

    // Process Management
    [DllImport("kernel32.dll")]
    public static extern bool SetPriorityClass(IntPtr handle, ProcessPriorityClass priorityClass);

    // Memory Management
    [DllImport("psapi.dll")]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);

    [DllImport("kernel32.dll")]
    public static extern bool GetSystemFileCacheSize(
        out IntPtr lpMinimumFileCacheSize,
        out IntPtr lpMaximumFileCacheSize,
        out IntPtr lpFlags
    );

    // Power Management
    [DllImport("powrprof.dll")]
    public static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, ref Guid SchemeGuid);

    // Disk I/O
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );
}
```

---

## Sonuç

Bu mimari dokümantasyon, WinCheck'in teknik altyapısını, performans izleme ve optimizasyon yeteneklerini detaylı olarak açıklamaktadır. Sistem, modern .NET 8 ve WinUI 3 teknolojileri ile Windows API'lerini bir araya getirerek, kullanıcılara kapsamlı sistem optimizasyonu sağlar.

**Önemli Özellikler:**
- Real-time process monitoring (ETW, Performance Counters)
- AI-destekli optimizasyon önerileri
- Güvenli servis ve startup yönetimi
- Donanım seviyesinde optimizasyonlar (CPU, RAM, Disk, Network)
- Reactive architecture ile real-time UI updates
- Kapsamlı logging ve telemetry

**Güvenlik İlkeleri:**
- Her işlem öncesi otomatik backup
- Whitelist-based approach
- UAC integration
- Rollback capability
