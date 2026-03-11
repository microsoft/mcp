// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Result of listing all available function templates for a language,
/// grouped by binding type (triggers, input bindings, output bindings).
/// </summary>
public sealed class TemplateListResult
{
    [JsonPropertyName("language")]
    public required string Language { get; init; }

    [JsonPropertyName("triggers")]
    public IReadOnlyList<TemplateSummary> Triggers { get; init; } = [];

    [JsonPropertyName("inputBindings")]
    public IReadOnlyList<TemplateSummary> InputBindings { get; init; } = [];

    [JsonPropertyName("outputBindings")]
    public IReadOnlyList<TemplateSummary> OutputBindings { get; init; } = [];
}
