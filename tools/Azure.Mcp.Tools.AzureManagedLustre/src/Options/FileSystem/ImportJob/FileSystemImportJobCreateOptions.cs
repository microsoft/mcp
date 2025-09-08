// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;

public sealed class FileSystemImportJobCreateOptions : BaseAzureManagedLustreOptions
{
    [JsonPropertyName(AzureManagedLustreOptionDefinitions.fileSystem)]
    public string? FileSystem { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.importPrefixes)]
    public IList<string>? ImportPrefixes { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.conflictResolutionMode)]
    public string? ConflictResolutionMode { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.maximumErrors)]
    public int? MaximumErrors { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.adminStatus)]
    public string? AdminStatus { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.name)]
    public string? Name { get; set; }
}
