using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Metadata;

internal sealed class CalculationOperationDependencyGraph
{
    private readonly ImmutableDictionary<string, ICalculationOperationDescriptor> _byCapabilityId;
    private readonly ImmutableDictionary<string, ImmutableArray<string>> _dependenciesByCapabilityId;
    private readonly ImmutableDictionary<string, ImmutableArray<string>> _dependentsByCapabilityId;

    private CalculationOperationDependencyGraph(
        ImmutableDictionary<string, ICalculationOperationDescriptor> byCapabilityId,
        ImmutableDictionary<string, ImmutableArray<string>> dependenciesByCapabilityId,
        ImmutableDictionary<string, ImmutableArray<string>> dependentsByCapabilityId)
    {
        _byCapabilityId = byCapabilityId;
        _dependenciesByCapabilityId = dependenciesByCapabilityId;
        _dependentsByCapabilityId = dependentsByCapabilityId;
    }

    internal static CalculationOperationDependencyGraph Build(IReadOnlyCollection<ICalculationOperationDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var byCapabilityId = descriptors
            .ToImmutableDictionary(
                descriptor => descriptor.CapabilityId.Value,
                descriptor => descriptor,
                StringComparer.OrdinalIgnoreCase);

        var dependenciesByCapabilityId = descriptors
            .ToImmutableDictionary(
                descriptor => descriptor.CapabilityId.Value,
                descriptor => descriptor.DependencyCapabilityIds
                    .Select(dependency => dependency.Value)
                    .ToImmutableArray(),
                StringComparer.OrdinalIgnoreCase);

        var dependentsBuilder = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var descriptor in descriptors)
        {
            if (!dependentsBuilder.ContainsKey(descriptor.CapabilityId.Value))
            {
                dependentsBuilder[descriptor.CapabilityId.Value] = [];
            }
        }

        foreach (var descriptor in descriptors)
        {
            foreach (var dependency in descriptor.DependencyCapabilityIds)
            {
                if (!dependentsBuilder.TryGetValue(dependency.Value, out var dependents))
                {
                    dependents = [];
                    dependentsBuilder[dependency.Value] = dependents;
                }

                dependents.Add(descriptor.CapabilityId.Value);
            }
        }

        var dependentsByCapabilityId = dependentsBuilder
            .ToImmutableDictionary(
                item => item.Key,
                item => item.Value
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToImmutableArray(),
                StringComparer.OrdinalIgnoreCase);

        return new CalculationOperationDependencyGraph(
            byCapabilityId,
            dependenciesByCapabilityId,
            dependentsByCapabilityId);
    }

    internal IReadOnlyCollection<ICalculationOperationDescriptor> Descriptors => _byCapabilityId.Values.ToArray();

    internal IReadOnlyCollection<string> CapabilityIds => _byCapabilityId.Keys.ToArray();

    internal IReadOnlyDictionary<string, ImmutableArray<string>> ValidationGraph => _dependenciesByCapabilityId;

    internal bool TryResolveDescriptor(string capabilityId, out ICalculationOperationDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            throw new CalculationOperationValidationException("CapabilityId is required.");
        }

        return _byCapabilityId.TryGetValue(capabilityId.Trim(), out descriptor!);
    }

    internal ImmutableArray<string> GetDependencies(string capabilityId)
    {
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            throw new CalculationOperationValidationException("CapabilityId is required.");
        }

        if (_dependenciesByCapabilityId.TryGetValue(capabilityId.Trim(), out var dependencies))
        {
            return dependencies;
        }

        throw new CalculationOperationValidationException($"Capability '{capabilityId}' was not found in the dependency graph.");
    }

    internal ImmutableArray<string> GetDependents(string capabilityId)
    {
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            throw new CalculationOperationValidationException("CapabilityId is required.");
        }

        if (_dependentsByCapabilityId.TryGetValue(capabilityId.Trim(), out var dependents))
        {
            return dependents;
        }

        throw new CalculationOperationValidationException($"Capability '{capabilityId}' was not found in the dependency graph.");
    }

    internal ImmutableArray<string> GetTopologicalOrdering()
    {
        var inDegree = _dependenciesByCapabilityId
            .ToDictionary(
                item => item.Key,
                item => item.Value.Count(dependency => _byCapabilityId.ContainsKey(dependency)),
                StringComparer.OrdinalIgnoreCase);

        var queue = new PriorityQueue<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in inDegree.Where(item => item.Value == 0))
        {
            queue.Enqueue(item.Key, item.Key);
        }

        var result = ImmutableArray.CreateBuilder<string>(_dependenciesByCapabilityId.Count);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            if (!_dependentsByCapabilityId.TryGetValue(current, out var dependents))
            {
                continue;
            }

            foreach (var dependent in dependents)
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                {
                    queue.Enqueue(dependent, dependent);
                }
            }
        }

        if (result.Count != _dependenciesByCapabilityId.Count)
        {
            throw new CalculationOperationValidationException("Dependency graph contains a cycle.");
        }

        return result.ToImmutable();
    }
}
