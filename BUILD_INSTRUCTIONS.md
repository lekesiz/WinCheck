# WinCheck - Build ve Çalıştırma Talimatları

## ✅ Hızlı Başlangıç

### 1. Visual Studio'da Aç
```
WinCheck.sln dosyasına çift tıklayın
VEYA
Visual Studio 2022'yi açın → File → Open → Project/Solution → WinCheck.sln seçin
```

### 2. Platform Ayarı
```
Üst toolbar'da:
- Solution Configurations: Debug
- Solution Platforms: x64  (ÖNEMLI: x64 seçili olmalı!)
```

### 3. Build Et
```
Menü: Build → Rebuild Solution
VEYA
Kısayol: Ctrl + Shift + B
```

### 4. Çalıştır
```
Menü: Debug → Start Debugging
VEYA
Kısayol: F5
```

---

## 🎯 İlk Çalıştırmada Yapılacaklar

### Adım 1: Settings'e Git
Uygulama açıldığında, sol menüden "Settings" sayfasına gidin.

### Adım 2: AI API Key Girin
Aşağıdaki AI servislerinden birinin API key'ini girin:

**OpenAI (Önerilen):**
- https://platform.openai.com/api-keys adresinden key alın
- Model: GPT-4
- Aylık $20'dan başlayan planlar

**Claude (Anthropic):**
- https://console.anthropic.com/ adresinden key alın
- Model: Claude 3 Sonnet
- Kullanım başına ödeme

**Gemini (Google):**
- https://makersuite.google.com/app/apikey adresinden key alın
- Model: Gemini Pro
- Ücretsiz tier mevcut

### Adım 3: API Key Doğrula
- API key'i girdikten sonra "Validate" butonuna tıklayın
- Yeşil onay işareti görmelisiniz

### Adım 4: Kaydet
- "Save Settings" butonuna tıklayın
- Settings `%LocalAppData%\WinCheck\settings.json` dosyasına kaydedilir

---

## 🚀 Özellikleri Test Etme

### Dashboard - AI Analizi
1. Dashboard sayfasına gidin
2. "Quick Scan" veya "Deep Scan" butonuna tıklayın
3. AI, sisteminizi analiz edecek ve sağlık skoru verecek

### Process Monitor
1. Process Monitor sayfasına gidin
2. "Start Monitoring" butonuna tıklayın
3. Şüpheli süreçler otomatik tespit edilir

### Service Optimizer
1. Service Optimizer sayfasına gidin
2. "Load Services" ile optimizasyon yapılabilecek servisleri görün
3. Güvenli servisleri devre dışı bırakabilirsiniz

### Disk Cleanup
1. Disk Cleanup sayfasına gidin
2. "Scan" ile temizlenebilir dosyaları tespit edin
3. "Clean" ile temizleme yapın

### Startup Manager
1. Startup Manager sayfasına gidin
2. Başlangıç programlarını görüntüleyin
3. Gereksiz programları devre dışı bırakın

### Registry Cleaner
1. Registry Cleaner sayfasına gidin
2. "Scan Registry" ile sorunları tespit edin
3. Otomatik yedekleme ile güvenle temizleyin

---

## ⚠️ Önemli Notlar

### Admin Yetkileri
Bazı özellikler admin yetkileri gerektirir:
- Registry temizleme
- Servis yönetimi
- Sistem dosyası analizi

**Çözüm:** Uygulamayı sağ tık → "Run as Administrator" ile çalıştırın

### İlk Build Süresi
İlk build 1-2 dakika sürebilir (NuGet paketleri indiriliyor)

### XAML Hataları
Eğer build hatası alırsanız:
1. Clean Solution (Build → Clean Solution)
2. Rebuild Solution (Build → Rebuild Solution)

---

## 📊 Beklenen Çıktılar

### Başarılı Build Çıktısı:
```
========== Rebuild All: 3 succeeded, 0 failed, 0 skipped ==========
```

### Çalışan Uygulama:
- Modern WinUI 3 arayüzü
- Sol tarafta navigation menu
- Dashboard'da sistem sağlık skoru
- Real-time CPU/Memory/Disk grafikleri

---

## 🐛 Sorun Giderme

### "Platform 'AnyCPU' is not supported"
**Çözüm:** Platform'u x64 olarak değiştirin

### "XAML Compiler error"
**Çözüm:**
```
1. Close Visual Studio
2. Delete src\WinCheck.App\obj and src\WinCheck.App\bin folders
3. Reopen Visual Studio
4. Rebuild Solution
```

### "AI Analysis failed"
**Çözüm:**
- API key'in doğru girildiğini kontrol edin
- İnternet bağlantınızı kontrol edin
- API key'in valid olduğunu doğrulayın

### "Service not found" DI hatası
**Çözüm:**
- App.xaml.cs dosyasında tüm servisler kayıtlı mı kontrol edin
- Solution'ı rebuild edin

---

## 📝 Geliştirici Notları

### Proje Yapısı
```
WinCheck/
├── src/
│   ├── WinCheck.App/          # WinUI 3 UI Layer
│   │   ├── Views/             # XAML Pages
│   │   └── ViewModels/        # View Models (MVVM)
│   ├── WinCheck.Core/         # Business Logic
│   │   ├── Interfaces/        # Service interfaces
│   │   └── Models/            # Data models
│   └── WinCheck.Infrastructure/ # Service implementations
│       ├── Services/          # 9 core services
│       └── AI/                # 3 AI providers
└── WinCheck.sln               # Solution file
```

### Dependency Injection
Tüm servisler App.xaml.cs içinde kayıtlı:
- Singleton: Servisler (state tutuyor)
- Transient: ViewModels (her sayfa yeni instance)

### AI Provider Değiştirme
Settings'ten provider değiştirildiğinde, uygulama restart gerektirir.

---

## 🎉 Başarılı Olunca

Uygulama çalıştığında:
1. Dashboard'da "Quick Scan" yapın
2. AI'ın sisteminizi analiz etmesini izleyin
3. Önerileri görüntüleyin
4. Güvenli optimizasyonları uygulayın

**İyi kullanımlar!** 🚀
