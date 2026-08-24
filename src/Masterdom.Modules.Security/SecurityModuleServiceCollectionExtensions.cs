using System.Text;
using Masterdom.Abstractions.Modules.Security;
using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence.Identity;
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
        AddDelegationManagementRuntime(services);

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

    private static void AddDelegationManagementRuntime(IServiceCollection services)
    {
        // Identity model repositories
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // Domain policies and services
        // IAuthorityLevelProvider is registered here, not by AddSecurityInfrastructureRuntime,
        // because its production implementation is Role-repository-backed (see ADR-0010) and
        // IRoleRepository is owned by this module, not by Masterdom.Infrastructure.
        services.AddScoped<IAuthorityLevelProvider, RoleAuthorityLevelProvider>();
        services.AddScoped<EffectiveAuthorityResolver>();
        services.AddScoped<DelegationValidator>();

        // Application authority provider (assembles facts from identity model)
        services.AddScoped<IDirectAuthorityProvider, DefaultDirectAuthorityProvider>();

        // Login-time authority resolution (CAP-023 Phase 2: server-derived JWT authority claims)
        services.AddScoped<ILoginAuthorityResolver, LoginAuthorityResolver>();

        // Repository and Unit of Work
        services.AddScoped<IDelegatedAuthorityRepository, DelegatedAuthorityRepository>();
        services.AddScoped<IIdentityAdministrationUnitOfWork, IdentityAdministrationUnitOfWork>();

        // Application service
        services.AddScoped<IDelegationApplicationService, DelegationApplicationService>();

        // Handlers
        services.AddScoped<ICommandHandler<CreateDelegationCommand, ExecutionResult<DelegatedAuthority>>, CreateDelegationCommandHandler>();
        services.AddScoped<ICommandHandler<RevokeDelegationCommand, ExecutionResult<DelegatedAuthority>>, RevokeDelegationCommandHandler>();
        services.AddScoped<IQueryHandler<GetDelegationByIdQuery, ExecutionResult<DelegatedAuthority>>, GetDelegationByIdQueryHandler>();
    }
}
