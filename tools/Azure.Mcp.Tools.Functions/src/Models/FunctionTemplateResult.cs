// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Result of fetching a specific function template.
/// By default (mode='new-project'), returns all files in a single 'files' list for creating complete projects.
/// When mode='add-function', separates files into 'functionFiles' and 'projectFiles' with merge instructions.
/// </summary>
public sealed class FunctionTemplateResult
{
    public required string Language { get; init; }

    public required string TemplateName { get; init; }

    public string? DisplayName { get; init; }

    public string? Description { get; init; }

    public string? BindingType { get; init; }

    public string? Resource { get; init; }

    /// <summary>
    /// All template files. Populated when mode is 'new-project' (default).
    /// </summary>
    public IReadOnlyList<ProjectTemplateFile>? Files { get; init; }

    /// <summary>
    /// Function-specific files (code, infra, docs). Populated when mode is 'add-function'.
    /// </summary>
    public IReadOnlyList<ProjectTemplateFile>? FunctionFiles { get; init; }

    /// <summary>
    /// Project configuration files (host.json, local.settings.json, etc.). Populated when mode is 'add-function'.
    /// </summary>
    public IReadOnlyList<ProjectTemplateFile>? ProjectFiles { get; init; }

    /// <summary>
    /// Instructions for merging project files with existing project. Populated when mode is 'add-function'.
    /// </summary>
    public string? MergeInstructions { get; init; }
}
