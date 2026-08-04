using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a typed condition model used by rule definitions.
/// </summary>
public sealed class RuleCondition
{
    private RuleCondition(
        RuleInputKey? inputKey,
        RuleComparisonOperator? comparisonOperator,
        RuleValue? expectedValue,
        RuleInputKey? compareInputKey,
        RuleValue? minimumValue,
        RuleValue? maximumValue,
        RuleCompositeOperator? compositeOperator,
        RuleArithmeticOperator? arithmeticOperator,
        RuleInputKey? expressionLeftKey,
        RuleInputKey? expressionRightKey,
        RuleValue? expressionExpectedValue)
    {
        InputKey = inputKey;
        ComparisonOperator = comparisonOperator;
        ExpectedValue = expectedValue;
        CompareInputKey = compareInputKey;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
        CompositeOperator = compositeOperator;
        ArithmeticOperator = arithmeticOperator;
        ExpressionLeftKey = expressionLeftKey;
        ExpressionRightKey = expressionRightKey;
        ExpressionExpectedValue = expressionExpectedValue;
    }

    public RuleInputKey? InputKey { get; }

    public RuleComparisonOperator? ComparisonOperator { get; }

    public RuleValue? ExpectedValue { get; }

    public RuleInputKey? CompareInputKey { get; }

    public RuleValue? MinimumValue { get; }

    public RuleValue? MaximumValue { get; }

    public RuleCompositeOperator? CompositeOperator { get; }

    public RuleArithmeticOperator? ArithmeticOperator { get; }

    public RuleInputKey? ExpressionLeftKey { get; }

    public RuleInputKey? ExpressionRightKey { get; }

    public RuleValue? ExpressionExpectedValue { get; }

    public static RuleCondition Boolean(RuleInputKey inputKey, bool expected)
    {
        ArgumentNullException.ThrowIfNull(inputKey);

        return new RuleCondition(
            inputKey,
            RuleComparisonOperator.Equal,
            RuleValue.FromBoolean(expected),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    public static RuleCondition Comparison(
        RuleInputKey inputKey,
        RuleComparisonOperator comparisonOperator,
        RuleValue expectedValue,
        RuleInputKey? compareInputKey = null)
    {
        ArgumentNullException.ThrowIfNull(inputKey);
        ArgumentNullException.ThrowIfNull(expectedValue);

        return new RuleCondition(
            inputKey,
            comparisonOperator,
            expectedValue,
            compareInputKey,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    public static RuleCondition Range(
        RuleInputKey inputKey,
        RuleValue minimumValue,
        RuleValue maximumValue)
    {
        ArgumentNullException.ThrowIfNull(inputKey);
        ArgumentNullException.ThrowIfNull(minimumValue);
        ArgumentNullException.ThrowIfNull(maximumValue);

        return new RuleCondition(
            inputKey,
            null,
            null,
            null,
            minimumValue,
            maximumValue,
            null,
            null,
            null,
            null,
            null);
    }

    public static RuleCondition Expression(
        RuleInputKey expressionLeftKey,
        RuleInputKey expressionRightKey,
        RuleArithmeticOperator arithmeticOperator,
        RuleComparisonOperator comparisonOperator,
        RuleValue expressionExpectedValue)
    {
        ArgumentNullException.ThrowIfNull(expressionLeftKey);
        ArgumentNullException.ThrowIfNull(expressionRightKey);
        ArgumentNullException.ThrowIfNull(expressionExpectedValue);

        return new RuleCondition(
            null,
            comparisonOperator,
            null,
            null,
            null,
            null,
            null,
            arithmeticOperator,
            expressionLeftKey,
            expressionRightKey,
            expressionExpectedValue);
    }

    public static RuleCondition Composite(RuleCompositeOperator compositeOperator)
    {
        return new RuleCondition(
            null,
            null,
            null,
            null,
            null,
            null,
            compositeOperator,
            null,
            null,
            null,
            null);
    }
}
