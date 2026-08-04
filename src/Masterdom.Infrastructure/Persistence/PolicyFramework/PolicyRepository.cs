using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;
using Masterdom.Modules.PolicyFramework.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Infrastructure.Persistence.PolicyFramework;

public sealed class PolicyRepository : IPolicyRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PolicyRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(PolicyAggregate policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        _dbContext.Policies.Add(policy);
    }

    public void Update(PolicyAggregate policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        _dbContext.Policies.Update(policy);
    }

    public PolicyAggregate? GetById(PolicyId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.Policies
            .Include(x => x.Versions)
            .Include(x => x.Assignments)
            .Include(x => x.Snapshots)
            .FirstOrDefault(x => x.Id == id);
    }

    public PolicyAggregate? GetApplicable(PolicyType policyType, PolicyScope scope, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(policyType);
        ArgumentNullException.ThrowIfNull(scope);

        return _dbContext.Policies
            .AsEnumerable()
            .Where(x => x.PolicyType == policyType)
            .Where(x => x.ResolveApplicableVersion(scope, asOfDate) is not null)
            .OrderByDescending(x => x.CurrentVersion.VersionNumber)
            .FirstOrDefault();
    }
}
