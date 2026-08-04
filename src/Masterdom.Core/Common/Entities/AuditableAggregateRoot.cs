namespace Masterdom.Core.Common.Entities;

public abstract class AuditableAggregateRoot : AggregateRoot
{
    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    public Guid? CreatedBy { get; protected set; }

    public Guid? UpdatedBy { get; protected set; }

    protected AuditableAggregateRoot()
    {
    }

    protected AuditableAggregateRoot(
        Guid id,
        DateTime createdAtUtc,
        Guid? createdBy = null)
        : base(id)
    {
        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy;
    }

    protected void SetUpdated(
        DateTime updatedAtUtc,
        Guid? updatedBy = null)
    {
        UpdatedAtUtc = updatedAtUtc;
        UpdatedBy = updatedBy;
    }
}
