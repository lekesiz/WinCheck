# WinCheck - OS Detection & Adaptive Optimization

## 🪟 Operating System Detection

### Detection Capabilities

#### Windows Version Detection
- **Windows 11** (All builds: 21H2, 22H2, 23H2, 24H2)
- **Windows 10** (1507, 1511, 1607, 1703, 1709, 1803, 1809, 1903, 1909, 2004, 20H2, 21H1, 21H2, 22H2)
- **Windows 8.1**
- **Windows 8**
- **Windows 7** (SP1)
- **Windows Server** (2012, 2016, 2019, 2022)

#### Detection Metrics
- OS Name
- Version (Major.Minor.Build.Revision)
- Edition (Home, Pro, Enterprise, Education)
- Architecture (x86, x64, ARM64)
- Installation Date
- Last Update Date
- Product Key Status
- Activation Status
- System Locale
- Install Language

---

## Implementation

### OS Detection Service

```csharp
public class OSDetectionService : IOSDetectionService
{
    public async Task<OSInfo> DetectOperatingSystemAsync()
    {
        var osInfo = new OSInfo();

        // Use multiple methods for accurate detection
        osInfo.Version = GetOSVersion();
        osInfo.Edition = GetOSEdition();
        osInfo.Build = GetOSBuild();
        osInfo.Architecture = GetArchitecture();
        osInfo.ProductName = GetProductName();
        osInfo.DisplayVersion = GetDisplayVersion();

        // Windows 11 vs Windows 10 detection
        osInfo.IsWindows11 = IsWindows11();

        // Additional info
        osInfo.InstallDate = GetInstallDate();
        osInfo.LastUpdateDate = GetLastUpdateDate();
        osInfo.IsActivated = CheckActivationStatus();

        return osInfo;
    }

    private Version GetOSVersion()
    {
        // Environment.OSVersion can be unreliable due to compatibility mode
        // Use RtlGetVersion instead

        var versionInfo = new OSVERSIONINFOEX();
        versionInfo.dwOSVersionInfoSize = Marshal.SizeOf(versionInfo);

        RtlGetVersion(ref versionInfo);

        return new Version(
            versionInfo.dwMajorVersion,
            versionInfo.dwMinorVersion,
            versionInfo.dwBuildNumber
        );
    }

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct OSVERSIONINFOEX
    {
        public int dwOSVersionInfoSize;
        public int dwMajorVersion;
        public int dwMinorVersion;
        public int dwBuildNumber;
        public int dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
        public ushort wServicePackMajor;
        public ushort wServicePackMinor;
        public ushort wSuiteMask;
        public byte wProductType;
        public byte wReserved;
    }

    private bool IsWindows11()
    {
        var version = GetOSVersion();

        // Windows 11 = Build 22000+
        return version.Build >= 22000;
    }

    private string GetOSBuild()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion"
        );

        // UBR = Update Build Revision
        var build = key?.GetValue("CurrentBuild")?.ToString();
        var ubr = key?.GetValue("UBR")?.ToString();

        return $"{build}.{ubr}";
    }

    private string GetDisplayVersion()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion"
        );

        // Windows 11: 23H2, 22H2, 21H2
        // Windows 10: 22H2, 21H2, 21H1, etc.
        return key?.GetValue("DisplayVersion")?.ToString() ?? "Unknown";
    }

    private string GetOSEdition()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion"
        );

        var edition = key?.GetValue("EditionID")?.ToString();

        return edition switch
        {
            "Core" => "Home",
            "Professional" => "Pro",
            "Enterprise" => "Enterprise",
            "Education" => "Education",
            "ServerStandard" => "Server Standard",
            "ServerDatacenter" => "Server Datacenter",
            _ => edition ?? "Unknown"
        };
    }

    private Architecture GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture;
    }

    private DateTime GetInstallDate()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion"
        );

        var installDate = key?.GetValue("InstallDate");
        if (installDate is int unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }

        return DateTime.MinValue;
    }

    private bool CheckActivationStatus()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT * FROM SoftwareLicensingProduct WHERE PartialProductKey <> null"
            );

            foreach (ManagementObject obj in searcher.Get())
            {
                var licenseStatus = Convert.ToInt32(obj["LicenseStatus"]);
                // 1 = Licensed
                if (licenseStatus == 1)
                    return true;
            }
        }
        catch { }

        return false;
    }
}
```

---

## Adaptive Optimization Based on OS Version

### Windows 11-Specific Optimizations

```csharp
public class Windows11Optimizer
{
    public async Task<List<OptimizationAction>> GetWindows11OptimizationsAsync()
    {
        var optimizations = new List<OptimizationAction>();

        // 1. Disable Windows 11 widgets (if not used)
        optimizations.Add(new OptimizationAction
        {
            Name = "Disable Widgets",
            Description = "Windows 11 widgets can consume resources",
            Category = OptimizationCategory.Performance,
            Impact = Impact.Medium,
            Action = () => DisableWidgets()
        });

        // 2. Optimize new Start Menu
        optimizations.Add(new OptimizationAction
        {
            Name = "Optimize Start Menu",
            Description = "Reduce Start Menu recommendations",
            Category = OptimizationCategory.Privacy,
            Impact = Impact.Low,
            Action = () => OptimizeStartMenu()
        });

        // 3. Disable Chat (if not used)
        optimizations.Add(new OptimizationAction
        {
            Name = "Disable Chat Icon",
            Description = "Remove Teams Chat integration",
            Category = OptimizationCategory.Performance,
            Impact = Impact.Low,
            Action = () => DisableChat()
        });

        // 4. Optimize Snap Layouts
        optimizations.Add(new OptimizationAction
        {
            Name = "Configure Snap Layouts",
            Description = "Optimize window snapping behavior",
            Category = OptimizationCategory.Usability,
            Impact = Impact.Low,
            Action = () => ConfigureSnapLayouts()
        });

        // 5. DirectStorage optimization (for gaming)
        if (HasNVMeSSD())
        {
            optimizations.Add(new OptimizationAction
            {
                Name = "Enable DirectStorage",
                Description = "Optimize for DirectStorage (Gaming)",
                Category = OptimizationCategory.Gaming,
                Impact = Impact.High,
                Action = () => EnableDirectStorage()
            });
        }

        return optimizations;
    }

    private async Task DisableWidgets()
    {
        // Registry key for widgets
        using var key = Registry.CurrentUser.CreateSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
        );

        key?.SetValue("TaskbarDa", 0, RegistryValueKind.DWord);
    }

    private async Task DisableChat()
    {
        using var key = Registry.CurrentUser.CreateSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
        );

        key?.SetValue("TaskbarMn", 0, RegistryValueKind.DWord);
    }
}
```

### Windows 10-Specific Optimizations

```csharp
public class Windows10Optimizer
{
    public async Task<List<OptimizationAction>> GetWindows10OptimizationsAsync(string buildVersion)
    {
        var optimizations = new List<OptimizationAction>();

        // 1. Disable Cortana (older builds)
        if (IsBuildBefore("1903", buildVersion))
        {
            optimizations.Add(new OptimizationAction
            {
                Name = "Disable Cortana",
                Description = "Cortana can be disabled in older Windows 10 builds",
                Category = OptimizationCategory.Privacy,
                Impact = Impact.Medium,
                Action = () => DisableCortana()
            });
        }

        // 2. Optimize Timeline (deprecated in newer builds)
        if (IsBuildBetween("1803", "21H2", buildVersion))
        {
            optimizations.Add(new OptimizationAction
            {
                Name = "Disable Timeline",
                Description = "Timeline feature uses resources",
                Category = OptimizationCategory.Performance,
                Impact = Impact.Low,
                Action = () => DisableTimeline()
            });
        }

        // 3. Disable People bar (older UI)
        optimizations.Add(new OptimizationAction
        {
            Name = "Disable People Bar",
            Description = "Remove People icon from taskbar",
            Category = OptimizationCategory.Usability,
            Impact = Impact.Low,
            Action = () => DisablePeopleBar()
        });

        // 4. Optimize Search Indexing
        optimizations.Add(new OptimizationAction
        {
            Name = "Optimize Search Indexing",
            Description = "Configure Windows Search for better performance",
            Category = OptimizationCategory.Performance,
            Impact = Impact.Medium,
            Action = () => OptimizeSearchIndexing()
        });

        return optimizations;
    }
}
```

### Windows 7 Legacy Support

```csharp
public class Windows7Optimizer
{
    public async Task<List<OptimizationAction>> GetWindows7OptimizationsAsync()
    {
        var optimizations = new List<OptimizationAction>();

        // Warning: Windows 7 is EOL
        optimizations.Add(new OptimizationAction
        {
            Name = "Security Warning",
            Description = "Windows 7 is no longer supported. Please upgrade to Windows 10/11.",
            Category = OptimizationCategory.Security,
            Impact = Impact.Critical,
            Action = () => ShowUpgradeWarning()
        });

        // Legacy optimizations
        optimizations.Add(new OptimizationAction
        {
            Name = "Disable Aero Glass",
            Description = "Disable visual effects for performance",
            Category = OptimizationCategory.Performance,
            Impact = Impact.Medium,
            Action = () => DisableAeroGlass()
        });

        optimizations.Add(new OptimizationAction
        {
            Name = "Disable Windows Search",
            Description = "Windows Search can be slow on older hardware",
            Category = OptimizationCategory.Performance,
            Impact = Impact.High,
            Action = () => DisableWindowsSearch()
        });

        return optimizations;
    }
}
```

---

## Adaptive Service Management

### Service Recommendations by OS Version

```csharp
public class AdaptiveServiceManager
{
    public async Task<List<ServiceRecommendation>> GetOSSpecificRecommendationsAsync(
        OSInfo osInfo)
    {
        var recommendations = new List<ServiceRecommendation>();

        if (osInfo.IsWindows11)
        {
            recommendations.AddRange(GetWindows11Services());
        }
        else if (osInfo.Version.Major == 10)
        {
            recommendations.AddRange(GetWindows10Services(osInfo.Build));
        }
        else if (osInfo.Version.Major == 6 && osInfo.Version.Minor == 1)
        {
            // Windows 7
            recommendations.AddRange(GetWindows7Services());
        }

        return recommendations;
    }

    private List<ServiceRecommendation> GetWindows11Services()
    {
        return new List<ServiceRecommendation>
        {
            new()
            {
                ServiceName = "WpnService",
                DisplayName = "Windows Push Notifications System Service",
                RecommendedStartMode = ServiceStartMode.Manual,
                Reason = "Only needed for notifications. Can be manual.",
                SafetyLevel = ServiceSafetyLevel.Safe
            },
            new()
            {
                ServiceName = "WSearch",
                DisplayName = "Windows Search",
                RecommendedStartMode = ServiceStartMode.Manual,
                Reason = "Windows 11 search can be sluggish. Manual start recommended.",
                SafetyLevel = ServiceSafetyLevel.Conditional
            }
        };
    }

    private List<ServiceRecommendation> GetWindows10Services(int build)
    {
        var services = new List<ServiceRecommendation>
        {
            new()
            {
                ServiceName = "DiagTrack",
                DisplayName = "Connected User Experiences and Telemetry",
                RecommendedStartMode = ServiceStartMode.Disabled,
                Reason = "Telemetry service. Safe to disable for privacy.",
                SafetyLevel = ServiceSafetyLevel.Recommended
            }
        };

        // Build-specific services
        if (build >= 18362) // 1903+
        {
            services.Add(new()
            {
                ServiceName = "CDPUserSvc",
                DisplayName = "Connected Devices Platform User Service",
                RecommendedStartMode = ServiceStartMode.Manual,
                Reason = "Only needed if using cross-device features",
                SafetyLevel = ServiceSafetyLevel.Conditional
            });
        }

        return services;
    }
}
```

---

## Feature Detection & Compatibility

```csharp
public class FeatureDetectionService
{
    public async Task<FeatureAvailability> DetectAvailableFeaturesAsync(OSInfo osInfo)
    {
        var features = new FeatureAvailability();

        // Windows 11 features
        if (osInfo.IsWindows11)
        {
            features.SupportsSnapLayouts = true;
            features.SupportsDirectStorage = HasNVMeSSD();
            features.SupportsAutoHDR = HasHDRCapableDisplay();
            features.SupportsAndroidApps = osInfo.Build >= 22000; // WSA
        }

        // Windows 10 features
        if (osInfo.Version.Major == 10)
        {
            features.SupportsWSL2 = osInfo.Build >= 18362; // 1903+
            features.SupportsSandbox = osInfo.Edition == "Pro" || osInfo.Edition == "Enterprise";
            features.SupportsGameMode = osInfo.Build >= 15063; // Creators Update+
        }

        // General features
        features.SupportsVirtualization = CheckVirtualizationSupport();
        features.SupportsBitLocker = osInfo.Edition != "Home";
        features.SupportsHyperV = osInfo.Edition == "Pro" || osInfo.Edition == "Enterprise";

        return features;
    }

    [DllImport("kernel32.dll")]
    private static extern bool IsProcessorFeaturePresent(int feature);

    private bool CheckVirtualizationSupport()
    {
        // PF_VIRT_FIRMWARE_ENABLED = 21
        return IsProcessorFeaturePresent(21);
    }
}
```

---

## AI-Powered OS-Specific Recommendations

```csharp
public class AIAdaptiveOptimizer
{
    private readonly IAIProvider _aiProvider;

    public async Task<OSOptimizationPlan> GenerateOptimizationPlanAsync(
        OSInfo osInfo,
        HardwareProfile hardware,
        UserUsageProfile usage)
    {
        var prompt = $@"
        Generate optimization recommendations for:

        OS: Windows {(osInfo.IsWindows11 ? "11" : "10")} {osInfo.DisplayVersion}
        Edition: {osInfo.Edition}
        Build: {osInfo.Build}
        Architecture: {osInfo.Architecture}

        Hardware:
        - CPU: {hardware.Cpu.Name}
        - RAM: {hardware.Memory.TotalGB} GB
        - Storage: {string.Join(", ", hardware.Storage.Select(s => s.IsSSD ? "SSD" : "HDD"))}

        Usage Pattern:
        - Primary Use: {usage.PrimaryUse}
        - Gaming: {usage.GamingHours}h/week
        - Work: {usage.WorkHours}h/week

        Provide:
        1. OS-specific optimizations (considering version and build)
        2. Services safe to disable
        3. Privacy enhancements
        4. Performance tweaks
        5. Feature recommendations

        Prioritize by impact and safety.
        ";

        var response = await _aiProvider.CompleteAsync(prompt);
        return ParseOptimizationPlan(response);
    }
}
```

---

## UI: OS Detection Display

```xml
<StackPanel Spacing="16">
    <TextBlock Text="Operating System" Style="{StaticResource TitleTextBlockStyle}"/>

    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="16">
        <Grid RowSpacing="12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="150"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <TextBlock Grid.Row="0" Grid.Column="0" Text="OS Name:"/>
            <TextBlock Grid.Row="0" Grid.Column="1"
                       Text="{x:Bind ViewModel.OS.ProductName}"
                       FontWeight="SemiBold"/>

            <TextBlock Grid.Row="1" Grid.Column="0" Text="Version:"/>
            <TextBlock Grid.Row="1" Grid.Column="1"
                       Text="{x:Bind ViewModel.OS.DisplayVersion}"/>

            <TextBlock Grid.Row="2" Grid.Column="0" Text="Build:"/>
            <TextBlock Grid.Row="2" Grid.Column="1"
                       Text="{x:Bind ViewModel.OS.Build}"/>

            <TextBlock Grid.Row="3" Grid.Column="0" Text="Edition:"/>
            <TextBlock Grid.Row="3" Grid.Column="1"
                       Text="{x:Bind ViewModel.OS.Edition}"/>

            <TextBlock Grid.Row="4" Grid.Column="0" Text="Architecture:"/>
            <TextBlock Grid.Row="4" Grid.Column="1"
                       Text="{x:Bind ViewModel.OS.Architecture}"/>

            <TextBlock Grid.Row="5" Grid.Column="0" Text="Activated:"/>
            <StackPanel Grid.Row="5" Grid.Column="1" Orientation="Horizontal" Spacing="8">
                <FontIcon Glyph="{x:Bind ViewModel.OS.ActivationIcon}"
                          Foreground="{x:Bind ViewModel.OS.ActivationColor}"/>
                <TextBlock Text="{x:Bind ViewModel.OS.ActivationStatus}"/>
            </StackPanel>
        </Grid>
    </Border>

    <!-- OS-Specific Recommendations -->
    <TextBlock Text="OS-Specific Optimizations"
               Style="{StaticResource SubtitleTextBlockStyle}"/>

    <ItemsControl ItemsSource="{x:Bind ViewModel.OSOptimizations}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Border Background="{ThemeResource CardBackgroundFillColorSecondaryBrush}"
                        Padding="12"
                        Margin="0,0,0,8"
                        CornerRadius="4">
                    <StackPanel Spacing="8">
                        <TextBlock Text="{Binding Name}" FontWeight="SemiBold"/>
                        <TextBlock Text="{Binding Description}"
                                   TextWrapping="Wrap"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        <Button Content="Apply" Command="{Binding ApplyCommand}"/>
                    </StackPanel>
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</StackPanel>
```

---

## Version-Specific Registry Tweaks

```csharp
public class VersionSpecificTweaks
{
    public async Task ApplyTweaksAsync(OSInfo osInfo)
    {
        if (osInfo.IsWindows11)
        {
            await ApplyWindows11TweaksAsync();
        }
        else if (osInfo.Version.Major == 10)
        {
            await ApplyWindows10TweaksAsync(osInfo.Build);
        }
    }

    private async Task ApplyWindows11TweaksAsync()
    {
        // Restore classic context menu (if user prefers)
        using var key = Registry.ClassesRoot.CreateSubKey(
            @"CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"
        );
        key?.SetValue("", "", RegistryValueKind.String);

        // Disable taskbar auto-hide delay
        using var taskbarKey = Registry.CurrentUser.CreateSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
        );
        taskbarKey?.SetValue("TaskbarAutoHideDelay", 0, RegistryValueKind.DWord);
    }

    private async Task ApplyWindows10TweaksAsync(int build)
    {
        // Build-specific tweaks
        if (build >= 17134) // 1803+
        {
            // Disable timeline
            using var key = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
            );
            key?.SetValue("ShowTaskViewButton", 0, RegistryValueKind.DWord);
        }

        // Optimize taskbar
        using var taskbarKey = Registry.CurrentUser.CreateSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
        );
        taskbarKey?.SetValue("TaskbarSmallIcons", 1, RegistryValueKind.DWord);
    }
}
```

---

This makes WinCheck truly adaptive to any Windows version! 🎯
