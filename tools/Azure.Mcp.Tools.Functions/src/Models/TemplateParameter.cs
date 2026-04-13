// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents a customizable parameter within a project template.
/// Templates may contain {{paramName}} placeholders that should be replaced
/// with actual values (e.g., runtime version) before use.
/// </summary>
public sealed class TemplateParameter
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string DefaultValue { get; init; }

    public IReadOnlyList<string>? ValidValues { get; init; }
}
