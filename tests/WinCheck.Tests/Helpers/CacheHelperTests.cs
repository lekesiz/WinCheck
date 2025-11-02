using WinCheck.Core.Helpers;

namespace WinCheck.Tests.Helpers;

[TestClass]
public class CacheHelperTests
{
    [TestMethod]
    public void GetOrAdd_WithNewKey_CallsFactoryAndCachesValue()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(1));
        int factoryCallCount = 0;

        // Act
        var result1 = cache.GetOrAdd("key1", () =>
        {
            factoryCallCount++;
            return 42;
        });

        var result2 = cache.GetOrAdd("key1", () =>
        {
            factoryCallCount++;
            return 99; // Different value, but should not be called
        });

        // Assert
        Assert.AreEqual(42, result1);
        Assert.AreEqual(42, result2); // Should return cached value
        Assert.AreEqual(1, factoryCallCount); // Factory should only be called once
    }

    [TestMethod]
    public async Task GetOrAdd_AfterExpiration_CallsFactoryAgain()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMilliseconds(100));
        int factoryCallCount = 0;

        // Act
        var result1 = cache.GetOrAdd("key1", () =>
        {
            factoryCallCount++;
            return 42;
        });

        // Wait for expiration
        await Task.Delay(150);

        var result2 = cache.GetOrAdd("key1", () =>
        {
            factoryCallCount++;
            return 99;
        });

        // Assert
        Assert.AreEqual(42, result1);
        Assert.AreEqual(99, result2); // Should return new value after expiration
        Assert.AreEqual(2, factoryCallCount); // Factory should be called twice
    }

    [TestMethod]
    public void TryGetValue_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(1));
        cache.Set("key1", 42);

        // Act
        var found = cache.TryGetValue("key1", out var value);

        // Assert
        Assert.IsTrue(found);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryGetValue_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(1));

        // Act
        var found = cache.TryGetValue("nonexistent", out var value);

        // Assert
        Assert.IsFalse(found);
        Assert.AreEqual(0, value); // Default value
    }

    [TestMethod]
    public void Set_WithCustomExpiration_ExpiresCorrectly()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(10)); // Long default
        cache.Set("key1", 42, TimeSpan.FromMilliseconds(50)); // Short custom

        // Act - immediately should exist
        var found1 = cache.TryGetValue("key1", out var value1);

        // Wait for expiration
        System.Threading.Thread.Sleep(100);

        var found2 = cache.TryGetValue("key1", out var value2);

        // Assert
        Assert.IsTrue(found1);
        Assert.AreEqual(42, value1);
        Assert.IsFalse(found2); // Should be expired
    }

    [TestMethod]
    public void Remove_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(1));
        cache.Set("key1", 42);

        // Act
        var removed = cache.Remove("key1");
        var found = cache.TryGetValue("key1", out _);

        // Assert
        Assert.IsTrue(removed);
        Assert.IsFalse(found);
    }

    [TestMethod]
    public void Remove_NonExistingKey_ReturnsFalse()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(1));

        // Act
        var removed = cache.Remove("nonexistent");

        // Assert
        Assert.IsFalse(removed);
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(1));
        cache.Set("key1", 1);
        cache.Set("key2", 2);
        cache.Set("key3", 3);

        // Act
        cache.Clear();

        // Assert
        Assert.IsFalse(cache.TryGetValue("key1", out _));
        Assert.IsFalse(cache.TryGetValue("key2", out _));
        Assert.IsFalse(cache.TryGetValue("key3", out _));
        Assert.AreEqual(0, cache.Count);
    }

    [TestMethod]
    public void RemoveExpired_OnlyRemovesExpiredEntries()
    {
        // Arrange
        var cache = new CacheHelper<string, int>(TimeSpan.FromMinutes(10));
        cache.Set("short", 1, TimeSpan.FromMilliseconds(50));
        cache.Set("long", 2, TimeSpan.FromMinutes(10));

        // Wait for short to expire
        System.Threading.Thread.Sleep(100);

        // Act
        cache.RemoveExpired();

        // Assert
        Assert.IsFalse(cache.TryGetValue("short", out _));
        Assert.IsTrue(cache.TryGetValue("long", out _));
    }

    [TestMethod]
    public void AppCache_SystemInfo_IsSharedInstance()
    {
        // Arrange & Act
        AppCache.SystemInfo.Set("test", "value1");
        var retrieved = AppCache.SystemInfo.TryGetValue("test", out var value);

        // Assert
        Assert.IsTrue(retrieved);
        Assert.AreEqual("value1", value);
    }

    [TestMethod]
    public void AppCache_ClearAll_ClearsAllCaches()
    {
        // Arrange
        AppCache.SystemInfo.Set("test1", "value1");
        AppCache.HardwareInfo.Set("test2", "value2");
        AppCache.ServiceInfo.Set("test3", "value3");

        // Act
        AppCache.ClearAll();

        // Assert
        Assert.IsFalse(AppCache.SystemInfo.TryGetValue("test1", out _));
        Assert.IsFalse(AppCache.HardwareInfo.TryGetValue("test2", out _));
        Assert.IsFalse(AppCache.ServiceInfo.TryGetValue("test3", out _));
    }
}
