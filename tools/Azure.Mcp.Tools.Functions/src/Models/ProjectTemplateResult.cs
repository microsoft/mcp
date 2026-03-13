// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the result of the get project template command,
/// containing setup instructions and project structure overview.
/// </summary>
public sealed class ProjectTemplateResult
{
    [JsonPropertyName("language")]
    public required string Language { get; init; }

    [JsonPropertyName("initInstructions")]
    public required string InitInstructions { get; init; }

    [JsonPropertyName("projectStructure")]
    public required IReadOnlyList<string> ProjectStructure { get; init; }
}
