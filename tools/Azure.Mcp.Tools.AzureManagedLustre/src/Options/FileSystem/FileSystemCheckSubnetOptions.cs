// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;

public sealed class FileSystemCheckSubnetOptions : BaseAzureManagedLustreOptions
{
    [property: JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [property: JsonPropertyName("size")]
    public int Size { get; set; }

    [property: JsonPropertyName("subnetId")]
    public string? SubnetId { get; set; }

    [property: JsonPropertyName("location")]
    public string? Location { get; set; }
}
