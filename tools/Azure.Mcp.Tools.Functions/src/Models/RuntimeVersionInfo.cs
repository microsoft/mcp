// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the supported runtime versions for a language including
/// supported, preview, deprecated, and default versions.
/// </summary>
public sealed class RuntimeVersionInfo
{
    [JsonPropertyName("supported")]
    public required IReadOnlyList<string> Supported { get; init; }

    [JsonPropertyName("preview")]
    public IReadOnlyList<string>? Preview { get; init; }

    [JsonPropertyName("deprecated")]
    public IReadOnlyList<string>? Deprecated { get; init; }

    [JsonPropertyName("default")]
    public required string Default { get; init; }

    [JsonPropertyName("frameworkSupported")]
    public IReadOnlyList<string>? FrameworkSupported { get; init; }
}
