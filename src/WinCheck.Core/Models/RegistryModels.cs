using System;
using System.Collections.Generic;

namespace WinCheck.Core.Models;

public class RegistryScanResult
{
    public DateTime ScanDate { get; set; }
    public int TotalIssuesFound { get; set; }
    public List<RegistryIssue> Issues { get; set; } = new();
    public TimeSpan ScanDuration { get; set; }
}

public class RegistryIssue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string KeyPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public RegistryIssueType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public RegistryIssueSeverity Severity { get; set; }
    public bool IsFixable { get; set; }
    public object? BackupValue { get; set; }
}

public enum RegistryIssueType
{
    InvalidFileExtension,
    OrphanedStartupEntry,
    InvalidFont,
    InvalidHelpFile,
    InvalidUninstallEntry,
    InvalidSharedDLL,
    InvalidMUICache,
    InvalidClassID,
    EmptyRegistryKey,
    InvalidApplicationPath
}

public enum RegistryIssueSeverity
{
    Low,
    Medium,
    High
}

public class RegistryCleanupResult
{
    public bool Success { get; set; }
    public int IssuesFixed { get; set; }
    public int IssuesFailed { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? BackupPath { get; set; }
}
