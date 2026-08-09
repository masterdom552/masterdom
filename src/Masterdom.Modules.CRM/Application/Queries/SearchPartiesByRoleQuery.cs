using Masterdom.Modules.CRM.Domain.Entities.Party;

namespace Masterdom.Modules.CRM.Application.Queries;

/// <summary>
/// Query entry point for searching parties by effective role.
/// </summary>
public sealed record SearchPartiesByRoleQuery(PartyRoleType RoleType, DateTime AsOfUtc, int Take = 50);
