// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the complete language details returned by the get languages list command,
/// combining language info with runtime version details and global runtime metadata.
/// </summary>
public sealed class LanguageDetails
{
    [JsonPropertyName("language")]
    public required string Language { get; init; }

    [JsonPropertyName("info")]
    public required LanguageInfo Info { get; init; }

    [JsonPropertyName("runtimeVersions")]
    public required RuntimeVersionInfo RuntimeVersions { get; init; }
}
