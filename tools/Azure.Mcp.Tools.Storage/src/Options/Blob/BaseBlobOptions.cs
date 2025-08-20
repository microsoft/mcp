// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Blob;

public class BaseBlobOptions : BaseContainerOptions
{
    [JsonPropertyName(StorageOptionDefinitions.BlobName)]
    public string? Blob { get; set; }
}
