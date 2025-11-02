# WinCheck Ekip El Kitabı

**Versiyon**: 1.0
**Son Güncelleme**: Kasım 2025
**Hedef Kitle**: Tüm ekip üyeleri

---

## Hoş Geldiniz! 👋

WinCheck ekibine hoş geldiniz! Bu el kitabı, ekip olarak nasıl çalıştığımızı, değerlerimizi, ve günlük işleyişimizi açıklar.

---

## 📋 İçindekiler

1. [Ekip Değerleri ve Kültür](#ekip-değerleri-ve-kültür)
2. [İletişim Kuralları](#iletişim-kuralları)
3. [Çalışma Saatleri ve Esneklik](#çalışma-saatleri-ve-esneklik)
4. [Toplantı Kültürü](#toplantı-kültürü)
5. [Karar Alma Süreci](#karar-alma-süreci)
6. [Kod ve Tasarım Standartları](#kod-ve-tasarım-standartları)
7. [Sorun Çözme ve Destek](#sorun-çözme-ve-destek)
8. [İyi Pratikler](#iyi-pratikler)
9. [Sık Sorulan Sorular](#sık-sorulan-sorular)

---

## 1. Ekip Değerleri ve Kültür

### 1.1 Temel Değerlerimiz

#### 🎯 **Kalite Öncelikli**
- Code quality > feature quantity
- "Bu kodu 6 ay sonra anlayabilir miyim?" sorusunu sorun
- Technical debt'e izin vermiyoruz (veya hemen ödüyoruz)

#### 🤝 **Şeffaflık ve İletişim**
- Sorunları saklama, erken paylaş
- "Bilmiyorum" demek güçlülük işaretidir
- Over-communicate > under-communicate

#### 📚 **Sürekli Öğrenme**
- Her gün biraz daha iyi
- Hatalardan öğreniriz
- Knowledge sharing teşvik edilir

#### ⚡ **Hız ve İterasyon**
- Perfect > good enough > nothing
- Ship fast, iterate faster
- Fail fast, learn faster

#### 🌟 **Ownership ve Sorumluluk**
- "Bu benim problemim" zihniyeti
- Takım başarısı = bireysel başarı
- Blame culture yok, solution culture var

### 1.2 Ekip Davranış Kuralları

**DO ✅:**
- Sorularını sor (aptal soru yoktur)
- Yardım iste ve yardım et
- Code review'lerde yapıcı ol
- Farklı fikirlere açık ol
- Hata yaptığında kabul et
- Başarıları kutla (küçük zaferler dahil)
- İyileştirme önerileri getir

**DON'T ❌:**
- Başkalarını suçlama
- Passive-aggressive olmak
- Silent disagreement (anlaşmadıysan konuş)
- Last-minute surprises
- Micromanagement
- Hero culture (tek başına kurtarıcı olmaya çalışma)

---

## 2. İletişim Kuralları

### 2.1 İletişim Kanalları

| Kanal | Ne Zaman Kullan | Response Time |
|-------|-----------------|---------------|
| **Slack/Teams (Urgent)** | Blocker, critical bug | < 30 dakika |
| **Slack/Teams (General)** | Sorular, tartışmalar | < 2 saat |
| **Email** | Formal communication, weekly reports | < 24 saat |
| **GitHub/Azure DevOps** | Code review, technical discussion | < 1 gün |
| **Video Call** | Karmaşık konular, pair programming | Anlaşmalı |
| **Daily Standup** | Status updates | Her gün 09:30 |

### 2.2 Senkron vs Asenkron İletişim

**Senkron (Hemen Cevap Gerekli):**
- Production down
- Blocker issue
- Security incident
- Time-sensitive decisions

**Asenkron (Zaman Toleransı Var):**
- Code review
- Design feedback
- Documentation questions
- Non-critical bugs

### 2.3 Mesaj Yazma İyi Pratikleri

**Kötü Örnek:**
```
"Kod çalışmıyor, yardım edin!"
```

**İyi Örnek:**
```
"Process Monitor'da bir sorun var. CPU metriklerini alırken
NullReferenceException alıyorum (line 142). Geçici olarak
null check ekledim ama root cause'u anlamadım.
30 dakika içinde bakabilecek var mı?"

Context:
- Branch: feature/process-monitor
- File: ProcessMonitorService.cs:142
- Error: NullReferenceException
- Steps to reproduce: ...
```

**Neden İyi:**
- Problem tanımı net
- Context verilmiş
- Denemeler paylaşılmış
- Urgency level belirtilmiş
- Actionable

### 2.4 Slack/Teams Channel Yapısı

**Channels:**
- `#general` - Genel duyurular, team chat
- `#dev` - Development discussions
- `#bugs` - Bug reports ve tracking
- `#code-review` - Pull request notifications
- `#design` - Design feedback
- `#random` - Non-work chat, fun
- `#standup` - Daily standup (async option)

**Etiquette:**
- @channel/@here dikkatli kullan (gerçekten herkese gerekiyorsa)
- Thread kullan (conversation'ları organize etmek için)
- Emoji reactions kullan (acknowledge için)
- Status messages güncel tut (away, lunch, focus mode)

---

## 3. Çalışma Saatleri ve Esneklik

### 3.1 Standart Çalışma Saatleri

**Core Hours** (herkeslocker available):
- **10:00 - 16:00** (Pazartesi-Cuma)
- Daily standup: 09:30 (mutlaka katılım)

**Flexible Hours:**
- Erken başlayanlar: 08:00-17:00
- Geç başlayanlar: 10:00-19:00
- Core hours'ı koruyun

**Remote/Hybrid:**
- Fully remote OK
- Office (if available): Tercihe bağlı
- İletişimde kalmak önemli

### 3.2 İzin ve Devamsızlık

**Planlı İzin:**
1. Scrum Master'a minimum 1 hafta önce bildir
2. Azure DevOps'ta takvim güncelle
3. Slack/Teams status'unu güncelle
4. Ongoing tasks'ları transition planning yap

**Acil Durum:**
1. Hemen Scrum Master'ı bilgilendir
2. Ongoing critical tasks varsa delegation

**Hastalık:**
- Hasta oldun → dinlen, çalışma
- Bildirimi yeter, detay paylaşmak zorunda değilsin

### 3.3 Odaklanma Zamanı (Focus Time)

**Focus Blocks:**
- Deep work için 2-4 saatlik kesintisiz bloklar
- Calendar'da "Focus Time" bloklarını işaretle
- Bu süreler de slack/teams notification'ları sustur
- Acil durumlar hariç interrupt etme

**Pomodoro Technique (Önerilen):**
- 25 dk fokus + 5 dk mola
- 4 Pomodoro sonrası 15-30 dk uzun mola

---

## 4. Toplantı Kültürü

### 4.1 Toplantı Prensipleri

**Before Meeting:**
- [ ] Agenda hazır (en az 24h önce)
- [ ] Materials paylaşılmış (review için)
- [ ] Participants doğru seçilmiş (gerekli kişiler)
- [ ] Time-boxed (net başlangıç ve bitiş)

**During Meeting:**
- [ ] Zamanında başla (5 dk tolerance max)
- [ ] Note-taker belirle
- [ ] Time-box'a uy
- [ ] Action items document et
- [ ] Kamera açık ol (if video call)

**After Meeting:**
- [ ] Meeting notes paylaş (1 saat içinde)
- [ ] Action items assign et (owner + deadline)
- [ ] Follow-up schedule et (if needed)

### 4.2 Daily Standup (15 dakika)

**Format:**
1. **Dün ne yaptım?** (1-2 dakika/kişi)
2. **Bugün ne yapacağım?**
3. **Herhangi bir blocker var mı?**

**DO:**
- Kısa ve öz ol (<2 dakika)
- Blocker'ları early raise et
- Commitment'larını update et

**DON'T:**
- Detaylı technical discussion (standup sonrası)
- Uzun explanations
- Blame or excuses

**Async Standup Option:**
- #standup channel'da sabah mesajı
- Template:
  ```
  Yesterday: Implemented Process Monitor UI
  Today: Add unit tests, start code review
  Blockers: None
  ```

### 4.3 Sprint Ceremonies

**Sprint Planning (2 saat - Her 2 haftada)**
- **Ne?** Next sprint için story selection
- **Kim?** Tüm ekip
- **Output:** Sprint backlog, sprint goal

**Sprint Review (1 saat - Her 2 haftada)**
- **Ne?** Sprint deliverable'ları demo
- **Kim?** Tüm ekip + stakeholders
- **Output:** Feedback, backlog updates

**Sprint Retrospective (1 saat - Her 2 haftada)**
- **Ne?** Process improvement
- **Kim?** Dev ekibi (no stakeholders)
- **Format:** Start-Stop-Continue
- **Output:** Action items

**Backlog Refinement (1 saat - Haftalık)**
- **Ne?** Upcoming stories grooming
- **Kim?** PO, Tech Lead, Scrum Master
- **Output:** Refined stories, estimates

### 4.4 Toplantı-Sız Günler

**"Maker Time" Days:**
- **Çarşamba:** Toplantı-sız gün (acil durumlar hariç)
- Deep work için rezerve
- Focus Time maksimize

---

## 5. Karar Alma Süreci

### 5.1 Karar Seviyeleri

#### Level 1: Individual (Bireysel Kararlar)
- **Kim?** Individual developer/designer
- **Ne?** Implementation details, tool choices
- **Örnek:** "Bu fonksiyonu LINQ ile mi yazsam foreach ile mi?"
- **Process:** Kendi karar ver, code review'da discuss

#### Level 2: Team (Ekip Kararları)
- **Kim?** Dev ekibi consensus
- **Ne?** Technical approach, design patterns
- **Örnek:** "State management için hangi library?"
- **Process:** Quick discussion, vote if needed

#### Level 3: Tech Lead (Mimari Kararlar)
- **Kim?** Technical Lead (consult with team)
- **Ne?** Architecture, major technical decisions
- **Örnek:** "Service architecture nasıl olacak?"
- **Process:** Proposal, team feedback, Tech Lead decides

#### Level 4: Product Owner (Feature Kararları)
- **Kim?** Product Owner
- **Ne?** Feature prioritization, scope
- **Örnek:** "Registry cleaner v1.0'da olacak mı?"
- **Process:** PO decides (market/business driven)

#### Level 5: Stakeholder (Stratejik Kararlar)
- **Kim?** Project Sponsor/Executives
- **Ne?** Budget, timeline, go/no-go
- **Örnek:** "Projeyi 2 hafta erteleyebiir miyiz?"
- **Process:** Formal proposal, stakeholder approval

### 5.2 Anlaşmazlık Çözme

**Adım 1: Direct Discussion**
- İki taraf konuşur, consensus bulmaya çalışır

**Adım 2: Team Discussion**
- Standup veya dedicated meeting'de tartış
- Pros/cons list
- Vote (if needed)

**Adım 3: Tech Lead Decision**
- Technical Lead'in final decision'ı
- Rationale document edilir

**Adım 4: Escalation**
- Hala çözülmediyse Scrum Master/PO devreye girer

**Disagree and Commit:**
- Karar alındıktan sonra herkes commit eder
- "Ben katılmadım ama kararı destekliyorum"

---

## 6. Kod ve Tasarım Standartları

### 6.1 Code Review Prensipler

**Review Checklist:**
- [ ] Code compiles
- [ ] Tests pass
- [ ] Code style guide'a uygun
- [ ] No code smells
- [ ] Proper error handling
- [ ] Documentation updated
- [ ] Performance considerations

**Review Etiquette:**

**Reviewer (DO):**
- ✅ Constructive feedback ver
- ✅ "Why" açıkla
- ✅ Alternatif solutions öner
- ✅ Praise good code
- ✅ Ask questions

**Reviewer (DON'T):**
- ❌ Nitpicking (önemsiz detaylar)
- ❌ "This is wrong" (explanation olmadan)
- ❌ Personal attacks
- ❌ Approval without reading

**Author (DO):**
- ✅ Feedback'i graciously kabul et
- ✅ Questions sor (anlamadıysan)
- ✅ Changes'i promptly yap
- ✅ Thank reviewer

**Author (DON'T):**
- ❌ Defensive olma
- ❌ Feedback'i ignore etme
- ❌ Changes yapmadan approve bekle

**Review Response Time:**
- **Urgent:** 2 saat
- **Normal:** 1 gün
- **Low Priority:** 2 gün

### 6.2 Git Workflow

**Branch Strategy (GitFlow):**
```
main (production)
  ├─ develop (integration)
      ├─ feature/process-monitor
      ├─ feature/disk-cleanup
      ├─ bugfix/memory-leak
      └─ hotfix/critical-bug
```

**Branch Naming:**
- Feature: `feature/short-description`
- Bugfix: `bugfix/issue-123-short-description`
- Hotfix: `hotfix/critical-bug`

**Commit Messages:**
```
Format:
<type>(<scope>): <subject>

<body>

<footer>

Types:
- feat: New feature
- fix: Bug fix
- docs: Documentation
- style: Formatting
- refactor: Code restructuring
- test: Adding tests
- chore: Maintenance

Example:
feat(process-monitor): Add CPU usage calculation

Implemented real-time CPU usage monitoring using Performance Counters.
Average calculation over 1-second interval.

Closes #42
```

**Pull Request Template:**
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests pass
- [ ] Manual testing done

## Checklist
- [ ] Code follows style guide
- [ ] Self-review done
- [ ] Documentation updated
- [ ] No new warnings

## Screenshots (if UI changes)
[Add screenshots]

## Related Issues
Closes #123
```

### 6.3 Code Style Guide

**C# Conventions:**
- **Naming:**
  - PascalCase: Classes, methods, properties
  - camelCase: Local variables, parameters
  - _camelCase: Private fields
  - UPPER_CASE: Constants

- **File Organization:**
  ```csharp
  // 1. Using statements
  using System;
  using WinCheck.Core;

  // 2. Namespace
  namespace WinCheck.Services;

  // 3. Class/Interface
  public class ProcessMonitorService : IProcessMonitorService
  {
      // 4. Constants
      private const int MaxProcesses = 1000;

      // 5. Fields
      private readonly ILogger _logger;

      // 6. Constructor
      public ProcessMonitorService(ILogger logger)
      {
          _logger = logger;
      }

      // 7. Public methods
      public async Task<ProcessMetrics> GetMetricsAsync()
      {
          // ...
      }

      // 8. Private methods
      private void ValidateInput()
      {
          // ...
      }
  }
  ```

- **SOLID Principles:**
  - Single Responsibility
  - Open/Closed
  - Liskov Substitution
  - Interface Segregation
  - Dependency Inversion

**XAML Conventions:**
- Indent: 4 spaces
- Attribute ordering: x:Name, Style, Properties, Events
- Use DataBinding over code-behind

### 6.4 Design Standards (UI/UX)

**Fluent Design System:**
- Acrylic materials for depth
- Reveal highlight for interactive elements
- Consistent spacing (8px grid)
- Typography scale (Segoe UI Variable)

**Accessibility:**
- WCAG 2.1 Level AA compliance
- Keyboard navigation
- Screen reader support
- Color contrast ratios

**Component Library:**
- Use Figma shared components
- Don't deviate without designer approval

---

## 7. Sorun Çözme ve Destek

### 7.1 Sorun Çözme Akışı

```
Sorun Karşılaştın
    ↓
Self-Debug (30 dakika)
├─ Google, Stack Overflow
├─ Documentation
├─ Logs analizi
└─ Code debugging
    ↓
    Çözülmedi mi?
    ↓
Ask Team (Slack)
├─ Clear problem statement
├─ What you tried
└─ Context
    ↓
    Hala çözülmedi?
    ↓
Pair Programming
├─ Book 30-60 min session
└─ Screen share + debug together
    ↓
    Hala çözülmedi?
    ↓
Escalate to Tech Lead
└─ Deep dive session
```

### 7.2 Blocker Protokolü

**Blocker Tanımı:**
- İşini yapamıyorsun
- 2+ saat progress yok
- Başka task'a geçemiyorsun

**Blocker Çıktığında:**
1. **Immediately** Slack'te paylaş (#dev channel)
   ```
   🚨 BLOCKER: [Kısa açıklama]

   Context: ...
   Tried: ...
   Need help: ...
   ```

2. **Daily Standup'ta** report et

3. **Scrum Master** prioritizes resolution
   - Pair programming assign eder
   - Veya alternative task bulur

**Blocker Resolve Edilince:**
- Slack'te update paylaş
- Learning'i document et (wiki/confluence)

### 7.3 Destek Matrisi

| Sorun Tipi | İlk Başvuru | Escalation |
|------------|-------------|------------|
| Code/Technical | #dev channel → Pair prog → Tech Lead | |
| Design/UX | #design channel → Designer | |
| Process/Agile | Scrum Master | |
| Requirements/Scope | Product Owner | |
| Environment/DevOps | Tech Lead | |
| HR/Admin | Scrum Master/HR | |

---

## 8. İyi Pratikler

### 8.1 Productivity Tips

**Deep Work:**
- Focus Time blocks kullan
- Pomodoro Technique (25 min focus + 5 min break)
- Distractions minimize et (notifications off)

**Task Management:**
- Start günü task planning ile (10 dakika)
- Prioritize (Eisenhower Matrix: Urgent/Important)
- Break down big tasks (max 4 saatlik chunks)

**Work-Life Balance:**
- Çalışma saatlerine uy (overtime sürdürülemez)
- Molalar önemli (burnout prevention)
- Akşamları disconnect (email checking yok)

### 8.2 Knowledge Sharing

**Documentation:**
- Code'da comment (why, not what)
- README'ler güncel tut
- Architecture Decision Records (ADR) yaz

**Learning Sessions:**
- Bi-weekly tech talks (30 min)
- Herkes sırayla sunar
- Topics: New tech, lessons learned, interesting bugs

**Pair Programming:**
- Haftada 2-3 saat (recommended)
- Driver-Navigator rotation
- Knowledge transfer + quality improvement

### 8.3 Continuous Improvement

**Kaizen Mindset:**
- Her gün %1 daha iyi
- Small wins = big impact
- Process'i sorgulamaktan korkma

**Feedback Culture:**
- Constructive feedback ver (hem positive hem improvement)
- Feedback istemeyi normalize et
- Regular 1-on-1s (with Tech Lead/SM)

**Retrospective Action Items:**
- Her retro'dan 1-3 action item
- Owner assign edilmeli
- Next retro'da follow-up

---

## 9. Sık Sorulan Sorular

**Q: Daily standup'a katılamayacağım, ne yapmalıyım?**
A: #standup channel'da async update paylaş. Format: Yesterday/Today/Blockers.

**Q: Kod yazmaya başlamadan önce design onayı gerekli mi?**
A: UI components için evet. Backend logic için approval gerekmez ama pre-implementation chat tavsiye edilir.

**Q: Pull Request onayı ne kadar sürer?**
A: Normal priority 1 gün, urgent 2 saat. Reviewer'lar notification'ları actively check etmeli.

**Q: Mülakatta "2 haftalık sprint" denildi ama pek sprint hissetmiyorum?**
A: Sprint = iterative development cycle. Her 2 haftada deliverable artımlı ürün çıkacak. Ceremonies takip et, rhythm gelecek.

**Q: Technical debt'e nasıl yaklaşmalıyız?**
A: Document et (tech debt backlog), her sprint'te en az 1 story (10% time budget). Critical debt = immediate fix.

**Q: Remote work'te yalnızlık hissediyorum, normal mi?**
A: Evet normal. Virtual coffee chats organize et (#random), video-on meetings, pair programming arttır.

**Q: Bireysel contribution nasıl measured edilir?**
A: Story points, code quality, team collaboration, initiative. Metrics'ten çok impact önemli.

**Q: Disagreement durumunda ne yapmalıyım?**
A: "Disagree and Commit" - fikrin heard edilsin, sonra ekip kararına commit et. Continuous disagreement ise escalate.

**Q: Work-life balance sağlanıyor mu?**
A: Evet. Core hours + flexibility. Overtime discouraged (sürdürülemez). Burnout prevention ciddi.

**Q: Project 14 hafta, sonrası ne olacak?**
A: Successful completion → extension/full-time opportunity possible. Portfolio piece + references garantili.

---

## 10. İlk Gün Onboarding Checklist

### First Day Checklist

**Administrative:**
- [ ] Welcome email aldın
- [ ] Contract signed
- [ ] NDA signed (if applicable)
- [ ] Equipment aldın (laptop, etc.)
- [ ] Email account active
- [ ] Slack/Teams joined

**Technical Setup:**
- [ ] Azure DevOps / GitHub access
- [ ] Repository cloned
- [ ] Development environment setup
- [ ] Project builds locally
- [ ] CI/CD pipeline access

**Team Introductions:**
- [ ] Team members introduced
- [ ] Roles ve responsibilities açıklandı
- [ ] Communication channels explained
- [ ] Meeting calendar added

**Project Context:**
- [ ] Product vision presentation izledin
- [ ] Architecture walkthrough aldın
- [ ] Code repository tour
- [ ] First task assigned

**Culture:**
- [ ] Team handbook read (bu doküman!)
- [ ] Team values discussed
- [ ] Q&A session

---

## Sonuç

Bu el kitabı living document. Feedback ve improvement önerileri her zaman welcome.

**Ekip başarısı = bireysel başarılarımızın toplamı.**

Harika bir takım oluşturalım! 🚀

---

**Dokuman Sahipliği:**
- **Owner:** Scrum Master
- **Contributors:** Tüm ekip
- **Update Frequency:** Continuous (as needed)
- **Feedback:** #general channel veya direkt Scrum Master'a

---

**Version History:**

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | Nov 2025 | Initial creation | Scrum Master |

---

**Bu kitabı okuduğun için teşekkürler! İyi çalışmalar! 💪**
