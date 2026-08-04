using Masterdom.Core.Errors;
using Xunit;

namespace Masterdom.Core.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_ShouldContainError()
    {
        var error = new Error(
            "TEST",
            "Failure",
            ErrorType.Failure);

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);

        var stored = Assert.Single(result.Errors);

        Assert.Equal(error, stored);
    }

    [Fact]
    public void Success_Generic_ShouldReturnValue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_Generic_ShouldNotReturnValue()
    {
        var error = new Error(
            "TEST",
            "Failure");

        var result = Result<int>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(default, result.Value);
    }

    [Fact]
    public void Success_WithErrors_ShouldThrow()
    {
        var error = new Error("X", "X");

        Assert.Throws<ArgumentException>(() =>
            new TestResult(true, new[] { error }));
    }

    [Fact]
    public void Failure_WithoutErrors_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new TestResult(false, Array.Empty<Error>()));
    }

    private sealed class TestResult : Result
    {
        public TestResult(bool success, IReadOnlyCollection<Error> errors)
            : base(success, errors)
        {
        }
    }
}
