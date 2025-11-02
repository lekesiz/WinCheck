using System;
using System.Threading.Tasks;

namespace WinCheck.Core.Helpers;

/// <summary>
/// Helper for retrying operations that may fail transiently
/// </summary>
public static class RetryHelper
{
    /// <summary>
    /// Retries an operation with exponential backoff
    /// </summary>
    /// <param name="operation">The operation to retry</param>
    /// <param name="maxAttempts">Maximum number of attempts (default: 3)</param>
    /// <param name="initialDelay">Initial delay between retries (default: 1 second)</param>
    /// <param name="maxDelay">Maximum delay between retries (default: 30 seconds)</param>
    /// <param name="onRetry">Callback invoked before each retry</param>
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts = 3,
        TimeSpan? initialDelay = null,
        TimeSpan? maxDelay = null,
        Action<int, Exception>? onRetry = null)
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);
        var maxDelayValue = maxDelay ?? TimeSpan.FromSeconds(30);
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == maxAttempts)
                {
                    throw;
                }

                // Invoke retry callback
                onRetry?.Invoke(attempt, ex);

                // Wait before retry with exponential backoff
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, maxDelayValue.TotalMilliseconds));
            }
        }

        // Should never reach here, but compiler requires it
        throw lastException ?? new InvalidOperationException("Retry operation failed");
    }

    /// <summary>
    /// Retries an operation (non-async version)
    /// </summary>
    public static T Retry<T>(
        Func<T> operation,
        int maxAttempts = 3,
        TimeSpan? initialDelay = null,
        TimeSpan? maxDelay = null,
        Action<int, Exception>? onRetry = null)
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);
        var maxDelayValue = maxDelay ?? TimeSpan.FromSeconds(30);
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == maxAttempts)
                {
                    throw;
                }

                // Invoke retry callback
                onRetry?.Invoke(attempt, ex);

                // Wait before retry
                System.Threading.Thread.Sleep(delay);
                delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, maxDelayValue.TotalMilliseconds));
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed");
    }

    /// <summary>
    /// Retries an async operation with a specific exception filter
    /// </summary>
    public static async Task<T> RetryOnAsync<T, TException>(
        Func<Task<T>> operation,
        int maxAttempts = 3,
        TimeSpan? initialDelay = null,
        Action<int, TException>? onRetry = null)
        where TException : Exception
    {
        var delay = initialDelay ?? TimeSpan.FromSeconds(1);
        TException? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (TException ex)
            {
                lastException = ex;

                if (attempt == maxAttempts)
                {
                    throw;
                }

                onRetry?.Invoke(attempt, ex);
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }

        throw (Exception?)lastException ?? new InvalidOperationException("Retry operation failed");
    }
}

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
    public Func<Exception, bool> ShouldRetry { get; set; } = _ => true;

    /// <summary>
    /// Default policy for network operations (5 attempts, longer delays)
    /// </summary>
    public static RetryPolicy NetworkPolicy => new()
    {
        MaxAttempts = 5,
        InitialDelay = TimeSpan.FromSeconds(2),
        MaxDelay = TimeSpan.FromMinutes(1),
        ShouldRetry = ex => ex is System.Net.Http.HttpRequestException ||
                            ex is System.Net.Sockets.SocketException ||
                            ex is TimeoutException
    };

    /// <summary>
    /// Default policy for file operations (3 attempts, short delays)
    /// </summary>
    public static RetryPolicy FilePolicy => new()
    {
        MaxAttempts = 3,
        InitialDelay = TimeSpan.FromMilliseconds(500),
        MaxDelay = TimeSpan.FromSeconds(5),
        ShouldRetry = ex => ex is System.IO.IOException ||
                            ex is UnauthorizedAccessException
    };

    /// <summary>
    /// Default policy for database operations (3 attempts, medium delays)
    /// </summary>
    public static RetryPolicy DatabasePolicy => new()
    {
        MaxAttempts = 3,
        InitialDelay = TimeSpan.FromSeconds(1),
        MaxDelay = TimeSpan.FromSeconds(10)
    };

    /// <summary>
    /// Executes operation with this policy
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Action<int, Exception>? onRetry = null)
    {
        Exception? lastException = null;
        var delay = InitialDelay;

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == MaxAttempts || !ShouldRetry(ex))
                {
                    throw;
                }

                onRetry?.Invoke(attempt, ex);
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, MaxDelay.TotalMilliseconds));
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed");
    }
}
