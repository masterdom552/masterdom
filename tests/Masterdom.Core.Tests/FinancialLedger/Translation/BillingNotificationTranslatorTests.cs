using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.Billing.Contracts.Published.Notifications;
using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Core.Tests.FinancialLedger.Translation;

public sealed class BillingNotificationTranslatorTests
{
    [Fact]
    public void TranslateBillPersisted_ShouldMapDeterministically()
    {
        var translator = new BillingNotificationTranslator();
        var billIdOne = Guid.NewGuid();
        var billIdTwo = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var occurredAtUtc = new DateTime(2026, 8, 15, 10, 30, 45, DateTimeKind.Utc);

        var notification = new BillPersistedNotification(
            "corr-ledger-001",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 31),
            [billIdOne, billIdTwo],
            2,
            occurredAtUtc,
            propertyId);

        var result = translator.TranslateBillPersisted(notification);

        Assert.Equal($"corr-ledger-001:{billIdOne:N}-{billIdTwo:N}", result.RequestId);
        Assert.Equal("corr-ledger-001", result.CorrelationId);
        Assert.Equal(result.RequestId, result.IdempotencyKey);
        Assert.Equal("billing", result.TenantId);
        Assert.Equal("UNSPECIFIED", result.CurrencyCode);
        Assert.Equal(occurredAtUtc, result.OccurredAt.UtcDateTime);
        Assert.Equal(FinancialTransactionType.Unspecified, result.TransactionType);
        Assert.Equal(FinancialDocumentType.Unspecified, result.DocumentType);
        Assert.Equal(PostingSource.Billing, result.Source);
    }

    [Fact]
    public void TranslateBillPersisted_ShouldMapIdentifiersAndReferences()
    {
        var translator = new BillingNotificationTranslator();
        var billId = Guid.NewGuid();

        var result = translator.TranslateBillPersisted(
            new BillPersistedNotification(
                "corr-ledger-002",
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 30),
                [billId],
                1,
                DateTime.UtcNow,
                Guid.NewGuid()));

        Assert.NotNull(result.Reference);
        Assert.Equal("BillPersistedNotification", result.Reference!.EntityType);
        Assert.Equal(result.RequestId, result.Reference.EntityId);
        Assert.Equal("FinancialPostingRequest", result.Reference.DocumentType);
        Assert.Equal("corr-ledger-002", result.Reference.ExternalReference);
        Assert.Equal("billing", result.Reference.TenantId);
    }

    [Fact]
    public void TranslateBillPersisted_ShouldMapBillingPeriodAndCountIntoMetadata()
    {
        var translator = new BillingNotificationTranslator();
        var notification = new BillPersistedNotification(
            "corr-ledger-003",
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 31),
            [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
            3,
            DateTime.UtcNow,
            Guid.NewGuid());

        var result = translator.TranslateBillPersisted(notification);

        Assert.NotNull(result.Metadata);
        Assert.Equal("2026-10-01", result.Metadata!.Extensions["billingPeriodStartDate"]);
        Assert.Equal("2026-10-31", result.Metadata.Extensions["billingPeriodEndDate"]);
        Assert.Equal("3", result.Metadata.Extensions["persistedBillCount"]);
    }

    [Fact]
    public void TranslateBillPersisted_ShouldMapOptionalPropertyId_WhenPresent()
    {
        var translator = new BillingNotificationTranslator();
        var propertyId = Guid.NewGuid();

        var result = translator.TranslateBillPersisted(
            new BillPersistedNotification(
                "corr-ledger-004",
                new DateOnly(2026, 11, 1),
                new DateOnly(2026, 11, 30),
                [Guid.NewGuid()],
                1,
                DateTime.UtcNow,
                propertyId));

        Assert.NotNull(result.Metadata);
        Assert.Equal(propertyId.ToString("N"), result.Metadata!.Extensions["propertyId"]);
    }

    [Fact]
    public void TranslateBillPersisted_ShouldOmitOptionalPropertyId_WhenAbsent()
    {
        var translator = new BillingNotificationTranslator();

        var result = translator.TranslateBillPersisted(
            new BillPersistedNotification(
                "corr-ledger-005",
                new DateOnly(2026, 12, 1),
                new DateOnly(2026, 12, 31),
                [Guid.NewGuid()],
                1,
                DateTime.UtcNow));

        Assert.NotNull(result.Metadata);
        Assert.False(result.Metadata!.Extensions.ContainsKey("propertyId"));
    }

    [Fact]
    public void TranslateBillPersisted_ShouldKeepLinesEmpty_ForBoundaryValidationOnly()
    {
        var translator = new BillingNotificationTranslator();

        var result = translator.TranslateBillPersisted(
            new BillPersistedNotification(
                "corr-ledger-006",
                new DateOnly(2027, 1, 1),
                new DateOnly(2027, 1, 31),
                [Guid.NewGuid()],
                1,
                DateTime.UtcNow));

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void TranslateBillPersisted_ShouldThrow_WhenNotificationIsNull()
    {
        var translator = new BillingNotificationTranslator();

        Assert.Throws<ArgumentNullException>(() => translator.TranslateBillPersisted(null!));
    }
}
