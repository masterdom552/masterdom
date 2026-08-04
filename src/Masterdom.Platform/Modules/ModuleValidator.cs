using System;

namespace Masterdom.Platform.Modules;

/// <summary>
/// Validates module metadata before module registration.
/// </summary>
public static class ModuleValidator
{
    /// <summary>
    /// Validates the specified module.
    /// </summary>
    public static void Validate(IModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        ValidateRequired("Id", module.Metadata.Id);
        ValidateRequired("Name", module.Metadata.Name);
        ValidateRequired("DisplayName", module.Metadata.DisplayName);
        ValidateRequired("Version", module.Metadata.Version);
        ValidateRequired("Description", module.Metadata.Description);
    }

    private static void ValidateRequired(string fieldName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ModuleValidationException(
                $"Module metadata field '{fieldName}' is required.");
        }
    }
}
