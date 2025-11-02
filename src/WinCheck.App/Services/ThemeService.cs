using Microsoft.UI.Xaml;
using Windows.Storage;

namespace WinCheck.App.Services;

/// <summary>
/// Service for managing application theme (Light/Dark/System)
/// </summary>
public class ThemeService
{
    private const string ThemeSettingKey = "AppTheme";
    private readonly ApplicationDataContainer _localSettings;
    private Window? _window;

    public ThemeService()
    {
        _localSettings = ApplicationData.Current.LocalSettings;
    }

    public enum AppTheme
    {
        Light,
        Dark,
        System
    }

    /// <summary>
    /// Gets the current theme setting
    /// </summary>
    public AppTheme CurrentTheme
    {
        get
        {
            if (_localSettings.Values.TryGetValue(ThemeSettingKey, out var value) && value is string themeStr)
            {
                return Enum.TryParse<AppTheme>(themeStr, out var theme) ? theme : AppTheme.System;
            }
            return AppTheme.System;
        }
    }

    /// <summary>
    /// Sets the application theme
    /// </summary>
    public void SetTheme(AppTheme theme, Window? window = null)
    {
        _window = window ?? _window;
        _localSettings.Values[ThemeSettingKey] = theme.ToString();

        if (_window?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                AppTheme.System => ElementTheme.Default,
                _ => ElementTheme.Default
            };
        }
    }

    /// <summary>
    /// Initializes theme on application startup
    /// </summary>
    public void Initialize(Window window)
    {
        _window = window;
        SetTheme(CurrentTheme, window);
    }

    /// <summary>
    /// Gets whether the current effective theme is dark
    /// </summary>
    public bool IsDarkTheme()
    {
        if (_window?.Content is FrameworkElement rootElement)
        {
            return rootElement.ActualTheme == ElementTheme.Dark;
        }
        return false;
    }

    /// <summary>
    /// Toggles between light and dark theme
    /// </summary>
    public void ToggleTheme()
    {
        var currentTheme = CurrentTheme;
        var newTheme = currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        SetTheme(newTheme);
    }
}
