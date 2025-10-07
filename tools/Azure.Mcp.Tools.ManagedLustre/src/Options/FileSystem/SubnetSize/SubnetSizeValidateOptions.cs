// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class SubnetSizeValidateOptions : BaseManagedLustreOptions
{
    [JsonPropertyName(ManagedLustreOptionDefinitions.sku)]
    public string? Sku { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.size)]
    public int Size { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.subnetId)]
    public string? SubnetId { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.location)]
    public string? Location { get; set; }
}
