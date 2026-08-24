using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityModule(builder.Configuration);
builder.Services.AddPropertyBusinessCapabilityRuntime();
builder.Services.AddPolicyFrameworkRuntime();

var connectionString =
    builder.Configuration.GetConnectionString("Masterdom")
    ?? Environment.GetEnvironmentVariable("MASTERDOM_CONNECTION_STRING")
    ?? throw new InvalidOperationException(
        "Connection string 'Masterdom' was not found and MASTERDOM_CONNECTION_STRING is not set.");

builder.Services.AddDbContext<MasterdomDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

if (args.Contains("--migrate"))
{
    using var migrationScope = app.Services.CreateScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

    app.Logger.LogInformation("Applying pending EF Core migrations to the 'Masterdom' database.");
    await dbContext.Database.MigrateAsync();
    app.Logger.LogInformation("Migrations applied successfully.");
    return;
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthenticationEndpoints();
app.MapPropertyEndpoints();
app.MapPolicyFrameworkEndpoints();
app.MapCrmEndpoints();
app.MapPeopleEndpoints();
app.MapLeaseEndpoints();
app.MapTenancyEndpoints();
app.MapMeteringEndpoints();
app.MapMaintenanceEndpoints();
app.MapInventoryEndpoints();
app.MapBillingEndpoints();
app.MapUtilityRatingEndpoints();
app.MapFinancialLedgerEndpoints();
app.MapPaymentEndpoints();
app.MapSubsidyOptimizationEndpoints();
app.MapIdentityAdministrationEndpoints();
app.MapDelegationEndpoints();
app.MapReportingEndpoints();
app.MapIntelligenceEndpoints();
app.MapNotificationEndpoints();
app.MapDocumentEndpoints();

app.Run();

/// <summary>
/// Application entry-point marker for integration tests.
/// </summary>
public partial class Program
{
}
