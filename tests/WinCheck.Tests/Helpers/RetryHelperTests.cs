using WinCheck.Core.Helpers;

namespace WinCheck.Tests.Helpers;

[TestClass]
public class RetryHelperTests
{
    [TestMethod]
    public async Task RetryAsync_SuccessOnFirstAttempt_DoesNotRetry()
    {
        // Arrange
        int attemptCount = 0;

        // Act
        var result = await RetryHelper.RetryAsync(async () =>
        {
            attemptCount++;
            await Task.CompletedTask;
            return 42;
        });

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(1, attemptCount);
    }

    [TestMethod]
    public async Task RetryAsync_FailsTwiceThenSucceeds_RetriesAndSucceeds()
    {
        // Arrange
        int attemptCount = 0;

        // Act
        var result = await RetryHelper.RetryAsync(async () =>
        {
            attemptCount++;
            await Task.CompletedTask;

            if (attemptCount < 3)
            {
                throw new InvalidOperationException("Temporary failure");
            }

            return 42;
        }, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10));

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(3, attemptCount);
    }

    [TestMethod]
    public async Task RetryAsync_AlwaysFails_ThrowsException()
    {
        // Arrange
        int attemptCount = 0;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await RetryHelper.RetryAsync(async () =>
            {
                attemptCount++;
                await Task.CompletedTask;
                throw new InvalidOperationException("Always fails");
#pragma warning disable CS0162 // Unreachable code detected
                return 42;
#pragma warning restore CS0162
            }, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10));
        });

        Assert.AreEqual(3, attemptCount);
    }

    [TestMethod]
    public async Task RetryAsync_WithOnRetryCallback_CallsCallback()
    {
        // Arrange
        int attemptCount = 0;
        int retryCallbackCount = 0;
        Exception? lastException = null;

        // Act
        try
        {
            await RetryHelper.RetryAsync(async () =>
            {
                attemptCount++;
                await Task.CompletedTask;
                throw new InvalidOperationException($"Attempt {attemptCount}");
#pragma warning disable CS0162
                return 42;
#pragma warning restore CS0162
            },
            maxAttempts: 3,
            initialDelay: TimeSpan.FromMilliseconds(10),
            onRetry: (attempt, ex) =>
            {
                retryCallbackCount++;
                lastException = ex;
            });
        }
        catch
        {
            // Expected to fail
        }

        // Assert
        Assert.AreEqual(3, attemptCount);
        Assert.AreEqual(2, retryCallbackCount); // Callback called on retry (not on last attempt)
        Assert.IsNotNull(lastException);
    }

    [TestMethod]
    public void Retry_NonAsync_WorksCorrectly()
    {
        // Arrange
        int attemptCount = 0;

        // Act
        var result = RetryHelper.Retry(() =>
        {
            attemptCount++;

            if (attemptCount < 2)
            {
                throw new InvalidOperationException("First attempt fails");
            }

            return 42;
        }, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(10));

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(2, attemptCount);
    }

    [TestMethod]
    public async Task RetryPolicy_NetworkPolicy_HasCorrectSettings()
    {
        // Arrange
        var policy = RetryPolicy.NetworkPolicy;

        // Assert
        Assert.AreEqual(5, policy.MaxAttempts);
        Assert.AreEqual(TimeSpan.FromSeconds(2), policy.InitialDelay);
        Assert.AreEqual(TimeSpan.FromMinutes(1), policy.MaxDelay);
    }

    [TestMethod]
    public async Task RetryPolicy_FilePolicy_HasCorrectSettings()
    {
        // Arrange
        var policy = RetryPolicy.FilePolicy;

        // Assert
        Assert.AreEqual(3, policy.MaxAttempts);
        Assert.AreEqual(TimeSpan.FromMilliseconds(500), policy.InitialDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(5), policy.MaxDelay);
    }

    [TestMethod]
    public async Task RetryPolicy_ExecuteAsync_WorksCorrectly()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxAttempts = 3,
            InitialDelay = TimeSpan.FromMilliseconds(10)
        };
        int attemptCount = 0;

        // Act
        var result = await policy.ExecuteAsync(async () =>
        {
            attemptCount++;
            await Task.CompletedTask;

            if (attemptCount < 2)
            {
                throw new InvalidOperationException("First attempt fails");
            }

            return 42;
        });

        // Assert
        Assert.AreEqual(42, result);
        Assert.AreEqual(2, attemptCount);
    }

    [TestMethod]
    public async Task RetryPolicy_ShouldRetry_FiltersExceptions()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxAttempts = 3,
            InitialDelay = TimeSpan.FromMilliseconds(10),
            ShouldRetry = ex => ex is InvalidOperationException
        };
        int attemptCount = 0;

        // Act & Assert - ArgumentException should not be retried
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
        {
            await policy.ExecuteAsync(async () =>
            {
                attemptCount++;
                await Task.CompletedTask;
                throw new ArgumentException("Should not retry");
#pragma warning disable CS0162
                return 42;
#pragma warning restore CS0162
            });
        });

        Assert.AreEqual(1, attemptCount); // Should fail immediately without retry
    }
}
