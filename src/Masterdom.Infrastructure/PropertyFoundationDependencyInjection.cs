using Masterdom.Modules.Properties.Application.Commands;
using Masterdom.Modules.Properties.Application.Handlers.Commands;
using Masterdom.Modules.Properties.Application.Handlers.Queries;
using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Repositories;
using Masterdom.Infrastructure.Persistence.Property;
using Masterdom.Infrastructure.Persistence.CRM;
using Masterdom.Infrastructure.Persistence.Lease;
using Masterdom.Infrastructure.Persistence.People;
using Masterdom.Infrastructure.Persistence.Tenancy;
using Masterdom.Infrastructure.Security;
using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Handlers.Commands;
using Masterdom.Modules.Lease.Application.Handlers.Queries;
using Masterdom.Modules.Lease.Application.Queries;
using Masterdom.Modules.Lease.Application.Services;
using Masterdom.Modules.Lease.Domain.Entities.Lease;
using Masterdom.Modules.Lease.Domain.Repositories;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Handlers.Commands;
using Masterdom.Modules.People.Application.Handlers.Queries;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Domain.Repositories;
using Masterdom.Modules.CRM.Application.Commands;
using Masterdom.Modules.CRM.Application.Handlers.Commands;
using Masterdom.Modules.CRM.Application.Handlers.Queries;
using Masterdom.Modules.CRM.Application.Queries;
using Masterdom.Modules.CRM.Application.Services;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Modules.CRM.Domain.Repositories;
using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Handlers.Commands;
using Masterdom.Modules.Tenancy.Application.Handlers.Queries;
using Masterdom.Modules.Tenancy.Application.Queries;
using Masterdom.Modules.Tenancy.Application.Services;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Masterdom.Modules.Tenancy.Domain.Repositories;
using Masterdom.Infrastructure.Persistence.Metering;
using Masterdom.Infrastructure.Persistence.Maintenance;
using Masterdom.Infrastructure.Persistence.Inventory;
using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Handlers.Commands;
using Masterdom.Modules.Metering.Application.Handlers.Queries;
using Masterdom.Modules.Metering.Application.Queries;
using Masterdom.Modules.Metering.Application.Services;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Metering.Domain.Repositories;
using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Handlers.Commands;
using Masterdom.Modules.Maintenance.Application.Handlers.Queries;
using Masterdom.Modules.Maintenance.Application.Queries;
using Masterdom.Modules.Maintenance.Application.Services;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using Masterdom.Modules.Maintenance.Domain.Repositories;
using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Inventory.Application.Handlers.Commands;
using Masterdom.Modules.Inventory.Application.Services;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Masterdom.Modules.Inventory.Domain.Repositories;
using Masterdom.Infrastructure.Persistence.Billing;
using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.Handlers.Commands;
using Masterdom.Modules.Billing.Application.Handlers.Queries;
using Masterdom.Modules.Billing.Application.Queries;
using Masterdom.Modules.Billing.Application.Services;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Domain.Repositories;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.ReadModels;
using Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Pipeline;
using Masterdom.Modules.Billing.Application.Capabilities.Billability;
using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;
using Masterdom.Modules.Billing.Application.Publication;
using Masterdom.Infrastructure.Persistence.FinancialLedger;
using Masterdom.Infrastructure.Persistence.UtilityRating;
using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Handlers.Commands;
using Masterdom.Modules.FinancialLedger.Application.Handlers.Queries;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Queries;
using Masterdom.Modules.FinancialLedger.Application.Services;
using Masterdom.Modules.FinancialLedger.Application.Translation;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Domain.Repositories;
using Masterdom.Infrastructure.Persistence.Payment;
using Masterdom.Infrastructure.Persistence.Documents;
using Masterdom.Infrastructure.Persistence.ReadModels;
using Masterdom.Infrastructure.Persistence.ReadModels.Providers;
using Masterdom.Infrastructure.Persistence.Reporting;
using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Handlers.Commands;
using Masterdom.Modules.Payment.Application.Handlers.Queries;
using Masterdom.Modules.Payment.Application.Queries;
using Masterdom.Modules.Payment.Application.Services;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Masterdom.Modules.Payment.Domain.Repositories;
using Masterdom.Modules.UtilityRating.Application.Commands;
using Masterdom.Modules.UtilityRating.Application.Handlers.Commands;
using Masterdom.Modules.UtilityRating.Application.Handlers.Queries;
using Masterdom.Modules.UtilityRating.Application.Queries;
using Masterdom.Modules.UtilityRating.Application.Services;
using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using Masterdom.Modules.UtilityRating.Domain.Repositories;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;
using Masterdom.Modules.Reporting.Application.Handlers.Queries;
using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Services;
using Masterdom.Modules.Notifications.Application.Commands;
using Masterdom.Modules.Notifications.Application.Handlers.Commands;
using Masterdom.Modules.Notifications.Application.Handlers.Queries;
using Masterdom.Modules.Notifications.Application.Models;
using Masterdom.Modules.Notifications.Application.Queries;
using Masterdom.Modules.Notifications.Application.Services;
using Masterdom.Modules.Documents.Application.Commands;
using Masterdom.Modules.Documents.Application.Handlers.Commands;
using Masterdom.Modules.Documents.Application.Handlers.Queries;
using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Queries;
using Masterdom.Modules.Documents.Application.Services;
using Masterdom.Modules.Settings.Application.Services;
using Masterdom.Modules.Intelligence.Application.Services;
using Masterdom.Infrastructure.Persistence.SubsidyOptimization;
using Masterdom.Modules.SubsidyOptimization.Application.Handlers.Queries;
using Masterdom.Modules.SubsidyOptimization.Application.Handlers.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Queries;
using Masterdom.Modules.SubsidyOptimization.Application.Services;
using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Domain.Repositories;
using Masterdom.Platform.CalculationEngine;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.Notifications;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Events;
using Masterdom.Platform.LanguageSupport;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Recommendation;
using Masterdom.Platform.ReadModels;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;
using PropertyAggregate = Masterdom.Modules.Properties.Domain.Entities.Property.Property;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Infrastructure;

/// <summary>
/// Registers Property Foundation runtime dependencies.
/// </summary>
public static class PropertyFoundationDependencyInjection
{
    /// <summary>
    /// Adds Property runtime composition with platform baseline adapters.
    /// </summary>
    public static IServiceCollection AddPropertyFoundationRuntime(this IServiceCollection services)
    {
        return services.AddPropertyBusinessCapabilityRuntime();
    }

    /// <summary>
    /// Adds Property capability runtime composition across Property, People, Lease, and Tenancy.
    /// </summary>
    public static IServiceCollection AddPropertyBusinessCapabilityRuntime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        AddPlatformFoundation(services);
        AddPropertyRuntime(services);
        AddCrmRuntime(services);
        AddPeopleRuntime(services);
        AddLeaseRuntime(services);
        AddTenancyRuntime(services);
        AddMeteringRuntime(services);
        AddInventoryRuntime(services);
        AddMaintenanceRuntime(services);
        AddBillingRuntime(services);
        AddUtilityRatingRuntime(services);
        AddFinancialLedgerRuntime(services);
        AddPaymentRuntime(services);
        AddReportingRuntime(services);
        AddNotificationsRuntime(services);
        AddDocumentsRuntime(services);
        AddSettingsRuntime(services);
        AddIntelligenceRuntime(services);
        AddSubsidyOptimizationRuntime(services);

        return services;
    }

    private static void AddPlatformFoundation(IServiceCollection services)
    {
        services.AddSecurityInfrastructureRuntime();

        services.AddScoped<ITenancyReadModelProvider, TenancyReadModelProvider>();
        services.AddScoped<IPropertyReadModelProvider, PropertyReadModelProvider>();
        services.AddScoped<IMeteringReadModelProvider, MeteringReadModelProvider>();
        services.AddScoped<IBillingReadModelProvider, BillingReadModelProvider>();
        services.AddScoped<IPaymentReadModelProvider, PaymentReadModelProvider>();
        services.AddScoped<IFinancialLedgerReadModelProvider, FinancialLedgerReadModelProvider>();
        services.AddScoped<IReadModelProvider>(sp => sp.GetRequiredService<ITenancyReadModelProvider>());
        services.AddScoped<IReadModelProvider>(sp => sp.GetRequiredService<IPropertyReadModelProvider>());
        services.AddScoped<IReadModelProvider>(sp => sp.GetRequiredService<IMeteringReadModelProvider>());
        services.AddScoped<IReadModelProvider>(sp => sp.GetRequiredService<IBillingReadModelProvider>());
        services.AddScoped<IReadModelProvider>(sp => sp.GetRequiredService<IPaymentReadModelProvider>());
        services.AddScoped<IReadModelProvider>(sp => sp.GetRequiredService<IFinancialLedgerReadModelProvider>());
        services.AddScoped<IReadModelRegistry, ReadModelRegistry>();
        services.AddScoped<IReadModelProjectionOrchestrator, ReadModelProjectionOrchestrator>();

        // Platform baseline in-memory dependencies required by orchestrators.
        services.AddSingleton<IConfigurationRepository, InMemoryConfigurationRepository>();
        services.AddSingleton<IConfigurationDefaults, DefaultConfigurationDefaults>();
        services.AddSingleton<IConfigurationResolver, ConfigurationResolver>();
        services.AddSingleton<IBusinessConfigurationCatalog, BusinessConfigurationCatalog>();

        services.AddSingleton(BusinessContextOptions.Default);
        services.AddScoped<BusinessContextBuilderRegistry>();
        services.AddScoped<IBusinessContextBuilder, BusinessContextBuilder>();

        services.AddSingleton<IRecommendationRepository, InMemoryRecommendationRepository>();
        services.AddSingleton<IDecisionRepository, InMemoryDecisionRepository>();
        services.AddSingleton<IOptimizationSessionRepository, InMemoryOptimizationSessionRepository>();
        services.AddScoped<RecommendationProviderRegistry>();
        services.AddScoped<RecommendationConsumerRegistry>();
        services.AddScoped(_ => CreateDefaultRecommendationConsumerExecutionContext());
        services.AddScoped(_ => RecommendationConsumerExecutionSummary.Empty);
        services.AddScoped<RecommendationPipeline>();

        services.AddSingleton<ILanguageContextAccessor, AsyncLocalLanguageContextAccessor>();
        services.AddScoped<ILanguageSettingsResolver, ConfigurationLanguageSettingsResolver>();
        services.AddSingleton<ILanguageResourceProvider>(sp =>
            new DefaultLanguageResourceProvider(
                embeddedResources: null,
                businessConfigurationCatalog: sp.GetRequiredService<IBusinessConfigurationCatalog>(),
                contextAccessor: sp.GetRequiredService<ILanguageContextAccessor>(),
                name: "default"));
        services.AddSingleton<ILanguageFormatterProvider, DefaultLanguageFormatterProvider>();
        services.AddScoped<ILanguageSupportService, DefaultLanguageSupportService>();

        services.AddSingleton<IMetadataRepository, InMemoryMetadataRepository>();
        services.AddSingleton<IMetadataResolver, MetadataResolver>();

        services.AddSingleton<IRuleRepository, InMemoryRuleRepository>();
        services.AddSingleton<IRuleResolver, RuleResolver>();

        services.AddSingleton<IWorkflowRepository, InMemoryWorkflowRepository>();
        services.AddSingleton<IWorkflowStateStore, InMemoryWorkflowStateStore>();
        services.AddSingleton<IWorkflowResolver, WorkflowResolver>();

        services.AddSingleton<IEventRepository, InMemoryEventRepository>();
        services.AddSingleton<IEventRegistry, EventRegistry>();
        services.AddSingleton<IEventHandlerResolver, EventHandlerResolver>();
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        services.AddSingleton<IEventStore, EventStore>();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<IDomainEventAdapter, DomainEventAdapter>();
        services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
    }

    private static void AddPropertyRuntime(IServiceCollection services)
    {
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IPropertyUnitOfWork, PropertyUnitOfWork>();
        services.AddScoped<IPropertyPlatformOrchestrator, PropertyPlatformOrchestrator>();
        services.AddScoped<IPropertyApplicationService, PropertyApplicationService>();

        AddPropertyCommandHandler<CreatePropertyCommand, ExecutionResult<PropertyAggregate>, CreatePropertyCommandHandler>(services);
        AddPropertyCommandHandler<RenamePropertyCommand, ExecutionResult<PropertyAggregate>, RenamePropertyCommandHandler>(services);
        AddPropertyCommandHandler<ChangePropertyStatusCommand, ExecutionResult<PropertyAggregate>, ChangePropertyStatusCommandHandler>(services);
        AddPropertyCommandHandler<CreateUnitCommand, ExecutionResult<Unit>, CreateUnitCommandHandler>(services);
        AddPropertyCommandHandler<RemoveUnitCommand, ExecutionResult<bool>, RemoveUnitCommandHandler>(services);

        AddPropertyCommandHandler<ChangeDescriptionCommand, ExecutionResult<PropertyAggregate>, ChangeDescriptionCommandHandler>(services);
        AddPropertyCommandHandler<ChangeRemarksCommand, ExecutionResult<PropertyAggregate>, ChangeRemarksCommandHandler>(services);
        AddPropertyCommandHandler<ChangeOwnerCommand, ExecutionResult<PropertyAggregate>, ChangeOwnerCommandHandler>(services);
        AddPropertyCommandHandler<ChangeAddressCommand, ExecutionResult<PropertyAggregate>, ChangeAddressCommandHandler>(services);
        AddPropertyCommandHandler<ConfigureSettingsCommand, ExecutionResult<PropertyAggregate>, ConfigureSettingsCommandHandler>(services);
        AddPropertyCommandHandler<ChangeParentPropertyCommand, ExecutionResult<PropertyAggregate>, ChangeParentPropertyCommandHandler>(services);
        AddPropertyCommandHandler<SetEffectivePeriodCommand, ExecutionResult<PropertyAggregate>, SetEffectivePeriodCommandHandler>(services);
        AddPropertyCommandHandler<SetDisplayOrderCommand, ExecutionResult<PropertyAggregate>, SetDisplayOrderCommandHandler>(services);
        AddPropertyCommandHandler<HidePropertyCommand, ExecutionResult<PropertyAggregate>, HidePropertyCommandHandler>(services);
        AddPropertyCommandHandler<ShowPropertyCommand, ExecutionResult<PropertyAggregate>, ShowPropertyCommandHandler>(services);
        AddPropertyCommandHandler<ChangeTypeCommand, ExecutionResult<PropertyAggregate>, ChangeTypeCommandHandler>(services);
        AddPropertyCommandHandler<AddExistingUnitCommand, ExecutionResult<Unit>, AddExistingUnitCommandHandler>(services);
        AddPropertyCommandHandler<UpsertMetadataCommand, ExecutionResult<PropertyAggregate>, UpsertMetadataCommandHandler>(services);
        AddPropertyCommandHandler<RemoveMetadataCommand, ExecutionResult<bool>, RemoveMetadataCommandHandler>(services);
        AddPropertyCommandHandler<Masterdom.Modules.Properties.Application.Commands.AddRelationshipCommand, ExecutionResult<PropertyAggregate>, Masterdom.Modules.Properties.Application.Handlers.Commands.AddRelationshipCommandHandler>(services);
        AddPropertyCommandHandler<Masterdom.Modules.Properties.Application.Commands.RemoveRelationshipCommand, ExecutionResult<bool>, Masterdom.Modules.Properties.Application.Handlers.Commands.RemoveRelationshipCommandHandler>(services);

        AddPropertyQueryHandler<GetPropertyByIdQuery, ExecutionResult<PropertyAggregate>, GetPropertyByIdQueryHandler>(services);
        AddPropertyQueryHandler<GetPropertyByCodeQuery, ExecutionResult<PropertyAggregate>, GetPropertyByCodeQueryHandler>(services);
        AddPropertyQueryHandler<ListUnitsQuery, ExecutionResult<IReadOnlyCollection<Unit>>, ListUnitsQueryHandler>(services);
        AddPropertyQueryHandler<SearchPropertiesQuery, ExecutionResult<IReadOnlyCollection<PropertyAggregate>>, SearchPropertiesQueryHandler>(services);
    }

    private static void AddPeopleRuntime(IServiceCollection services)
    {
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<Masterdom.Modules.People.Application.Support.IPersonUnitOfWork, PersonUnitOfWork>();
        services.AddScoped<Masterdom.Modules.People.Application.Support.IPersonPlatformOrchestrator, PersonPlatformOrchestrator>();
        services.AddScoped<IPersonApplicationService, PersonApplicationService>();

        AddPeopleCommandHandler<CreatePersonCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, CreatePersonCommandHandler>(services);
        AddPeopleCommandHandler<RenamePersonCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, RenamePersonCommandHandler>(services);
        AddPeopleCommandHandler<ChangePersonStatusCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, ChangePersonStatusCommandHandler>(services);
        AddPeopleCommandHandler<AddContactCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, AddContactCommandHandler>(services);
        AddPeopleCommandHandler<RemoveContactCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<bool>, RemoveContactCommandHandler>(services);
        AddPeopleCommandHandler<AddIdentityDocumentCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, AddIdentityDocumentCommandHandler>(services);
        AddPeopleCommandHandler<Masterdom.Modules.People.Application.Commands.AddRelationshipCommand, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, Masterdom.Modules.People.Application.Handlers.Commands.AddRelationshipCommandHandler>(services);

        AddPeopleQueryHandler<GetPersonByIdQuery, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, GetPersonByIdQueryHandler>(services);
        AddPeopleQueryHandler<GetPersonByNumberQuery, Masterdom.Modules.People.Application.Support.ExecutionResult<Person>, GetPersonByNumberQueryHandler>(services);
        AddPeopleQueryHandler<SearchPeopleQuery, Masterdom.Modules.People.Application.Support.ExecutionResult<IReadOnlyCollection<Person>>, SearchPeopleQueryHandler>(services);
    }

    private static void AddCrmRuntime(IServiceCollection services)
    {
        services.AddScoped<IPartyRepository, PartyRepository>();
        services.AddScoped<Masterdom.Modules.CRM.Application.Support.IPartyUnitOfWork, PartyUnitOfWork>();
        services.AddScoped<Masterdom.Modules.CRM.Application.Support.IPartyPlatformOrchestrator, PartyPlatformOrchestrator>();
        services.AddScoped<IPartyApplicationService, PartyApplicationService>();

        AddCrmCommandHandler<CreatePartyCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, CreatePartyCommandHandler>(services);
        AddCrmCommandHandler<UpdatePartyCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, UpdatePartyCommandHandler>(services);
        AddCrmCommandHandler<DeactivatePartyCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, DeactivatePartyCommandHandler>(services);
        AddCrmCommandHandler<AddContactMethodCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, AddContactMethodCommandHandler>(services);
        AddCrmCommandHandler<RemoveContactMethodCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<bool>, RemoveContactMethodCommandHandler>(services);
        AddCrmCommandHandler<AddAddressCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, AddAddressCommandHandler>(services);
        AddCrmCommandHandler<RemoveAddressCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<bool>, RemoveAddressCommandHandler>(services);
        AddCrmCommandHandler<CreateRelationshipCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, CreateRelationshipCommandHandler>(services);
        AddCrmCommandHandler<Masterdom.Modules.CRM.Application.Commands.RemoveRelationshipCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<bool>, Masterdom.Modules.CRM.Application.Handlers.Commands.RemoveRelationshipCommandHandler>(services);
        AddCrmCommandHandler<AssignPartyRoleCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, AssignPartyRoleCommandHandler>(services);
        AddCrmCommandHandler<RemovePartyRoleCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<bool>, RemovePartyRoleCommandHandler>(services);
        AddCrmCommandHandler<DeactivatePartyRoleCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<bool>, DeactivatePartyRoleCommandHandler>(services);
        AddCrmCommandHandler<ReactivatePartyRoleCommand, Masterdom.Modules.CRM.Application.Support.ExecutionResult<bool>, ReactivatePartyRoleCommandHandler>(services);

        AddCrmQueryHandler<GetPartyByIdQuery, Masterdom.Modules.CRM.Application.Support.ExecutionResult<Party>, GetPartyByIdQueryHandler>(services);
        AddCrmQueryHandler<SearchPartiesQuery, Masterdom.Modules.CRM.Application.Support.ExecutionResult<IReadOnlyCollection<Party>>, SearchPartiesQueryHandler>(services);
        AddCrmQueryHandler<GetPartyRolesQuery, Masterdom.Modules.CRM.Application.Support.ExecutionResult<IReadOnlyCollection<PartyRoleAssignment>>, GetPartyRolesQueryHandler>(services);
        AddCrmQueryHandler<SearchPartiesByRoleQuery, Masterdom.Modules.CRM.Application.Support.ExecutionResult<IReadOnlyCollection<Party>>, SearchPartiesByRoleQueryHandler>(services);
    }

    private static void AddLeaseRuntime(IServiceCollection services)
    {
        services.AddScoped<ILeaseRepository, LeaseRepository>();
        services.AddScoped<Masterdom.Modules.Lease.Application.Support.ILeaseUnitOfWork, LeaseUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Lease.Application.Support.ILeasePlatformOrchestrator, LeasePlatformOrchestrator>();
        services.AddScoped<ILeaseApplicationService, LeaseApplicationService>();
        services.AddScoped<ILeasePolicyCatalog, LeasePolicyCatalog>();

        AddLeaseCommandHandler<CreateLeaseCommand, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, CreateLeaseCommandHandler>(services);
        AddLeaseCommandHandler<ActivateLeaseCommand, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, ActivateLeaseCommandHandler>(services);
        AddLeaseCommandHandler<RenewLeaseCommand, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, RenewLeaseCommandHandler>(services);
        AddLeaseCommandHandler<TerminateLeaseCommand, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, TerminateLeaseCommandHandler>(services);
        AddLeaseCommandHandler<ExpireLeaseCommand, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, ExpireLeaseCommandHandler>(services);
        AddLeaseCommandHandler<CloseLeaseCommand, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, CloseLeaseCommandHandler>(services);
        AddLeaseCommandHandler<ChangeCommercialTermsCommand, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, ChangeCommercialTermsCommandHandler>(services);

        AddLeaseQueryHandler<GetLeaseByIdQuery, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, GetLeaseByIdQueryHandler>(services);
        AddLeaseQueryHandler<GetLeaseByNumberQuery, Masterdom.Modules.Lease.Application.Support.ExecutionResult<LeaseAggregate>, GetLeaseByNumberQueryHandler>(services);
    }

    private static void AddTenancyRuntime(IServiceCollection services)
    {
        services.AddScoped<ITenancyRepository, TenancyRepository>();
        services.AddScoped<Masterdom.Modules.Tenancy.Application.Support.ITenancyUnitOfWork, TenancyUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Tenancy.Application.Support.ITenancyPlatformOrchestrator, TenancyPlatformOrchestrator>();
        services.AddScoped<ITenancyApplicationService, TenancyApplicationService>();

        AddTenancyCommandHandler<CreateTenancyCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, CreateTenancyCommandHandler>(services);
        AddTenancyCommandHandler<AddOccupantCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, AddOccupantCommandHandler>(services);
        AddTenancyCommandHandler<RemoveOccupantCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<bool>, RemoveOccupantCommandHandler>(services);
        AddTenancyCommandHandler<RecordMoveInCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, RecordMoveInCommandHandler>(services);
        AddTenancyCommandHandler<RecordMoveOutCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, RecordMoveOutCommandHandler>(services);
        AddTenancyCommandHandler<CloseTenancyCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, CloseTenancyCommandHandler>(services);
        AddTenancyCommandHandler<ArchiveTenancyCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, ArchiveTenancyCommandHandler>(services);
        AddTenancyCommandHandler<UpdateTenancyNotesCommand, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, UpdateTenancyNotesCommandHandler>(services);

        AddTenancyQueryHandler<GetTenancyByIdQuery, Masterdom.Modules.Tenancy.Application.Support.ExecutionResult<TenancyAggregate>, GetTenancyByIdQueryHandler>(services);
    }

    private static void AddMeteringRuntime(IServiceCollection services)
    {
        services.AddScoped<IMeterRepository, MeterRepository>();
        services.AddScoped<Masterdom.Modules.Metering.Application.Support.IMeteringUnitOfWork, MeteringUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Metering.Application.Support.IMeteringPlatformOrchestrator, MeteringPlatformOrchestrator>();
        services.AddScoped<IMeteringApplicationService, MeteringApplicationService>();

        AddMeteringCommandHandler<InstallMeterCommand, Masterdom.Modules.Metering.Application.Support.ExecutionResult<MeterAggregate>, InstallMeterCommandHandler>(services);
        AddMeteringCommandHandler<SubmitReadingCommand, Masterdom.Modules.Metering.Application.Support.ExecutionResult<MeterAggregate>, SubmitReadingCommandHandler>(services);
        AddMeteringCommandHandler<ApproveReadingCommand, Masterdom.Modules.Metering.Application.Support.ExecutionResult<MeterAggregate>, ApproveReadingCommandHandler>(services);
        AddMeteringCommandHandler<CorrectReadingCommand, Masterdom.Modules.Metering.Application.Support.ExecutionResult<MeterAggregate>, CorrectReadingCommandHandler>(services);
        AddMeteringCommandHandler<RetireMeterCommand, Masterdom.Modules.Metering.Application.Support.ExecutionResult<MeterAggregate>, RetireMeterCommandHandler>(services);

        AddMeteringQueryHandler<GetMeterByIdQuery, Masterdom.Modules.Metering.Application.Support.ExecutionResult<MeterAggregate>, GetMeterByIdQueryHandler>(services);
        AddMeteringQueryHandler<GetMeterByNumberQuery, Masterdom.Modules.Metering.Application.Support.ExecutionResult<MeterAggregate>, GetMeterByNumberQueryHandler>(services);
    }

    private static void AddBillingRuntime(IServiceCollection services)
    {
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<Masterdom.Modules.Billing.Application.Support.IBillingUnitOfWork, BillingUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Billing.Application.Support.IBillingPlatformOrchestrator, BillingPlatformOrchestrator>();
        services.AddScoped<IBillingApplicationService, BillingApplicationService>();

        services.AddScoped<IChargeCompositionReadService, BillingChargeCompositionReadService>();
        services.AddScoped<ChargeCompositionPipeline>(serviceProvider =>
            new ChargeCompositionPipeline(serviceProvider.GetRequiredService<IChargeCompositionReadService>()));
        services.AddScoped<BillabilityDeterminationService>();
        services.AddScoped<BillPersistenceOperation>();
        services.AddScoped<BillingNotificationProjector>();
        services.AddScoped<IBillPersistenceService, BillPersistenceService>();
        services.AddScoped<BillPersistenceCapability>();

        AddBillingCommandHandler<GenerateBillCommand, Masterdom.Modules.Billing.Application.Support.ExecutionResult<BillAggregate>, GenerateBillCommandHandler>(services);
        AddBillingCommandHandler<FinalizeBillCommand, Masterdom.Modules.Billing.Application.Support.ExecutionResult<BillAggregate>, FinalizeBillCommandHandler>(services);
        AddBillingCommandHandler<AddAdjustmentCommand, Masterdom.Modules.Billing.Application.Support.ExecutionResult<BillAggregate>, AddAdjustmentCommandHandler>(services);
        AddBillingCommandHandler<ApplyCreditCommand, Masterdom.Modules.Billing.Application.Support.ExecutionResult<BillAggregate>, ApplyCreditCommandHandler>(services);
        AddBillingCommandHandler<VoidBillCommand, Masterdom.Modules.Billing.Application.Support.ExecutionResult<BillAggregate>, VoidBillCommandHandler>(services);

        AddBillingQueryHandler<GetBillByIdQuery, Masterdom.Modules.Billing.Application.Support.ExecutionResult<BillAggregate>, GetBillByIdQueryHandler>(services);
        AddBillingQueryHandler<GetBillByNumberQuery, Masterdom.Modules.Billing.Application.Support.ExecutionResult<BillAggregate>, GetBillByNumberQueryHandler>(services);
    }

    private static void AddUtilityRatingRuntime(IServiceCollection services)
    {
        services.AddScoped<IUtilityRatingRepository, UtilityRatingRepository>();
        services.AddScoped<Masterdom.Modules.UtilityRating.Application.Support.IUtilityRatingUnitOfWork, UtilityRatingUnitOfWork>();
        services.AddScoped<Masterdom.Modules.UtilityRating.Application.Support.IUtilityRatingPlatformOrchestrator, UtilityRatingPlatformOrchestrator>();
        services.AddScoped<Masterdom.Modules.UtilityRating.Application.Services.IUtilityRatingApplicationService, UtilityRatingApplicationService>();

        AddUtilityRatingCommandHandler<RateConsumptionCommand, Masterdom.Modules.UtilityRating.Application.Support.ExecutionResult<UtilityRatingAggregate>, RateConsumptionCommandHandler>(services);
        AddUtilityRatingQueryHandler<GetRatingByIdQuery, Masterdom.Modules.UtilityRating.Application.Support.ExecutionResult<UtilityRatingAggregate>, GetRatingByIdQueryHandler>(services);
        AddUtilityRatingQueryHandler<GetLatestRatingQuery, Masterdom.Modules.UtilityRating.Application.Support.ExecutionResult<UtilityRatingAggregate>, GetLatestRatingQueryHandler>(services);
    }

    private static void AddMaintenanceRuntime(IServiceCollection services)
    {
        services.AddScoped<IMaintenanceTicketRepository, MaintenanceTicketRepository>();
        services.AddScoped<Masterdom.Modules.Maintenance.Application.Support.IMaintenanceUnitOfWork, MaintenanceUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Maintenance.Application.Support.IMaintenancePlatformOrchestrator, MaintenancePlatformOrchestrator>();
        services.AddScoped<IMaintenanceApplicationService, MaintenanceApplicationService>();

        AddMaintenanceCommandHandler<CreateMaintenanceTicketCommand, Masterdom.Modules.Maintenance.Application.Support.ExecutionResult<MaintenanceTicketAggregate>, CreateMaintenanceTicketCommandHandler>(services);
        AddMaintenanceCommandHandler<AssignMaintenanceTicketCommand, Masterdom.Modules.Maintenance.Application.Support.ExecutionResult<MaintenanceTicketAggregate>, AssignMaintenanceTicketCommandHandler>(services);
        AddMaintenanceCommandHandler<CloseMaintenanceTicketCommand, Masterdom.Modules.Maintenance.Application.Support.ExecutionResult<MaintenanceTicketAggregate>, CloseMaintenanceTicketCommandHandler>(services);
        AddMaintenanceQueryHandler<GetMaintenanceTicketByIdQuery, Masterdom.Modules.Maintenance.Application.Support.ExecutionResult<MaintenanceTicketAggregate>, GetMaintenanceTicketByIdQueryHandler>(services);
    }

    private static void AddInventoryRuntime(IServiceCollection services)
    {
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<Masterdom.Modules.Inventory.Application.Support.IInventoryUnitOfWork, InventoryUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Inventory.Application.Support.IInventoryPlatformOrchestrator, InventoryPlatformOrchestrator>();
        services.AddScoped<Masterdom.Modules.Inventory.Application.Support.IInventoryStockLocationLookup, InventoryStockLocationLookup>();
        services.AddScoped<IInventoryApplicationService, InventoryApplicationService>();

        AddInventoryCommandHandler<CreateInventoryItemCommand, Masterdom.Modules.Inventory.Application.Support.ExecutionResult<InventoryItemAggregate>, CreateInventoryItemCommandHandler>(services);
        AddInventoryCommandHandler<ReceiveStockCommand, Masterdom.Modules.Inventory.Application.Support.ExecutionResult<InventoryItemAggregate>, ReceiveStockCommandHandler>(services);
        AddInventoryCommandHandler<AdjustStockCommand, Masterdom.Modules.Inventory.Application.Support.ExecutionResult<InventoryItemAggregate>, AdjustStockCommandHandler>(services);
        AddInventoryCommandHandler<TransferInventoryCommand, Masterdom.Modules.Inventory.Application.Support.ExecutionResult<InventoryItemAggregate>, TransferInventoryCommandHandler>(services);
    }

    private static void AddFinancialLedgerRuntime(IServiceCollection services)
    {
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<Masterdom.Modules.FinancialLedger.Application.Support.ILedgerUnitOfWork, LedgerUnitOfWork>();
        services.AddScoped<Masterdom.Modules.FinancialLedger.Application.Support.ILedgerPlatformOrchestrator, LedgerPlatformOrchestrator>();
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
        services.AddScoped<BillingNotificationTranslator>();
        services.AddScoped<BillingSnapshotPostingValidator>();
        services.AddScoped<BillingFinancialPostingRequestFactory>();
        services.AddScoped<LegacyPostingAdapter>();
        services.AddScoped<BillingSnapshotPostingPreparationService>();
        services.AddScoped<IPersistedPreparedJournalRepository, PersistedPreparedJournalRepository>();
        services.AddScoped<PreparedJournalPersistenceService>();

        AddFinancialLedgerCommandHandler<OpenLedgerCommand, Masterdom.Modules.FinancialLedger.Application.Support.ExecutionResult<LedgerAggregate>, OpenLedgerCommandHandler>(services);
        AddFinancialLedgerCommandHandler<PostBillingJournalCommand, Masterdom.Modules.FinancialLedger.Application.Support.ExecutionResult<LedgerAggregate>, PostBillingJournalCommandHandler>(services);
        AddFinancialLedgerCommandHandler<PostPaymentJournalCommand, Masterdom.Modules.FinancialLedger.Application.Support.ExecutionResult<LedgerAggregate>, PostPaymentJournalCommandHandler>(services);
        AddFinancialLedgerCommandHandler<ReverseJournalCommand, Masterdom.Modules.FinancialLedger.Application.Support.ExecutionResult<LedgerAggregate>, ReverseJournalCommandHandler>(services);
        AddFinancialLedgerCommandHandler<CompletePostingBatchCommand, Masterdom.Modules.FinancialLedger.Application.Support.ExecutionResult<LedgerAggregate>, CompletePostingBatchCommandHandler>(services);

        AddFinancialLedgerQueryHandler<GetLedgerByIdQuery, Masterdom.Modules.FinancialLedger.Application.Support.ExecutionResult<LedgerAggregate>, GetLedgerByIdQueryHandler>(services);
        AddFinancialLedgerQueryHandler<GetLedgerByCodeQuery, Masterdom.Modules.FinancialLedger.Application.Support.ExecutionResult<LedgerAggregate>, GetLedgerByCodeQueryHandler>(services);
    }

    private static void AddPaymentRuntime(IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<Masterdom.Modules.Payment.Application.Support.IPaymentUnitOfWork, PaymentUnitOfWork>();
        services.AddScoped<Masterdom.Modules.Payment.Application.Support.IPaymentPlatformOrchestrator, PaymentPlatformOrchestrator>();
        services.AddScoped<IPaymentApplicationService, PaymentApplicationService>();

        AddPaymentCommandHandler<ReceivePaymentCommand, Masterdom.Modules.Payment.Application.Support.ExecutionResult<PaymentAggregate>, ReceivePaymentCommandHandler>(services);
        AddPaymentCommandHandler<AllocatePaymentCommand, Masterdom.Modules.Payment.Application.Support.ExecutionResult<PaymentAggregate>, AllocatePaymentCommandHandler>(services);
        AddPaymentCommandHandler<ReversePaymentCommand, Masterdom.Modules.Payment.Application.Support.ExecutionResult<PaymentAggregate>, ReversePaymentCommandHandler>(services);
        AddPaymentCommandHandler<VoidPaymentCommand, Masterdom.Modules.Payment.Application.Support.ExecutionResult<PaymentAggregate>, VoidPaymentCommandHandler>(services);

        AddPaymentQueryHandler<GetPaymentByIdQuery, Masterdom.Modules.Payment.Application.Support.ExecutionResult<PaymentAggregate>, GetPaymentByIdQueryHandler>(services);
        AddPaymentQueryHandler<GetPaymentByReferenceQuery, Masterdom.Modules.Payment.Application.Support.ExecutionResult<PaymentAggregate>, GetPaymentByReferenceQueryHandler>(services);
    }

    private static void AddReportingRuntime(IServiceCollection services)
    {
        services.AddSingleton<Masterdom.Platform.ReadModels.IReportReadModelRegistry, ReportReadModelRegistry>();
        services.AddSingleton<IReportTemplateStore, InMemoryReportTemplateStore>();
        services.AddSingleton<IReportSnapshotStore, InMemoryReportSnapshotStore>();
        services.AddScoped<IReportPermissionService, ReportPermissionService>();
        services.AddScoped<IReportExportService, ReportExportService>();
        services.AddScoped<Masterdom.Modules.Reporting.Application.Support.IReportPlatformOrchestrator, ReportingPlatformOrchestrator>();
        services.AddScoped<IReportApplicationService, ReportApplicationService>();

        AddReportingQueryHandler<GenerateReportQuery, Masterdom.Modules.Reporting.Application.Support.ExecutionResult<GeneratedReport>, GenerateReportQueryHandler>(services);
    }

    private static void AddNotificationsRuntime(IServiceCollection services)
    {
        services.AddSingleton<INotificationRegistry, MetadataDrivenNotificationRegistry>();
        services.AddSingleton<INotificationTemplateRegistry, NotificationTemplateRegistry>();
        services.AddSingleton<INotificationPreferenceStore, InMemoryNotificationPreferenceStore>();
        services.AddSingleton<INotificationHistoryStore, InMemoryNotificationHistoryStore>();
        services.AddSingleton<INotificationDeliveryQueue, InMemoryNotificationDeliveryQueue>();

        services.AddScoped<INotificationTemplateRenderer, DefaultNotificationTemplateRenderer>();
        services.AddScoped<INotificationRecipientResolver, DirectRecipientResolver>();
        services.AddScoped<INotificationChannelResolver, PreferenceNotificationChannelResolver>();
        services.AddScoped<INotificationAuthorizationService, NotificationAuthorizationService>();

        services.AddScoped<IDeliveryProvider, EmailDeliveryProvider>();
        services.AddScoped<IDeliveryProvider, SmsDeliveryProvider>();
        services.AddScoped<IDeliveryProvider, PushDeliveryProvider>();
        services.AddScoped<IDeliveryProvider, WhatsAppDeliveryProvider>();

        services.AddScoped<INotificationGenerationEngine, NotificationGenerationEngine>();
        services.AddScoped<INotificationDeliveryProcessor, NotificationDeliveryProcessor>();
        services.AddScoped<INotificationApplicationService, NotificationApplicationService>();

        AddNotificationsCommandHandler<GenerateNotificationCommand, Masterdom.Modules.Notifications.Application.Support.ExecutionResult<GeneratedNotification>, GenerateNotificationCommandHandler>(services);
        AddNotificationsQueryHandler<GetNotificationHistoryQuery, Masterdom.Modules.Notifications.Application.Support.ExecutionResult<IReadOnlyCollection<NotificationHistoryEntry>>, GetNotificationHistoryQueryHandler>(services);
    }

    private static void AddDocumentsRuntime(IServiceCollection services)
    {
        services.AddSingleton<IDocumentReadModelRegistry, MetadataDrivenDocumentReadModelRegistry>();
        services.AddSingleton<IDocumentTemplateStore, PersistentDocumentTemplateStore>();
        services.AddSingleton<IDocumentHistoryStore, PersistentDocumentHistoryStore>();
        services.AddScoped<IDocumentPermissionService, DocumentPermissionService>();
        services.AddScoped<IDocumentRenderer, TextDocumentRenderer>();
        services.AddScoped<IDocumentPlatformOrchestrator, DocumentPlatformOrchestrator>();
        services.AddScoped<IDocumentApplicationService, DocumentApplicationService>();

        AddDocumentsCommandHandler<GenerateDocumentCommand, Masterdom.Modules.Documents.Application.Support.ExecutionResult<GeneratedDocument>, GenerateDocumentCommandHandler>(services);
        AddDocumentsCommandHandler<RegenerateDocumentCommand, Masterdom.Modules.Documents.Application.Support.ExecutionResult<GeneratedDocument>, RegenerateDocumentCommandHandler>(services);
        AddDocumentsQueryHandler<PreviewDocumentQuery, Masterdom.Modules.Documents.Application.Support.ExecutionResult<GeneratedDocument>, PreviewDocumentQueryHandler>(services);
        AddDocumentsQueryHandler<DownloadDocumentQuery, Masterdom.Modules.Documents.Application.Support.ExecutionResult<GeneratedDocument>, DownloadDocumentQueryHandler>(services);
        AddDocumentsQueryHandler<GetDocumentHistoryQuery, Masterdom.Modules.Documents.Application.Support.ExecutionResult<IReadOnlyCollection<DocumentHistoryEntry>>, GetDocumentHistoryQueryHandler>(services);
    }

    private static void AddSettingsRuntime(IServiceCollection services)
    {
        services.AddScoped<SettingsCapabilityBehaviorService>();
    }

    private static void AddIntelligenceRuntime(IServiceCollection services)
    {
        services.AddScoped<IntelligenceCapabilityBehaviorService>();
    }

    private static void AddSubsidyOptimizationRuntime(IServiceCollection services)
    {
        services.AddScoped<IOptimizationRunRepository, OptimizationRunRepository>();
        services.AddScoped<Masterdom.Modules.SubsidyOptimization.Application.Support.ISubsidyOptimizationUnitOfWork, SubsidyOptimizationUnitOfWork>();
        services.AddScoped<Masterdom.Modules.SubsidyOptimization.Application.Support.ISubsidyOptimizationPlatformOrchestrator, SubsidyOptimizationPlatformOrchestrator>();
        services.AddScoped<ISubsidyOptimizationApplicationService, SubsidyOptimizationApplicationService>();

        AddSubsidyOptimizationQueryHandler<GetOptimizationRunByIdQuery, Masterdom.Modules.SubsidyOptimization.Application.Support.ExecutionResult<OptimizationRunAggregate>, GetOptimizationRunByIdQueryHandler>(services);
        AddSubsidyOptimizationQueryHandler<GetLatestOptimizationRunQuery, Masterdom.Modules.SubsidyOptimization.Application.Support.ExecutionResult<OptimizationRunAggregate>, GetLatestOptimizationRunQueryHandler>(services);

        AddSubsidyOptimizationCommandHandler<ExecuteSubsidyOptimizationCommand, Masterdom.Modules.SubsidyOptimization.Application.Support.ExecutionResult<OptimizationRunAggregate>, ExecuteSubsidyOptimizationCommandHandler>(services);
        AddSubsidyOptimizationCommandHandler<CreateScenarioVersionCommand, Masterdom.Modules.SubsidyOptimization.Application.Support.ExecutionResult<OptimizationRunAggregate>, CreateScenarioVersionCommandHandler>(services);
        AddSubsidyOptimizationCommandHandler<ArchiveRecommendationCommand, Masterdom.Modules.SubsidyOptimization.Application.Support.ExecutionResult<OptimizationRunAggregate>, ArchiveRecommendationCommandHandler>(services);
        AddSubsidyOptimizationCommandHandler<ArchiveOptimizationRunCommand, Masterdom.Modules.SubsidyOptimization.Application.Support.ExecutionResult<OptimizationRunAggregate>, ArchiveOptimizationRunCommandHandler>(services);

        services.AddCalculationEngine();
        services.AddScoped<ConsumptionEstimator>();
        services.AddScoped<ForecastEngine>();
        services.AddScoped<ScenarioGenerator>();
        services.AddScoped<ScenarioEvaluator>();
        services.AddScoped<ConfidenceScorer>();
        services.AddScoped<SubsidyCalculationRuntimeInvoker>();
        services.AddScoped<RecommendationExplanationBuilder>();
        services.AddScoped<RecommendationEvidenceBuilder>();
        services.AddScoped<RecommendationGenerator>();
        services.AddScoped<OptimizationSessionBuilder>();
        services.AddScoped<ISubsidyMaximizerService, SubsidyMaximizerService>();
    }

    private static void AddPropertyCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new PropertyCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddPropertyQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new PropertyQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddPeopleCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.People.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.People.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new PeopleCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddPeopleQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.People.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.People.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new PeopleQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddCrmCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.CRM.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.CRM.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new CrmCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddCrmQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.CRM.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.CRM.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new CrmQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddLeaseCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Lease.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Lease.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new LeaseCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddLeaseQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Lease.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Lease.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new LeaseQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddTenancyCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Tenancy.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Tenancy.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new TenancyCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddTenancyQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Tenancy.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Tenancy.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new TenancyQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddMeteringCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Metering.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Metering.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new MeteringCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddMeteringQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Metering.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Metering.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new MeteringQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddMaintenanceCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Maintenance.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Maintenance.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new MaintenanceCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddMaintenanceQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Maintenance.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Maintenance.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new MaintenanceQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddInventoryCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Inventory.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Inventory.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new InventoryCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddBillingCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Billing.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Billing.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new BillingCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddBillingQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Billing.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Billing.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new BillingQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddUtilityRatingQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.UtilityRating.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.UtilityRating.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            serviceProvider.GetRequiredService<THandler>());
    }

    private static void AddUtilityRatingCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.UtilityRating.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.UtilityRating.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            serviceProvider.GetRequiredService<THandler>());
    }

    private static void AddSubsidyOptimizationQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.SubsidyOptimization.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.SubsidyOptimization.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new SubsidyOptimizationQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddSubsidyOptimizationCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.SubsidyOptimization.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.SubsidyOptimization.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new SubsidyOptimizationCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddFinancialLedgerCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.FinancialLedger.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.FinancialLedger.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new FinancialLedgerCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddFinancialLedgerQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.FinancialLedger.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.FinancialLedger.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new FinancialLedgerQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddPaymentCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Payment.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Payment.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new PaymentCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddPaymentQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Payment.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Payment.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new PaymentQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddReportingQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Reporting.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Reporting.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new ReportingQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddNotificationsCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Notifications.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Notifications.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new NotificationsCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddNotificationsQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Notifications.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Notifications.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new NotificationsQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddDocumentsCommandHandler<TCommand, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Documents.Application.Support.ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Documents.Application.Support.ICommandHandler<TCommand, TResult>>(serviceProvider =>
            new DocumentsCommandAuthorizationDecorator<TCommand, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static void AddDocumentsQueryHandler<TQuery, TResult, THandler>(IServiceCollection services)
        where THandler : class, Masterdom.Modules.Documents.Application.Support.IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<Masterdom.Modules.Documents.Application.Support.IQueryHandler<TQuery, TResult>>(serviceProvider =>
            new DocumentsQueryAuthorizationDecorator<TQuery, TResult>(
                serviceProvider.GetRequiredService<THandler>(),
                serviceProvider.GetRequiredService<IRequestAuthorizationService>()));
    }

    private static RecommendationConsumerExecutionContext CreateDefaultRecommendationConsumerExecutionContext()
    {
        var effectiveDateUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 4), DateTimeKind.Utc);
        var createdAtUtc = DateTime.UtcNow;

        var recommendation = Recommendation.CreateDraft(
            RecommendationId.New(),
            RecommendationType.Create("generic"),
            RecommendationPriority.Create(10),
            RecommendationConfidence.Create(0.5m),
            new RecommendationEvidence("placeholder", "Default execution context"),
            new RecommendationExplanation("Placeholder context"),
            new RecommendationMetadata(
                createdAtUtc,
                effectiveDateUtc,
                version: "v1",
                source: "platform"));

        var bundle = RecommendationBundle
            .CreateDraft(
                RecommendationBundleId.New(),
                [recommendation],
                createdAtUtc,
                effectiveDateUtc,
                version: "v1")
            .Open()
            .FinalizeBundle();

        var businessContext = new BusinessContext(
            BusinessContextVersion.BaselineV1,
            new BusinessContextMetadata(
                createdAtUtc,
                effectiveDateUtc,
                configurationVersion: "cfg-v1",
                language: "en-US",
                securityContext: "system",
                userId: "system",
                portfolioId: "default",
                providerExecutionOrder: Array.Empty<string>(),
                warnings: Array.Empty<string>()),
            snapshots: new Dictionary<string, BusinessContextSnapshot>(),
            references: Array.Empty<BusinessContextReference>());

        var session = OptimizationSession
            .Create(
                OptimizationSessionId.New(),
                new OptimizationSessionMetadata(
                    createdAtUtc,
                    effectiveDateUtc,
                    contextVersion: businessContext.Version.ToString(),
                    recommendationVersion: "v1"))
            .Start(createdAtUtc);

        return new RecommendationConsumerExecutionContext(
            recommendation,
            bundle,
            session,
            businessContext,
            correlationId: Guid.CreateVersion7(),
            executionTimestampUtc: createdAtUtc,
            effectiveDateUtc: effectiveDateUtc,
            configurationVersion: businessContext.Metadata.ConfigurationVersion);
    }
}
