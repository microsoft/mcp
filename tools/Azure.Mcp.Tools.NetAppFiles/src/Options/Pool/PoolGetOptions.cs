// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Pool;

public class PoolGetOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }
}
