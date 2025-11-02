# WinCheck - Hardware Detection & Testing Module

## 🔧 Hardware Detection System

### Supported Components

#### 1. CPU Detection & Testing
**Detection:**
- CPU Model, Manufacturer
- Core count (Physical + Logical)
- Base/Boost frequencies
- Cache sizes (L1, L2, L3)
- Architecture (x86, x64, ARM64)
- Virtualization support (VT-x, AMD-V)
- Instruction sets (SSE, AVX, AVX2, AVX-512)

**Tests:**
- Single-core performance test
- Multi-core performance test
- Thermal stress test
- Cache performance test
- Prime number calculation benchmark

#### 2. Memory (RAM) Detection & Testing
**Detection:**
- Total physical memory
- Available memory
- Memory type (DDR3, DDR4, DDR5)
- Memory speed (MHz)
- Number of slots used/total
- Dual/Quad channel configuration

**Tests:**
- Memory read speed test
- Memory write speed test
- Memory latency test
- Memory stress test (stability)
- Error checking (ECC validation if supported)

#### 3. Storage Detection & Testing
**Detection:**
- Drive type (HDD, SSD, NVMe)
- Interface (SATA, PCIe, NVMe)
- Capacity (total, used, free)
- SMART attributes
- Partition layout
- File system (NTFS, FAT32, exFAT)

**Tests:**
- Sequential read speed
- Sequential write speed
- Random read IOPS
- Random write IOPS
- SMART health check
- Bad sector scan

#### 4. GPU Detection & Testing
**Detection:**
- GPU model and manufacturer
- VRAM size
- Driver version
- DirectX version
- OpenGL version
- Multiple GPU detection (SLI/CrossFire)

**Tests:**
- GPU compute test
- 3D rendering benchmark
- Memory bandwidth test
- Temperature monitoring
- Driver compatibility check

#### 5. Network Adapters
**Detection:**
- Adapter name and type (Ethernet, WiFi, VPN)
- MAC address
- IP addresses (IPv4, IPv6)
- Link speed
- Driver version

**Tests:**
- Connection speed test
- Latency test (ping)
- DNS resolution test
- Packet loss test

#### 6. Audio Devices
**Detection:**
- Audio devices (playback, recording)
- Driver versions
- Sample rates supported

**Tests:**
- Playback test
- Recording test
- Driver functionality

#### 7. Display/Monitors
**Detection:**
- Monitor models
- Resolutions
- Refresh rates
- Color depth
- HDR support

#### 8. Motherboard
**Detection:**
- Manufacturer and model
- BIOS version
- Chipset
- USB controllers
- PCIe slots

#### 9. Battery (Laptops)
**Detection:**
- Battery capacity (design vs current)
- Charge cycles
- Wear level
- Chemistry type

**Tests:**
- Battery health estimation
- Discharge rate test

#### 10. Cooling System
**Detection:**
- Fan count and locations
- Temperature sensors
- Fan speeds (RPM)

**Tests:**
- Fan control test
- Temperature stress test

---

## Implementation

### Core Service

```csharp
public interface IHardwareDetectionService
{
    Task<HardwareProfile> DetectAllHardwareAsync();
    Task<TestResults> RunHardwareTestsAsync(HardwareTestOptions options);
    Task<HardwareHealth> GetHardwareHealthAsync();
}

public class HardwareProfile
{
    public CpuInfo Cpu { get; set; }
    public MemoryInfo Memory { get; set; }
    public List<StorageInfo> Storage { get; set; }
    public List<GpuInfo> Gpus { get; set; }
    public List<NetworkAdapterInfo> NetworkAdapters { get; set; }
    public MotherboardInfo Motherboard { get; set; }
    public BatteryInfo Battery { get; set; }
    public List<MonitorInfo> Monitors { get; set; }
}
```

### CPU Detection

```csharp
public class CpuDetectionService
{
    [DllImport("kernel32.dll")]
    private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

    public async Task<CpuInfo> DetectCpuAsync()
    {
        var cpu = new CpuInfo();

        // WMI Query
        using var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_Processor"
        );

        foreach (ManagementObject obj in searcher.Get())
        {
            cpu.Name = obj["Name"]?.ToString();
            cpu.Manufacturer = obj["Manufacturer"]?.ToString();
            cpu.NumberOfCores = Convert.ToInt32(obj["NumberOfCores"]);
            cpu.NumberOfLogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
            cpu.MaxClockSpeed = Convert.ToInt32(obj["MaxClockSpeed"]);
            cpu.CurrentClockSpeed = Convert.ToInt32(obj["CurrentClockSpeed"]);
            cpu.L2CacheSize = Convert.ToInt32(obj["L2CacheSize"]);
            cpu.L3CacheSize = Convert.ToInt32(obj["L3CacheSize"]);
            cpu.Architecture = obj["Architecture"]?.ToString();
        }

        // Check instruction sets
        cpu.SupportedInstructions = DetectInstructionSets();

        return cpu;
    }

    private List<string> DetectInstructionSets()
    {
        var instructions = new List<string>();

        // Using CPUID instruction
        // This would require P/Invoke or unsafe code
        // For now, using RegistryInfo or environment

        if (IsProcessorFeaturePresent(ProcessorFeature.InstructionsXMMIAvailable))
            instructions.Add("SSE");

        if (IsProcessorFeaturePresent(ProcessorFeature.InstructionsXMMI64Available))
            instructions.Add("SSE2");

        // AVX detection would need CPUID

        return instructions;
    }

    [DllImport("kernel32.dll")]
    private static extern bool IsProcessorFeaturePresent(ProcessorFeature feature);
}
```

### Storage Detection with SMART

```csharp
public class StorageDetectionService
{
    public async Task<List<StorageInfo>> DetectStorageAsync()
    {
        var storage = new List<StorageInfo>();

        // Physical Disks
        using var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_DiskDrive"
        );

        foreach (ManagementObject disk in searcher.Get())
        {
            var info = new StorageInfo
            {
                Model = disk["Model"]?.ToString(),
                InterfaceType = disk["InterfaceType"]?.ToString(),
                Size = Convert.ToInt64(disk["Size"]),
                MediaType = disk["MediaType"]?.ToString(),
                SerialNumber = disk["SerialNumber"]?.ToString()
            };

            // Determine if SSD or HDD
            info.IsSSD = DetermineIfSSD(info);

            // Get SMART data
            info.SmartData = await GetSmartDataAsync(disk["DeviceID"].ToString());

            storage.Add(info);
        }

        return storage;
    }

    private bool DetermineIfSSD(StorageInfo info)
    {
        // Check media type or model name
        if (info.MediaType?.Contains("SSD") == true)
            return true;

        if (info.Model?.Contains("SSD") == true ||
            info.Model?.Contains("NVMe") == true)
            return true;

        // Check for rotational speed (SSDs have 0)
        // This requires additional WMI query or S.M.A.R.T.

        return false;
    }

    private async Task<SmartData> GetSmartDataAsync(string deviceId)
    {
        // SMART data requires IOCTL calls
        var smart = new SmartData();

        using var handle = CreateFile(
            deviceId,
            FileAccess.ReadWrite,
            FileShare.ReadWrite,
            IntPtr.Zero,
            FileMode.Open,
            FileAttributes.Normal,
            IntPtr.Zero
        );

        if (handle.IsInvalid)
            return smart;

        // SMART_READ_DATA IOCTL
        var smartData = new byte[512];
        // ... IOCTL implementation

        smart.Temperature = ExtractSmartAttribute(smartData, 0xC2);
        smart.PowerOnHours = ExtractSmartAttribute(smartData, 0x09);
        smart.ReallocatedSectors = ExtractSmartAttribute(smartData, 0x05);
        smart.HealthPercentage = CalculateHealthPercentage(smartData);

        return smart;
    }
}
```

### Hardware Testing

```csharp
public class HardwareTestingService
{
    public async Task<CpuTestResult> TestCpuAsync(CpuTestOptions options)
    {
        var result = new CpuTestResult();

        // Single-core test
        result.SingleCoreScore = await RunSingleCoreTestAsync(options.Duration);

        // Multi-core test
        result.MultiCoreScore = await RunMultiCoreTestAsync(options.Duration);

        // Temperature monitoring
        result.MaxTemperature = await MonitorTemperatureDuringTestAsync();

        return result;
    }

    private async Task<int> RunSingleCoreTestAsync(TimeSpan duration)
    {
        var stopwatch = Stopwatch.StartNew();
        long operations = 0;

        // CPU-intensive calculation (prime numbers)
        while (stopwatch.Elapsed < duration)
        {
            // Calculate primes
            for (int i = 2; i < 10000; i++)
            {
                if (IsPrime(i))
                    operations++;
            }
        }

        return (int)(operations / duration.TotalSeconds);
    }

    private async Task<int> RunMultiCoreTestAsync(TimeSpan duration)
    {
        var coreCount = Environment.ProcessorCount;
        var tasks = new List<Task<int>>();

        for (int i = 0; i < coreCount; i++)
        {
            tasks.Add(Task.Run(() => RunSingleCoreTestAsync(duration)));
        }

        var results = await Task.WhenAll(tasks);
        return results.Sum();
    }
}

public class MemoryTestingService
{
    public async Task<MemoryTestResult> TestMemoryAsync(MemoryTestOptions options)
    {
        var result = new MemoryTestResult();

        // Allocate test buffer
        var testSize = options.TestSizeMB * 1024 * 1024;
        var buffer = new byte[testSize];

        // Write test
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < testSize; i++)
        {
            buffer[i] = (byte)(i % 256);
        }
        stopwatch.Stop();
        result.WriteSpeedMBps = testSize / stopwatch.Elapsed.TotalSeconds / 1024 / 1024;

        // Read test
        stopwatch.Restart();
        long sum = 0;
        for (int i = 0; i < testSize; i++)
        {
            sum += buffer[i];
        }
        stopwatch.Stop();
        result.ReadSpeedMBps = testSize / stopwatch.Elapsed.TotalSeconds / 1024 / 1024;

        // Latency test
        result.LatencyNs = await MeasureMemoryLatencyAsync();

        return result;
    }

    private async Task<double> MeasureMemoryLatencyAsync()
    {
        // Pointer chasing for latency measurement
        const int iterations = 1000000;
        var array = new int[1024];

        // Create random access pattern
        var random = new Random();
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = random.Next(array.Length);
        }

        var stopwatch = Stopwatch.StartNew();
        int index = 0;
        for (int i = 0; i < iterations; i++)
        {
            index = array[index];
        }
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalNanoseconds / iterations;
    }
}

public class StorageTestingService
{
    public async Task<StorageTestResult> TestStorageAsync(
        string driveLetter,
        StorageTestOptions options)
    {
        var result = new StorageTestResult();
        var testFilePath = Path.Combine(driveLetter, "wincheck_test.tmp");

        try
        {
            // Sequential write test
            result.SequentialWriteMBps = await TestSequentialWriteAsync(
                testFilePath,
                options.TestSizeMB
            );

            // Sequential read test
            result.SequentialReadMBps = await TestSequentialReadAsync(
                testFilePath,
                options.TestSizeMB
            );

            // Random I/O test
            (result.RandomReadIOPS, result.RandomWriteIOPS) =
                await TestRandomIOAsync(testFilePath);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        return result;
    }

    private async Task<double> TestSequentialWriteAsync(string path, int sizeMB)
    {
        var data = new byte[1024 * 1024]; // 1 MB buffer
        var random = new Random();
        random.NextBytes(data);

        var stopwatch = Stopwatch.StartNew();

        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write,
                                        FileShare.None, 1024 * 1024,
                                        FileOptions.WriteThrough))
        {
            for (int i = 0; i < sizeMB; i++)
            {
                await fs.WriteAsync(data, 0, data.Length);
            }
            await fs.FlushAsync();
        }

        stopwatch.Stop();
        return sizeMB / stopwatch.Elapsed.TotalSeconds;
    }
}
```

---

## UI Implementation

### Hardware Detection Page

```xml
<Page x:Class="WinCheck.App.Views.HardwareDetectionPage">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="12">
            <TextBlock Text="Hardware Detection & Testing"
                       Style="{StaticResource TitleTextBlockStyle}"/>
            <StackPanel Orientation="Horizontal" Spacing="12">
                <Button Content="Detect Hardware"
                        Command="{x:Bind ViewModel.DetectHardwareCommand}"/>
                <Button Content="Run All Tests"
                        Command="{x:Bind ViewModel.RunAllTestsCommand}"
                        Style="{StaticResource AccentButtonStyle}"/>
            </StackPanel>
        </StackPanel>

        <!-- Hardware List -->
        <ScrollViewer Grid.Row="1">
            <StackPanel Spacing="16" Padding="24">

                <!-- CPU -->
                <Expander Header="CPU" IsExpanded="True">
                    <Grid Padding="16" RowSpacing="8">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="150"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Model:"/>
                        <TextBlock Grid.Row="0" Grid.Column="1"
                                   Text="{x:Bind ViewModel.Cpu.Name}"/>

                        <TextBlock Grid.Row="1" Grid.Column="0" Text="Cores:"/>
                        <TextBlock Grid.Row="1" Grid.Column="1"
                                   Text="{x:Bind ViewModel.Cpu.CoreCount}"/>

                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Clock Speed:"/>
                        <TextBlock Grid.Row="2" Grid.Column="1"
                                   Text="{x:Bind ViewModel.Cpu.ClockSpeedGHz}"/>

                        <Button Grid.Row="3" Grid.Column="1" Content="Test CPU"
                                Command="{x:Bind ViewModel.TestCpuCommand}"/>

                        <ProgressBar Grid.Row="4" Grid.ColumnSpan="2"
                                     Visibility="{x:Bind ViewModel.CpuTestRunning}"
                                     IsIndeterminate="True"/>
                    </Grid>
                </Expander>

                <!-- Memory -->
                <Expander Header="Memory (RAM)">
                    <!-- Similar structure -->
                </Expander>

                <!-- Storage -->
                <Expander Header="Storage">
                    <ListView ItemsSource="{x:Bind ViewModel.StorageDevices}">
                        <!-- Storage items -->
                    </ListView>
                </Expander>

                <!-- GPU -->
                <Expander Header="Graphics Card">
                    <!-- GPU info -->
                </Expander>

                <!-- Test Results -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16"
                        Visibility="{x:Bind ViewModel.ShowTestResults}">
                    <StackPanel Spacing="12">
                        <TextBlock Text="Test Results"
                                   Style="{StaticResource SubtitleTextBlockStyle}"/>

                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>

                            <StackPanel Grid.Column="0" Spacing="8">
                                <TextBlock Text="CPU Score:"/>
                                <TextBlock Text="{x:Bind ViewModel.CpuScore}"
                                           FontSize="24" FontWeight="Bold"/>
                            </StackPanel>

                            <StackPanel Grid.Column="1" Spacing="8">
                                <TextBlock Text="Memory Speed:"/>
                                <TextBlock Text="{x:Bind ViewModel.MemorySpeedMBps}"
                                           FontSize="24" FontWeight="Bold"/>
                            </StackPanel>
                        </Grid>
                    </StackPanel>
                </Border>

            </StackPanel>
        </ScrollViewer>
    </Grid>
</Page>
```

---

## AI-Powered Hardware Insights

```csharp
public class AIHardwareAnalyzer
{
    private readonly IAIProvider _aiProvider;

    public async Task<HardwareInsights> AnalyzeHardwareAsync(HardwareProfile profile)
    {
        var prompt = $@"
        Analyze this PC hardware configuration:

        CPU: {profile.Cpu.Name}
        - Cores: {profile.Cpu.NumberOfCores}
        - Speed: {profile.Cpu.MaxClockSpeed} MHz

        RAM: {profile.Memory.TotalGB} GB {profile.Memory.Type}

        Storage:
        {string.Join("\n", profile.Storage.Select(s =>
            $"- {s.Model} ({s.CapacityGB} GB, {(s.IsSSD ? "SSD" : "HDD")})"))}

        GPU: {profile.Gpus.FirstOrDefault()?.Name}

        Provide:
        1. Bottleneck analysis
        2. Upgrade recommendations
        3. Performance optimization tips
        4. Compatibility issues
        5. Overall system balance score (0-100)
        ";

        var response = await _aiProvider.CompleteAsync(prompt);
        return ParseHardwareInsights(response);
    }
}
```

---

This comprehensive hardware detection and testing module will make WinCheck stand out! 🚀
