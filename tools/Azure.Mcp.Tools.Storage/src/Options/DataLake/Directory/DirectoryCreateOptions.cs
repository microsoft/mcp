// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.DataLake.Directory;

public class DirectoryCreateOptions : BaseStorageOptions
{
    [JsonPropertyName(StorageOptionDefinitions.DirectoryPathName)]
    public string? DirectoryPath { get; set; }
}
