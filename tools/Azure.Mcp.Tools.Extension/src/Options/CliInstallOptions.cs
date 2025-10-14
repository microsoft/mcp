// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Extension.Options;

public class CliInstallOptions : GlobalOptions
{
    [JsonPropertyName(ExtensionOptionDefinitions.CliInstall.CliTypeName)]
    public string? CliType { get; set; }
}
