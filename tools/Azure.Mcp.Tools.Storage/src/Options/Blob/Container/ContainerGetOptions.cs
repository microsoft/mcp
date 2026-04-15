// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Blob.Container;

public class ContainerGetOptions : BaseContainerOptions
{
    [JsonPropertyName(StorageOptionDefinitions.PrefixName)]
    public string? Prefix { get; set; }
}
