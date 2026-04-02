// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Result of fetching a specific function template.
/// By default (--output New), returns all files in 'files' list plus separated 'functionFiles', 'projectFiles', and 'mergeInstructions'.
/// When --output Add, returns only 'functionFiles' and 'projectFiles' with merge instructions (no 'files' list).
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
    /// All template files combined. Populated when --output is New (default).
    /// </summary>
    public IReadOnlyList<ProjectTemplateFile>? Files { get; init; }

    /// <summary>
    /// Function-specific files (code, infra, docs). Populated in both New and Add modes.
    /// </summary>
    public IReadOnlyList<ProjectTemplateFile>? FunctionFiles { get; init; }

    /// <summary>
    /// Project configuration files (host.json, local.settings.json, etc.). Populated in both New and Add modes.
    /// </summary>
    public IReadOnlyList<ProjectTemplateFile>? ProjectFiles { get; init; }

    /// <summary>
    /// Instructions for merging project files with existing project. Populated in both New and Add modes.
    /// </summary>
    public string? MergeInstructions { get; init; }
}
