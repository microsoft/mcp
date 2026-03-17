// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents a single file in a project template, containing
/// the file path/name and its text content.
/// </summary>
public sealed class ProjectTemplateFile
{
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}
