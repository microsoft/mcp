// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Blob;

public class BlobGetOptions : BaseBlobOptions
{
    [JsonPropertyName(StorageOptionDefinitions.PrefixName)]
    public string? Prefix { get; set; }
}
