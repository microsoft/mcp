// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedLustre.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;

public class AutoexportJobDeleteOptions : BaseManagedLustreOptions
{
    [JsonPropertyName("filesystem-name")]
    public string? FileSystemName { get; set; }

    [JsonPropertyName("job-name")]
    public string? JobName { get; set; }
}
