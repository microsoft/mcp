// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public class ImportJob
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("provisioningState")]
    public string ProvisioningState { get; set; } = string.Empty;

    [JsonPropertyName("conflictResolutionMode")]
    public string? ConflictResolutionMode { get; set; }

    [JsonPropertyName("importPrefixes")]
    public string[]? ImportPrefixes { get; set; }

    [JsonPropertyName("maximumErrors")]
    public int? MaximumErrors { get; set; }

    [JsonPropertyName("totalBlobsImported")]
    public long? TotalBlobsImported { get; set; }

    [JsonPropertyName("totalErrors")]
    public long? TotalErrors { get; set; }

    [JsonPropertyName("totalConflicts")]
    public long? TotalConflicts { get; set; }

    [JsonPropertyName("totalBlobsWalked")]
    public long? TotalBlobsWalked { get; set; }

    [JsonPropertyName("blobsWalkedPerSecond")]
    public long? BlobsWalkedPerSecond { get; set; }

    [JsonPropertyName("importedFiles")]
    public long? ImportedFiles { get; set; }

    [JsonPropertyName("importedDirectories")]
    public long? ImportedDirectories { get; set; }

    [JsonPropertyName("importedSymlinks")]
    public long? ImportedSymlinks { get; set; }

    [JsonPropertyName("preexistingFiles")]
    public long? PreexistingFiles { get; set; }

    [JsonPropertyName("preexistingDirectories")]
    public long? PreexistingDirectories { get; set; }

    [JsonPropertyName("preexistingSymlinks")]
    public long? PreexistingSymlinks { get; set; }

    [JsonPropertyName("blobsImportedPerSecond")]
    public long? BlobsImportedPerSecond { get; set; }

    [JsonPropertyName("adminStatus")]
    public string? AdminStatus { get; set; }
}
