// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureManagedLustre.Options.FileSystem;

public sealed class SkuGetOptions : BaseAzureManagedLustreOptions
{
    [property: JsonPropertyName("location")]
    public string? Location { get; set; }
}
