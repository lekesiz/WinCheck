using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;

namespace WinCheck.Core.Services;

/// <summary>
/// Automatic update service using GitHub releases
/// </summary>
public class AutoUpdateService
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private const string GitHubApiUrl = "https://api.github.com/repos/{OWNER}/{REPO}/releases/latest";
    private const string CurrentVersion = "1.0.1";

    public AutoUpdateService(ILogger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "WinCheck-AutoUpdate");
    }

    /// <summary>
    /// Checks if a new version is available
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdatesAsync(string owner, string repo)
    {
        try
        {
            var url = GitHubApiUrl.Replace("{OWNER}", owner).Replace("{REPO}", repo);
            var response = await _httpClient.GetStringAsync(url);
            var release = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (release == null) return null;

            var latestVersion = release.TagName?.TrimStart('v');
            var currentVersion = CurrentVersion;

            if (IsNewerVersion(currentVersion, latestVersion))
            {
                return new UpdateInfo
                {
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion ?? "unknown",
                    ReleaseNotes = release.Body ?? string.Empty,
                    DownloadUrl = release.Assets?.FirstOrDefault()?.BrowserDownloadUrl ?? string.Empty,
                    PublishedAt = release.PublishedAt
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.Log($"Update check failed: {ex.Message}", "AutoUpdate");
            return null;
        }
    }

    /// <summary>
    /// Downloads and installs the update
    /// </summary>
    public async Task<bool> DownloadAndInstallUpdateAsync(string downloadUrl)
    {
        try
        {
            _logger.Log("Downloading update...", "AutoUpdate");

            var tempPath = Path.GetTempPath();
            var installerPath = Path.Combine(tempPath, "WinCheck_Update.exe");

            // Download installer
            var fileBytes = await _httpClient.GetByteArrayAsync(downloadUrl);
            await File.WriteAllBytesAsync(installerPath, fileBytes);

            _logger.Log("Update downloaded. Starting installer...", "AutoUpdate");

            // Start installer
            var startInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true,
                Verb = "runas" // Run as administrator
            };

            Process.Start(startInfo);

            // Exit current application
            Environment.Exit(0);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Log($"Update installation failed: {ex.Message}", "AutoUpdate");
            return false;
        }
    }

    /// <summary>
    /// Compares two version strings
    /// </summary>
    private bool IsNewerVersion(string? current, string? latest)
    {
        if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(latest))
            return false;

        try
        {
            var currentParts = current.Split('.').Select(int.Parse).ToArray();
            var latestParts = latest.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Min(currentParts.Length, latestParts.Length); i++)
            {
                if (latestParts[i] > currentParts[i]) return true;
                if (latestParts[i] < currentParts[i]) return false;
            }

            return latestParts.Length > currentParts.Length;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Update information
/// </summary>
public class UpdateInfo
{
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}

/// <summary>
/// GitHub release JSON model
/// </summary>
internal class GitHubRelease
{
    public string? TagName { get; set; }
    public string? Name { get; set; }
    public string? Body { get; set; }
    public DateTime PublishedAt { get; set; }
    public List<GitHubAsset>? Assets { get; set; }
}

internal class GitHubAsset
{
    public string? Name { get; set; }
    public string? BrowserDownloadUrl { get; set; }
}
