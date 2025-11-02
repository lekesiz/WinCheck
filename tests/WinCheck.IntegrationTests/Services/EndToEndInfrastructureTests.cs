using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinCheck.Core.Helpers;

namespace WinCheck.IntegrationTests.Services;

/// <summary>
/// End-to-end integration tests simulating real-world scenarios
/// </summary>
[TestClass]
public class EndToEndInfrastructureTests
{
    [TestMethod]
    public async Task CompleteWorkflow_APICall_WithAllInfrastructure()
    {
        // Arrange
        PerformanceStats.Clear();
        var cache = AppCache.SystemInfo;
        var rateLimiter = new RateLimiter(10, TimeSpan.FromSeconds(1));
        var policy = RetryPolicy.NetworkPolicy;

        cache.Clear();
        string apiKey = "sk-testapikey1234567890abcdefghijklmnop";
        int actualApiCallCount = 0;

        // Simulated API call function
        Func<Task<string>> simulatedApiCall = async () =>
        {
            // 1. Validate API key
            if (!ValidationHelper.IsValidApiKeyFormat(apiKey))
                throw new ArgumentException("Invalid API key");

            // 2. Rate limit
            await rateLimiter.WaitAsync();

            // 3. Actual API call (simulated)
            actualApiCallCount++;
            await Task.Delay(10); // Simulate network latency

            // Simulate transient failure on first attempt
            if (actualApiCallCount == 1)
                throw new HttpRequestException("Temporary network error");

            return "API Response Data";
        };

        // Act - Complete workflow
        string? result = null;
        using (var monitor = PerformanceMonitor.Start("APICall", PerformanceStats.Record))
        {
            result = cache.GetOrAdd("api-result", () =>
            {
                return policy.ExecuteAsync(simulatedApiCall).Result;
            }) as string;
        }

        // Second call should hit cache
        var cachedResult = cache.GetOrAdd("api-result", () =>
        {
            actualApiCallCount++;
            return simulatedApiCall().Result;
        }) as string;

        // Assert
        Assert.AreEqual("API Response Data", result);
        Assert.AreEqual("API Response Data", cachedResult);
        Assert.AreEqual(2, actualApiCallCount, "First call fails, second succeeds, third hits cache");

        var stats = PerformanceStats.GetStats("APICall");
        Assert.IsNotNull(stats);
        Assert.AreEqual(1, stats.Count);
    }

    [TestMethod]
    public async Task HighConcurrency_MultipleComponents_WorkTogether()
    {
        // Arrange
        var cache = new CacheHelper<int, string>(TimeSpan.FromSeconds(2));
        var rateLimiter = new RateLimiter(20, TimeSpan.FromSeconds(1));
        int operationCount = 0;

        // Act - 50 concurrent operations
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            return await rateLimiter.ExecuteAsync(async () =>
            {
                // Only 5 unique keys, so caching should reduce operations
                var key = i % 5;

                return cache.GetOrAdd(key, () =>
                {
                    Interlocked.Increment(ref operationCount);
                    Thread.Sleep(5); // Simulate work
                    return $"Result-{key}";
                });
            });
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(50, results.Length);
        // Due to concurrent access and timing, some operations may execute multiple times
        // but should be significantly less than 50
        Assert.IsTrue(operationCount <= 15, $"Caching should reduce operations significantly, got {operationCount} out of 50");
    }

    [TestMethod]
    public async Task RealWorld_DiskCleanup_Workflow()
    {
        // Arrange
        PerformanceStats.Clear();
        var scanCache = new CacheHelper<string, long>(TimeSpan.FromMinutes(5));
        var rateLimiter = RateLimiters.DiskOperations;

        var directories = new[] { "Temp", "Cache", "Logs", "Downloads" };
        int scanCount = 0;

        Func<string, Task<long>> scanDirectory = async (dir) =>
        {
            return await Task.Run(() =>
            {
                // Validate directory name
                var sanitized = ValidationHelper.SanitizeFileName(dir);
                if (string.IsNullOrEmpty(sanitized))
                    throw new ArgumentException("Invalid directory name");

                scanCount++;
                Thread.Sleep(20); // Simulate disk I/O
                return (long)(new Random().Next(1000, 10000)); // Simulate size
            });
        };

        // Act - Scan with performance monitoring
        var results = new List<long>();

        using (var monitor = PerformanceMonitor.Start("DiskCleanupScan", PerformanceStats.Record))
        {
            foreach (var dir in directories)
            {
                var size = await rateLimiter.ExecuteAsync(async () =>
                {
                    return await RetryHelper.RetryAsync(async () =>
                    {
                        return scanCache.GetOrAdd(dir, () => scanDirectory(dir).Result);
                    }, maxAttempts: 2, initialDelay: TimeSpan.FromMilliseconds(100));
                });

                results.Add(size);
            }
        }

        // Scan again (should hit cache)
        scanCount = 0;
        foreach (var dir in directories)
        {
            var cached = scanCache.GetOrAdd(dir, () => scanDirectory(dir).Result);
        }

        // Assert
        Assert.AreEqual(4, results.Count);
        Assert.AreEqual(0, scanCount, "Second scan should hit cache");

        var stats = PerformanceStats.GetStats("DiskCleanupScan");
        Assert.IsNotNull(stats);
        Assert.AreEqual(1, stats.Count);
    }

    [TestMethod]
    public async Task RealWorld_ProcessMonitoring_Workflow()
    {
        // Arrange
        var processCache = AppCache.ProcessInfo;
        processCache.Clear();
        var rateLimiter = new RateLimiter(100, TimeSpan.FromSeconds(1));

        var processIds = Enumerable.Range(1000, 20).ToArray();
        int validationCount = 0;

        // Act - Monitor processes with validation and caching
        var tasks = processIds.Select(async pid =>
        {
            return await rateLimiter.ExecuteAsync(async () =>
            {
                // Validate process ID
                if (!ValidationHelper.IsValidProcessId(pid))
                {
                    validationCount++;
                    return null;
                }

                // Check cache first
                return processCache.GetOrAdd(pid, () =>
                {
                    // Simulate process info retrieval
                    return new
                    {
                        ProcessId = pid,
                        Name = $"Process{pid}",
                        CpuUsage = new Random().NextDouble() * 100
                    };
                });
            });
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(20, results.Length);
        Assert.IsTrue(results.All(r => r != null), "All process IDs should be valid");
    }

    [TestMethod]
    public async Task ErrorHandling_GracefulDegradation()
    {
        // Arrange
        var cache = new CacheHelper<string, string>(TimeSpan.FromSeconds(1));
        var policy = new RetryPolicy
        {
            MaxAttempts = 3,
            InitialDelay = TimeSpan.FromMilliseconds(10),
            ShouldRetry = ex => ex is InvalidOperationException
        };

        int attemptCount = 0;
        var fallbackValue = "Fallback Data";

        // Act - Operation that fails all retries
        string? result = null;
        try
        {
            result = await policy.ExecuteAsync<string>(async () =>
            {
                attemptCount++;
                throw new InvalidOperationException("Permanent failure");
#pragma warning disable CS0162 // Unreachable code detected
                return "Never reached";
#pragma warning restore CS0162
            });
        }
        catch (InvalidOperationException)
        {
            // Graceful degradation - use cached fallback
            result = cache.GetOrAdd("fallback", () => fallbackValue);
        }

        // Assert
        Assert.AreEqual(3, attemptCount, "Should retry 3 times");
        Assert.AreEqual(fallbackValue, result, "Should use fallback value");
    }
}
