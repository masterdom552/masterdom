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

app.UseAuthentication();
app.UseAuthorization();

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
app.MapNotificationEndpoints();
app.MapDocumentEndpoints();

app.Run();

/// <summary>
/// Application entry-point marker for integration tests.
/// </summary>
public partial class Program
{
}
