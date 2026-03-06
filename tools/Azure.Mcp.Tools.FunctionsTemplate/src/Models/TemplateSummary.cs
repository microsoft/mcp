// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FunctionsTemplate.Models;

/// <summary>
/// Summary of a single function template, used in list mode.
/// The <see cref="TemplateName"/> is the value to pass to --template in get mode.
/// </summary>
public sealed class TemplateSummary
{
    [JsonPropertyName("templateName")]
    public required string TemplateName { get; init; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }

    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; init; }

    [JsonPropertyName("resource")]
    public string? Resource { get; init; }
}
