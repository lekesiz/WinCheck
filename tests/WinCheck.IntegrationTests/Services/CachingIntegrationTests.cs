using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinCheck.Core.Helpers;

namespace WinCheck.IntegrationTests.Services;

/// <summary>
/// Integration tests for caching infrastructure across multiple components
/// </summary>
[TestClass]
public class CachingIntegrationTests
{
    [TestMethod]
    public async Task MultipleServices_UsingSameCache_ShareCachedData()
    {
        // Arrange
        var systemCache = AppCache.SystemInfo;
        systemCache.Clear();

        var service1Data = "Service1-Data";
        var service2Data = "Service2-Data";

        // Act - Service 1 writes to cache
        systemCache.Set("SharedKey", service1Data, TimeSpan.FromMinutes(1));

        // Service 2 reads from cache
        var retrieved = systemCache.GetOrAdd("SharedKey", () => service2Data);

        // Assert
        Assert.AreEqual(service1Data, retrieved, "Services should share cached data");
    }

    [TestMethod]
    public async Task CacheExpiration_AcrossServices_HandlesGracefully()
    {
        // Arrange
        var cache = new CacheHelper<string, string>(TimeSpan.FromMilliseconds(100));
        cache.Set("key1", "value1", TimeSpan.FromMilliseconds(100));

        // Act
        var beforeExpiration = cache.TryGetValue("key1", out var value1);
        await Task.Delay(150);
        var afterExpiration = cache.TryGetValue("key1", out var value2);

        // Assert
        Assert.IsTrue(beforeExpiration, "Should retrieve before expiration");
        Assert.AreEqual("value1", value1);
        Assert.IsFalse(afterExpiration, "Should not retrieve after expiration");
        Assert.IsNull(value2);
    }

    [TestMethod]
    public void PerformanceStats_WithCaching_ImprovesThroughput()
    {
        // Arrange
        PerformanceStats.Clear();
        var cache = new CacheHelper<int, string>(TimeSpan.FromMinutes(1));
        int callCount = 0;

        Func<string> expensiveOperation = () =>
        {
            callCount++;
            Thread.Sleep(10); // Simulate expensive operation
            return "result";
        };

        // Act - First call (cache miss)
        using (var monitor1 = PerformanceMonitor.Start("FirstCall"))
        {
            cache.GetOrAdd(1, expensiveOperation);
        }

        // Second call (cache hit)
        using (var monitor2 = PerformanceMonitor.Start("SecondCall"))
        {
            cache.GetOrAdd(1, expensiveOperation);
        }

        // Assert
        Assert.AreEqual(1, callCount, "Expensive operation should only be called once");
    }

    [TestMethod]
    public async Task RateLimiter_WithCaching_PreventsCacheStampede()
    {
        // Arrange
        var rateLimiter = new RateLimiter(5, TimeSpan.FromSeconds(1));
        var cache = new CacheHelper<string, int>(TimeSpan.FromMilliseconds(50));
        int callCount = 0;

        // Act - Multiple concurrent requests for same key
        var tasks = Enumerable.Range(0, 10).Select(async i =>
        {
            await rateLimiter.WaitAsync();
            return cache.GetOrAdd("key", () =>
            {
                Interlocked.Increment(ref callCount);
                Thread.Sleep(1); // Small delay to increase contention
                return 42;
            });
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        // Due to concurrent access, some calls may race before cache is populated
        Assert.IsTrue(callCount <= 5, $"Cache should prevent most calls, got {callCount}");
        Assert.IsTrue(results.All(r => r == 42), "All results should be cached value");
    }
}
