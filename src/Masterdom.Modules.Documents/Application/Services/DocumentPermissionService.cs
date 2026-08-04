using Masterdom.Core.Security;

namespace Masterdom.Modules.Documents.Application.Services;

public sealed class DocumentPermissionService : IDocumentPermissionService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public DocumentPermissionService(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
    }

    public void EnsureCanGenerate(string documentType)
    {
        _ = documentType;

        var user = _currentUserAccessor.GetCurrentUser();
        if (!user.IsAuthenticated)
        {
            throw new InvalidOperationException("Document generation requires authentication.");
        }

        if (user.IsInRole(MasterdomRoles.SuperUser) || user.HasPermission("documents.generate"))
        {
            return;
        }

        throw new InvalidOperationException("The current user does not have document generation permission.");
    }

    public void EnsureCanDownload(string documentId)
    {
        _ = documentId;

        var user = _currentUserAccessor.GetCurrentUser();
        if (!user.IsAuthenticated)
        {
            throw new InvalidOperationException("Document download requires authentication.");
        }

        if (user.IsInRole(MasterdomRoles.SuperUser) || user.HasPermission("documents.read"))
        {
            return;
        }

        throw new InvalidOperationException("The current user does not have document download permission.");
    }
}
