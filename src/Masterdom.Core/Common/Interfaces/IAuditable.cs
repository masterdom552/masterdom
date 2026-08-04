namespace Masterdom.Core.Common.Interfaces;

public interface IAuditable
{
    DateTime CreatedAtUtc { get; }

    DateTime? UpdatedAtUtc { get; }

    Guid? CreatedBy { get; }

    Guid? UpdatedBy { get; }
}
