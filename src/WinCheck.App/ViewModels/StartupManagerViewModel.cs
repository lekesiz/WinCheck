using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WinCheck.Core.Interfaces;
using WinCheck.Core.Models;

namespace WinCheck.App.ViewModels;

public partial class StartupManagerViewModel : ObservableObject
{
    private readonly IStartupManagerService _startupService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _totalPrograms;

    [ObservableProperty]
    private int _enabledPrograms;

    [ObservableProperty]
    private int _disabledPrograms;

    [ObservableProperty]
    private int _estimatedBootTime;

    [ObservableProperty]
    private int _potentialTimeSaving;

    [ObservableProperty]
    private ObservableCollection<StartupProgramViewModel> _startupPrograms = new();

    [ObservableProperty]
    private ObservableCollection<StartupRecommendation> _recommendations = new();

    [ObservableProperty]
    private StartupProgramViewModel? _selectedProgram;

    public StartupManagerViewModel(IStartupManagerService startupService)
    {
        _startupService = startupService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading startup programs...";

            // Load programs with safety
            try
            {
                var programs = await _startupService.GetStartupProgramsAsync();
                StartupPrograms.Clear();

                if (programs != null)
                {
                    foreach (var program in programs.OrderByDescending(p => p.Impact).ThenBy(p => p.Name))
                    {
                        try
                        {
                            if (program != null)
                            {
                                StartupPrograms.Add(new StartupProgramViewModel(program, _startupService));
                            }
                        }
                        catch
                        {
                            // Skip problematic programs
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading programs: {ex.Message}";
            }

            // Load impact analysis with safety
            try
            {
                var analysis = await _startupService.AnalyzeBootImpactAsync();
                if (analysis != null)
                {
                    TotalPrograms = analysis.TotalStartupPrograms;
                    EnabledPrograms = analysis.EnabledPrograms;
                    DisabledPrograms = analysis.DisabledPrograms;
                    EstimatedBootTime = analysis.EstimatedBootTimeSeconds;
                    PotentialTimeSaving = analysis.PotentialTimeSavingSeconds;

                    Recommendations.Clear();
                    if (analysis.Recommendations != null)
                    {
                        foreach (var recommendation in analysis.Recommendations.Take(10))
                        {
                            try
                            {
                                if (recommendation != null)
                                {
                                    Recommendations.Add(recommendation);
                                }
                            }
                            catch
                            {
                                // Skip problematic recommendations
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error analyzing impact: {ex.Message}";
            }

            StatusMessage = $"Loaded {TotalPrograms} startup programs";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading startup programs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DisableAllRecommendedAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Disabling recommended programs...";

            int disabledCount = 0;
            foreach (var recommendation in Recommendations.Where(r => r.RecommendDisable))
            {
                var success = await _startupService.SetStartupStateAsync(recommendation.Program, false);
                if (success)
                {
                    disabledCount++;
                }
            }

            StatusMessage = $"Disabled {disabledCount} programs";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error disabling programs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EnableAllAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Enabling all programs...";

            int enabledCount = 0;
            foreach (var programVM in StartupPrograms.Where(p => !p.IsEnabled))
            {
                var success = await _startupService.SetStartupStateAsync(programVM.Program, true);
                if (success)
                {
                    enabledCount++;
                }
            }

            StatusMessage = $"Enabled {enabledCount} programs";
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error enabling programs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// Wrapper for StartupProgram with toggle functionality
/// </summary>
public partial class StartupProgramViewModel : ObservableObject
{
    private readonly IStartupManagerService _startupService;

    [ObservableProperty]
    private StartupProgram _program;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private bool _isToggling;

    public string DisplayName => Program?.Name ?? "Unknown";
    public string LocationText => Program?.Location.ToString() ?? "Unknown";
    public string ImpactText => Program?.Impact.ToString() ?? "Unknown";
    public string ImpactColor => Program?.Impact switch
    {
        StartupImpact.VeryHigh => "#DC3545",
        StartupImpact.High => "#FD7E14",
        StartupImpact.Medium => "#FFC107",
        StartupImpact.Low => "#28A745",
        _ => "#6C757D"
    } ?? "#6C757D";
    public string PublisherText => string.IsNullOrEmpty(Program?.Publisher) ? "Unknown" : Program.Publisher;
    public string SignedText => (Program?.IsSigned ?? false) ? "Signed" : "Not signed";
    public string DelayText => (Program?.EstimatedDelayMs ?? 0) > 0 ? $"{Program.EstimatedDelayMs}ms" : "N/A";

    public StartupProgramViewModel(StartupProgram program, IStartupManagerService startupService)
    {
        _program = program ?? throw new ArgumentNullException(nameof(program));
        _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
        _isEnabled = program?.IsEnabled ?? false;
    }

    [RelayCommand]
    private async Task ToggleStateAsync()
    {
        try
        {
            IsToggling = true;
            var newState = !IsEnabled;
            var success = await _startupService.SetStartupStateAsync(Program, newState);

            if (success)
            {
                IsEnabled = newState;
                Program.IsEnabled = newState;
            }
        }
        catch (Exception)
        {
            // Revert on error
        }
        finally
        {
            IsToggling = false;
        }
    }

    [RelayCommand]
    private async Task RemoveAsync()
    {
        try
        {
            IsToggling = true;
            await _startupService.RemoveFromStartupAsync(Program);
        }
        finally
        {
            IsToggling = false;
        }
    }
}
