// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models;

public record WordResult
{
    [JsonPropertyName("word")]
    public string? Word { get; set; }

    [JsonPropertyName("offset")]
    public ulong? Offset { get; set; }

    [JsonPropertyName("duration")]
    public ulong? Duration { get; set; }
}
