// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class SubnetSizeAskOptions : BaseManagedLustreOptions
{
    [JsonPropertyName(ManagedLustreOptionDefinitions.sku)]
    public string? Sku { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.size)]
    public int Size { get; set; }
}
