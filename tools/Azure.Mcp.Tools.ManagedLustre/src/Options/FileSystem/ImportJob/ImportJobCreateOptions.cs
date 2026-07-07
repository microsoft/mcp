// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedLustre.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;

public class ImportJobCreateOptions : BaseManagedLustreOptions
{
    /// <summary>
    /// The name of the filesystem.
    /// </summary>
    [JsonPropertyName("filesystemName")]
    public string? FileSystemName { get; set; }

    /// <summary>
    /// The name of the import job.
    /// </summary>
    [JsonPropertyName("jobName")]
    public string? JobName { get; set; }

    /// <summary>
    /// How to handle conflicting files during import.
    /// </summary>
    [JsonPropertyName("conflictResolutionMode")]
    public string? ConflictResolutionMode { get; set; }

    /// <summary>
    /// Blob prefixes to import.
    /// </summary>
    [JsonPropertyName("importPrefixes")]
    public string[]? ImportPrefixes { get; set; }

    /// <summary>
    /// Maximum errors allowed before job failure.
    /// </summary>
    [JsonPropertyName("maximumErrors")]
    public long? MaximumErrors { get; set; }
}
