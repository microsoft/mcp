// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Result of fetching a specific function template, including all source files
/// separated into function-specific files and project-level files with merge instructions.
/// </summary>
public sealed class FunctionTemplateResult
{
    public required string Language { get; init; }

    public required string TemplateName { get; init; }

    public string? DisplayName { get; init; }

    public string? Description { get; init; }

    public string? BindingType { get; init; }

    public string? Resource { get; init; }

    public IReadOnlyList<ProjectTemplateFile> FunctionFiles { get; init; } = [];

    public IReadOnlyList<ProjectTemplateFile> ProjectFiles { get; init; } = [];

    public string MergeInstructions { get; init; } = string.Empty;
}
