// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Blob;

public class BlobUploadOptions : BaseBlobOptions
{
    [JsonPropertyName(StorageOptionDefinitions.LocalFilePathName)]
    public string? LocalFilePath { get; set; }
}
