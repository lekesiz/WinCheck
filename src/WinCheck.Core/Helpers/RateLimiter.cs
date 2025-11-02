using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WinCheck.Core.Helpers;

/// <summary>
/// Rate limiter for controlling operation frequency
/// </summary>
public class RateLimiter
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<DateTime> _requestTimes;
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly object _lock = new();

    /// <summary>
    /// Creates a rate limiter
    /// </summary>
    /// <param name="maxRequests">Maximum requests allowed</param>
    /// <param name="timeWindow">Time window for rate limiting</param>
    public RateLimiter(int maxRequests, TimeSpan timeWindow)
    {
        if (maxRequests <= 0)
            throw new ArgumentException("Max requests must be greater than 0", nameof(maxRequests));

        if (timeWindow <= TimeSpan.Zero)
            throw new ArgumentException("Time window must be greater than 0", nameof(timeWindow));

        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
        _semaphore = new SemaphoreSlim(maxRequests, maxRequests);
        _requestTimes = new ConcurrentQueue<DateTime>();
    }

    /// <summary>
    /// Waits for rate limit to allow the operation
    /// </summary>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            // Clean old request times
            var now = DateTime.UtcNow;
            while (_requestTimes.TryPeek(out var oldTime))
            {
                if (now - oldTime > _timeWindow)
                {
                    _requestTimes.TryDequeue(out _);
                }
                else
                {
                    break;
                }
            }

            lock (_lock)
            {
                if (_requestTimes.Count < _maxRequests)
                {
                    _requestTimes.Enqueue(now);
                    return;
                }
            }

            // Wait a bit before trying again
            var oldestRequest = _requestTimes.TryPeek(out var oldest) ? oldest : now;
            var waitTime = _timeWindow - (now - oldestRequest);

            if (waitTime > TimeSpan.Zero)
            {
                await Task.Delay(waitTime, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Executes an action within rate limits
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        await WaitAsync(cancellationToken);
        return await action();
    }

    /// <summary>
    /// Executes an action within rate limits (non-generic)
    /// </summary>
    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await WaitAsync(cancellationToken);
        await action();
    }
}

/// <summary>
/// Rate limiter collection for different services
/// </summary>
public static class RateLimiters
{
    /// <summary>
    /// OpenAI rate limiter (60 requests per minute for free tier)
    /// </summary>
    public static RateLimiter OpenAI { get; } = new(60, TimeSpan.FromMinutes(1));

    /// <summary>
    /// Claude rate limiter (50 requests per minute)
    /// </summary>
    public static RateLimiter Claude { get; } = new(50, TimeSpan.FromMinutes(1));

    /// <summary>
    /// Gemini rate limiter (60 requests per minute)
    /// </summary>
    public static RateLimiter Gemini { get; } = new(60, TimeSpan.FromMinutes(1));

    /// <summary>
    /// General API rate limiter (30 requests per minute)
    /// </summary>
    public static RateLimiter GeneralAPI { get; } = new(30, TimeSpan.FromMinutes(1));

    /// <summary>
    /// Disk operation rate limiter (prevent disk thrashing)
    /// </summary>
    public static RateLimiter DiskOperations { get; } = new(10, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Registry operation rate limiter
    /// </summary>
    public static RateLimiter RegistryOperations { get; } = new(20, TimeSpan.FromSeconds(1));
}

/// <summary>
/// Token bucket rate limiter (more sophisticated)
/// </summary>
public class TokenBucketRateLimiter
{
    private readonly int _capacity;
    private readonly int _refillRate;
    private readonly TimeSpan _refillInterval;
    private int _tokens;
    private DateTime _lastRefill;
    private readonly object _lock = new();

    /// <summary>
    /// Creates a token bucket rate limiter
    /// </summary>
    /// <param name="capacity">Maximum tokens</param>
    /// <param name="refillRate">Tokens to add per interval</param>
    /// <param name="refillInterval">Refill interval</param>
    public TokenBucketRateLimiter(int capacity, int refillRate, TimeSpan refillInterval)
    {
        _capacity = capacity;
        _refillRate = refillRate;
        _refillInterval = refillInterval;
        _tokens = capacity;
        _lastRefill = DateTime.UtcNow;
    }

    /// <summary>
    /// Tries to consume tokens
    /// </summary>
    public bool TryConsume(int tokens = 1)
    {
        lock (_lock)
        {
            Refill();

            if (_tokens >= tokens)
            {
                _tokens -= tokens;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Waits until tokens are available
    /// </summary>
    public async Task<bool> ConsumeAsync(int tokens = 1, CancellationToken cancellationToken = default)
    {
        while (!TryConsume(tokens))
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            // Wait for next refill
            await Task.Delay(100, cancellationToken);
        }

        return true;
    }

    private void Refill()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastRefill;
        var intervalsElapsed = (int)(elapsed.TotalMilliseconds / _refillInterval.TotalMilliseconds);

        if (intervalsElapsed > 0)
        {
            var tokensToAdd = intervalsElapsed * _refillRate;
            _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
            _lastRefill = now;
        }
    }

    /// <summary>
    /// Gets current token count
    /// </summary>
    public int AvailableTokens
    {
        get
        {
            lock (_lock)
            {
                Refill();
                return _tokens;
            }
        }
    }
}
