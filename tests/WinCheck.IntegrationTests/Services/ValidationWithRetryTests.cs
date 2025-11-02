using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinCheck.Core.Helpers;

namespace WinCheck.IntegrationTests.Services;

/// <summary>
/// Integration tests combining validation with retry logic
/// </summary>
[TestClass]
public class ValidationWithRetryTests
{
    [TestMethod]
    public async Task RetryHelper_WithValidation_RetriesAllExceptionsByDefault()
    {
        // Arrange
        int attemptCount = 0;
        string? apiKey = null;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
        {
            await RetryHelper.RetryAsync(async () =>
            {
                attemptCount++;

                // Validate API key
                if (!ValidationHelper.IsValidApiKeyFormat(apiKey!))
                    throw new ArgumentException("Invalid API key format");

                return "Success";
            }, maxAttempts: 3);
        });

        // RetryHelper retries all exceptions by default, including validation errors
        Assert.AreEqual(3, attemptCount, "RetryHelper retries all exceptions by default");
    }

    [TestMethod]
    public async Task RetryPolicy_WithInputValidation_FiltersExceptions()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxAttempts = 3,
            ShouldRetry = ex => ex is not ArgumentException // Don't retry validation errors
        };

        int attemptCount = 0;

        // Act & Assert - ArgumentException should not be retried
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
        {
            await policy.ExecuteAsync(async () =>
            {
                attemptCount++;

                if (!ValidationHelper.IsValidPortNumber(99999)) // Invalid port
                    throw new ArgumentException("Invalid port");

                return "Success";
            });
        });

        Assert.AreEqual(1, attemptCount, "Validation errors should not trigger retries");
    }

    [TestMethod]
    public async Task ValidationHelper_WithCaching_CachesValidationResults()
    {
        // Arrange
        var validationCache = new CacheHelper<string, bool>(TimeSpan.FromMinutes(1));
        int validationCallCount = 0;

        Func<string, bool> expensiveValidation = (input) =>
        {
            validationCallCount++;
            Thread.Sleep(10); // Simulate expensive validation
            return ValidationHelper.IsValidIPAddress(input);
        };

        // Act
        var ip = "192.168.1.1";

        var result1 = validationCache.GetOrAdd(ip, () => expensiveValidation(ip));
        var result2 = validationCache.GetOrAdd(ip, () => expensiveValidation(ip));
        var result3 = validationCache.GetOrAdd(ip, () => expensiveValidation(ip));

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
        Assert.AreEqual(1, validationCallCount, "Validation should only be called once");
    }

    [TestMethod]
    public void SanitizeFileName_WithRetry_HandlesEdgeCases()
    {
        // Arrange
        var testCases = new[]
        {
            ("normal.txt", "normal.txt"),
            ("file<>:name.txt", "file___name.txt"),
            ("", "unnamed"),
            (null!, "unnamed"),
            ("    ", "unnamed")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = RetryHelper.Retry(() =>
            {
                return ValidationHelper.SanitizeFileName(input);
            }, maxAttempts: 1);

            Assert.AreEqual(expected, result, $"Failed for input: {input}");
        }
    }

    [TestMethod]
    public async Task RateLimiter_WithValidation_PreventsInvalidRequests()
    {
        // Arrange
        var rateLimiter = new RateLimiter(5, TimeSpan.FromSeconds(1));
        var validRequests = 0;
        var invalidRequests = 0;

        var apiKeys = new[]
        {
            "sk-validkey1234567890abcdefghijklmnopqrst",  // Valid
            "invalid",                                      // Invalid
            "sk-validkey9876543210zyxwvutsrqponmlkjih",  // Valid
            "",                                             // Invalid
            "sk-validkeyABCDEFGHIJKLMNOPQRSTUVWXYZ123"   // Valid
        };

        // Act
        var tasks = apiKeys.Select(async key =>
        {
            // Validate before rate limiting
            if (!ValidationHelper.IsValidApiKeyFormat(key))
            {
                Interlocked.Increment(ref invalidRequests);
                return false;
            }

            // Only rate limit valid requests
            await rateLimiter.WaitAsync();
            Interlocked.Increment(ref validRequests);
            return true;
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(3, validRequests, "Should process valid requests");
        Assert.AreEqual(2, invalidRequests, "Should reject invalid requests");
    }

    [TestMethod]
    public async Task PerformanceMonitor_WithValidation_TracksValidationTime()
    {
        // Arrange
        PerformanceStats.Clear();
        var testData = Enumerable.Range(1, 100).Select(i => $"192.168.1.{i}").ToList();

        // Act
        using (var monitor = PerformanceMonitor.Start("BulkValidation", PerformanceStats.Record))
        {
            var validationTasks = testData.Select(async ip =>
            {
                return await Task.Run(() => ValidationHelper.IsValidIPAddress(ip));
            });

            var results = await Task.WhenAll(validationTasks);
        }

        // Assert
        var stats = PerformanceStats.GetStats("BulkValidation");
        Assert.IsNotNull(stats);
        Assert.AreEqual(1, stats.Count);
        Assert.IsTrue(stats.TotalDuration > TimeSpan.Zero);
        Assert.IsTrue(stats.TotalDuration < TimeSpan.FromSeconds(1), "Bulk validation should be fast");
    }
}
