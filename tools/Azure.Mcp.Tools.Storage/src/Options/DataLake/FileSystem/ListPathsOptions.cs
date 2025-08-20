// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.DataLake.FileSystem;

public class ListPathsOptions : BaseFileSystemOptions
{
    [JsonPropertyName(StorageOptionDefinitions.FilterPathName)]
    public string? FilterPath { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.RecursiveName)]
    public bool Recursive { get; set; } = false;
}
