using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinCheck.Core.Helpers;

namespace WinCheck.IntegrationTests.Services;

/// <summary>
/// Integration tests for retry logic combined with rate limiting
/// </summary>
[TestClass]
public class RetryWithRateLimitingTests
{
    [TestMethod]
    public async Task RetryHelper_WithRateLimiter_RespectsRateLimits()
    {
        // Arrange
        var rateLimiter = new RateLimiter(3, TimeSpan.FromSeconds(1));
        int attemptCount = 0;
        var startTime = DateTime.UtcNow;

        // Act
        var result = await RetryHelper.RetryAsync(async () =>
        {
            await rateLimiter.WaitAsync();
            attemptCount++;

            if (attemptCount < 3)
                throw new InvalidOperationException("Temporary failure");

            return "Success";
        }, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(50));

        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.AreEqual("Success", result);
        Assert.AreEqual(3, attemptCount);
    }

    [TestMethod]
    public async Task RetryPolicy_NetworkPolicy_WithRateLimiting_HandlesConcurrentRequests()
    {
        // Arrange
        var rateLimiter = RateLimiters.GeneralAPI;
        var policy = RetryPolicy.NetworkPolicy;
        int successCount = 0;

        // Act - Simulate concurrent API calls
        var tasks = Enumerable.Range(0, 5).Select(async i =>
        {
            return await rateLimiter.ExecuteAsync(async () =>
            {
                return await policy.ExecuteAsync(async () =>
                {
                    Interlocked.Increment(ref successCount);
                    return $"Result-{i}";
                });
            });
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(5, successCount);
        Assert.AreEqual(5, results.Length);
    }

    [TestMethod]
    public async Task TokenBucketRateLimiter_WithRetry_ConsumesTokensCorrectly()
    {
        // Arrange
        var tokenBucket = new TokenBucketRateLimiter(
            capacity: 5,
            refillRate: 2,
            refillInterval: TimeSpan.FromMilliseconds(100)
        );

        // Act - Consume initial tokens
        var consumed1 = await tokenBucket.ConsumeAsync(3);
        var consumed2 = await tokenBucket.ConsumeAsync(2);

        // Wait for refill
        await Task.Delay(150);
        var consumed3 = await tokenBucket.ConsumeAsync(2);

        // Assert
        Assert.IsTrue(consumed1, "Should consume first batch");
        Assert.IsTrue(consumed2, "Should consume second batch");
        Assert.IsTrue(consumed3, "Should consume after refill");
    }

    [TestMethod]
    public async Task RetryHelper_WithCaching_ReducesRetryAttempts()
    {
        // Arrange
        var cache = new CacheHelper<string, string>(TimeSpan.FromSeconds(5));
        int expensiveCallCount = 0;

        Func<Task<string>> expensiveOperation = async () =>
        {
            expensiveCallCount++;
            await Task.Delay(10);

            // Fail first 2 times, then succeed
            if (expensiveCallCount < 3)
                throw new InvalidOperationException("Temporary failure");

            return "Success";
        };

        // Act - First call with retry (should fail twice, succeed third time)
        var result1 = await RetryHelper.RetryAsync(async () =>
        {
            return cache.GetOrAdd("key1", () => expensiveOperation().Result);
        }, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10));

        // Reset for second test
        expensiveCallCount = 0;

        // Second call with same key (should hit cache, no retries needed)
        var result2 = cache.GetOrAdd("key1", () =>
        {
            expensiveCallCount++;
            return "Should not be called";
        });

        // Assert
        Assert.AreEqual("Success", result1);
        Assert.AreEqual("Success", result2);
        Assert.AreEqual(0, expensiveCallCount, "Second call should hit cache");
    }

    [TestMethod]
    public async Task PerformanceMonitor_WithRetryAndRateLimit_TracksCorrectly()
    {
        // Arrange
        PerformanceStats.Clear();
        var rateLimiter = new RateLimiter(10, TimeSpan.FromSeconds(1));
        int attemptCount = 0;

        // Act
        using (var monitor = PerformanceMonitor.Start("RetryOperation", PerformanceStats.Record))
        {
            await RetryHelper.RetryAsync(async () =>
            {
                await rateLimiter.WaitAsync();
                attemptCount++;

                if (attemptCount < 2)
                    throw new InvalidOperationException("Temporary failure");

                return "Success";
            }, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10));
        }

        // Assert
        var stats = PerformanceStats.GetStats("RetryOperation");
        Assert.IsNotNull(stats);
        Assert.AreEqual(1, stats.Count);
        Assert.IsTrue(stats.TotalDuration > TimeSpan.Zero);
    }
}
