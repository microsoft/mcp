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

    [JsonPropertyName("totalBlobsErrored")]
    public long? TotalBlobsErrored { get; set; }

    [JsonPropertyName("totalRequests")]
    public long? TotalRequests { get; set; }

    [JsonPropertyName("adminStatus")]
    public string? AdminStatus { get; set; }
}