using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a typed value consumed by the rules engine.
/// </summary>
public sealed class RuleValue
{
    private RuleValue(
        RuleValueKind kind,
        bool? booleanValue,
        decimal? numberValue,
        string? textValue)
    {
        Kind = kind;
        BooleanValue = booleanValue;
        NumberValue = numberValue;
        TextValue = textValue;
    }

    public RuleValueKind Kind { get; }

    public bool? BooleanValue { get; }

    public decimal? NumberValue { get; }

    public string? TextValue { get; }

    public static RuleValue FromBoolean(bool value)
    {
        return new RuleValue(RuleValueKind.Boolean, value, null, null);
    }

    public static RuleValue FromNumber(decimal value)
    {
        return new RuleValue(RuleValueKind.Number, null, value, null);
    }

    public static RuleValue FromText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RuleValidationException("Text rule value is required.");
        }

        return new RuleValue(RuleValueKind.Text, null, null, value.Trim());
    }

    public bool AsBoolean()
    {
        if (Kind != RuleValueKind.Boolean || !BooleanValue.HasValue)
        {
            throw new RuleValidationException("Rule value is not a boolean.");
        }

        return BooleanValue.Value;
    }

    public decimal AsNumber()
    {
        if (Kind != RuleValueKind.Number || !NumberValue.HasValue)
        {
            throw new RuleValidationException("Rule value is not a number.");
        }

        return NumberValue.Value;
    }

    public string AsText()
    {
        if (Kind != RuleValueKind.Text || string.IsNullOrWhiteSpace(TextValue))
        {
            throw new RuleValidationException("Rule value is not text.");
        }

        return TextValue;
    }
}
