using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Masterdom.Core.Security;
using Masterdom.Modules.Authentication.Application.Models;
using Microsoft.IdentityModel.Tokens;

namespace Masterdom.Modules.Authentication.Application.Services;

/// <summary>
/// Issues bearer JWTs using the same signing-key configuration already
/// consumed by the platform's JWT bearer validation
/// (see <c>Authentication:Bearer:SigningKey</c>). No new secret mechanism
/// is introduced.
/// </summary>
public sealed class JwtTokenIssuer : IJwtTokenIssuer
{
    private readonly JwtTokenIssuerOptions _options;

    public JwtTokenIssuer(JwtTokenIssuerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public LoginResult Issue(
        Guid userId,
        string username,
        Guid? personId,
        IReadOnlyCollection<Guid> ownedPropertyIds,
        LoginAuthorityClaims authority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentNullException.ThrowIfNull(ownedPropertyIds);
        ArgumentNullException.ThrowIfNull(authority);

        var now = DateTime.UtcNow;
        var expiresAtUtc = now.Add(_options.AccessTokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
        };

        if (personId.HasValue)
        {
            claims.Add(new Claim(MasterdomClaimTypes.PersonId, personId.Value.ToString()));
        }

        // OwnedProperty reflects literal Property.OwnerId ownership only.
        foreach (var propertyId in ownedPropertyIds)
        {
            claims.Add(new Claim(MasterdomClaimTypes.OwnedProperty, propertyId.ToString()));
        }

        // PropertyScope reflects the server-resolved effective scope (owned + active delegations).
        foreach (var propertyId in authority.PropertyScopes)
        {
            claims.Add(new Claim(MasterdomClaimTypes.PropertyScope, propertyId.ToString()));
        }

        foreach (var roleCode in authority.RoleCodes)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleCode));
        }

        foreach (var permission in authority.Permissions)
        {
            claims.Add(new Claim(MasterdomClaimTypes.Permission, permission));
        }

        if (authority.AuthorityLevel.HasValue)
        {
            claims.Add(new Claim(MasterdomClaimTypes.AuthorityLevel, authority.AuthorityLevel.Value.ToString()));
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: string.IsNullOrWhiteSpace(_options.Issuer) ? null : _options.Issuer,
            audience: string.IsNullOrWhiteSpace(_options.Audience) ? null : _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResult(accessToken, expiresAtUtc);
    }
}
