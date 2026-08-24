using Masterdom.Host.Api;
using Masterdom.Host.Bootstrap;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityModule(builder.Configuration);
builder.Services.AddPropertyBusinessCapabilityRuntime();
builder.Services.AddPolicyFrameworkRuntime();
builder.Services.AddScoped<BootstrapProvisioningService>();

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

if (args.Contains("--bootstrap"))
{
    using var bootstrapScope = app.Services.CreateScope();
    var bootstrapService = bootstrapScope.ServiceProvider.GetRequiredService<BootstrapProvisioningService>();

    var request = new BootstrapRequest(
        Username: app.Configuration["Bootstrap:Username"]
            ?? Environment.GetEnvironmentVariable("MASTERDOM_BOOTSTRAP_USERNAME")
            ?? string.Empty,
        Password: app.Configuration["Bootstrap:Password"]
            ?? Environment.GetEnvironmentVariable("MASTERDOM_BOOTSTRAP_PASSWORD")
            ?? string.Empty,
        FirstName: app.Configuration["Bootstrap:FirstName"]
            ?? Environment.GetEnvironmentVariable("MASTERDOM_BOOTSTRAP_FIRST_NAME")
            ?? "System",
        LastName: app.Configuration["Bootstrap:LastName"]
            ?? Environment.GetEnvironmentVariable("MASTERDOM_BOOTSTRAP_LAST_NAME")
            ?? "Administrator");

    app.Logger.LogInformation("Running initial bootstrap provisioning.");
    var result = await bootstrapService.RunAsync(request);

    if (result.Success)
    {
        app.Logger.LogInformation("Bootstrap completed successfully.");
        Environment.Exit(0);
    }

    app.Logger.LogError("Bootstrap failed: {Message}", result.Message);
    Environment.Exit(1);
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
