using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Application.Capabilities.Billability.Contracts;

namespace Masterdom.Modules.Billing.Application.Capabilities.Billability;

/// <summary>
/// Capability service that determines billable tenancy candidates for a billing period.
/// </summary>
public sealed class BillabilityDeterminationService
{
    public BillabilityResolutionResult Determine(BillabilityResolutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var included = new List<BillabilityCandidate>();
        var excluded = new List<ExcludedBillabilityCandidate>();

        foreach (var projection in request.Candidates)
        {
            ArgumentNullException.ThrowIfNull(projection);

            var candidate = new BillabilityCandidate(
                projection.TenancyReference,
                projection.LeaseReference,
                projection.PropertyReference,
                projection.UnitId,
                projection.PrimaryOccupantReference);

            var reasons = DetermineReasons(request.BillingPeriod, projection).ToList();
            if (reasons.Count == 0)
            {
                included.Add(candidate);
                continue;
            }

            excluded.Add(new ExcludedBillabilityCandidate(candidate, BillabilityDecision.Excluded(reasons)));
        }

        return new BillabilityResolutionResult(included, excluded);
    }

    private static IEnumerable<BillabilityExclusionReason> DetermineReasons(
        BillingPeriod billingPeriod,
        BillabilityResolutionRequest.CandidateProjection projection)
    {
        if (HasMissingReference(projection))
        {
            yield return BillabilityExclusionReason.MissingReference;
            yield break;
        }

        if (!IsActiveStatus(projection.LeaseStatus))
        {
            if (IsExpiredLease(projection, billingPeriod))
            {
                yield return BillabilityExclusionReason.ExpiredLease;
            }
            else if (IsFutureLease(projection, billingPeriod))
            {
                yield return BillabilityExclusionReason.FutureLease;
            }
            else
            {
                yield return BillabilityExclusionReason.InactiveLease;
            }
        }

        if (!IsActiveStatus(projection.TenancyStatus))
        {
            yield return BillabilityExclusionReason.InactiveTenancy;
        }

        if (IsVacantUnit(projection.UnitOccupancyStatus))
        {
            yield return BillabilityExclusionReason.VacantUnit;
        }

        if (projection.PrimaryOccupantReference is null)
        {
            yield return BillabilityExclusionReason.NoPrimaryOccupant;
        }

        if (!OverlapsBillingPeriod(billingPeriod, projection))
        {
            yield return BillabilityExclusionReason.OutsideBillingPeriod;
        }
    }

    private static bool HasMissingReference(BillabilityResolutionRequest.CandidateProjection projection)
    {
        return projection.TenancyReference is null
            || projection.LeaseReference is null
            || projection.PropertyReference is null
            || projection.UnitId is null
            || projection.UnitId == Guid.Empty;
    }

    private static bool IsActiveStatus(string status)
    {
        return string.Equals(status?.Trim(), "Active", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVacantUnit(string occupancyStatus)
    {
        return string.Equals(occupancyStatus?.Trim(), "Vacant", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFutureLease(BillabilityResolutionRequest.CandidateProjection projection, BillingPeriod billingPeriod)
    {
        return projection.LeaseEffectiveDate > billingPeriod.EndDate;
    }

    private static bool IsExpiredLease(BillabilityResolutionRequest.CandidateProjection projection, BillingPeriod billingPeriod)
    {
        return projection.LeaseExpiryDate < billingPeriod.StartDate;
    }

    private static bool OverlapsBillingPeriod(BillingPeriod billingPeriod, BillabilityResolutionRequest.CandidateProjection projection)
    {
        var occupancyStart = MaxDate(projection.MoveInDate, projection.LeaseEffectiveDate);
        var occupancyEnd = MinDate(projection.MoveOutDate ?? DateOnly.MaxValue, projection.LeaseExpiryDate);

        return occupancyStart <= billingPeriod.EndDate && billingPeriod.StartDate <= occupancyEnd;
    }

    private static DateOnly MaxDate(DateOnly left, DateOnly right)
    {
        return left > right ? left : right;
    }

    private static DateOnly MinDate(DateOnly left, DateOnly right)
    {
        return left < right ? left : right;
    }
}
