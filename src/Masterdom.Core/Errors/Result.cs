using System.Collections.ObjectModel;

namespace Masterdom.Core.Errors;

/// <summary>
/// Represents the outcome of an operation.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, IReadOnlyCollection<Error> errors)
    {
        if (isSuccess && errors.Count > 0)
            throw new ArgumentException("A successful result cannot contain errors.");

        if (!isSuccess && errors.Count == 0)
            throw new ArgumentException("A failed result must contain at least one error.");

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<Error> Errors { get; }

    public static Result Success() =>
        new(true, Array.Empty<Error>());

    public static Result Failure(Error error) =>
        new(false, new ReadOnlyCollection<Error>(new[] { error }));

    public static Result Failure(IEnumerable<Error> errors) =>
        new(false, new ReadOnlyCollection<Error>(errors.ToArray()));
}
