// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;

public sealed class SubnetSizeValidateOptions : BaseAzureManagedLustreOptions
{
    [JsonPropertyName(AzureManagedLustreOptionDefinitions.sku)]
    public string? Sku { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.size)]
    public int Size { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.subnetId)]
    public string? SubnetId { get; set; }

    [JsonPropertyName(AzureManagedLustreOptionDefinitions.location)]
    public string? Location { get; set; }
}
