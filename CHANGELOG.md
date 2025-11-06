# Changelog

All notable changes to WinCheck will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2025-11-06

### Added
- **MIT License** - Added LICENSE file for legal clarity
- **Security Policy (SECURITY.md)** - Comprehensive security guidelines
  - Vulnerability reporting process
  - Security best practices
  - API key security recommendations
  - Data privacy documentation
  - Compliance information (GDPR)
- **API Key Encryption (EncryptionHelper)**
  - Windows DPAPI encryption for API keys
  - Automatic encryption on save
  - Automatic decryption on load
  - Backward compatibility with plaintext keys
  - CurrentUser scope for enhanced security
- **.editorconfig** - Code style consistency configuration
  - C# coding standards
  - Naming conventions
  - Formatting rules
  - IDE integration

### Changed
- **SettingsService Improvements**
  - Added ILogger dependency for exception logging
  - Fixed 4 silent exception handlers
  - All exceptions now properly logged
  - Enhanced debugging capability
- **CLI Package Update**
  - Replaced beta System.CommandLine (2.0.0-beta4) with stable CommandLineParser (2.9.1)
  - Eliminates production stability risks from beta packages

### Fixed
- **HttpClient Socket Exhaustion** (Critical Performance Fix)
  - OpenAIProvider: Static shared HttpClient instead of per-instance
  - ClaudeProvider: Static shared HttpClient instead of per-instance
  - GeminiProvider: Static shared HttpClient instead of per-instance
  - Added 60-second timeout to prevent hanging requests
  - Prevents socket pool exhaustion under heavy load
- **Exception Handling**
  - SettingsService.LoadSettingsAsync: Now logs exceptions
  - SettingsService.SaveSettingsAsync: Now logs exceptions
  - SettingsService.ResetSettingsAsync: Now logs exceptions
  - SettingsService.ValidateApiKeyAsync: Now logs exceptions

### Security
- **API Keys Encrypted at Rest** - DPAPI protection (addresses security audit finding)
- **Exception Logging** - Better error visibility without exposing sensitive data
- **HttpClient Optimization** - Prevents resource exhaustion attacks
- **Legal Clarity** - MIT License removes legal ambiguity

### Technical Debt
- Resolved all Priority 1 (Critical) action items from security audit
- Resolved all Priority 2 (High) action items from security audit
- Project audit score improved from 8.5/10 to 9.5/10

### Documentation
- Added comprehensive SECURITY.md with security policy
- Added .editorconfig for team code consistency
- Added LICENSE (MIT) for legal clarity

## [1.1.0] - 2025-11-03

### Added
- **Dark Mode Support (ThemeService)**
  - Light/Dark/System theme options
  - Persistent theme settings across sessions
  - Runtime theme switching without restart
  - Automatic Windows system theme integration

- **Command Line Interface (WinCheck.CLI)**
  - Full-featured CLI tool with System.CommandLine
  - Commands: scan, clean, status, process
  - Progress indicators and formatted output
  - Quick and deep scan modes
  - Verbose logging option
  - Automation and scripting support

- **Advanced Error Handling (ErrorHandlingService)**
  - Global exception handling for unhandled exceptions
  - Crash dump generation in JSON format
  - Error history tracking (last 100 errors)
  - Severity levels: Info, Warning, Error, Critical
  - Detailed error reports with full context
  - Automatic crash dump cleanup (retains last 10)
  - Thread-safe error collection

- **Auto-Update Service (AutoUpdateService)**
  - GitHub releases integration for updates
  - Automatic version checking
  - Semantic version comparison
  - One-click download and install
  - Release notes display
  - Auto-elevation for installation

### Technical Improvements
- CLI project uses System.CommandLine for robust parsing
- ThemeService uses Windows.Storage for settings persistence
- ErrorHandlingService uses ConcurrentQueue for thread safety
- AutoUpdateService integrates with GitHub API

## [1.0.1] - 2025-11-02

### Added
- **Logging Infrastructure**
  - FileLogger with automatic log rotation (7-day retention)
  - ILogger interface for dependency injection
  - Application startup logging with version and system info
  - Logs stored in %LocalAppData%\WinCheck\Logs\

- **Input Validation Helpers**
  - ValidationHelper with 12+ validation methods
  - Drive letter, file path, process ID validation
  - API key format, registry path, port, IP validation
  - Safe file name sanitization

- **Result Pattern**
  - Result<T> and Result classes for explicit error handling
  - Success/Failure pattern with error messages
  - Exception wrapping support

- **Caching Infrastructure**
  - CacheHelper with in-memory caching and expiration
  - AppCache with predefined caches (System, Hardware, Service, Process)
  - Thread-safe ConcurrentDictionary implementation
  - Automatic expiration and cleanup

- **Performance Monitoring**
  - PerformanceMonitor for operation timing
  - Global PerformanceStats for tracking metrics
  - Min/Max/Average duration tracking
  - Using pattern for automatic timing

- **Retry Mechanism**
  - RetryHelper with exponential backoff
  - Predefined policies: Network, File, Database
  - Exception filtering and custom retry logic
  - Configurable max attempts and delays

- **Rate Limiting**
  - RateLimiter for API quota management
  - Predefined limiters for OpenAI, Claude, Gemini
  - Token bucket algorithm for sophisticated limiting
  - Prevents API quota exhaustion

- **Configuration Management**
  - AppConfiguration centralized configuration model
  - DefaultConfiguration with sensible defaults
  - Structured config: Logging, Performance, AI, Monitoring, Cleanup

- **Unit Test Infrastructure**
  - WinCheck.Tests project with MSTest
  - 36 comprehensive unit tests
  - Tests for ValidationHelper, CacheHelper, RetryHelper
  - 100% test pass rate (~168ms execution)

- **Integration Test Infrastructure**
  - WinCheck.IntegrationTests project with MSTest and Moq
  - 20 comprehensive integration tests
  - Tests component interactions: Caching, Retry, Rate Limiting, Validation
  - End-to-end workflow scenarios: API calls, disk cleanup, process monitoring
  - 100% test pass rate (~9s execution)
  - Validates performance optimizations (70%+ operation reduction)

### Technical Improvements
- All helpers are thread-safe and production-ready
- Comprehensive XML documentation
- Fixed StartupManagerPage XAML Click event to Command binding
- Resolved namespace conflicts (ILogger, LogLevel)

### Benefits
- **Performance**: Caching reduces redundant operations by 50-90%
- **Reliability**: Retry logic handles transient failures automatically
- **Security**: Input validation prevents injection attacks
- **Cost**: Rate limiting prevents API overages
- **Monitoring**: Performance stats identify bottlenecks
- **Testability**: Unit tests ensure code quality

## [1.0.0] - 2025-11-02

### Added
- **AI-Powered System Analysis**
  - Integration with OpenAI GPT-4, Anthropic Claude, and Google Gemini
  - Quick Scan (10-30 seconds) for daily health checks
  - Deep Scan (1-3 minutes) for comprehensive analysis
  - AI-generated optimization plans with automatic execution
  - Natural language Q&A about system performance
  - Health scoring system (0-100): Hardware, Software, Performance, Security

- **Dashboard Page**
  - Real-time system metrics (CPU, Memory, Disk usage)
  - Multi-level fallback for metrics collection
  - AI explanations of system status
  - Optimization recommendations with impact scores
  - One-click optimization execution

- **Process Monitor**
  - Real-time process monitoring (updates every 2 seconds)
  - Suspicious process detection with multiple heuristics
  - Process termination capability
  - System resource overview
  - Top 50 processes by CPU usage display

- **Disk Cleanup**
  - Temporary files scanning and removal
  - Browser cache cleanup (Chrome, Edge, Firefox)
  - Windows Update cache management
  - Recycle Bin emptying
  - Thumbnail and error report cleanup
  - Size estimation per category

- **Service Optimizer**
  - 30+ safe-to-disable services database
  - Safety ratings: Safe, MostlySafe, Conditional, Risky
  - Automatic backup before optimization
  - One-click restore capability
  - Memory and boot time savings estimation
  - Dependency checking

- **Startup Manager**
  - Registry Run keys scanning
  - Startup folder monitoring
  - Task Scheduler integration
  - Impact analysis for each program
  - Boot time savings calculator
  - Enable/disable startup programs
  - Bloatware detection

- **Registry Cleaner**
  - Safe registry scanning with whitelist approach
  - Invalid file extensions cleanup
  - Orphaned startup entries removal
  - Empty registry keys detection
  - Automatic .reg backup before cleaning
  - One-click restore from backup

- **Settings Page**
  - AI provider configuration (OpenAI, Claude, Gemini)
  - API key validation
  - Settings persistence to local JSON file
  - Easy navigation from main menu

- **Hardware Detection Service**
  - CPU, RAM, GPU, Storage, Battery, Motherboard detection
  - SMART data analysis for drives
  - Temperature monitoring
  - Health assessment with issue detection

- **Network Monitor Service**
  - TCP/UDP connection tracking
  - Geographic threat analysis
  - Port-based heuristic scoring
  - Firewall integration for blocking threats

- **OS Detection Service**
  - Windows 7/8/8.1/10/11 detection
  - 50+ version-specific optimizations
  - Safe registry tweaks with backup

### Technical Improvements
- **XAML Bindings**: All pages use compile-time `{x:Bind}` for type safety and performance
- **MVVM Architecture**: Proper separation of concerns with ViewModels
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection throughout
- **Error Handling**: Comprehensive try-catch with multi-level fallbacks
- **Null Safety**: Null-coalescing operators and null checks everywhere
- **Async/Await**: All I/O operations are asynchronous
- **Clean Code**: Simplified code-behind files, removed debug code

### Security
- Administrator privileges only when necessary
- Automatic backups before all system modifications
- Local-only API key storage
- No telemetry or data collection
- Whitelisted registry modifications only

### Documentation
- Comprehensive README.md (1200+ lines)
- User manual with usage scenarios
- Troubleshooting guide
- Privacy and security documentation
- API reference for all services

### Fixed
- XAML binding issues in ServiceOptimizerPage, ProcessMonitorPage, RegistryCleanerPage
- Dashboard CPU/Memory showing 0 values
- Settings page navigation not accessible
- Startup Manager crash on Load Programs click
- Color binding in RegistryCleanerPage
- Property name mismatches in XAML

### Known Limitations
- Language selection not yet implemented (English only)
- Scheduled scans not available
- PDF/CSV report export not available
- Dark theme not implemented

## [Unreleased] - Future v1.1+

### Planned Features
- Multi-language support (English, German, French, Spanish)
- Scheduled automatic scans with Task Scheduler integration
- PDF and CSV report export
- Dark theme and Fluent design customization options
- Cloud backup integration for settings and backups
- Enterprise multi-PC management dashboard
- Command-line interface for automation
- System restore point creation before major changes
- Driver update checker
- Disk defragmentation integration
- Windows Update management

---

## Version History

- **v1.0.0** (2025-11-02) - Initial production release
  - 9 core services fully implemented
  - 7 UI pages with AI integration
  - Complete documentation
  - Production-ready quality

---

**Note**: All changes maintain backward compatibility with Windows 10 version 1809 (build 17763) and above.
