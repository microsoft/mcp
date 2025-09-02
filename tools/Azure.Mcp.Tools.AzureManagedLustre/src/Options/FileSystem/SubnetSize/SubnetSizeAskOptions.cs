// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;

public sealed class SubnetSizeAskOptions : BaseAzureManagedLustreOptions
{
    [JsonPropertyName(AzureManagedLustreOptionDefinitions.sku)]
    public string? Sku { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.size)]
    public int Size { get; set; }
}
