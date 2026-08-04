using System.Collections.Immutable;
using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingFinancialPostingRequestFactory
{
    public FinancialPostingRequest Create(
        BillingSnapshotPostingSourceModel source,
        PostingLineGenerationResult generatedLines,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(generatedLines);

        if (occurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Occurred timestamp must be UTC.", nameof(occurredAtUtc));
        }

        var requestId = BuildRequestId(source);
        var postingLines = generatedLines.Lines
            .Select(ToFinancialPostingLine)
            .ToImmutableArray();

        var metadata = ImmutableDictionary<string, string>.Empty
            .Add("billId", source.BillId.ToString("N"))
            .Add("billNumber", source.BillNumber)
            .Add("billingPeriodStartDate", source.BillingPeriodStartDate.ToString("yyyy-MM-dd"))
            .Add("billingPeriodEndDate", source.BillingPeriodEndDate.ToString("yyyy-MM-dd"))
            .Add("propertyId", source.PropertyId.ToString("N"))
            .Add("tenancyId", source.TenancyId.ToString("N"))
            .Add("leaseId", source.LeaseId.ToString("N"));

        return new FinancialPostingRequest
        {
            RequestId = requestId,
            CorrelationId = string.IsNullOrWhiteSpace(source.CorrelationId) ? requestId : source.CorrelationId!,
            CausationId = null,
            IdempotencyKey = requestId,
            TenantId = "billing",
            CurrencyCode = source.CurrencyCode,
            OccurredAt = occurredAtUtc,
            TransactionType = FinancialTransactionType.Charge,
            DocumentType = FinancialDocumentType.Invoice,
            Source = PostingSource.Billing,
            Reference = new FinancialPostingReference
            {
                EntityType = nameof(BillingSnapshotPostingSourceModel),
                EntityId = source.BillId.ToString("N"),
                DocumentType = "BillSnapshotModel",
                DocumentNumber = source.BillNumber,
                ExternalReference = source.CorrelationId,
                TenantId = "billing"
            },
            Metadata = new FinancialPostingMetadata { Extensions = metadata },
            Lines = postingLines,
            ContractVersion = 1
        };
    }

    private static FinancialPostingLine ToFinancialPostingLine(GeneratedPostingLine line)
    {
        var metadata = line.Metadata.Aggregate(
            ImmutableDictionary<string, string>.Empty,
            static (acc, entry) => acc.SetItem(entry.Key, entry.Value));

        return new FinancialPostingLine
        {
            LineId = line.LineId,
            Direction = line.Direction,
            Amount = line.Amount,
            CurrencyCode = line.CurrencyCode,
            Description = line.Description,
            Reference = new FinancialPostingReference
            {
                EntityType = "BillingCharge",
                LineId = line.LineId
            },
            Metadata = new FinancialPostingMetadata
            {
                Extensions = metadata
            }
        };
    }

    private static string BuildRequestId(BillingSnapshotPostingSourceModel source)
    {
        return $"bill-snapshot:{source.BillId:N}:{source.BillingPeriodStartDate:yyyyMMdd}:{source.BillingPeriodEndDate:yyyyMMdd}";
    }
}
