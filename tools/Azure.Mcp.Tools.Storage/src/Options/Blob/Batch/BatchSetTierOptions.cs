// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Blob.Batch;

public class BatchSetTierOptions : BaseContainerOptions
{
    [JsonPropertyName(StorageOptionDefinitions.TierName)]
    public string? Tier { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.BlobNamesParam)]
    public string[]? BlobNames { get; set; }
}
