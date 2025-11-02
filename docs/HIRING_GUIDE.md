# WinCheck - Ekip İşe Alım Kılavuzu ve Mülakat Süreci

**Versiyon**: 1.0
**Son Güncelleme**: Kasım 2025
**Toplam Pozisyon**: 6 kişi

---

## İçindekiler

1. [İşe Alım Stratejisi](#işe-alım-stratejisi)
2. [Pozisyon Detayları ve İş İlanları](#pozisyon-detayları-ve-iş-ilanları)
3. [Mülakat Süreci](#mülakat-süreci)
4. [Teknik Değerlendirme](#teknik-değerlendirme)
5. [Referans Kontrol](#referans-kontrol)
6. [Teklif ve Onboarding](#teklif-ve-onboarding)

---

## 1. İşe Alım Stratejisi

### 1.1 İşe Alım Timeline'ı

```
Week -4 to -2: Recruitment Phase
├─ Job postings published
├─ CV screening
├─ Phone screening
└─ Technical assessment

Week -1: Final Selection
├─ On-site/video interviews
├─ Reference checks
├─ Offer letters
└─ Acceptance

Week 1: Onboarding
└─ Team starts!
```

### 1.2 İşe Alım Öncelikleri

**Öncelik Sırası (Kritik Pozisyonlar Önce):**

1. **Technical Lead** (Week -4) - En kritik, erken başlamalı
2. **Mid-Level Developer** (Week -4) - Core development
3. **UX/UI Designer** (Week -3) - Design başlamalı
4. **QA Engineer** (Week -2) - Sprint 3'te başlıyor
5. **Scrum Master** (Week -3) - Proje başında olmalı
6. **Product Owner** (Week -3) - Vision belirlemeli

### 1.3 İşe Alım Kanalları

**Primary Channels:**
- LinkedIn Jobs
- Stack Overflow Jobs
- GitHub Jobs
- AngelList (startup talent)

**Secondary Channels:**
- Referrals (en iyi kaynak!)
- Tech meetups & conferences
- University partnerships
- Recruitment agencies (last resort)

**Budget:**
- Job postings: $500
- Recruitment agency (if needed): 15-20% of salary
- Referral bonus: $1,000 per hire

---

## 2. Pozisyon Detayları ve İş İlanları

### 2.1 Technical Lead / Senior .NET Developer

#### İş İlanı Metni

```markdown
# Senior .NET Developer / Technical Lead - WinCheck

## About WinCheck
WinCheck is a next-generation Windows system optimization tool built with cutting-edge
technologies: .NET 8, WinUI 3, and modern Windows APIs. We're building the future of
system maintenance software.

## The Role
We're seeking an exceptional Senior .NET Developer to lead the technical implementation
of WinCheck. You'll architect the system, mentor the team, and build critical components.

## What You'll Do
- Design and implement system architecture using .NET 8 and WinUI 3
- Develop core services (Process Monitor, Service Optimizer, etc.)
- Integrate with Windows APIs (WMI, ETW, P/Invoke)
- Optimize performance (< 2s startup, < 100MB RAM)
- Mentor mid-level developer
- Code review and quality assurance
- DevOps and CI/CD pipeline

## Requirements
MUST HAVE:
- 5+ years C# / .NET experience
- Expert-level .NET 6+ knowledge
- WinUI 3, UWP, or WPF experience
- Windows API experience (P/Invoke, COM)
- MVVM pattern and reactive programming
- Git, Azure DevOps or GitHub
- Design patterns (SOLID, DRY, etc.)

NICE TO HAVE:
- Windows Internals knowledge
- ETW (Event Tracing) experience
- C++/WinRT
- Performance profiling (PerfView, dotTrace)
- MSIX packaging
- Security & authentication

## What We Offer
- Competitive salary: $1,600/week
- 14-week contract (with extension possibility)
- Remote/Hybrid work
- Modern tech stack
- Autonomy and ownership
- Impactful project

## Tech Stack
- .NET 8, C# 12
- WinUI 3 (Windows App SDK)
- Windows APIs (WMI, ETW, P/Invoke)
- Azure DevOps / GitHub
- MSTest, xUnit
- Serilog, Reactive Extensions

## How to Apply
Send your CV and GitHub profile to: jobs@wincheck.com
Subject: "Senior .NET Developer - [Your Name]"

Include:
- CV/Resume
- GitHub/Portfolio
- Brief cover letter (why WinCheck?)
```

#### Mülakat Soruları (Technical Lead)

**Round 1: Phone Screening (30 dakika)**

1. **Experience Validation**
   - ".NET 8'de .NET 6'ya göre en önemli değişiklikler neler?"
   - "WinUI 3 ile WPF arasındaki temel farklar?"
   - "Son projende karşılaştığın en zor teknik challenge neydi?"

2. **Architecture**
   - "Bir Windows desktop uygulamasını nasıl mimarilendirirsin?"
   - "MVVM pattern'ı açıkla ve avantajları neler?"
   - "Dependency Injection neden önemli?"

3. **Windows Specifics**
   - "P/Invoke nedir ve ne zaman kullanırsın?"
   - "WMI (Windows Management Instrumentation) ile ne tür bilgiler alabilirsin?"
   - "Process monitoring için hangi yaklaşımları kullanırsın?"

**Round 2: Technical Assessment (Take-home, 3-4 saat)**

**Task: Mini Process Monitor**
```
Bir console application yazın (C# .NET 8):

Requirements:
1. Tüm running processes'leri listeleyin
2. Her process için:
   - Process name
   - Memory usage (MB)
   - CPU usage (%)
   - Thread count
3. Memory kullanımına göre top 10 process
4. BONUS: Real-time update (her 1 saniye)

Deliverables:
- Source code (GitHub repo veya ZIP)
- README (nasıl çalıştırılır)
- Unit tests (en az 3 test)

Evaluation Criteria:
- Code quality & organization
- Performance
- Error handling
- Documentation
```

**Round 3: On-site/Video Interview (2 saat)**

**Part 1: Technical Deep Dive (60 dakika)**

1. **Code Review Exercise** (30 dk)
   - Kötü yazılmış bir kod parçası göster
   - "Bu kodda hangi problemleri görüyorsun?"
   - "Nasıl refactor ederdin?"

   Example bad code:
   ```csharp
   public class ProcessManager
   {
       public static List<string> GetProcesses()
       {
           var processes = Process.GetProcesses();
           var names = new List<string>();
           foreach(var p in processes)
           {
               try
               {
                   names.Add(p.ProcessName);
               }
               catch { }
           }
           return names;
       }
   }
   ```

2. **Architecture Design** (30 dk)
   - Whiteboard exercise
   - "WinCheck için bir modül tasarla: Service Optimizer"
   - Requirements:
     - Windows services listele
     - Güvenli disable edilebilir servisleri tespit et
     - Backup/restore mekanizması
   - Beklenen:
     - Interface design
     - Class diagram
     - Error handling strategy
     - Testing approach

**Part 2: Leadership & Soft Skills (30 dakika)**

1. **Mentorship**
   - "Junior developer'a nasıl code review feedback verirsin?"
   - "Team'de teknik bir anlaşmazlık olduğunda nasıl çözersin?"

2. **Problem Solving**
   - "Production'da critical bug - nasıl handle edersin?"
   - "Performance issue var, nereden başlarsın?"

3. **Communication**
   - "Technical olmayan stakeholder'a karmaşık bir teknik konuyu nasıl anlatırsın?"

**Part 3: Cultural Fit (30 dakika)**

1. "Neden WinCheck?"
2. "İdeal çalışma ortamın nasıl?"
3. "Remote/hybrid çalışma deneyimin var mı?"
4. "3 ay sonra hangi başarıları görmek istersin?"

#### Değerlendirme Kriterleri

| Kriter | Ağırlık | Passing Score |
|--------|---------|---------------|
| Technical Skills | 40% | 8/10 |
| Architecture & Design | 25% | 7/10 |
| Leadership & Mentorship | 15% | 7/10 |
| Communication | 10% | 8/10 |
| Cultural Fit | 10% | 7/10 |
| **TOTAL** | **100%** | **≥ 75%** |

**Decision Matrix:**
- **85%+**: Strong hire - offer immediately
- **75-84%**: Good hire - offer with conditions
- **65-74%**: Marginal - second interview or pass
- **<65%**: No hire

---

### 2.2 Mid-Level .NET Developer

#### İş İlanı Metni

```markdown
# Mid-Level .NET Developer (WinUI 3) - WinCheck

## About the Role
Join our small, focused team building WinCheck - a modern Windows optimization tool.
You'll work on UI development, business logic, and learn from an experienced Tech Lead.

## What You'll Do
- Develop beautiful WinUI 3 interfaces
- Implement ViewModels (MVVM pattern)
- Build business logic services (Disk Cleanup, Registry, etc.)
- Write unit tests
- Collaborate with designer and QA
- Learn Windows APIs and system programming

## Requirements
MUST HAVE:
- 3+ years C# / .NET experience
- XAML experience (WinUI 3, UWP, or WPF)
- MVVM pattern understanding
- Data binding, dependency injection
- Unit testing
- Git

NICE TO HAVE:
- Fluent Design System
- Animations (Composition API)
- Community Toolkit
- Reactive Extensions
- Windows Registry API

## What We Offer
- Salary: $1,100/week
- 14-week contract
- Mentorship from senior developer
- Modern tech stack
- Portfolio-worthy project

## Apply
jobs@wincheck.com - "Mid-Level .NET Developer - [Your Name]"
```

#### Mülakat Soruları (Mid-Level Developer)

**Round 1: Phone Screening (20 dakika)**

1. "XAML ile ne tür projeler yaptın?"
2. "MVVM pattern'ı açıkla - neden kullanırız?"
3. "Data binding nasıl çalışır?"
4. "Unit test yazmayı sever misin? Neden?"

**Round 2: Technical Assessment (Take-home, 2-3 saat)**

**Task: Simple CRUD App**
```
WinUI 3 app: Todo List

Features:
1. Add/Edit/Delete tasks
2. Mark as complete
3. Filter (All/Active/Completed)
4. Persist to local file (JSON)

Technical Requirements:
- MVVM pattern
- Data binding
- Commands (RelayCommand)
- ObservableCollection
- Value converters (if needed)

Bonus:
- Unit tests
- Animations
```

**Round 3: Interview (1 saat)**

**Technical (40 dk)**
1. Code review (take-home assignment)
2. XAML layout exercise (whiteboard)
3. "Async/await nasıl çalışır?"
4. "INotifyPropertyChanged nedir?"

**Soft Skills (20 dk)**
1. "Mentoring almayı sever misin?"
2. "Pair programming deneyimin var mı?"
3. "En gurur duyduğun kod?"

#### Değerlendirme

| Kriter | Ağırlık | Passing Score |
|--------|---------|---------------|
| XAML/UI Skills | 35% | 7/10 |
| C# Fundamentals | 30% | 7/10 |
| Problem Solving | 20% | 6/10 |
| Communication | 10% | 7/10 |
| Growth Mindset | 5% | 7/10 |
| **TOTAL** | **100%** | **≥ 70%** |

---

### 2.3 QA Engineer / Test Automation

#### İş İlanı

```markdown
# QA Engineer (Test Automation) - WinCheck

## The Role
Ensure WinCheck is rock-solid through comprehensive testing strategy, automation,
and quality advocacy.

## Responsibilities
- Create test plan & test cases
- Manual and automated testing
- Integration & performance testing
- Security testing
- Bug tracking & reporting
- CI/CD integration

## Requirements
MUST HAVE:
- 3+ years QA/Testing experience
- Test automation (Selenium, Appium, WinAppDriver)
- C# and unit testing frameworks
- Test case design
- Bug tracking tools (Jira, Azure DevOps)

NICE TO HAVE:
- Windows application testing
- Performance testing (JMeter, LoadRunner)
- Security testing
- ISTQB certification

## Offer
- $600/week (0.75 FTE - 30h/week)
- 12 weeks (start Week 3)
- Remote work
- Impactful role

## Apply
jobs@wincheck.com - "QA Engineer - [Your Name]"
```

#### Mülakat

**Round 1: Phone Screen (20 dk)**
1. "Test automation experience?"
2. "Testing pyramid nedir?"
3. "Regression test suite'i nasıl oluşturursun?"
4. "Critical bug production'da - ne yaparsın?"

**Round 2: Assessment (2 saat)**
```
Test Case Design Exercise:

Feature: Process Manager - Kill Process
- User can terminate a running process
- Confirmation dialog shown
- Process terminates successfully
- Error handling (access denied, etc.)

Tasks:
1. Write test cases (10+ scenarios)
2. Identify edge cases
3. Prioritize tests (critical/high/medium/low)
4. Write 1 automated test (pseudo-code or C#)
```

**Round 3: Interview (45 dk)**
- Test case review
- Automation strategy discussion
- Tool preferences
- Quality advocacy

#### Değerlendirme

| Kriter | Ağırlık | Passing Score |
|--------|---------|---------------|
| Testing Knowledge | 35% | 7/10 |
| Automation Skills | 30% | 7/10 |
| Attention to Detail | 20% | 8/10 |
| Communication | 15% | 7/10 |
| **TOTAL** | **100%** | **≥ 70%** |

---

### 2.4 UX/UI Designer

#### İş İlanı

```markdown
# Senior UX/UI Designer - WinCheck

## The Role
Design the user experience and interface for WinCheck - a modern Windows system
optimization tool. Bring Fluent Design System to life.

## Responsibilities
- User research & personas
- Information architecture
- Wireframes & user flows
- High-fidelity mockups (Figma)
- Design system & component library
- Icon design (50+ icons)
- Developer handoff

## Requirements
MUST HAVE:
- 4+ years UX/UI design
- Figma or Adobe XD expertise
- Design systems
- Fluent Design System knowledge
- Accessibility (WCAG 2.1)
- User research methodologies

NICE TO HAVE:
- Windows app design
- Motion design / After Effects
- HTML/CSS
- Usability testing tools

## Offer
- $1,000/week (0.5 FTE - 20h/week)
- 6 weeks
- Remote work
- Portfolio piece

## Apply
jobs@wincheck.com - "UX/UI Designer - [Your Name]"
Include: Portfolio (required)
```

#### Mülakat

**Round 1: Portfolio Review (30 dk)**
- 3-5 best projects
- Design process walkthrough
- Tools and methods
- Fluent Design System familiarity

**Round 2: Design Challenge (Take-home, 4 saat)**
```
Design Task: Dashboard for WinCheck

Brief:
- User needs to see system health at a glance
- Metrics: CPU, RAM, Disk, Startup programs
- Quick actions: Clean, Optimize
- Recent activity feed

Deliverables:
1. Wireframe (low-fi)
2. High-fidelity mockup (Figma)
3. Design rationale (PDF, 1 page)

Evaluation:
- Visual design
- Usability
- Fluent Design System usage
- Information hierarchy
```

**Round 3: Interview (1 saat)**
- Design challenge presentation
- Design process discussion
- Collaboration with developers
- Accessibility knowledge

#### Değerlendirme

| Kriter | Ağırlık | Passing Score |
|--------|---------|---------------|
| Visual Design | 35% | 8/10 |
| UX Thinking | 30% | 8/10 |
| Fluent Design | 15% | 7/10 |
| Collaboration | 10% | 7/10 |
| Portfolio Quality | 10% | 8/10 |
| **TOTAL** | **100%** | **≥ 75%** |

---

### 2.5 Scrum Master / Project Manager

#### İş İlanı

```markdown
# Scrum Master / Project Manager - WinCheck

## The Role
Facilitate Agile processes, remove impediments, and ensure successful delivery
of WinCheck v1.0.

## Responsibilities
- Sprint planning & backlog grooming
- Daily standups facilitation
- Sprint review & retrospective
- Impediment removal
- Metrics tracking (velocity, burndown)
- Stakeholder communication
- Risk management

## Requirements
MUST HAVE:
- 3+ years Scrum Master / PM experience
- Agile/Scrum methodologies
- Azure DevOps or Jira
- Project planning tools
- Risk management
- Excellent communication

NICE TO HAVE:
- Certified Scrum Master (CSM)
- PMP certification
- Technology project experience

## Offer
- $250/week (0.25 FTE - 10h/week)
- 14 weeks
- Remote work

## Apply
jobs@wincheck.com - "Scrum Master - [Your Name]"
```

#### Mülakat

**Round 1: Phone Screen (20 dk)**
1. "Scrum rituals nelerdir?"
2. "Team velocity düştüğünde ne yaparsın?"
3. "Difficult stakeholder scenario nasıl handle edersin?"

**Round 2: Case Study (30 dk)**
```
Scenario:
- Sprint 3, Day 7
- Velocity: 45 planned, 20 completed
- 1 developer sick
- Critical bug found
- Stakeholder wants new feature

Questions:
1. Ne yaparsın?
2. Hangi önceliklendirme yapar sın?
3. Nasıl communicate edersin?
```

**Round 3: Interview (30 dk)**
- Case study discussion
- Agile philosophy
- Tool preferences
- Availability confirmation

#### Değerlendirme

| Kriter | Ağırlık | Passing Score |
|--------|---------|---------------|
| Agile Knowledge | 35% | 8/10 |
| Facilitation Skills | 25% | 7/10 |
| Problem Solving | 20% | 7/10 |
| Communication | 20% | 8/10 |
| **TOTAL** | **100%** | **≥ 75%** |

---

### 2.6 Product Owner

#### İş İlanı

```markdown
# Product Owner - WinCheck

## The Role
Define product vision, prioritize features, and ensure we build the right product.

## Responsibilities
- Product vision & roadmap
- User stories & acceptance criteria
- Backlog prioritization
- Sprint demo participation
- Stakeholder management
- Go-to-market strategy

## Requirements
MUST HAVE:
- 4+ years Product Owner/Manager
- User story writing
- Agile methodologies
- Market analysis
- Stakeholder management
- Technical background

NICE TO HAVE:
- CSPO certification
- System optimization tools knowledge
- Windows ecosystem knowledge
- B2C product experience

## Offer
- $200/week (0.1 FTE - 4h/week)
- 14 weeks
- Remote work

## Apply
jobs@wincheck.com - "Product Owner - [Your Name]"
```

#### Mülakat

**Single Interview (45 dk)**

1. **Product Thinking** (20 dk)
   - "Nasıl bir product vision oluşturursun?"
   - "Feature prioritization için hangi framework kullanırsın?"
   - "WinCheck için hangi metrikleri track ederdin?"

2. **User Story Exercise** (15 dk)
   ```
   Feature: Disk Cleanup

   Task: Write 3-5 user stories

   Example format:
   As a [user]
   I want [feature]
   So that [benefit]

   Acceptance criteria: ...
   ```

3. **Stakeholder Management** (10 dk)
   - "Stakeholder technical olmayan, feature scope büyütmek istiyor - ne yaparsın?"
   - "Roadmap nasıl communicate edersin?"

#### Değerlendirme

| Kriter | Ağırlık | Passing Score |
|--------|---------|---------------|
| Product Thinking | 40% | 8/10 |
| User Story Writing | 25% | 7/10 |
| Stakeholder Mgmt | 20% | 8/10 |
| Business Acumen | 15% | 7/10 |
| **TOTAL** | **100%** | **≥ 75%** |

---

## 3. Genel Mülakat Süreci

### 3.1 Süreç Adımları

```
1. Application Received
   ↓
2. CV Screening (2-3 days)
   - Technical requirements check
   - Experience validation
   - Red flags?
   ↓
3. Phone Screening (30 min)
   - Basic technical questions
   - Culture fit initial assessment
   - Logistics (availability, salary expectations)
   ↓
4. Technical Assessment (take-home)
   - Practical coding/design task
   - 2-4 hours time limit
   - 3-5 days to complete
   ↓
5. Technical Interview (1-2 hours)
   - Deep dive on assessment
   - Architecture/design questions
   - Problem-solving
   ↓
6. Team Interview (30-45 min)
   - Soft skills
   - Cultural fit
   - Team compatibility
   ↓
7. Reference Check (2-3 references)
   ↓
8. Offer Letter (24-48 hours)
   ↓
9. Acceptance & Onboarding

Total Time: 2-3 weeks
```

### 3.2 Interview Panel

**Technical Lead Position:**
- Panel: CTO/Tech Director, Senior Engineer, PM
- Duration: 2 hours

**Other Positions:**
- Panel: Technical Lead (hired first), PM
- Duration: 1-1.5 hours

### 3.3 Evaluation Form Template

```markdown
# Interview Evaluation Form

**Candidate**: __________________
**Position**: __________________
**Interviewer**: __________________
**Date**: __________________

## Technical Skills (40%)

| Skill | Score (1-10) | Notes |
|-------|--------------|-------|
| Core Technology | | |
| Problem Solving | | |
| Code Quality | | |
| Testing | | |

**Average**: _____

## Experience (25%)

| Area | Score (1-10) | Notes |
|------|--------------|-------|
| Relevant Experience | | |
| Similar Projects | | |
| Domain Knowledge | | |

**Average**: _____

## Soft Skills (20%)

| Skill | Score (1-10) | Notes |
|-------|--------------|-------|
| Communication | | |
| Teamwork | | |
| Learning Ability | | |
| Proactivity | | |

**Average**: _____

## Cultural Fit (15%)

| Aspect | Score (1-10) | Notes |
|--------|--------------|-------|
| Values Alignment | | |
| Work Style | | |
| Motivation | | |

**Average**: _____

## Overall Assessment

**Total Score**: _____ / 100

**Recommendation**:
- [ ] Strong Hire (85%+)
- [ ] Hire (75-84%)
- [ ] Maybe (65-74%)
- [ ] No Hire (<65%)

**Comments**:
_________________________________
_________________________________
_________________________________

**Signature**: __________________
```

---

## 4. Referans Kontrol

### 4.1 Referans Kontrol Soruları

**Temel Sorular:**
1. "Adayı ne kadar süredir tanıyorsunuz?"
2. "Hangi pozisyonda çalıştı?"
3. "Tekrar birlikte çalışmak ister misiniz? Neden?"

**Technical Leads için:**
1. "Teknik liderlik becerileri nasıldı?"
2. "Mimari kararlar alırken nasıl bir yaklaşımı vardı?"
3. "Performans sorunlarını nasıl çözerdi?"
4. "Code quality'e önem verir miydi?"
5. "Mentorship becerileri nasıldı?"

**Developers için:**
1. "Code quality genel olarak nasıldı?"
2. "Deadline'ları karşılayabilir miydi?"
3. "Team collaboration nasıldı?"
4. "Yeni teknolojileri öğrenmeye açık mıydı?"

**Soft Skills:**
1. "İletişim becerileri nasıldı?"
2. "Stres altında nasıl performans gösterirdi?"
3. "Geri bildirim almaya açık mıydı?"
4. "İyileştirme alanları nelerdi?"

### 4.2 Red Flags

**Immediate Disqualification:**
- Yalan CV/portfolio
- Referanslardan çok negatif feedback
- Etik problemler
- İletişimsizlik (telefon/email cevap vermiyor)

**Warning Signs:**
- Referanslar ulaşılamıyor
- Belirsiz cevaplar
- Önceki işten ani ayrılma (explanation olmadan)
- Over-promise, under-deliver pattern

---

## 5. Teklif Süreci

### 5.1 Offer Letter Template

```markdown
[Company Letterhead]

[Date]

[Candidate Name]
[Address]

Dear [Name],

We are pleased to offer you the position of [Position] at [Company Name] for the
WinCheck project.

## Position Details

**Position**: [Position Title]
**Project**: WinCheck v1.0
**Duration**: [X] weeks
**Start Date**: [Date]
**FTE**: [0.X] (X hours/week)
**Work Location**: Remote / Hybrid

## Compensation

**Rate**: $[X]/week
**Total Project Value**: $[X]
**Payment Schedule**: Bi-weekly / Monthly

## Benefits

- Modern technology stack
- Flexible working hours
- Remote work
- Potential for extension/full-time

## Reporting

**Reports to**: [Technical Lead / Project Manager]
**Team Size**: 6 people (total)

## Start Date & Onboarding

**Start Date**: [Date]
**Onboarding**: Week 1 (full support provided)

## Acceptance

Please confirm your acceptance by signing and returning this letter by [Date].

We're excited to have you join the WinCheck team!

Sincerely,

[Hiring Manager Name]
[Title]

---

**Acceptance**:

I, [Candidate Name], accept the above offer.

Signature: ________________    Date: ________________
```

### 5.2 Offer Negotiation Guidelines

**Non-negotiable:**
- Project scope
- Timeline (14 weeks)
- Work requirements (deliverables)

**Potentially Negotiable:**
- Rate (±10%)
- Start date (±1 week)
- Work hours distribution (flexibility)
- Remote vs. hybrid

**Decision Timeline:**
- Offer sent: Day 0
- Candidate response: 3-5 business days
- If declined: Move to backup candidate within 24h

---

## 6. Backup Plan

### 6.1 Backup Candidates

**Strategy**: Keep 2-3 backup candidates per position (especially critical roles)

**Communication with Backups:**
```
"Thank you for your interest in [Position]. You were among our top candidates.
While we've moved forward with another candidate, we'd like to keep you in mind
for future opportunities. May we stay in touch?"
```

### 6.2 Contingency Plans

**If Technical Lead falls through:**
- Option A: Promote from Mid-Level Dev + hire new Mid-Level
- Option B: Senior contractor (higher cost but faster)
- Option C: Delay project by 2 weeks

**If Designer falls through:**
- Option A: Use template designs (Material Design adaptation)
- Option B: Freelance designer (per deliverable)

**If Developer falls through:**
- Option A: Technical Lead takes on more dev work
- Option B: Contractor developer

---

## 7. Onboarding Checklist

### 7.1 Pre-Start (Week -1)

**HR & Admin:**
- [ ] Contract signed
- [ ] NDA signed (if applicable)
- [ ] Equipment ordered (laptop, monitors, etc.)
- [ ] Accounts created (email, Azure DevOps, Slack)
- [ ] Welcome email sent (with day 1 agenda)

**Technical:**
- [ ] Repository access granted
- [ ] Development environment guide sent
- [ ] Architecture docs shared
- [ ] Meeting invites sent

### 7.2 Day 1

**Morning (9:00-12:00):**
- [ ] Welcome & team introductions (30 min)
- [ ] HR orientation (30 min)
- [ ] Equipment setup (1 hour)
- [ ] Tool access verification (1 hour)

**Afternoon (13:00-18:00):**
- [ ] Product vision presentation (30 min)
- [ ] Architecture walkthrough (1 hour)
- [ ] Code repository tour (1 hour)
- [ ] First task assignment (30 min)
- [ ] Q&A / open discussion (1 hour)

### 7.3 Week 1

**Daily:**
- [ ] Pair programming sessions (2h/day)
- [ ] Standup participation
- [ ] Code review observation

**By End of Week:**
- [ ] Project builds locally
- [ ] First PR submitted
- [ ] Documentation read
- [ ] Team bonding (virtual coffee/lunch)

---

## 8. Hiring Budget

### 8.1 Cost Breakdown

| Position | Rate/Week | Weeks | Total Cost |
|----------|-----------|-------|------------|
| Technical Lead | $1,600 | 14 | $22,400 |
| Mid-Level Dev | $1,100 | 14 | $15,400 |
| QA Engineer | $600 | 12 | $7,200 |
| UX/UI Designer | $1,000 | 6 | $6,000 |
| Scrum Master | $250 | 14 | $3,500 |
| Product Owner | $200 | 14 | $2,800 |
| **Subtotal** | | | **$57,300** |
| Recruiting Costs (job postings, etc.) | | | $500 |
| Equipment (if needed) | | | $5,000 |
| **TOTAL** | | | **$62,800** |

### 8.2 Cost Optimization Tips

1. **Referrals**: Save recruitment fees, offer referral bonus ($1,000 < 15-20% agency fee)
2. **Contract-to-Hire**: Good performers → full-time (retention)
3. **Remote Work**: Wider talent pool, potentially lower rates
4. **Junior + Mentorship**: Mid-Level Dev could be Junior + more mentorship (save 30%)

---

## Sonuç ve Öneriler

### En İyi Pratikler

1. ✅ **Hire for Attitude, Train for Skill** - Özellikle junior pozisyonlar için
2. ✅ **Practical Assessments** - Teorik sorulardan çok practical tasks
3. ✅ **Fast Feedback** - Kandidatlara hızlı geri dönüş (24-48 saat)
4. ✅ **Transparent Process** - Adaylar süreci bilsin
5. ✅ **Sell the Vision** - WinCheck'in ne kadar heyecan verici olduğunu anlatın
6. ✅ **Backup Plans** - Her kritik rol için backup candidate
7. ✅ **Quick Onboarding** - İlk gün produktif olabilsinler

### Red Flags to Avoid

1. ❌ Çok uzun hiring process (>3 weeks)
2. ❌ Belirsiz expectations
3. ❌ Aşırı teknik sorular (algoritma olimpiyatı değil)
4. ❌ Cultural fit'i göz ardı etmek
5. ❌ Referans check'i atlamak
6. ❌ Sadece maliyet odaklı hiring

### Success Metrics

**Hiring Success Metrics:**
- Time to hire: < 3 weeks
- Offer acceptance rate: > 80%
- 90-day retention: > 90%
- Team satisfaction: 8+/10

---

**Document Approval:**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Hiring Manager | __________ | __________ | ____/____/____ |
| HR | __________ | __________ | ____/____/____ |
| Budget Owner | __________ | __________ | ____/____/____ |
