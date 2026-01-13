// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;

public sealed class AutoexportJobCreateOptions : BaseManagedLustreOptions
{
    [JsonPropertyName("filesystem-name")]
    public string? FileSystemName { get; set; }

    [JsonPropertyName("job-name")]
    public string? JobName { get; set; }

    [JsonPropertyName("autoexport-prefix")]
    public string? AutoexportPrefix { get; set; }

    [JsonPropertyName("admin-status")]
    public string? AdminStatus { get; set; }
}
