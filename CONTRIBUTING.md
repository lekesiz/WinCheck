# Contributing to WinCheck

Thank you for your interest in contributing to WinCheck! This document provides guidelines and instructions for contributing to the project.

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)
- [Architecture Overview](#architecture-overview)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors, regardless of:
- Experience level
- Gender identity and expression
- Sexual orientation
- Disability
- Personal appearance
- Body size
- Race
- Ethnicity
- Age
- Religion
- Nationality

### Expected Behavior

- **Be respectful**: Use welcoming and inclusive language
- **Be collaborative**: Accept constructive criticism gracefully
- **Be professional**: Focus on what's best for the project and community
- **Be patient**: Remember that everyone was a beginner once

### Unacceptable Behavior

- Trolling, insulting/derogatory comments, personal or political attacks
- Public or private harassment
- Publishing others' private information without permission
- Other conduct which could reasonably be considered inappropriate

---

## Getting Started

### Prerequisites

1. **Install Development Tools**:
   - [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community Edition is free)
   - OR [Visual Studio Code](https://code.visualstudio.com/) + [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

2. **Fork and Clone**:
   ```bash
   # Fork repository on GitHub (click "Fork" button)

   # Clone your fork
   git clone https://github.com/YOUR-USERNAME/wincheck.git
   cd wincheck

   # Add upstream remote
   git remote add upstream https://github.com/lekesiz/wincheck.git
   ```

3. **Set Up Development Environment**:
   ```bash
   # Restore NuGet packages
   dotnet restore

   # Build solution
   dotnet build -c Debug

   # Run application
   cd src\WinCheck.App
   dotnet run
   ```

4. **Configure AI Provider** (for testing):
   - Get API key from OpenAI/Claude/Gemini
   - Add to Settings in running application
   - See [API Setup Guide](docs/API_SETUP_GUIDE.md)

---

## Development Workflow

### 1. Create Feature Branch

```bash
# Sync with upstream
git fetch upstream
git checkout main
git merge upstream/main

# Create feature branch
git checkout -b feature/your-feature-name
```

**Branch naming conventions**:
- `feature/feature-name` - New features
- `fix/bug-name` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring
- `test/description` - Test additions/improvements

### 2. Make Changes

- Write code following [Coding Standards](#coding-standards)
- Add/update tests
- Update documentation
- Commit frequently with clear messages

### 3. Commit Guidelines

**Commit message format**:
```
<type>: <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Formatting, missing semicolons, etc.
- `refactor`: Code restructuring without behavior change
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Example**:
```
feat: Add duplicate file detection to DiskCleanupService

- Implement MD5 hash-based duplicate detection
- Add DuplicateFileGroup model
- Update UI to display duplicate files with selectable actions

Closes #42
```

### 4. Push and Create Pull Request

```bash
# Push to your fork
git push origin feature/your-feature-name

# Create PR on GitHub
# Compare: lekesiz/wincheck:main <- YOUR-USERNAME/wincheck:feature/your-feature-name
```

---

## Coding Standards

### C# Style Guidelines

We follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) with these additions:

#### Naming Conventions

```csharp
// Classes, methods, properties: PascalCase
public class DiskCleanupService { }
public void AnalyzeDisk() { }
public string StatusMessage { get; set; }

// Private fields: _camelCase with underscore prefix
private readonly IDiskCleanupService _diskCleanup;
private bool _isScanning;

// Local variables, parameters: camelCase
var totalSize = 0;
public void ProcessFile(string fileName) { }

// Constants: PascalCase
public const int MaxRetryCount = 3;

// Interfaces: IPascalCase
public interface IAIProvider { }
```

#### Code Organization

```csharp
// Order of members:
public class ExampleViewModel : ObservableObject
{
    // 1. Constants
    private const int DefaultTimeout = 5000;

    // 2. Private fields
    private readonly IExampleService _service;

    // 3. Observable properties (CommunityToolkit.Mvvm)
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // 4. Public properties
    public ObservableCollection<string> Items { get; } = new();

    // 5. Constructor(s)
    public ExampleViewModel(IExampleService service)
    {
        _service = service;
    }

    // 6. Relay commands (CommunityToolkit.Mvvm)
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // Implementation
    }

    // 7. Private methods
    private void HelperMethod()
    {
        // Implementation
    }
}
```

#### Async/Await Patterns

```csharp
// ✅ GOOD: Async method with Async suffix
public async Task<DiskAnalysis> AnalyzeDiskAsync()
{
    var result = await _service.GetDataAsync();
    return result;
}

// ❌ BAD: Missing Async suffix
public async Task<DiskAnalysis> AnalyzeDisk() { }

// ✅ GOOD: ConfigureAwait(false) in library code
public async Task<string> GetDataAsync()
{
    var response = await httpClient.GetStringAsync(url).ConfigureAwait(false);
    return response;
}
```

#### Exception Handling

```csharp
// ✅ GOOD: Specific exceptions with helpful messages
try
{
    await _diskCleanup.CleanAsync();
}
catch (UnauthorizedAccessException ex)
{
    StatusMessage = "Administrator rights required. Please run as administrator.";
    _logger.LogError(ex, "Access denied during cleanup");
}
catch (IOException ex)
{
    StatusMessage = $"File operation failed: {ex.Message}";
    _logger.LogError(ex, "I/O error during cleanup");
}

// ❌ BAD: Catching all exceptions
catch (Exception ex)
{
    // Too generic!
}
```

#### Null Handling

```csharp
// ✅ GOOD: Null-conditional operator
var fileName = file?.Name ?? "Unknown";

// ✅ GOOD: Null check before use
if (selectedProcess != null)
{
    await TerminateProcessAsync(selectedProcess.ProcessId);
}

// ✅ GOOD: Nullable reference types enabled
public string? OptionalMessage { get; set; }
```

### XAML Style Guidelines

```xml
<!-- ✅ GOOD: Proper indentation, grouped properties -->
<Button
    Content="Scan Disk"
    Command="{x:Bind ViewModel.ScanCommand}"
    IsEnabled="{x:Bind ViewModel.IsScanning, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}"
    Style="{StaticResource AccentButtonStyle}"
    Margin="0,8,0,0"
    HorizontalAlignment="Stretch" />

<!-- ✅ GOOD: x:Bind instead of Binding (better performance) -->
<TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />

<!-- ❌ BAD: Old-style Binding -->
<TextBlock Text="{Binding StatusMessage}" />
```

### Comments and Documentation

```csharp
/// <summary>
/// Analyzes disk usage and identifies cleanable categories.
/// </summary>
/// <param name="driveLetter">The drive letter to analyze (e.g., "C")</param>
/// <returns>A <see cref="DiskAnalysis"/> containing cleanup categories and size information.</returns>
/// <exception cref="UnauthorizedAccessException">Thrown when access to system folders is denied.</exception>
public async Task<DiskAnalysis> AnalyzeDiskAsync(string driveLetter)
{
    // Complex logic deserves inline comments
    // Calculate total size by iterating each category
    long totalBytes = 0;
    foreach (var category in analysis.CleanupCategories)
    {
        totalBytes += category.SizeBytes;
    }

    return analysis;
}
```

**When to add comments**:
- ✅ Public API methods (XML documentation required)
- ✅ Complex algorithms or business logic
- ✅ Workarounds for known issues
- ✅ TODO/HACK/FIXME markers with issue numbers

**When NOT to add comments**:
- ❌ Self-explanatory code
- ❌ Repeating what the code already says

---

## Testing Guidelines

### Unit Tests

Create tests in `src\WinCheck.Tests\` following this structure:

```
WinCheck.Tests/
├── Services/
│   ├── DiskCleanupServiceTests.cs
│   ├── ProcessMonitorServiceTests.cs
│   └── ...
├── ViewModels/
│   ├── DashboardViewModelTests.cs
│   └── ...
└── Helpers/
    └── ...
```

**Test naming convention**:
```csharp
[TestClass]
public class DiskCleanupServiceTests
{
    // MethodName_StateUnderTest_ExpectedBehavior
    [TestMethod]
    public async Task AnalyzeDiskAsync_WithValidDrive_ReturnsAnalysis()
    {
        // Arrange
        var service = new DiskCleanupService();

        // Act
        var result = await service.AnalyzeDiskAsync("C");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.CleanupCategories.Count > 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task AnalyzeDiskAsync_WithInvalidDrive_ThrowsException()
    {
        // Arrange
        var service = new DiskCleanupService();

        // Act
        await service.AnalyzeDiskAsync("Z"); // Invalid drive

        // Assert handled by ExpectedException
    }
}
```

### Manual Testing Checklist

Before submitting PR, manually test:

**Dashboard**:
- [ ] Quick Scan completes successfully
- [ ] Deep Scan completes successfully
- [ ] Optimize executes plan
- [ ] Ask AI returns response
- [ ] All three AI providers work (OpenAI, Claude, Gemini)

**Process Monitor**:
- [ ] Start Monitoring shows processes
- [ ] Suspicious process detection works
- [ ] Terminate process works (with admin rights)
- [ ] Stop Monitoring clears list

**Disk Cleanup**:
- [ ] Scan shows categories and sizes
- [ ] Clean removes files and shows total freed
- [ ] No errors when run as admin

**Service Optimizer**:
- [ ] Load Services populates list
- [ ] Optimize modifies services
- [ ] Create Backup saves .reg file
- [ ] Restore Backup works

**Startup Manager**:
- [ ] Load Programs shows startup items
- [ ] Enable/Disable toggles programs

**Registry Cleaner**:
- [ ] Scan finds issues
- [ ] Clean removes issues and creates backup
- [ ] Restore works

**Settings**:
- [ ] API keys validate correctly
- [ ] Provider selection switches active provider
- [ ] Settings persist after restart

---

## Pull Request Process

### Before Submitting PR

1. **Sync with upstream**:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Run full build**:
   ```bash
   dotnet build -c Release
   ```

3. **Fix any compiler warnings** (goal: zero warnings)

4. **Run tests** (when available):
   ```bash
   dotnet test
   ```

5. **Update documentation** if changing:
   - Public APIs
   - User-facing features
   - Configuration options

### PR Template

When creating PR, use this template:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Related Issues
Closes #123

## Testing
- [ ] Tested locally on Windows 10
- [ ] Tested locally on Windows 11
- [ ] Tested with OpenAI provider
- [ ] Tested with Claude provider
- [ ] Tested with Gemini provider
- [ ] Manual testing performed (see checklist in [CONTRIBUTING.md])

## Screenshots (if applicable)
[Add screenshots of UI changes]

## Checklist
- [ ] My code follows the style guidelines of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have updated documentation (README.md, code comments, etc.)
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
```

### PR Review Process

1. **Automated checks** (GitHub Actions):
   - Build succeeds
   - Tests pass (when available)

2. **Code review**:
   - At least 1 maintainer approval required
   - Address all review comments

3. **Merge**:
   - Maintainer will merge using "Squash and merge"
   - Your commits will be squashed into single commit

---

## Issue Reporting

### Bug Reports

Use this template when reporting bugs:

```markdown
**Describe the bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

**Expected behavior**
A clear and concise description of what you expected to happen.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Environment (please complete the following information):**
 - OS: [e.g. Windows 11 22H2]
 - WinCheck Version: [e.g. 1.0.0]
 - AI Provider: [e.g. OpenAI]

**Additional context**
Add any other context about the problem here.
```

### Feature Requests

Use this template for feature requests:

```markdown
**Is your feature request related to a problem? Please describe.**
A clear and concise description of what the problem is. Ex. I'm always frustrated when [...]

**Describe the solution you'd like**
A clear and concise description of what you want to happen.

**Describe alternatives you've considered**
A clear and concise description of any alternative solutions or features you've considered.

**Additional context**
Add any other context or screenshots about the feature request here.
```

---

## Architecture Overview

### Project Structure

```
WinCheck/
├── src/
│   ├── WinCheck.App/              # WinUI 3 UI Layer
│   │   ├── Views/                 # XAML Pages
│   │   ├── ViewModels/            # MVVM ViewModels
│   │   ├── Converters/            # Value Converters
│   │   └── App.xaml.cs            # DI Configuration
│   │
│   ├── WinCheck.Core/             # Core Layer (Interfaces, Models)
│   │   ├── Interfaces/            # Service contracts
│   │   └── Models/                # Data models, DTOs
│   │
│   └── WinCheck.Infrastructure/   # Infrastructure Layer
│       ├── Services/              # Service implementations
│       └── AI/                    # AI provider implementations
│
├── docs/                          # Documentation
├── tests/                         # Unit/Integration tests
└── README.md
```

### Dependency Flow

```
WinCheck.App
    ↓ depends on
WinCheck.Infrastructure
    ↓ depends on
WinCheck.Core
```

**Rules**:
- ✅ UI can reference Infrastructure and Core
- ✅ Infrastructure can reference Core
- ❌ Core should NOT reference Infrastructure or UI
- ❌ No circular dependencies

### Design Patterns Used

1. **MVVM (Model-View-ViewModel)**:
   - Views: XAML files
   - ViewModels: C# classes with ObservableObject
   - Models: Data classes in WinCheck.Core

2. **Dependency Injection**:
   - All services registered in `App.xaml.cs`
   - ViewModels receive services via constructor injection

3. **Repository Pattern** (planned):
   - For settings persistence
   - For backup management

4. **Strategy Pattern**:
   - AI providers implement `IAIProvider` interface
   - Runtime selection based on user settings

### Adding a New Feature

#### Example: Add Startup Impact Graph

1. **Create Model** (WinCheck.Core/Models/):
   ```csharp
   public class StartupImpactData
   {
       public string ProgramName { get; set; }
       public double ImpactScore { get; set; }
       public TimeSpan BootDelay { get; set; }
   }
   ```

2. **Update Service Interface** (WinCheck.Core/Interfaces/):
   ```csharp
   public interface IStartupManagerService
   {
       // Existing methods...

       Task<List<StartupImpactData>> GetImpactGraphDataAsync();
   }
   ```

3. **Implement Service** (WinCheck.Infrastructure/Services/):
   ```csharp
   public class StartupManagerService : IStartupManagerService
   {
       public async Task<List<StartupImpactData>> GetImpactGraphDataAsync()
       {
           // Implementation
       }
   }
   ```

4. **Update ViewModel** (WinCheck.App/ViewModels/):
   ```csharp
   public partial class StartupManagerViewModel : ObservableObject
   {
       [ObservableProperty]
       private ObservableCollection<StartupImpactData> _impactData = new();

       [RelayCommand]
       private async Task LoadImpactGraphAsync()
       {
           var data = await _startupService.GetImpactGraphDataAsync();
           ImpactData.Clear();
           foreach (var item in data)
           {
               ImpactData.Add(item);
           }
       }
   }
   ```

5. **Update View** (WinCheck.App/Views/):
   ```xml
   <Page x:Class="WinCheck.App.Views.StartupManagerPage">
       <!-- Existing UI -->

       <charts:CartesianChart ItemsSource="{x:Bind ViewModel.ImpactData, Mode=OneWay}" />
   </Page>
   ```

6. **Update Tests**:
   ```csharp
   [TestMethod]
   public async Task GetImpactGraphDataAsync_ReturnsData()
   {
       var service = new StartupManagerService();
       var result = await service.GetImpactGraphDataAsync();
       Assert.IsTrue(result.Count > 0);
   }
   ```

7. **Update Documentation**:
   - README.md: Add feature description
   - User manual: Add usage instructions

---

## Community

### Communication Channels

- **GitHub Issues**: Bug reports, feature requests
- **GitHub Discussions**: General questions, ideas
- **Pull Requests**: Code contributions

### Recognition

Contributors are recognized in:
- `README.md` Contributors section
- Release notes for version they contributed to
- GitHub insights

Thank you for contributing to WinCheck! 🎉

---

**Questions?** Open a [GitHub Discussion](https://github.com/lekesiz/wincheck/discussions)

**Last Updated**: November 2025
