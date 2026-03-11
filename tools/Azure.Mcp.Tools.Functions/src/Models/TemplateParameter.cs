// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents a customizable parameter within a project template.
/// Templates may contain {{paramName}} placeholders that should be replaced
/// with actual values (e.g., runtime version) before use.
/// </summary>
public sealed class TemplateParameter
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("defaultValue")]
    public required string DefaultValue { get; init; }

    [JsonPropertyName("validValues")]
    public IReadOnlyList<string>? ValidValues { get; init; }
}
