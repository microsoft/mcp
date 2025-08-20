// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Blob.Container;

public class ContainerCreateOptions : BaseContainerOptions
{
    [JsonPropertyName(StorageOptionDefinitions.BlobContainerPublicAccessName)]
    public string? BlobContainerPublicAccess { get; set; }
}
