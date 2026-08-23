namespace Masterdom.Modules.Intelligence.Application.Support;

/// <summary>
/// Represents a query execution result that may succeed or fail.
/// </summary>
public class ExecutionResult
{
    protected ExecutionResult(bool isSuccess, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    public static ExecutionResult Success()
    {
        return new ExecutionResult(true, null, null);
    }

    public static ExecutionResult Failure(string errorCode, string errorMessage)
    {
        return new ExecutionResult(false, errorCode, errorMessage);
    }
}

/// <summary>
/// Represents a query execution result with a value.
/// </summary>
public sealed class ExecutionResult<T> : ExecutionResult
{
    private ExecutionResult(bool isSuccess, T? value, string? errorCode, string? errorMessage)
        : base(isSuccess, errorCode, errorMessage)
    {
        Value = value;
    }

    public T? Value { get; }

    public static ExecutionResult<T> Success(T value)
    {
        return new ExecutionResult<T>(true, value, null, null);
    }

    public new static ExecutionResult<T> Failure(string errorCode, string errorMessage)
    {
        return new ExecutionResult<T>(false, default, errorCode, errorMessage);
    }
}
