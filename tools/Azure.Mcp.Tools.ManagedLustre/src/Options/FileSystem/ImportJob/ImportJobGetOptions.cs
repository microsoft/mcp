// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedLustre.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;

public class ImportJobGetOptions : BaseManagedLustreOptions
{
    /// <summary>
    /// The name of the filesystem.
    /// </summary>
    [JsonPropertyName("filesystemName")]
    public string? FileSystemName { get; set; }

    /// <summary>
    /// The name of the import job. If not provided, lists all import jobs.
    /// </summary>
    [JsonPropertyName("jobName")]
    public string? JobName { get; set; }
}
