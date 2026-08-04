using System.Collections.Immutable;
using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.Billing.Contracts.Published.Notifications;

namespace Masterdom.Modules.FinancialLedger.Application.Translation;

public sealed class BillingNotificationTranslator
{
    private const string BillingTenantId = "billing";
    private const string UnspecifiedCurrencyCode = "UNSPECIFIED";
    private const string BillingPeriodStartDateKey = "billingPeriodStartDate";
    private const string BillingPeriodEndDateKey = "billingPeriodEndDate";
    private const string PersistedBillIdsKey = "persistedBillIds";
    private const string PersistedBillCountKey = "persistedBillCount";
    private const string PropertyIdKey = "propertyId";

    public FinancialPostingRequest TranslateBillPersisted(BillPersistedNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var persistedBillIds = notification.PersistedBillIds
            .Select(x => x.ToString("N"))
            .ToArray();

        var requestId = BuildRequestId(notification.CorrelationId, persistedBillIds);

        var metadataExtensions = ImmutableDictionary<string, string>.Empty
            .Add(BillingPeriodStartDateKey, notification.BillingPeriodStartDate.ToString("yyyy-MM-dd"))
            .Add(BillingPeriodEndDateKey, notification.BillingPeriodEndDate.ToString("yyyy-MM-dd"))
            .Add(PersistedBillIdsKey, string.Join(",", persistedBillIds))
            .Add(PersistedBillCountKey, notification.PersistedBillCount.ToString());

        if (notification.PropertyId.HasValue)
        {
            metadataExtensions = metadataExtensions.Add(PropertyIdKey, notification.PropertyId.Value.ToString("N"));
        }

        return new FinancialPostingRequest
        {
            RequestId = requestId,
            CorrelationId = notification.CorrelationId,
            CausationId = null,
            IdempotencyKey = requestId,
            TenantId = BillingTenantId,
            CurrencyCode = UnspecifiedCurrencyCode,
            OccurredAt = new DateTimeOffset(notification.ExecutionTimestampUtc, TimeSpan.Zero),
            TransactionType = FinancialTransactionType.Unspecified,
            DocumentType = FinancialDocumentType.Unspecified,
            Source = PostingSource.Billing,
            Reference = new FinancialPostingReference
            {
                EntityType = nameof(BillPersistedNotification),
                EntityId = requestId,
                DocumentType = nameof(FinancialPostingRequest),
                ExternalReference = notification.CorrelationId,
                TenantId = BillingTenantId
            },
            Metadata = new FinancialPostingMetadata
            {
                Extensions = metadataExtensions
            },
            Lines = ImmutableArray<FinancialPostingLine>.Empty,
            ContractVersion = 1
        };
    }

    private static string BuildRequestId(string correlationId, IEnumerable<string> persistedBillIds)
    {
        return $"{correlationId}:{string.Join("-", persistedBillIds)}";
    }
}
