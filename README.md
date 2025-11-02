# WinCheck - Windows Sistem Bakım ve Optimizasyon Aracı

## Genel Bakış

WinCheck, Windows 10/11 işletim sistemleri için geliştirilmiş modern, yüksek performanslı bir sistem bakım ve optimizasyon aracıdır. WinUI 3 ve Windows App SDK kullanılarak geliştirilmiştir.

## Teknik Özellikler

### Platform ve Framework
- **Framework**: .NET 8.0 (LTS)
- **UI Framework**: WinUI 3 (Windows App SDK 1.5+)
- **Dil**: C# 12.0
- **Minimum Gereksinim**: Windows 10 version 1809 (build 17763) veya üzeri
- **Önerilen**: Windows 11 22H2 veya üzeri

### Mimari Tasarım
- **Tasarım Deseni**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Asenkron İşlemler**: Task-based Asynchronous Pattern (TAP)
- **Yapı**: Modüler, loosely-coupled architecture

## Temel Özellikler

### 1. Disk Analiz ve Temizleme Modülü
- **Geçici Dosya Temizliği**
  - Windows Temp klasörleri
  - Browser cache (Edge, Chrome, Firefox)
  - .NET Temp dosyaları
  - Thumbnail cache

- **Sistem Dosya Analizi**
  - Disk kullanım haritası (TreeMap görselleştirme)
  - Büyük dosya tespiti
  - Duplicate dosya bulma (hash-based)

### 2. Registry Optimizasyonu
- **Güvenli Registry Temizliği**
  - Geçersiz uygulama referansları
  - Eski MUI cache kayıtları
  - Kullanılmayan file extensions
  - Otomatik yedekleme sistemi

### 3. Başlangıç Programları Yönetimi
- Task Scheduler analizi
- Registry Run keys kontrolü
- Services yönetimi
- Performans etkisi gösterimi

### 4. Sistem Durumu Analizi
- **SMART Disk Kontrolü**
- **RAM Testi** (Windows Memory Diagnostic API)
- **Sistem Dosya Bütünlüğü** (SFC/DISM entegrasyonu)
- **Driver Güncellik Kontrolü**

### 5. Gizlilik ve Güvenlik
- Windows telemetry ayarları
- Gereksiz servis devre dışı bırakma
- Kullanılmayan network protokolleri temizleme

### 6. Performans İzleme
- Real-time CPU, RAM, Disk kullanımı
- Arka plan işlem analizi
- Sistem kaynak geçmişi (grafikler)

## Teknoloji Yığını

### Core Teknolojiler
```
- .NET 8.0 SDK
- WinUI 3 (Microsoft.WindowsAppSDK)
- Windows Implementation Libraries (WIL)
- C++/WinRT (kritik sistem işlemleri için)
```

### NuGet Paketleri
```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.5.*" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.*" />
<PackageReference Include="CommunityToolkit.WinUI.UI.Controls" Version="7.1.*" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.*" />
```

### Modern UI Bileşenleri
- **WinUI 3 Controls**: NavigationView, InfoBar, TeachingTip
- **Community Toolkit**: DataGrid, TokenizingTextBox, SettingsCard
- **Fluent Design System**: Acrylic, Reveal, Shadow effects
- **Animations**: Composition API ile smooth transitions

## Proje Yapısı

```
WinCheck/
├── src/
│   ├── WinCheck.App/                 # WinUI 3 Application
│   │   ├── Views/                    # XAML Pages
│   │   ├── ViewModels/               # View Models
│   │   ├── Converters/               # Value Converters
│   │   ├── Styles/                   # Resource Dictionaries
│   │   └── Assets/                   # Images, Icons
│   │
│   ├── WinCheck.Core/                # Business Logic
│   │   ├── Services/
│   │   │   ├── DiskCleanupService.cs
│   │   │   ├── RegistryService.cs
│   │   │   ├── StartupManagerService.cs
│   │   │   └── SystemAnalyzerService.cs
│   │   ├── Models/
│   │   └── Interfaces/
│   │
│   ├── WinCheck.Infrastructure/      # Platform Specific
│   │   ├── Native/                   # P/Invoke declarations
│   │   ├── Helpers/
│   │   └── Extensions/
│   │
│   └── WinCheck.Tests/               # Unit & Integration Tests
│       ├── Services/
│       └── Helpers/
│
├── docs/
│   ├── ARCHITECTURE.md
│   ├── API_REFERENCE.md
│   └── USER_GUIDE.md
│
├── WinCheck.sln
└── README.md
```

## Kurulum ve Geliştirme

### Gereksinimler
1. Visual Studio 2022 (17.8 veya üzeri)
   - Workloads: ".NET Desktop Development", "Windows application development"
2. Windows SDK 10.0.22621.0 veya üzeri
3. .NET 8.0 SDK

### Proje Oluşturma
```bash
# WinUI 3 şablonunu yükle
dotnet new install Microsoft.WindowsAppSDK.Templates

# Proje oluştur
dotnet new winui -n WinCheck.App -o src/WinCheck.App

# Class library'ler oluştur
dotnet new classlib -n WinCheck.Core -o src/WinCheck.Core -f net8.0-windows10.0.22621.0
dotnet new classlib -n WinCheck.Infrastructure -o src/WinCheck.Infrastructure -f net8.0-windows10.0.22621.0

# Test projesi
dotnet new mstest -n WinCheck.Tests -o src/WinCheck.Tests
```

### Build ve Run
```bash
cd src/WinCheck.App
dotnet restore
dotnet build -c Release
dotnet run
```

## Güvenlik ve İzinler

### Gerekli Yetkiler
- **Administrator Privileges**: Registry ve sistem dosyaları için
- **Restricted Capabilities**:
  ```xml
  <Capability Name="runFullTrust" />
  <rescap:Capability Name="confirmAppClose" />
  ```

### Güvenlik Önlemleri
- Tüm sistem değişiklikleri öncesi otomatik yedekleme
- User Account Control (UAC) entegrasyonu
- İşlem öncesi onay mekanizması
- Rollback capability

## Performans Optimizasyonları

1. **Asenkron I/O**: Tüm disk işlemleri async
2. **Parallel Processing**: PLINQ ile çoklu dosya analizi
3. **Memory Management**: Span<T>, Memory<T> kullanımı
4. **Lazy Loading**: UI bileşenlerinde deferred loading
5. **Caching**: Sık kullanılan sistem bilgileri cache'lenir

## Lokalizasyon
- **ResX** dosyaları ile çoklu dil desteği
- Desteklenen diller: TR, EN, DE, FR, ES
- RTL (Right-to-Left) dil desteği

## Dağıtım

### MSIX Packaging
```xml
<PropertyGroup>
  <WindowsPackageType>MSIX</WindowsPackageType>
  <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
  <PublishProfile>win-x64</PublishProfile>
</PropertyGroup>
```

### Dağıtım Kanalları
1. **Microsoft Store** (önerilen)
2. **GitHub Releases** (sideload)
3. **Enterprise Deployment** (Intune, SCCM)

## Lisans
MIT License

## Katkıda Bulunma
Contributions are welcome! Please read CONTRIBUTING.md

## Yol Haritası

### v1.0 (Q1 2025)
- ✓ Temel disk temizleme
- ✓ Registry optimizasyonu
- ✓ Başlangıç yönetimi

### v1.5 (Q2 2025)
- Scheduled tasks
- Cloud backup integration
- Advanced analytics dashboard

### v2.0 (Q3 2025)
- AI-powered optimization suggestions
- Network performance optimization
- Multi-PC management (Enterprise)

## İletişim ve Destek
- GitHub Issues: [project-url]/issues
- Email: support@wincheck.app
- Documentation: https://docs.wincheck.app
