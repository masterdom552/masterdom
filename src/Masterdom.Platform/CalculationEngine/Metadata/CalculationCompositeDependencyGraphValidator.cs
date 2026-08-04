namespace Masterdom.Platform.CalculationEngine.Metadata;

internal sealed class CalculationCompositeDependencyGraphValidator
{
    internal void Validate(
        IReadOnlyCollection<ICalculationOperationDescriptor> descriptors,
        bool allowCompositeDependencies = false)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var graph = CalculationOperationDependencyGraph.Build(descriptors);
        _ = graph.GetTopologicalOrdering();

        var composites = descriptors
            .Where(descriptor => descriptor.CompositionLevel == CalculationOperationCompositionLevel.Composite)
            .ToArray();

        var compositeDependencyEdges = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var composite in composites)
        {
            var duplicateDependencies = composite.DependencyCapabilityIds
                .GroupBy(dependency => dependency.Value, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            if (duplicateDependencies.Length > 0)
            {
                throw new CalculationOperationValidationException(
                    $"Composite '{composite.OperationName}' contains duplicate dependency capability ids: {string.Join(", ", duplicateDependencies)}.");
            }

            foreach (var dependency in composite.DependencyCapabilityIds)
            {
                if (string.Equals(dependency.Value, composite.CapabilityId.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new CalculationOperationValidationException(
                        $"Composite '{composite.OperationName}' cannot depend on itself.");
                }

                if (!graph.TryResolveDescriptor(dependency.Value, out var dependencyDescriptor))
                {
                    throw new CalculationOperationValidationException(
                        $"Composite '{composite.OperationName}' references missing dependency '{dependency.Value}'.");
                }

                if (dependencyDescriptor.CompositionLevel == CalculationOperationCompositionLevel.Composite)
                {
                    if (!allowCompositeDependencies)
                    {
                        throw new CalculationOperationValidationException(
                            $"Composite '{composite.OperationName}' cannot depend on Level 2 composite '{dependency.Value}'.");
                    }

                    AddEdge(compositeDependencyEdges, composite.CapabilityId.Value, dependency.Value);
                    continue;
                }

                if (dependencyDescriptor.CompositionLevel != CalculationOperationCompositionLevel.Primitive)
                {
                    throw new CalculationOperationValidationException(
                        $"Composite '{composite.OperationName}' dependency '{dependency.Value}' must belong to Level 1 primitives.");
                }
            }
        }

        ValidateCompositeDependencyCycles(compositeDependencyEdges);
    }

    private static void AddEdge(IDictionary<string, List<string>> graph, string from, string to)
    {
        if (!graph.TryGetValue(from, out var edges))
        {
            edges = [];
            graph[from] = edges;
        }

        edges.Add(to);
    }

    private static void ValidateCompositeDependencyCycles(IReadOnlyDictionary<string, List<string>> graph)
    {
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Keys)
        {
            DetectCycle(node, graph, visiting, visited);
        }
    }

    private static void DetectCycle(
        string node,
        IReadOnlyDictionary<string, List<string>> graph,
        ISet<string> visiting,
        ISet<string> visited)
    {
        if (visited.Contains(node))
        {
            return;
        }

        if (!visiting.Add(node))
        {
            throw new CalculationOperationValidationException(
                $"Composite dependency graph contains a cycle at '{node}'.");
        }

        if (graph.TryGetValue(node, out var edges))
        {
            foreach (var next in edges)
            {
                DetectCycle(next, graph, visiting, visited);
            }
        }

        visiting.Remove(node);
        visited.Add(node);
    }
}
