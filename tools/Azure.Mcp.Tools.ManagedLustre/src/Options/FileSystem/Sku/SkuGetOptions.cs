// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class SkuGetOptions : BaseManagedLustreOptions
{
    [property: JsonPropertyName("location")]
    public string? Location { get; set; }
}
