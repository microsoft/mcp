// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Result of fetching a specific function template, including all source files
/// separated into function-specific files and project-level files with merge instructions.
/// </summary>
public sealed class FunctionTemplateResult
{
    [JsonPropertyName("language")]
    public required string Language { get; init; }

    [JsonPropertyName("templateName")]
    public required string TemplateName { get; init; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("bindingType")]
    public string? BindingType { get; init; }

    [JsonPropertyName("resource")]
    public string? Resource { get; init; }

    [JsonPropertyName("functionFiles")]
    public IReadOnlyList<ProjectTemplateFile> FunctionFiles { get; init; } = [];

    [JsonPropertyName("projectFiles")]
    public IReadOnlyList<ProjectTemplateFile> ProjectFiles { get; init; } = [];

    [JsonPropertyName("mergeInstructions")]
    public string MergeInstructions { get; init; } = string.Empty;
}
