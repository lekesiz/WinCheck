# WinCheck Deployment Guide

This guide covers building, packaging, and deploying WinCheck for various scenarios.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Building from Source](#building-from-source)
- [Packaging for Distribution](#packaging-for-distribution)
- [Deployment Methods](#deployment-methods)
- [Automated Build Scripts](#automated-build-scripts)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Development Environment

#### Required Software

1. **Visual Studio 2022 (17.8 or later)** - Recommended
   - Download: [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
   - Required Workloads:
     - ".NET desktop development"
     - "Windows application development"
   - Optional but recommended:
     - "Git for Windows"

2. **OR Visual Studio Code** - Alternative
   - Download: [VS Code](https://code.visualstudio.com/)
   - Required Extensions:
     - C# (Microsoft)
     - .NET Install Tool

3. **.NET 8.0 SDK**
   - Download: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verify installation:
     ```powershell
     dotnet --version
     # Should show: 8.0.x
     ```

4. **Windows App SDK 1.5** (included with Visual Studio workloads)
   - Or install separately: [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)

#### Optional Tools

- **Git** (for cloning repository)
  - Download: [Git for Windows](https://git-scm.com/download/win)

- **Windows Terminal** (better PowerShell experience)
  - Download from Microsoft Store

---

## Building from Source

### Method 1: Visual Studio 2022 (Easiest)

#### Step 1: Clone Repository

```bash
git clone https://github.com/yourusername/wincheck.git
cd wincheck
```

#### Step 2: Open Solution

1. Launch Visual Studio 2022
2. File → Open → Project/Solution
3. Navigate to `WinCheck.sln`
4. Click Open

#### Step 3: Restore Dependencies

Visual Studio automatically restores NuGet packages on first build.

Manual restore:
- Right-click solution in Solution Explorer
- Select "Restore NuGet Packages"

#### Step 4: Build Solution

**Debug Build** (for development):
1. Set build configuration to **Debug**
2. Set platform to **x64**
3. Press `Ctrl+Shift+B` or Build → Build Solution

**Release Build** (for distribution):
1. Set build configuration to **Release**
2. Set platform to **x64**
3. Press `Ctrl+Shift+B`

Build output location:
```
src\WinCheck.App\bin\x64\Release\net8.0-windows10.0.22621.0\
```

#### Step 5: Run Application

1. Set **WinCheck.App** as startup project (right-click → Set as Startup Project)
2. Press `F5` (Debug) or `Ctrl+F5` (Release)

---

### Method 2: Command Line / VS Code

#### Step 1: Clone Repository

```powershell
git clone https://github.com/yourusername/wincheck.git
cd wincheck
```

#### Step 2: Restore Dependencies

```powershell
dotnet restore
```

Expected output:
```
Restore completed in 3.45 sec for C:\...\WinCheck.Core\WinCheck.Core.csproj.
Restore completed in 3.52 sec for C:\...\WinCheck.Infrastructure\WinCheck.Infrastructure.csproj.
Restore completed in 4.12 sec for C:\...\WinCheck.App\WinCheck.App.csproj.
```

#### Step 3: Build Solution

**Debug Build**:
```powershell
dotnet build -c Debug
```

**Release Build**:
```powershell
dotnet build -c Release
```

Build flags explained:
- `-c Release`: Release configuration (optimized)
- `-c Debug`: Debug configuration (with symbols)
- `/p:Platform=x64`: Force x64 architecture

Full command with all options:
```powershell
dotnet build -c Release /p:Platform=x64 --no-incremental
```

#### Step 4: Run Application

**From build output**:
```powershell
cd src\WinCheck.App\bin\x64\Release\net8.0-windows10.0.22621.0\
.\WinCheck.App.exe
```

**Using dotnet run**:
```powershell
cd src\WinCheck.App
dotnet run -c Release
```

---

### Method 3: MSBuild (Advanced)

For users with only MSBuild installed (no full Visual Studio):

```powershell
# Locate MSBuild
$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

# Restore NuGet packages
dotnet restore WinCheck.sln

# Build solution
& $msbuild WinCheck.sln /p:Configuration=Release /p:Platform=x64 /t:Rebuild
```

---

## Packaging for Distribution

### Option 1: Publish Self-Contained (Recommended)

Creates a fully self-contained package with .NET runtime included.

#### Using Command Line

```powershell
dotnet publish src\WinCheck.App\WinCheck.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -p:PublishReadyToRun=true `
  -o publish\win-x64
```

Parameters explained:
- `-c Release`: Release configuration
- `-r win-x64`: Target Windows x64
- `--self-contained true`: Include .NET runtime
- `PublishSingleFile=false`: Keep files separate (faster startup)
- `PublishReadyToRun=true`: AOT compilation for faster startup
- `-o publish\win-x64`: Output directory

Output: `publish\win-x64\` folder containing all files

**Folder size**: ~150-200 MB (includes .NET runtime)

#### Create ZIP Package

```powershell
# Compress to ZIP
Compress-Archive -Path publish\win-x64\* -DestinationPath WinCheck-v1.0.0-win-x64.zip

# Verify ZIP
Get-Item WinCheck-v1.0.0-win-x64.zip
```

Distribution:
1. Upload ZIP to GitHub Releases
2. Users extract and run `WinCheck.App.exe`
3. No installation required

---

### Option 2: Framework-Dependent Deployment

Smaller package size, requires .NET 8.0 Runtime on target PC.

```powershell
dotnet publish src\WinCheck.App\WinCheck.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -o publish\framework-dependent
```

**Folder size**: ~5-10 MB (excludes .NET runtime)

**Prerequisite**: Users must install [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

---

### Option 3: Single-File Deployment

Everything in one executable (slower startup, but portable).

```powershell
dotnet publish src\WinCheck.App\WinCheck.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish\single-file
```

Output: Single `WinCheck.App.exe` (~150 MB)

**Pros**:
- One file to distribute
- No extraction needed

**Cons**:
- Slower first startup (extracts to temp folder)
- Larger file size

---

### Option 4: MSIX Package (Microsoft Store Ready)

Creates installable package for Microsoft Store or sideloading.

#### Prerequisites

1. Certificate for signing (self-signed for testing, purchased for production)
2. MSIX Packaging Tool or Visual Studio

#### Create MSIX with Visual Studio

1. Right-click **WinCheck.App** project
2. Select **Publish** → **Create App Packages**
3. Choose **Sideloading** or **Microsoft Store**
4. Configure:
   - Version: 1.0.0.0
   - Architecture: x64
   - Certificate: Select or create
5. Click **Create**

Output: `.msix` or `.msixbundle` file

#### Install MSIX Locally

```powershell
# Install package
Add-AppxPackage -Path WinCheck_1.0.0.0_x64.msix

# Launch app
Start-Process shell:AppsFolder\<PackageFamilyName>!App
```

#### Distribute MSIX

**For Testing** (Sideloading):
1. Export certificate (.cer file)
2. Share both `.msix` and `.cer` files
3. Users install certificate, then `.msix`

**For Production** (Microsoft Store):
1. Create Developer account
2. Upload `.msixupload` package
3. Pass certification
4. Publish to Store

---

## Deployment Methods

### Method 1: GitHub Releases (Recommended for Open Source)

#### Step 1: Create Release Build

```powershell
# Build and publish
dotnet publish src\WinCheck.App\WinCheck.App.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishReadyToRun=true -o publish\release

# Create ZIP
Compress-Archive -Path publish\release\* `
  -DestinationPath WinCheck-v1.0.0-win-x64.zip
```

#### Step 2: Create GitHub Release

1. Go to your GitHub repository
2. Click **Releases** → **Draft a new release**
3. Tag: `v1.0.0`
4. Title: `WinCheck v1.0.0 - Initial Release`
5. Description: (see template below)
6. Attach `WinCheck-v1.0.0-win-x64.zip`
7. Click **Publish release**

**Release Notes Template**:
```markdown
## WinCheck v1.0.0

### Features
- AI-powered system analysis (OpenAI, Claude, Gemini)
- Real-time process monitoring
- Disk cleanup and optimization
- Service optimization
- Startup program management
- Registry cleaning

### System Requirements
- Windows 10 (22H2) or Windows 11
- No installation required (self-contained)

### Installation
1. Download `WinCheck-v1.0.0-win-x64.zip`
2. Extract to any folder
3. Run `WinCheck.App.exe`
4. Configure AI provider in Settings

### Known Issues
- XAML hot reload not supported
- Requires administrator rights for some features

**Full Changelog**: https://github.com/yourusername/wincheck/compare/v0.9.0...v1.0.0
```

#### Step 3: Users Download and Run

Users can:
1. Visit Releases page
2. Download latest ZIP
3. Extract and run

No installation wizard needed!

---

### Method 2: Direct Download (Website/Cloud Storage)

Host the ZIP file on:
- **GitHub Releases** (recommended, free, version controlled)
- **OneDrive/Google Drive** (easy, but version management manual)
- **Your own website** (full control)

Example download page:
```html
<h2>Download WinCheck</h2>
<a href="WinCheck-v1.0.0-win-x64.zip">
  Download WinCheck v1.0.0 (152 MB)
</a>

<h3>System Requirements</h3>
<ul>
  <li>Windows 10 (build 19045) or later</li>
  <li>150 MB disk space</li>
</ul>

<h3>Installation</h3>
<ol>
  <li>Download ZIP file</li>
  <li>Extract to C:\Program Files\WinCheck</li>
  <li>Run WinCheck.App.exe</li>
</ol>
```

---

### Method 3: Microsoft Store (Production)

#### Advantages
- Automatic updates
- User trust (verified publisher)
- Easy discovery
- Sandboxed environment

#### Requirements
1. Microsoft Partner Center account ($19 one-time fee)
2. MSIX package (signed)
3. App certification (2-3 days review)

#### Steps
1. Register at [Microsoft Partner Center](https://partner.microsoft.com/)
2. Create new app submission
3. Upload MSIX package
4. Provide screenshots, description
5. Submit for certification
6. Wait 2-3 business days
7. App published to Microsoft Store

**Update process**: Upload new MSIX with incremented version number.

---

### Method 4: Enterprise Deployment (SCCM/Intune)

For corporate environments.

#### SCCM (System Center Configuration Manager)

1. Package application as `.msi` or MSIX
2. Import to SCCM
3. Distribute to target devices
4. Deploy silently

#### Microsoft Intune

1. Upload `.msix` or `.intunewin` package
2. Configure deployment settings
3. Assign to device groups
4. Deploy automatically

**Silent install command** (if using MSI):
```powershell
msiexec /i WinCheck.msi /quiet /norestart
```

---

## Automated Build Scripts

### PowerShell Build Script

Create `build.ps1` in repository root:

```powershell
# build.ps1 - Automated WinCheck Build Script

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "dist",
    [string]$Version = "1.0.0"
)

Write-Host "=== WinCheck Build Script ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Version: $Version" -ForegroundColor Yellow

# Clean previous builds
Write-Host "`nCleaning previous builds..." -ForegroundColor Green
Remove-Item -Path $OutputDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.App\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.App\obj" -Recurse -Force -ErrorAction SilentlyContinue

# Restore dependencies
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Green
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: NuGet restore failed!" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "`nBuilding solution..." -ForegroundColor Green
dotnet build -c $Configuration --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}

# Publish self-contained
Write-Host "`nPublishing self-contained package..." -ForegroundColor Green
dotnet publish src\WinCheck.App\WinCheck.App.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -o "$OutputDir\win-x64"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Publish failed!" -ForegroundColor Red
    exit 1
}

# Create ZIP package
Write-Host "`nCreating ZIP package..." -ForegroundColor Green
$zipFile = "WinCheck-v$Version-win-x64.zip"
Compress-Archive -Path "$OutputDir\win-x64\*" -DestinationPath $zipFile -Force

# Summary
Write-Host "`n=== Build Complete ===" -ForegroundColor Cyan
Write-Host "Output: $zipFile" -ForegroundColor Green
Write-Host "Size: $((Get-Item $zipFile).Length / 1MB) MB" -ForegroundColor Yellow
Write-Host "`nTo test: Extract $zipFile and run WinCheck.App.exe" -ForegroundColor Yellow
```

**Usage**:
```powershell
# Default (Release, v1.0.0)
.\build.ps1

# Custom version
.\build.ps1 -Version "1.1.0"

# Debug build
.\build.ps1 -Configuration Debug
```

---

### Batch Build Script

Create `build.bat` for simple double-click builds:

```batch
@echo off
echo === WinCheck Build Script ===
echo.

REM Clean
echo Cleaning...
rmdir /s /q dist 2>nul
dotnet clean

REM Restore
echo Restoring packages...
dotnet restore
if errorlevel 1 goto error

REM Build
echo Building...
dotnet build -c Release
if errorlevel 1 goto error

REM Publish
echo Publishing...
dotnet publish src\WinCheck.App\WinCheck.App.csproj ^
  -c Release -r win-x64 --self-contained true ^
  -p:PublishReadyToRun=true -o dist\win-x64
if errorlevel 1 goto error

echo.
echo === Build Complete ===
echo Output: dist\win-x64\WinCheck.App.exe
pause
exit /b 0

:error
echo.
echo === Build Failed ===
pause
exit /b 1
```

**Usage**: Double-click `build.bat`

---

### GitHub Actions CI/CD

Create `.github/workflows/build.yml`:

```yaml
name: Build and Release

on:
  push:
    tags:
      - 'v*'
  workflow_dispatch:

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build -c Release --no-restore

    - name: Publish
      run: |
        dotnet publish src/WinCheck.App/WinCheck.App.csproj `
          -c Release -r win-x64 --self-contained true `
          -p:PublishReadyToRun=true -o publish/win-x64

    - name: Create ZIP
      run: |
        Compress-Archive -Path publish/win-x64/* `
          -DestinationPath WinCheck-${{ github.ref_name }}-win-x64.zip

    - name: Create Release
      uses: softprops/action-gh-release@v1
      if: startsWith(github.ref, 'refs/tags/')
      with:
        files: WinCheck-*.zip
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

**Trigger**: Push git tag:
```bash
git tag v1.0.0
git push origin v1.0.0
```

GitHub automatically builds and creates release!

---

## Troubleshooting

### Build Error: "SDK not found"

**Error**:
```
error MSB4236: The SDK 'Microsoft.NET.Sdk' specified could not be found.
```

**Solution**:
1. Install .NET 8.0 SDK
2. Verify: `dotnet --version`
3. Restart terminal/IDE

---

### Build Error: "Windows App SDK missing"

**Error**:
```
error : The project depends on the following workload packs that are not installed: Microsoft.WindowsAppSDK
```

**Solution** (Visual Studio):
1. Tools → Get Tools and Features
2. Select "Windows application development" workload
3. Install

**Solution** (Command Line):
```powershell
dotnet workload install microsoft-windows-sdk-net-ref-pack-10.0.22621
dotnet workload install microsoft-net-sdk-windowsdesktop
```

---

### Publish Error: "Access Denied"

**Error**:
```
Access to the path '...\publish\win-x64\WinCheck.App.exe' is denied.
```

**Causes**:
1. Application is running (close it)
2. Antivirus scanning file (add exclusion)
3. File is read-only

**Solution**:
```powershell
# Stop all WinCheck processes
Get-Process WinCheck.App -ErrorAction SilentlyContinue | Stop-Process -Force

# Remove read-only
Get-ChildItem -Path publish -Recurse | ForEach-Object { $_.IsReadOnly = $false }

# Retry publish
dotnet publish ...
```

---

### ZIP Error: File too large

**Error**:
```
Compress-Archive : The file '...' exceeds the maximum size of 2GB
```

**Solution**: Use 7-Zip instead:
```powershell
# Install 7-Zip
winget install 7zip.7zip

# Create ZIP
& "C:\Program Files\7-Zip\7z.exe" a -tzip WinCheck-v1.0.0.zip .\publish\win-x64\*
```

---

### Runtime Error on Target PC

**Error**:
```
This application requires the .NET Desktop Runtime 8.0
```

**Solution**:
- **If self-contained**: Shouldn't happen. Verify publish used `--self-contained true`
- **If framework-dependent**: User must install .NET 8.0 Runtime

Provide users with:
1. [Download .NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Select "Desktop Runtime" (not SDK)
3. Install and run WinCheck again

---

## Version Management

### Updating Version Number

Version is defined in: `src\WinCheck.App\WinCheck.App.csproj`

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
  <FileVersion>1.0.0.0</FileVersion>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
</PropertyGroup>
```

**Before each release**:
1. Update version number (follow semantic versioning: MAJOR.MINOR.PATCH)
2. Commit change
3. Create git tag: `git tag v1.0.0`
4. Push tag: `git push origin v1.0.0`

---

## Production Checklist

Before releasing version 1.0.0:

- [ ] Update version number in `.csproj`
- [ ] Update `README.md` with latest features
- [ ] Update `CHANGELOG.md` with all changes
- [ ] Build Release configuration
- [ ] Test on clean Windows 10 VM
- [ ] Test on clean Windows 11 VM
- [ ] Verify all AI providers work
- [ ] Test without administrator rights (features gracefully fail)
- [ ] Scan executable with antivirus (VirusTotal)
- [ ] Create SHA256 checksum for ZIP file
- [ ] Update documentation screenshots
- [ ] Create GitHub release with:
  - Version tag (v1.0.0)
  - Release notes
  - ZIP file
  - SHA256 checksum
- [ ] Announce release (social media, forums, etc.)

**SHA256 Checksum**:
```powershell
Get-FileHash WinCheck-v1.0.0-win-x64.zip -Algorithm SHA256
```

Include in release notes for users to verify download integrity.

---

**Last Updated**: November 2025
**Version**: 1.0.0
