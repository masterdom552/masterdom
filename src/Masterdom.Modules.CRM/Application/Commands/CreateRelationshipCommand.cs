using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Commands;

/// <summary>
/// Command entry point for creating a relationship.
/// </summary>
public sealed record CreateRelationshipCommand(
    PartyId PartyId,
    Relationship Relationship,
    DateTime UpdatedAtUtc,
    string? UpdatedBy = null);
