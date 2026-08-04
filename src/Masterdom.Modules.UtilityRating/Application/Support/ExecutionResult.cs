namespace Masterdom.Modules.UtilityRating.Application.Support;

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
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return new ExecutionResult(false, errorCode.Trim(), errorMessage.Trim());
    }
}

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
        ArgumentNullException.ThrowIfNull(value);
        return new ExecutionResult<T>(true, value, null, null);
    }

    public static ExecutionResult<T> Failure(string errorCode, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return new ExecutionResult<T>(false, default, errorCode.Trim(), errorMessage.Trim());
    }
}
