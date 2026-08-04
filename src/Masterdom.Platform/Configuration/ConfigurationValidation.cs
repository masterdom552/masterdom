using System;
using System.Collections.Generic;
using System.Linq;

namespace Masterdom.Platform.Configuration;

internal static class ConfigurationValidation
{
    public static void ValidateForStorage(IReadOnlyList<ConfigurationRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        ValidateDuplicateIdentifiers(records);
        ValidateOverlappingPeriods(records);
    }

    public static void ValidateNoActiveOverlaps(
        IReadOnlyList<ConfigurationRecord> effectiveRecords,
        DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(effectiveRecords);

        var overlappingGroups = effectiveRecords
            .GroupBy(record => record.Scope)
            .Where(group => group.Count() > 1)
            .ToList();

        if (overlappingGroups.Count == 0)
        {
            return;
        }

        var scope = overlappingGroups[0].Key;
        throw new PlatformConfigurationValidationException(
            $"Multiple active configuration versions were found for scope '{scope.Kind}' at '{asOfUtc:O}'.");
    }

    private static void ValidateDuplicateIdentifiers(IReadOnlyList<ConfigurationRecord> records)
    {
        var duplicateIds = records
            .GroupBy(record => record.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateIds.Count == 0)
        {
            return;
        }

        throw new PlatformConfigurationValidationException(
            $"Duplicate configuration identifiers were found: {string.Join(", ", duplicateIds)}.");
    }

    private static void ValidateOverlappingPeriods(IReadOnlyList<ConfigurationRecord> records)
    {
        var groups = records
            .GroupBy(record => new { record.Key, record.Scope })
            .ToList();

        foreach (var group in groups)
        {
            var sorted = group
                .OrderBy(record => record.Period.EffectiveFromUtc)
                .ToList();

            for (int i = 0; i < sorted.Count - 1; i++)
            {
                var current = sorted[i];
                var next = sorted[i + 1];
                var endsAfterNextStarts =
                    !current.Period.EffectiveToUtc.HasValue ||
                    current.Period.EffectiveToUtc.Value > next.Period.EffectiveFromUtc;

                if (!endsAfterNextStarts)
                {
                    continue;
                }

                throw new PlatformConfigurationValidationException(
                    $"Overlapping configuration periods were found for key '{group.Key.Key.Value}' and scope '{group.Key.Scope.Kind}'.");
            }
        }
    }
}
