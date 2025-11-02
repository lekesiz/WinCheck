using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.App.ViewModels;

public partial class RegistryCleanerViewModel : ObservableObject
{
    private readonly IRegistryCleanerService _registryService;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isCleaning;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _totalIssues;

    [ObservableProperty]
    private int _selectedIssuesCount;

    [ObservableProperty]
    private int _highSeverityCount;

    [ObservableProperty]
    private int _mediumSeverityCount;

    [ObservableProperty]
    private int _lowSeverityCount;

    [ObservableProperty]
    private DateTime _lastScanDate;

    [ObservableProperty]
    private TimeSpan _lastScanDuration;

    [ObservableProperty]
    private ObservableCollection<RegistryIssueViewModel> _issues = new();

    [ObservableProperty]
    private RegistryIssueViewModel? _selectedIssue;

    [ObservableProperty]
    private bool _selectAll = true;

    [ObservableProperty]
    private string? _lastBackupPath;

    public RegistryCleanerViewModel(IRegistryCleanerService registryService)
    {
        _registryService = registryService;
    }

    [RelayCommand]
    private async Task ScanRegistryAsync()
    {
        try
        {
            IsScanning = true;
            StatusMessage = "Scanning registry for issues...";

            var result = await _registryService.ScanRegistryAsync();

            Issues.Clear();
            foreach (var issue in result.Issues.OrderByDescending(i => i.Severity).ThenBy(i => i.Type))
            {
                Issues.Add(new RegistryIssueViewModel(issue) { IsSelected = true });
            }

            TotalIssues = result.TotalIssuesFound;
            LastScanDate = result.ScanDate;
            LastScanDuration = result.ScanDuration;

            UpdateSeverityCounts();
            UpdateSelectedCount();

            StatusMessage = $"Found {TotalIssues} registry issues in {LastScanDuration.TotalSeconds:F1} seconds";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error scanning registry: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task CleanRegistryAsync()
    {
        try
        {
            IsCleaning = true;
            StatusMessage = "Creating backup and cleaning registry...";

            // Get selected issues
            var selectedIssues = Issues.Where(i => i.IsSelected).Select(i => i.Issue).ToList();

            if (selectedIssues.Count == 0)
            {
                StatusMessage = "No issues selected for cleanup";
                return;
            }

            // Create backup first
            var backupPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WinCheck",
                "Backups",
                $"Registry_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg"
            );

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(backupPath)!);
            await _registryService.CreateBackupAsync(backupPath);
            LastBackupPath = backupPath;

            // Clean registry
            var result = await _registryService.CleanRegistryAsync(selectedIssues);

            if (result.Success)
            {
                // Remove fixed issues from list
                var fixedIssues = selectedIssues.Take(result.IssuesFixed).Select(i => i.Id).ToHashSet();
                var remainingIssues = Issues.Where(i => !fixedIssues.Contains(i.Issue.Id)).ToList();

                Issues.Clear();
                foreach (var issue in remainingIssues)
                {
                    Issues.Add(issue);
                }

                TotalIssues = Issues.Count;
                UpdateSeverityCounts();
                UpdateSelectedCount();

                StatusMessage = $"Successfully fixed {result.IssuesFixed} issues. Backup saved to: {backupPath}";
            }
            else
            {
                StatusMessage = $"Cleanup completed with errors: {result.IssuesFailed} failed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cleaning registry: {ex.Message}";
        }
        finally
        {
            IsCleaning = false;
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        if (string.IsNullOrEmpty(LastBackupPath))
        {
            StatusMessage = "No backup available to restore";
            return;
        }

        try
        {
            StatusMessage = "Restoring registry backup...";
            var success = await _registryService.RestoreBackupAsync(LastBackupPath);

            if (success)
            {
                StatusMessage = "Registry backup restored successfully";
                await ScanRegistryAsync();
            }
            else
            {
                StatusMessage = "Failed to restore registry backup";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error restoring backup: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ToggleSelectAll()
    {
        foreach (var issue in Issues)
        {
            issue.IsSelected = SelectAll;
        }
        UpdateSelectedCount();
    }

    [RelayCommand]
    private void SelectBySeverity(string severity)
    {
        var severityEnum = Enum.Parse<RegistryIssueSeverity>(severity);
        foreach (var issue in Issues)
        {
            issue.IsSelected = issue.Issue.Severity == severityEnum;
        }
        UpdateSelectedCount();
    }

    private void UpdateSeverityCounts()
    {
        HighSeverityCount = Issues.Count(i => i.Issue.Severity == RegistryIssueSeverity.High);
        MediumSeverityCount = Issues.Count(i => i.Issue.Severity == RegistryIssueSeverity.Medium);
        LowSeverityCount = Issues.Count(i => i.Issue.Severity == RegistryIssueSeverity.Low);
    }

    private void UpdateSelectedCount()
    {
        SelectedIssuesCount = Issues.Count(i => i.IsSelected);
    }

    partial void OnIssuesChanged(ObservableCollection<RegistryIssueViewModel> value)
    {
        // Subscribe to selection changes
        foreach (var issue in value)
        {
            issue.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RegistryIssueViewModel.IsSelected))
                {
                    UpdateSelectedCount();
                }
            };
        }
    }
}

/// <summary>
/// Wrapper for RegistryIssue with selection state
/// </summary>
public partial class RegistryIssueViewModel : ObservableObject
{
    [ObservableProperty]
    private RegistryIssue _issue;

    [ObservableProperty]
    private bool _isSelected;

    public string SeverityColor => Issue.Severity switch
    {
        RegistryIssueSeverity.High => "#DC3545",
        RegistryIssueSeverity.Medium => "#FFC107",
        RegistryIssueSeverity.Low => "#28A745",
        _ => "#6C757D"
    };

    public string SeverityText => Issue.Severity.ToString();
    public string TypeText => FormatIssueType(Issue.Type);
    public string KeyPathShort => ShortenPath(Issue.KeyPath, 60);
    public string FixableText => Issue.IsFixable ? "Yes" : "No";

    public RegistryIssueViewModel(RegistryIssue issue)
    {
        _issue = issue;
    }

    private static string FormatIssueType(RegistryIssueType type)
    {
        return type switch
        {
            RegistryIssueType.InvalidFileExtension => "Invalid File Extension",
            RegistryIssueType.OrphanedStartupEntry => "Orphaned Startup",
            RegistryIssueType.InvalidFont => "Invalid Font",
            RegistryIssueType.InvalidHelpFile => "Invalid Help File",
            RegistryIssueType.InvalidUninstallEntry => "Invalid Uninstall Entry",
            RegistryIssueType.InvalidSharedDLL => "Invalid Shared DLL",
            RegistryIssueType.InvalidMUICache => "Invalid MUI Cache",
            RegistryIssueType.InvalidClassID => "Invalid Class ID",
            RegistryIssueType.EmptyRegistryKey => "Empty Key",
            RegistryIssueType.InvalidApplicationPath => "Invalid App Path",
            _ => type.ToString()
        };
    }

    private static string ShortenPath(string path, int maxLength)
    {
        if (path.Length <= maxLength)
            return path;

        var parts = path.Split('\\');
        if (parts.Length <= 2)
            return path;

        return $"{parts[0]}\\...\\{parts[^1]}";
    }
}
