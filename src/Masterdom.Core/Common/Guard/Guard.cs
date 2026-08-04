using Masterdom.Core.Common.Exceptions;

namespace Masterdom.Core.Common.Guard;

public static class Guard
{
    public static void AgainstNull(
        object? value,
        string name)
    {
        if (value is null)
            throw new ArgumentNullException(name);
    }

    public static void AgainstNullOrWhiteSpace(
        string? value,
        string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} cannot be empty.");
    }

    public static void AgainstNegative(
        int value,
        string name)
    {
        if (value < 0)
            throw new DomainException($"{name} cannot be negative.");
    }

    public static void AgainstFalse(
        bool condition,
        string message)
    {
        if (!condition)
            throw new BusinessRuleException(message);
    }

    public static void AgainstTrue(
        bool condition,
        string message)
    {
        if (condition)
            throw new BusinessRuleException(message);
    }
}
