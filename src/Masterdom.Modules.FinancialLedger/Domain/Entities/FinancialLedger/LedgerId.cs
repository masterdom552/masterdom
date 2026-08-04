using Masterdom.Core.Primitives;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed record LedgerId(Guid Value) : EntityId(Value)
{
    public static LedgerId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static LedgerId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("LedgerId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
