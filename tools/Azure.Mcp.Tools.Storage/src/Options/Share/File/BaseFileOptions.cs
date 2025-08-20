// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Options.Share.File;

public class BaseFileOptions : BaseShareOptions
{
    [JsonPropertyName(StorageOptionDefinitions.DirectoryPathName)]
    public string? DirectoryPath { get; set; }
}
