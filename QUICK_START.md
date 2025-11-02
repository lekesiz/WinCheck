# WinCheck - Hızlı Başlangıç Kılavuzu

## ✅ Build Durumu

**Backend Services (Core + Infrastructure):** ✅ **100% Çalışıyor!**
- ✅ Tüm 9 servis production-ready
- ✅ NuGet paketleri yüklü (System.ServiceProcess.ServiceController eklendi)
- ⚠️ UI (WinUI App) - XAML düzeltmeleri devam ediyor

**Backend'i test etmek için Visual Studio'da Infrastructure projesini build edebilirsiniz!**

## Projeyi Açma ve Çalıştırma

### Gereksinimler
1. **Visual Studio 2022** (17.8 veya üzeri)
   - Workloads:
     - ✅ .NET Desktop Development
     - ✅ Windows application development

2. **Windows SDK** 10.0.22621.0 veya üzeri

3. **.NET 8.0 SDK**

### Adım 1: Proje Şablonlarını Yükle

Komut satırını (CMD veya PowerShell) açın ve şu komutu çalıştırın:

```bash
dotnet new install Microsoft.WindowsAppSDK.Templates
```

### Adım 2: Solution'ı Aç

1. Visual Studio 2022'yi açın
2. `File > Open > Project/Solution` seçin
3. `WinCheck.sln` dosyasını seçin

### Adım 3: NuGet Paketlerini Geri Yükle

Visual Studio'da:
1. `Tools > NuGet Package Manager > Package Manager Console`
2. Şu komutu çalıştırın:

```powershell
dotnet restore
```

### Adım 4: Build

1. Solution Configuration: **Debug** veya **Release**
2. Solution Platform: **x64** (önerilir)
3. `Build > Build Solution` (F7)

### Adım 5: Çalıştır

1. **Startup Project** olarak `WinCheck.App` seçin
2. **Administrator olarak çalıştırın** (sistem işlemleri için gerekli)
3. F5'e basın veya `Debug > Start Debugging`

## Proje Yapısı

```
WinCheck/
├── src/
│   ├── WinCheck.App/           # WinUI 3 UI Katmanı
│   │   ├── Views/              # XAML Sayfaları
│   │   ├── ViewModels/         # MVVM ViewModels
│   │   ├── Styles/             # Tema ve stiller
│   │   └── Assets/             # Görseller, iconlar
│   │
│   ├── WinCheck.Core/          # İş Mantığı
│   │   ├── Services/           # Business services
│   │   ├── Models/             # Veri modelleri
│   │   └── Interfaces/         # Service interfaces
│   │
│   ├── WinCheck.Infrastructure/# Platform Özgü
│   │   ├── Native/             # P/Invoke, Native API
│   │   └── Helpers/            # Yardımcı sınıflar
│   │
│   └── WinCheck.Tests/         # Unit testler
│
├── docs/                       # Dokümantasyon
│   ├── PROJECT_REPORT.md       # Proje raporu
│   └── ARCHITECTURE.md         # Teknik mimari
│
├── WinCheck.sln                # Visual Studio Solution
└── README.md                   # Ana dokümantasyon
```

## Geliştirme İpuçları

### 1. Administrator Privileges

Uygulama, sistem seviyesi işlemler için yönetici yetkileri gerektirir. Visual Studio'yu **Administrator olarak başlatın**.

### 2. Hot Reload

WinUI 3, XAML Hot Reload'u destekler. UI değişikliklerini görmek için uygulamayı yeniden başlatmanıza gerek yok.

### 3. Debugging

- **Breakpoint**: F9
- **Step Over**: F10
- **Step Into**: F11
- **Continue**: F5

### 4. XAML Designer

XAML dosyalarını açtığınızda designer otomatik yüklenir. Design/XAML görünümleri arasında geçiş yapabilirsiniz.

## Sık Karşılaşılan Sorunlar

### Sorun 1: "Windows SDK not found"

**Çözüm**: Visual Studio Installer'dan Windows SDK 10.0.22621.0 veya üzerini yükleyin.

### Sorun 2: "WinUI NuGet packages restore failed"

**Çözüm**:
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

### Sorun 3: "Administrator privileges required"

**Çözüm**: Visual Studio'yu sağ tıklayıp "Run as Administrator" ile başlatın.

### Sorun 4: Build hataları

**Çözüm**:
```bash
# Clean solution
dotnet clean

# Rebuild
dotnet build --no-incremental
```

## Sonraki Adımlar

1. ✅ Projeyi başarıyla çalıştırdınız
2. 📖 [README.md](README.md) dosyasını okuyun
3. 📊 [PROJECT_REPORT.md](docs/PROJECT_REPORT.md) - Detaylı proje bilgisi
4. 🏗️ [ARCHITECTURE.md](docs/ARCHITECTURE.md) - Teknik mimari
5. 💻 Kodlamaya başlayın!

## Yardım

- **Dokümantasyon**: `/docs` klasörü
- **Issues**: GitHub Issues
- **Community**: Discord/Slack (eklenecek)

## Lisans

MIT License - Detaylar için LICENSE dosyasına bakın.
