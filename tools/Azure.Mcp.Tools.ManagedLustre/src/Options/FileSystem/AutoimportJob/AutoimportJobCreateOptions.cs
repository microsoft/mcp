// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;

public sealed class AutoimportJobCreateOptions : BaseManagedLustreOptions
{
    [JsonPropertyName("filesystem-name")]
    public string? FileSystemName { get; set; }

    [JsonPropertyName("job-name")]
    public string? JobName { get; set; }

    [JsonPropertyName("conflict-resolution-mode")]
    public string? ConflictResolutionMode { get; set; }

    [JsonPropertyName("autoimport-prefixes")]
    public List<string>? AutoimportPrefixes { get; set; }

    [JsonPropertyName("admin-status")]
    public string? AdminStatus { get; set; }

    [JsonPropertyName("enable-deletions")]
    public bool? EnableDeletions { get; set; }

    [JsonPropertyName("maximum-errors")]
    public long? MaximumErrors { get; set; }
}
