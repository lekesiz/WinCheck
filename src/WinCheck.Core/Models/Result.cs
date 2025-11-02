using System;

namespace WinCheck.Core.Models;

/// <summary>
/// Result pattern for operations that can succeed or fail
/// </summary>
/// <typeparam name="T">Type of the result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string Error { get; }

    private Result(bool isSuccess, T? value, string error)
    {
        if (isSuccess && value == null)
            throw new InvalidOperationException("Success result must have a value");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error message");

        IsSuccess = isSuccess;
        Value = value;
        Error = error ?? string.Empty;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty);
    }

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    public static Result<T> Failure(Exception exception)
    {
        return new Result<T>(false, default, exception.Message);
    }
}

/// <summary>
/// Result pattern for operations that don't return a value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    private Result(bool isSuccess, string error)
    {
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error message");

        IsSuccess = isSuccess;
        Error = error ?? string.Empty;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success()
    {
        return new Result(true, string.Empty);
    }

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    public static Result Failure(Exception exception)
    {
        return new Result(false, exception.Message);
    }
}
