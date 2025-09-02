// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class FileSystemImportJobCreateOptions : BaseManagedLustreOptions
{
    [JsonPropertyName(ManagedLustreOptionDefinitions.fileSystem)]
    public string? FileSystem { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.importPrefixes)]
    public IList<string>? ImportPrefixes { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.conflictResolutionMode)]
    public string? ConflictResolutionMode { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.maximumErrors)]
    public int? MaximumErrors { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.adminStatus)]
    public string? AdminStatus { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.jobName)]
    public string? Name { get; set; }
}
