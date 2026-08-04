namespace Masterdom.Core.Errors;

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
public sealed class Result<T> : Result
{
    private Result(
        T? value,
        bool isSuccess,
        IReadOnlyCollection<Error> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) =>
        new(value, true, Array.Empty<Error>());

    public static new Result<T> Failure(Error error) =>
        new(default, false, new[] { error });

    public static new Result<T> Failure(IEnumerable<Error> errors) =>
        new(default, false, errors.ToArray());
}
