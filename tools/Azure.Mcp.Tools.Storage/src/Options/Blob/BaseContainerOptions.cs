// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Blob;

public class BaseContainerOptions : BaseStorageOptions
{
    [JsonPropertyName(StorageOptionDefinitions.ContainerName)]
    public string? Container { get; set; }
}
