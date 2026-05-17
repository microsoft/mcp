// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents a single template entry from the CDN manifest.
/// Contains metadata, the GitHub repository URL, and the folder path to the template files.
/// </summary>
public sealed class TemplateManifestEntry
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public string? ShortDescription { get; init; }

    public string? LongDescription { get; init; }

    public required string Language { get; init; }

    public string? BindingType { get; init; }

    public string? Resource { get; init; }

    public string? Iac { get; init; }

    public int Priority { get; init; }

    public IReadOnlyList<string> Categories { get; init; } = [];

    public IReadOnlyList<string> Tags { get; init; } = [];

    public string? Author { get; init; }

    public required string RepositoryUrl { get; init; }

    public required string FolderPath { get; init; }

    public IReadOnlyList<string> WhatsIncluded { get; init; } = [];
}
