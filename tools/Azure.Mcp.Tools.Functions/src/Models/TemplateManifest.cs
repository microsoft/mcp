// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the CDN manifest containing all available Azure Functions templates,
/// fetched from the Azure Functions CDN endpoint.
/// </summary>
public sealed class TemplateManifest
{
    [JsonPropertyName("generatedAt")]
    public string? GeneratedAt { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("totalTemplates")]
    public int TotalTemplates { get; init; }

    [JsonPropertyName("languages")]
    public IReadOnlyList<string> Languages { get; init; } = [];

    [JsonPropertyName("templates")]
    public IReadOnlyList<TemplateManifestEntry> Templates { get; init; } = [];
}
