namespace Masterdom.Modules.Authentication.Application.Services;

public sealed class JwtTokenIssuerOptions
{
    public required string SigningKey { get; init; }

    public string? Issuer { get; init; }

    public string? Audience { get; init; }

    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);
}
