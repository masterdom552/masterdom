using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Handlers.Commands;
using Masterdom.Modules.FinancialLedger.Application.Handlers.Queries;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Queries;
using Masterdom.Modules.FinancialLedger.Application.Services;
using Masterdom.Modules.FinancialLedger.Application.Support;
using Masterdom.Modules.FinancialLedger.Application.Translation;
using Masterdom.Modules.FinancialLedger.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Infrastructure;

/// <summary>
/// Registers infrastructure services and adapters.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure registrations to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString =
            configuration.GetConnectionString("Masterdom")
            ?? throw new InvalidOperationException(
                "Connection string 'Masterdom' was not found.");

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<ILedgerUnitOfWork, LedgerUnitOfWork>();
        services.AddScoped<ILedgerPlatformOrchestrator, LedgerPlatformOrchestrator>();
        services.AddScoped<ILedgerApplicationService, LedgerApplicationService>();
        services.AddSingleton(new ChartOfAccountsOptions());
        services.AddSingleton(new BillingPostingRuleEngineOptions());
        services.AddScoped<IChartOfAccounts, InMemoryChartOfAccounts>();
        services.AddScoped<IPostingRuleProvider, BillingPostingRuleEngine>();
        services.AddScoped<IJournalNumberGenerator, BusinessJournalNumberGenerator>();
        services.AddScoped<BillingPostingPolicy>();
        services.AddScoped<PostingLineGenerator>();
        services.AddScoped<JournalPreparationService>();
        services.AddScoped<BillingSnapshotTranslator>();
        services.AddScoped<BillingSnapshotPostingValidator>();
        services.AddScoped<BillingFinancialPostingRequestFactory>();
        services.AddScoped<LegacyPostingAdapter>();
        services.AddScoped<BillingSnapshotPostingPreparationService>();
        services.AddScoped<IPersistedPreparedJournalRepository, PersistedPreparedJournalRepository>();
        services.AddScoped<PreparedJournalPersistenceService>();

        services.AddScoped<ICommandHandler<OpenLedgerCommand, ExecutionResult<LedgerAggregate>>, OpenLedgerCommandHandler>();
        services.AddScoped<ICommandHandler<PostBillingJournalCommand, ExecutionResult<LedgerAggregate>>, PostBillingJournalCommandHandler>();
        services.AddScoped<ICommandHandler<PostPaymentJournalCommand, ExecutionResult<LedgerAggregate>>, PostPaymentJournalCommandHandler>();
        services.AddScoped<ICommandHandler<ReverseJournalCommand, ExecutionResult<LedgerAggregate>>, ReverseJournalCommandHandler>();
        services.AddScoped<ICommandHandler<CompletePostingBatchCommand, ExecutionResult<LedgerAggregate>>, CompletePostingBatchCommandHandler>();

        services.AddScoped<IQueryHandler<GetLedgerByIdQuery, ExecutionResult<LedgerAggregate>>, GetLedgerByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetLedgerByCodeQuery, ExecutionResult<LedgerAggregate>>, GetLedgerByCodeQueryHandler>();

        return services;
    }
}
