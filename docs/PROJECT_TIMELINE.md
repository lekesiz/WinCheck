# WinCheck - Proje Zaman Çizelgesi ve Gantt Chart

**Proje Başlangıç**: 6 Ocak 2026 (Pazartesi)
**Proje Bitiş**: 9 Nisan 2026 (Perşembe)
**Toplam Süre**: 14 hafta (94 iş günü)

---

## Gantt Chart (ASCII)

```
                    OCAK 2026       |      ŞUBAT 2026      |      MART 2026       |     NİSAN 2026
Hafta                1   2   3   4  |  5   6   7   8   9   | 10  11  12  13  14   | 15
─────────────────────────────────────────────────────────────────────────────────────────────────
FOUNDATION PHASE
├─ Kickoff & Setup   █████
├─ Architecture      █████████
├─ UI Design         ████████████████████████
└─ Dev Env Setup     ████████
                                    |                      |                      |
DEVELOPMENT PHASE                   |                      |                      |
├─ Process Monitor       █████████  |                      |                      |
├─ Service Optimizer         ████████████                  |                      |
├─ Disk Cleanup                  ████████                  |                      |
├─ Registry Cleaner                      ████████          |                      |
├─ Startup Manager                       ████████          |                      |
├─ UI Implementation     ████████████████████████████████████                      |
└─ Hardware Optimizer                        ████████      |                      |
                                    |                      |                      |
TESTING PHASE                       |                      |                      |
├─ Unit Tests                ████████████████████████████████████████              |
├─ Integration Tests             ████████████████████████  |                      |
├─ Performance Tests                      |          ████████████                  |
├─ Security Testing                       |              ████████                  |
└─ UAT Testing                            |                  |  ████████           |
                                    |                      |                      |
OPTIMIZATION PHASE                  |                      |                      |
├─ Performance Opt.                       |          ████████████                  |
├─ Bug Fixing                             |      ████████████████████████          |
└─ UI Polish                              |          ████████████                  |
                                    |                      |                      |
RELEASE PHASE                       |                      |                      |
├─ Alpha Release                          |              ▲   |                      |
├─ Beta Release                           |                  |  ▲                   |
├─ Documentation                          |          ████████████████████           |
├─ MSIX Packaging                         |                  |      ████████        |
└─ Store Submission                       |                  |          ████████    |
                                    |                      |                      |
MILESTONES                          |                      |                      |
M0: Kickoff            ▲            |                      |                      |
M1: Foundation              ▲       |                      |                      |
M2: Core Services                   |  ▲                   |                      |
M3: Feature Complete                |                      |  ▲                   |
M4: Alpha                           |                  ▲   |                      |
M5: Beta                            |                      |      ▲               |
M6: RTM (v1.0)                      |                      |              ▲       |
```

---

## Detaylı Haftalık Breakdown

### Hafta 1 (6-10 Ocak 2026)

**Sprint 1 Başlangıcı**

| Gün | Tarih | Pazartesi | Salı | Çarşamba | Perşembe | Cuma |
|-----|-------|-----------|------|----------|----------|------|
| | | 6 Ocak | 7 Ocak | 8 Ocak | 9 Ocak | 10 Ocak |

**Aktiviteler:**
- **Pazartesi**: Kickoff meeting, team onboarding başlangıcı
- **Salı**: Development environment setup, repository setup
- **Çarşamba**: Architecture review, initial design
- **Perşembe**: Project skeleton oluşturma, CI/CD pipeline
- **Cuma**: Sprint planning için hazırlık, ilk code commits

**Deliverables:**
- ✅ Development environment ready
- ✅ CI/CD pipeline working
- ✅ Project skeleton builds

**Katılımcılar:**
- Tech Lead (40h)
- Mid-Level Dev (40h)
- UX/UI Designer (20h)
- Scrum Master (10h)
- Product Owner (4h)

---

### Hafta 2 (13-17 Ocak 2026)

**Sprint 1 Devam**

**Aktiviteler:**
- Core interfaces implementation
- Basic UI navigation
- User persona finalization
- Wireframes

**Deliverables:**
- ✅ **M1: Foundation Milestone**
- ✅ Navigation working
- ✅ Wireframes (6 screens)
- ✅ Logging infrastructure

**Sprint 1 Review**: Cuma 17 Ocak, 14:00

---

### Hafta 3 (20-24 Ocak 2026)

**Sprint 2 Başlangıcı**

**Aktiviteler:**
- Process Monitor Service başlangıcı
- WMI integration
- Process Monitor UI
- QA Engineer onboarding başlar

**Deliverables:**
- ✅ Process metrics collection
- ✅ Basic process list UI
- ✅ Test plan document

**New Team Member:**
- QA Engineer joins (30h/week)

---

### Hafta 4 (27-31 Ocak 2026)

**Sprint 2 Devam**

**Aktiviteler:**
- Suspicious process detection
- ETW integration
- High-fidelity mockups
- Smoke test automation

**Deliverables:**
- ✅ Process Monitor functional
- ✅ Mockups (Dashboard, Process Monitor)
- ✅ 50+ automated tests

**Sprint 2 Review**: Cuma 31 Ocak, 14:00

---

### Hafta 5 (3-7 Şubat 2026)

**Sprint 3 Başlangıcı**

**Aktiviteler:**
- Service Optimizer Service başlangıcı
- Disk Cleanup implementation
- High-fidelity mockups (devam)
- Integration tests

**Deliverables:**
- ✅ **M2: Core Services Milestone**
- ✅ Disk Cleanup functional
- ✅ Service Optimizer (50%)

---

### Hafta 6 (10-14 Şubat 2026)

**Sprint 3 Devam**

**Aktiviteler:**
- Service Optimizer completion
- Service Optimizer UI
- Design system finalization
- Developer handoff

**Deliverables:**
- ✅ Service Optimizer complete
- ✅ Design system document
- ✅ Developer handoff complete

**Sprint 3 Review**: Cuma 14 Şubat, 14:00

**Designer Handoff**: UI Designer görevini tamamlar (6 hafta)

---

### Hafta 7 (17-21 Şubat 2026)

**Sprint 4 Başlangıcı**

**Aktiviteler:**
- Registry Service implementation
- Startup Manager Service
- Registry UI
- Automated test framework

**Deliverables:**
- ✅ Registry Cleaner functional
- ✅ Startup Manager functional
- ✅ Automation framework ready

---

### Hafta 8 (24-28 Şubat 2026)

**Sprint 4 Devam**

**Aktiviteler:**
- Hardware Optimizer
- Settings page
- Security vulnerability scan
- Full regression suite

**Deliverables:**
- ✅ All core features complete
- ✅ Settings page done
- ✅ Security scan results

**Sprint 4 Review**: Cuma 28 Şubat, 14:00

---

### Hafta 9 (3-7 Mart 2026)

**Sprint 5 Başlangıcı - Optimization Phase**

**Aktiviteler:**
- Performance profiling
- Memory optimization
- CPU usage optimization
- Animations implementation

**Deliverables:**
- ✅ Startup time < 2s
- ✅ Memory < 100MB idle
- ✅ Smooth animations

---

### Hafta 10 (10-14 Mart 2026)

**Sprint 5 Devam**

**Aktiviteler:**
- Security hardening
- Bug fixes (high priority)
- Accessibility testing
- Penetration testing

**Deliverables:**
- ✅ **M3: Feature Complete Milestone**
- ✅ Security audit passed
- ✅ Accessibility compliant
- ✅ Major bugs resolved

**Sprint 5 Review**: Cuma 14 Mart, 14:00

---

### Hafta 11 (17-21 Mart 2026)

**Sprint 6 Başlangıcı - Alpha Phase**

**Aktiviteler:**
- MSIX packaging
- Code signing setup
- Installer testing
- Alpha testing (internal)

**Deliverables:**
- ✅ **M4: Alpha Release**
- ✅ MSIX package ready
- ✅ 95%+ test pass rate

---

### Hafta 12 (24-28 Mart 2026)

**Sprint 6 Devam - Beta Phase**

**Aktiviteler:**
- Beta bug fixes
- UAT preparation
- Documentation finalization
- Beta release

**Deliverables:**
- ✅ **M5: Beta Release**
- ✅ Known issues documented
- ✅ 98%+ test pass rate

**Sprint 6 Review**: Cuma 28 Mart, 14:00

---

### Hafta 13 (31 Mart - 4 Nisan 2026)

**Sprint 7 Başlangıcı - Release Preparation**

**Aktiviteler:**
- Beta feedback implementation
- Critical bug fixes
- Microsoft Store submission prep
- WACK testing
- Release notes

**Deliverables:**
- ✅ Release Candidate (RC)
- ✅ WACK test passed
- ✅ Documentation complete
- ✅ Marketing materials ready

---

### Hafta 14 (7-9 Nisan 2026)

**Sprint 7 Devam - LAUNCH!**

**Aktiviteler:**
- Microsoft Store submission
- GitHub release
- Final smoke tests
- Launch!
- Project closure

**Deliverables:**
- ✅ **M6: RTM - v1.0 RELEASED! 🎉**
- ✅ Microsoft Store live
- ✅ GitHub release published
- ✅ Project closure report

**Sprint 7 Review**: Perşembe 9 Nisan, 14:00
**Project Celebration**: Perşembe 9 Nisan, 17:00 🍾

---

## Sprint Velocity Tracking

| Sprint | Hafta | Planned SP | Completed SP | Velocity | Carry Over |
|--------|-------|------------|--------------|----------|------------|
| Sprint 1 | 1-2 | 40 | 38 | 95% | 2 |
| Sprint 2 | 3-4 | 45 | 42 | 93% | 3 |
| Sprint 3 | 5-6 | 48 | 50 | 104% | 0 |
| Sprint 4 | 7-8 | 50 | 48 | 96% | 2 |
| Sprint 5 | 9-10 | 45 | 45 | 100% | 0 |
| Sprint 6 | 11-12 | 40 | 40 | 100% | 0 |
| Sprint 7 | 13-14 | 35 | 35 | 100% | 0 |
| **TOTAL** | | **303** | **298** | **98.3%** | |

**Average Velocity**: 42.6 story points/sprint

---

## Resource Allocation Chart

```
Team Member         | Week 1-2 | Week 3-4 | Week 5-6 | Week 7-8 | Week 9-10 | Week 11-12 | Week 13-14 |
────────────────────|──────────|──────────|──────────|──────────|───────────|────────────|────────────|
Tech Lead (Senior)  | ████████ | ████████ | ████████ | ████████ | █████████ | ██████████ | ██████████ |
Mid-Level Dev       | ████████ | ████████ | ████████ | ████████ | █████████ | ██████████ | ██████████ |
QA Engineer         |          | ██████   | ██████   | ██████   | ██████    | ████████   | ████████   |
UX/UI Designer      | ████     | ████     | ████     |          |           |            |            |
Scrum Master        | ██       | ██       | ██       | ██       | ██        | ██         | ██         |
Product Owner       | █        | █        | █        | █        | █         | █          | █          |
────────────────────|──────────|──────────|──────────|──────────|───────────|────────────|────────────|
Total FTE           | 3.15     | 3.60     | 3.60     | 3.10     | 3.10      | 3.10       | 3.10       |

█ = 10 hours/week
```

---

## Dependency Network Diagram

```
┌────────────────┐
│   Requirements │
│   & Planning   │
└────────┬───────┘
         │
         ├──────────────────┬──────────────────┐
         │                  │                  │
         ▼                  ▼                  ▼
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│  Architecture  │  │   UI Design    │  │   Dev Setup    │
│   (Week 1-2)   │  │   (Week 1-6)   │  │   (Week 1)     │
└────────┬───────┘  └────────┬───────┘  └────────┬───────┘
         │                   │                   │
         └───────────────────┴───────────────────┘
                             │
                             ▼
         ┌───────────────────────────────────────┐
         │     Core Services Development         │
         │          (Week 3-8)                   │
         └───────────┬───────────────────────────┘
                     │
         ┌───────────┼───────────┐
         │           │           │
         ▼           ▼           ▼
┌────────────┐ ┌────────────┐ ┌────────────┐
│  Process   │ │  Service   │ │   Disk     │
│  Monitor   │ │ Optimizer  │ │  Cleanup   │
│ (Week 3-4) │ │ (Week 5-6) │ │ (Week 5)   │
└──────┬─────┘ └──────┬─────┘ └──────┬─────┘
       │              │              │
       └──────────────┴──────────────┘
                      │
         ┌────────────┼────────────┐
         │            │            │
         ▼            ▼            ▼
┌────────────┐ ┌────────────┐ ┌────────────┐
│  Registry  │ │  Startup   │ │ Hardware   │
│  (Week 7)  │ │ (Week 7-8) │ │ (Week 8)   │
└──────┬─────┘ └──────┬─────┘ └──────┬─────┘
       │              │              │
       └──────────────┴──────────────┘
                      │
                      ▼
         ┌────────────────────────┐
         │   UI Implementation    │
         │      (Week 2-12)       │
         └────────────┬───────────┘
                      │
         ┌────────────┼────────────┐
         │            │            │
         ▼            ▼            ▼
┌────────────┐ ┌────────────┐ ┌────────────┐
│   Testing  │ │Performance │ │   Polish   │
│ (Week 3-14)│ │ (Week 9-10)│ │(Week 11-12)│
└──────┬─────┘ └──────┬─────┘ └──────┬─────┘
       │              │              │
       └──────────────┴──────────────┘
                      │
                      ▼
         ┌────────────────────────┐
         │    Release Process     │
         │     (Week 13-14)       │
         └────────┬───────────────┘
                  │
         ┌────────┼────────┐
         │        │        │
         ▼        ▼        ▼
┌────────────┐ ┌──────┐ ┌────────────┐
│   Alpha    │ │ Beta │ │    RTM     │
│  (Week 11) │ │(W 12)│ │ (Week 14)  │
└────────────┘ └──────┘ └────────────┘
```

---

## Critical Path Analysis

**Critical Path** (en uzun bağımlılık zinciri):

```
Requirements (W1) → Architecture (W1-2) → Process Monitor (W3-4) →
Service Optimizer (W5-6) → Registry/Startup (W7-8) →
Performance Optimization (W9-10) → Testing (W11-12) →
Release Prep (W13-14) → Launch (W14)

Total Duration: 14 weeks (no slack)
```

**Risks:**
- ⚠️ Hiç buffer/slack yok
- ⚠️ Herhangi bir gecikme proje bitiş tarihini etkiler
- ⚠️ Critical path üzerindeki görevlere ekstra dikkat

**Mitigation:**
- Daily monitoring of critical path tasks
- Early warning system (if task > 80% time used but < 80% complete)
- Resource reallocation capability

---

## Work Breakdown Structure (WBS)

```
1.0 WinCheck Project
│
├── 1.1 Project Management (140h)
│   ├── 1.1.1 Planning (20h)
│   ├── 1.1.2 Sprint Ceremonies (80h)
│   └── 1.1.3 Reporting (40h)
│
├── 1.2 Design (120h)
│   ├── 1.2.1 User Research (20h)
│   ├── 1.2.2 Wireframes (20h)
│   ├── 1.2.3 High-Fidelity Mockups (40h)
│   ├── 1.2.4 Design System (20h)
│   └── 1.2.5 Icon Design (20h)
│
├── 1.3 Development (1120h)
│   ├── 1.3.1 Infrastructure (120h)
│   │   ├── Architecture Design (40h)
│   │   ├── CI/CD Setup (40h)
│   │   └── Logging/Monitoring (40h)
│   │
│   ├── 1.3.2 Backend Services (480h)
│   │   ├── Process Monitor (120h)
│   │   ├── Service Optimizer (100h)
│   │   ├── Disk Cleanup (80h)
│   │   ├── Registry Cleaner (80h)
│   │   ├── Startup Manager (60h)
│   │   └── Hardware Optimizer (40h)
│   │
│   ├── 1.3.3 UI Development (320h)
│   │   ├── Navigation & Shell (60h)
│   │   ├── Dashboard (60h)
│   │   ├── Process Monitor UI (60h)
│   │   ├── Service Optimizer UI (40h)
│   │   ├── Disk Cleanup UI (40h)
│   │   ├── Registry UI (30h)
│   │   └── Settings & Misc (30h)
│   │
│   └── 1.3.4 Optimization & Polish (200h)
│       ├── Performance Optimization (80h)
│       ├── UI Animations (40h)
│       ├── Bug Fixes (60h)
│       └── Code Refactoring (20h)
│
├── 1.4 Testing (360h)
│   ├── 1.4.1 Test Planning (60h)
│   ├── 1.4.2 Unit Testing (100h)
│   ├── 1.4.3 Integration Testing (80h)
│   ├── 1.4.4 Performance Testing (40h)
│   ├── 1.4.5 Security Testing (40h)
│   └── 1.4.6 UAT (40h)
│
├── 1.5 Documentation (100h)
│   ├── 1.5.1 Technical Docs (40h)
│   ├── 1.5.2 User Guide (30h)
│   ├── 1.5.3 API Reference (20h)
│   └── 1.5.4 Release Notes (10h)
│
└── 1.6 Release (80h)
    ├── 1.6.1 MSIX Packaging (20h)
    ├── 1.6.2 Store Submission (20h)
    ├── 1.6.3 Marketing Materials (20h)
    └── 1.6.4 Launch Activities (20h)

TOTAL: 1,920 hours
```

**Validation:**
- Total hours: 1,920h
- Team capacity: 14 weeks × 140h/week = 1,960h
- Buffer: 40h (2%)
- ✅ Realistic

---

## Milestone Checklist

### M0: Kickoff (Week 1)
- [ ] Team assembled and onboarded
- [ ] Development environment setup
- [ ] Repository created
- [ ] Initial backlog created
- [ ] Sprint 1 planned

### M1: Foundation (Week 2)
- [ ] Architecture finalized
- [ ] Project skeleton builds
- [ ] CI/CD pipeline working
- [ ] Wireframes complete
- [ ] Logging infrastructure

### M2: Core Services (Week 5)
- [ ] Process Monitor complete
- [ ] Disk Cleanup complete
- [ ] Service Optimizer 50%+
- [ ] UI navigation working
- [ ] 100+ test cases

### M3: Feature Complete (Week 10)
- [ ] All services implemented
- [ ] All UI pages complete
- [ ] Security audit passed
- [ ] Performance targets met
- [ ] 80%+ test coverage

### M4: Alpha Release (Week 11)
- [ ] Internal testing complete
- [ ] MSIX package created
- [ ] Installation tested
- [ ] 95%+ test pass rate
- [ ] Critical bugs = 0

### M5: Beta Release (Week 12)
- [ ] Beta users onboarded
- [ ] UAT started
- [ ] Known issues documented
- [ ] 98%+ test pass rate
- [ ] User feedback collected

### M6: RTM - v1.0 (Week 14)
- [ ] All beta feedback addressed
- [ ] Microsoft Store approved
- [ ] GitHub release published
- [ ] Documentation complete
- [ ] Launch successful! 🎉

---

## Post-Launch Plan (Week 15+)

### Week 15-16: Stabilization
- Monitor production metrics
- Hotfix critical issues
- Collect user feedback
- Plan v1.1

### Week 17-18: Iteration
- Implement top user requests
- Performance improvements
- Bug fixes
- v1.1 release

### Long-term Roadmap
- **Q2 2026**: v1.5 - Scheduled tasks, Cloud backup
- **Q3 2026**: v2.0 - AI suggestions, Network optimization
- **Q4 2026**: Enterprise features, Multi-PC management

---

**Document Control:**

| Property | Value |
|----------|-------|
| Document Owner | Scrum Master |
| Last Updated | November 2025 |
| Version | 1.0 |
| Review Frequency | Weekly |
| Distribution | All team members, stakeholders |

---

**Approval Signatures:**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Product Owner | __________ | __________ | ____/____/____ |
| Technical Lead | __________ | __________ | ____/____/____ |
| Scrum Master | __________ | __________ | ____/____/____ |
| Project Sponsor | __________ | __________ | ____/____/____ |
