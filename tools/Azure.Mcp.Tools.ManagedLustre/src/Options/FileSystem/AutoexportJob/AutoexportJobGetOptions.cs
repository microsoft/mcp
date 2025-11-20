// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;

public class AutoexportJobGetOptions : BaseManagedLustreOptions
{
    [JsonPropertyName("filesystem-name")]
    public string? FileSystemName { get; set; }

    [JsonPropertyName("job-name")]
    public string? JobName { get; set; }
}
