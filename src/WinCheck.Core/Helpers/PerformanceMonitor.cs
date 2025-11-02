using System;
using System.Diagnostics;

namespace WinCheck.Core.Helpers;

/// <summary>
/// Helper for monitoring operation performance
/// </summary>
public class PerformanceMonitor : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;
    private readonly Action<string, TimeSpan>? _onComplete;
    private bool _disposed;

    private PerformanceMonitor(string operationName, Action<string, TimeSpan>? onComplete = null)
    {
        _operationName = operationName;
        _onComplete = onComplete;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Starts monitoring an operation
    /// </summary>
    public static PerformanceMonitor Start(string operationName, Action<string, TimeSpan>? onComplete = null)
    {
        return new PerformanceMonitor(operationName, onComplete);
    }

    /// <summary>
    /// Gets the elapsed time
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Stops the monitor and returns elapsed time
    /// </summary>
    public TimeSpan Stop()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            var elapsed = _stopwatch.Elapsed;
            _onComplete?.Invoke(_operationName, elapsed);
            _disposed = true;
            return elapsed;
        }

        return _stopwatch.Elapsed;
    }

    public void Dispose()
    {
        Stop();
    }
}

/// <summary>
/// Extension methods for easy performance monitoring
/// </summary>
public static class PerformanceMonitorExtensions
{
    /// <summary>
    /// Measures the execution time of an action
    /// </summary>
    public static TimeSpan Measure(this Action action, string operationName, Action<string, TimeSpan>? onComplete = null)
    {
        using var monitor = PerformanceMonitor.Start(operationName, onComplete);
        action();
        return monitor.Elapsed;
    }

    /// <summary>
    /// Measures the execution time of a func
    /// </summary>
    public static (T Result, TimeSpan Duration) Measure<T>(this Func<T> func, string operationName, Action<string, TimeSpan>? onComplete = null)
    {
        using var monitor = PerformanceMonitor.Start(operationName, onComplete);
        var result = func();
        return (result, monitor.Elapsed);
    }
}

/// <summary>
/// Global performance statistics collector
/// </summary>
public static class PerformanceStats
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, OperationStats> _stats = new();

    /// <summary>
    /// Records an operation execution
    /// </summary>
    public static void Record(string operationName, TimeSpan duration)
    {
        _stats.AddOrUpdate(operationName,
            _ => new OperationStats { Count = 1, TotalDuration = duration, MinDuration = duration, MaxDuration = duration },
            (_, existing) =>
            {
                existing.Count++;
                existing.TotalDuration += duration;
                if (duration < existing.MinDuration) existing.MinDuration = duration;
                if (duration > existing.MaxDuration) existing.MaxDuration = duration;
                return existing;
            });
    }

    /// <summary>
    /// Gets statistics for an operation
    /// </summary>
    public static OperationStats? GetStats(string operationName)
    {
        return _stats.TryGetValue(operationName, out var stats) ? stats : null;
    }

    /// <summary>
    /// Gets all operation statistics
    /// </summary>
    public static System.Collections.Generic.Dictionary<string, OperationStats> GetAllStats()
    {
        return new System.Collections.Generic.Dictionary<string, OperationStats>(_stats);
    }

    /// <summary>
    /// Clears all statistics
    /// </summary>
    public static void Clear()
    {
        _stats.Clear();
    }

    public class OperationStats
    {
        public int Count { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan MinDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public TimeSpan AverageDuration => Count > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / Count) : TimeSpan.Zero;
    }
}
