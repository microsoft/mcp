// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

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

    /// <summary>
    /// Project-level files that initialize a new Azure Functions project for this language.
    /// </summary>
    [JsonPropertyName("projectFiles")]
    public required IReadOnlyList<string> ProjectFiles { get; init; }

    /// <summary>
    /// Supported runtime versions for this language.
    /// </summary>
    [JsonPropertyName("runtimeVersions")]
    public required RuntimeVersionInfo RuntimeVersions { get; init; }

    /// <summary>
    /// Step-by-step setup instructions for this language.
    /// </summary>
    [JsonPropertyName("initInstructions")]
    public required string InitInstructions { get; init; }

    /// <summary>
    /// Description of the project directory structure for this language.
    /// </summary>
    [JsonPropertyName("projectStructure")]
    public required IReadOnlyList<string> ProjectStructure { get; init; }

    /// <summary>
    /// Template parameters for placeholder replacement (e.g., {{javaVersion}}).
    /// Null if this language has no configurable placeholders.
    /// </summary>
    [JsonPropertyName("templateParameters")]
    public IReadOnlyList<TemplateParameter>? TemplateParameters { get; init; }

    /// <summary>
    /// Notes about why this language is recommended for its runtime.
    /// For example, "Recommended for Node.js runtime for type safety and better tooling."
    /// Null if not a recommended choice.
    /// </summary>
    [JsonPropertyName("recommendationNotes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecommendationNotes { get; init; }
}
