using System.Text;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Security;
using Masterdom.Modules.Security.Application.Commands;
using Masterdom.Modules.Security.Application.Handlers.Commands;
using Masterdom.Modules.Security.Application.Handlers.Queries;
using Masterdom.Modules.Security.Application.Queries;
using Masterdom.Modules.Security.Application.Services;
using Masterdom.Modules.Security.Application.Support;
using Masterdom.Modules.Security.Domain.Repositories;
using Masterdom.Modules.Security.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Masterdom.Modules.Security;

public static class SecurityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var signingKey = configuration["Authentication:Bearer:SigningKey"]
            ?? Environment.GetEnvironmentVariable("MASTERDOM_AUTHENTICATION_SIGNING_KEY");
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "Authentication:Bearer:SigningKey must be configured for bearer authentication.");
        }

        var issuer = configuration["Authentication:Bearer:Issuer"]
            ?? Environment.GetEnvironmentVariable("MASTERDOM_AUTHENTICATION_ISSUER");
        var audience = configuration["Authentication:Bearer:Audience"]
            ?? Environment.GetEnvironmentVariable("MASTERDOM_AUTHENTICATION_AUDIENCE");

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorizationBuilder();
        services.AddSecurityInfrastructureRuntime();

        AddIdentityAdministrationRuntime(services);

        return services;
    }

    private static void AddIdentityAdministrationRuntime(IServiceCollection services)
    {
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IIdentityAdministrationUnitOfWork, IdentityAdministrationUnitOfWork>();
        services.AddScoped<IIdentityAdministrationService, IdentityAdministrationService>();
        services.AddScoped<ICommandHandler<CreateRoleCommand, ExecutionResult<Masterdom.Core.Identity.Entities.Role.Role>>, CreateRoleCommandHandler>();
        services.AddScoped<IQueryHandler<GetRoleByCodeQuery, ExecutionResult<Masterdom.Core.Identity.Entities.Role.Role>>, GetRoleByCodeQueryHandler>();
    }
}
