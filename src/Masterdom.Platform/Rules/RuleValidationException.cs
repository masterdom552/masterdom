using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents rules-engine validation failures.
/// </summary>
public sealed class RuleValidationException : Exception
{
    public RuleValidationException(string message)
        : base(message)
    {
    }
}
