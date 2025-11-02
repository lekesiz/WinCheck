# WinCheck Build Script
# Automated build and packaging for WinCheck v1.0.0

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "publish",
    [string]$Version = "1.0.0"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   WinCheck Build Script v1.0.0     " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Output: $OutputDir\win-x64" -ForegroundColor Yellow
Write-Host ""

# Step 1: Clean previous builds
Write-Host "[1/6] Cleaning previous builds..." -ForegroundColor Green
Remove-Item -Path $OutputDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.App\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.App\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.Core\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.Core\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.Infrastructure\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src\WinCheck.Infrastructure\obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   Clean complete" -ForegroundColor Gray

# Step 2: Restore NuGet packages
Write-Host ""
Write-Host "[2/6] Restoring NuGet packages..." -ForegroundColor Green
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: NuGet restore failed!" -ForegroundColor Red
    exit 1
}
Write-Host "   Restore complete" -ForegroundColor Gray

# Step 3: Build solution
Write-Host ""
Write-Host "[3/6] Building solution..." -ForegroundColor Green
dotnet build -c $Configuration /p:Platform=x64 --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "   Build complete" -ForegroundColor Gray

# Step 4: Publish self-contained
Write-Host ""
Write-Host "[4/6] Publishing self-contained package..." -ForegroundColor Green
dotnet publish src\WinCheck.App\WinCheck.App.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:Platform=x64 `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -o "$OutputDir\win-x64"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "   Publish complete" -ForegroundColor Gray

# Step 5: Create ZIP package
Write-Host ""
Write-Host "[5/6] Creating ZIP package..." -ForegroundColor Green
$zipFile = "WinCheck-v$Version-win-x64.zip"
Remove-Item -Path $zipFile -Force -ErrorAction SilentlyContinue
Compress-Archive -Path "$OutputDir\win-x64\*" -DestinationPath $zipFile -Force

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: ZIP creation failed!" -ForegroundColor Red
    exit 1
}
Write-Host "   ZIP created: $zipFile" -ForegroundColor Gray

# Step 6: Calculate SHA256
Write-Host ""
Write-Host "[6/6] Calculating SHA256 checksum..." -ForegroundColor Green
$hash = Get-FileHash $zipFile -Algorithm SHA256
Write-Host "   SHA256: $($hash.Hash)" -ForegroundColor Gray

# Summary
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "         Build Complete!             " -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package Details:" -ForegroundColor Yellow
Write-Host "   File: $zipFile" -ForegroundColor White
Write-Host "   Size: $([math]::Round((Get-Item $zipFile).Length / 1MB, 2)) MB" -ForegroundColor White
Write-Host "   SHA256: $($hash.Hash)" -ForegroundColor White
Write-Host ""
Write-Host "Distribution folder: $OutputDir\win-x64" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Test: Extract $zipFile and run WinCheck.App.exe" -ForegroundColor White
Write-Host "   2. Upload to GitHub Releases: https://github.com/lekesiz/WinCheck/releases/new" -ForegroundColor White
Write-Host "   3. Tag: v$Version" -ForegroundColor White
Write-Host ""
