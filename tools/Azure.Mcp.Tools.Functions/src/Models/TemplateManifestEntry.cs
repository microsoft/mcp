// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents a single template entry from the CDN manifest.
/// Contains metadata, the GitHub repository URL, and the folder path to the template files.
/// </summary>
public sealed class TemplateManifestEntry
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; init; }

    [JsonPropertyName("longDescription")]
    public string? LongDescription { get; init; }

    [JsonPropertyName("language")]
    public required string Language { get; init; }

    [JsonPropertyName("bindingType")]
    public string? BindingType { get; init; }

    [JsonPropertyName("resource")]
    public string? Resource { get; init; }

    [JsonPropertyName("iac")]
    public string? Iac { get; init; }

    [JsonPropertyName("priority")]
    public int Priority { get; init; }

    [JsonPropertyName("categories")]
    public IReadOnlyList<string> Categories { get; init; } = [];

    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = [];

    [JsonPropertyName("author")]
    public string? Author { get; init; }

    [JsonPropertyName("repositoryUrl")]
    public required string RepositoryUrl { get; init; }

    [JsonPropertyName("folderPath")]
    public required string FolderPath { get; init; }

    [JsonPropertyName("whatsIncluded")]
    public IReadOnlyList<string> WhatsIncluded { get; init; } = [];
}
