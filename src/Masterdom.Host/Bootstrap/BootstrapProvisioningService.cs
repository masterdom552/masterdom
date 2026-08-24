using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.UserRole;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Domain.Repositories;
using Masterdom.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Host.Bootstrap;

/// <summary>
/// Input for the one-time initial bootstrap provisioning operation.
/// </summary>
public sealed record BootstrapRequest(
    string Username,
    string Password,
    string FirstName,
    string LastName);

/// <summary>
/// Result of a bootstrap provisioning attempt. Never carries the plaintext
/// password or its hash.
/// </summary>
public sealed record BootstrapResult(bool Success, string Message, Guid? UserId = null);

/// <summary>
/// Provisions the initial trusted administrative identity on a fresh
/// deployment: a <see cref="Person"/>, <see cref="IdentityProfile"/>,
/// <see cref="User"/>, <see cref="Credential"/>, a
/// <see cref="RoleAuthorityLevel.PrimarySuperUser"/>-level <see cref="Role"/>,
/// and the primary <see cref="UserRole"/> assignment linking them.
///
/// This is a narrow, one-time bootstrap mechanism -- not a general identity-
/// administration surface. It never mints a JWT; the provisioned identity
/// authenticates afterward through the existing, unmodified CAP-023 login
/// flow. It never creates a Property or any data outside the identity graph
/// above.
/// </summary>
public sealed class BootstrapProvisioningService
{
    private readonly MasterdomDbContext _dbContext;
    private readonly IPersonRepository _personRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPasswordHasher _passwordHasher;

    public BootstrapProvisioningService(
        MasterdomDbContext dbContext,
        IPersonRepository personRepository,
        IRoleRepository roleRepository,
        ICredentialRepository credentialRepository,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<BootstrapResult> RunAsync(
        BootstrapRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new BootstrapResult(false, "Bootstrap username was not supplied.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return new BootstrapResult(false, "Bootstrap password was not supplied or is too short (minimum 8 characters).");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return new BootstrapResult(false, "Bootstrap first name and last name were not supplied.");
        }

        var alreadyBootstrapped = await IsAlreadyBootstrappedAsync(cancellationToken);
        if (alreadyBootstrapped)
        {
            return new BootstrapResult(
                false,
                "Bootstrap has already been performed; a PrimarySuperUser role already exists.");
        }

        try
        {
            var username = Username.Create(request.Username);

            var person = Person.Create(
                PersonNumber.Create($"BOOTSTRAP-{Guid.NewGuid():N}"),
                PersonName.Create(request.FirstName, request.LastName),
                Gender.PreferNotToSay);

            var identityProfile = IdentityProfile.Create(
                IdentityProfileCode.Create($"BOOTSTRAP-{Guid.NewGuid():N}"),
                IdentityProfileType.Person);
            identityProfile.LinkPerson(person.Id);

            var user = User.Create(
                UserCode.Create($"BOOTSTRAP-{Guid.NewGuid():N}"),
                identityProfile.Id,
                username);

            var passwordHash = _passwordHasher.Hash(request.Password);
            var credential = Credential.Create(user.Id, passwordHash);

            var role = Role.Create(
                RoleCode.Create("SUPERUSER"),
                RoleName.Create("Super User"),
                RoleAuthorityLevel.PrimarySuperUser);

            var userRole = UserRole.Create(
                user.Id,
                role.Id,
                assignedBy: null,
                isPrimaryRole: true,
                reason: "Initial bootstrap administrator");
            userRole.Activate();

            _personRepository.Add(person);
            _dbContext.IdentityProfiles.Add(identityProfile);
            _dbContext.Users.Add(user);
            _credentialRepository.Add(credential);
            _roleRepository.Add(role);
            _dbContext.UserRoles.Add(userRole);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BootstrapResult(true, "Bootstrap completed successfully.", user.Id.Value);
        }
        catch (Exception ex)
        {
            return new BootstrapResult(false, $"Bootstrap failed: {ex.Message}");
        }
    }

    private async Task<bool> IsAlreadyBootstrappedAsync(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return roles.Any(r => r.AuthorityLevel == RoleAuthorityLevel.PrimarySuperUser);
    }
}
