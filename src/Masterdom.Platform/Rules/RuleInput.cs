using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents rule-evaluation input values.
/// </summary>
public sealed class RuleInput
{
    private readonly IReadOnlyList<RuleInputItem> _items;

    public RuleInput(IEnumerable<RuleInputItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items = items.ToList();
    }

    public IReadOnlyList<RuleInputItem> Items => _items;

    public bool TryGetValue(RuleInputKey key, out RuleValue? value)
    {
        ArgumentNullException.ThrowIfNull(key);

        var item = _items.FirstOrDefault(i => i.Key.Equals(key));
        value = item?.Value;

        return item is not null;
    }

    public RuleValue GetRequiredValue(RuleInputKey key)
    {
        if (!TryGetValue(key, out var value) || value is null)
        {
            throw new RuleValidationException(
                $"Rule input value '{key.Value}' was not provided.");
        }

        return value;
    }
}
