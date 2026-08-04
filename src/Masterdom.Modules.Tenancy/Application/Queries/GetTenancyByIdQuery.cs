using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Queries;

public sealed record GetTenancyByIdQuery(TenancyId TenancyId);
