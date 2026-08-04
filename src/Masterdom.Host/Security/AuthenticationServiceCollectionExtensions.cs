using System.Text;
using Masterdom.Core.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Masterdom.Host.Security;

internal static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddMasterdomIdentityIntegration(
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

        return services;
    }
}
