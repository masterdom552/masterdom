using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Core;

internal sealed class ValidationPipeline
{
    private readonly List<ValidationStep> _steps = new();

    public ValidationPipeline Add(string name, Action validate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Validation step name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(validate);

        _steps.Add(new ValidationStep(name.Trim(), validate));

        return this;
    }

    public void Execute()
    {
        foreach (var step in _steps)
        {
            step.Validate();
        }
    }

    private sealed record ValidationStep(string Name, Action Validate);
}
