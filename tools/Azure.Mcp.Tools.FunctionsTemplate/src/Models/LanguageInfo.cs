// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FunctionsTemplate.Models;

/// <summary>
/// Represents detailed information about a supported Azure Functions language.
/// </summary>
public sealed class LanguageInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("runtime")]
    public required string Runtime { get; init; }

    [JsonPropertyName("programmingModel")]
    public required string ProgrammingModel { get; init; }

    [JsonPropertyName("prerequisites")]
    public required IReadOnlyList<string> Prerequisites { get; init; }

    [JsonPropertyName("developmentTools")]
    public required IReadOnlyList<string> DevelopmentTools { get; init; }

    [JsonPropertyName("initCommand")]
    public required string InitCommand { get; init; }

    [JsonPropertyName("runCommand")]
    public required string RunCommand { get; init; }

    [JsonPropertyName("buildCommand")]
    public string? BuildCommand { get; init; }
}
