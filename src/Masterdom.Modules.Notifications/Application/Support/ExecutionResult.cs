namespace Masterdom.Modules.Notifications.Application.Support;

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

    public static ExecutionResult Success() => new(true, null, null);

    public static ExecutionResult Failure(string errorCode, string errorMessage) =>
        new(false, errorCode, errorMessage);
}

public sealed class ExecutionResult<T> : ExecutionResult
{
    private ExecutionResult(bool isSuccess, T? value, string? errorCode, string? errorMessage)
        : base(isSuccess, errorCode, errorMessage)
    {
        Value = value;
    }

    public T? Value { get; }

    public static ExecutionResult<T> Success(T value) => new(true, value, null, null);

    public new static ExecutionResult<T> Failure(string errorCode, string errorMessage) =>
        new(false, default, errorCode, errorMessage);
}
