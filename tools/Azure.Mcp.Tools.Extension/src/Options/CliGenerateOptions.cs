// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Extension.Options;

public class CliGenerateOptions : GlobalOptions
{
    [JsonPropertyName(ExtensionOptionDefinitions.CliGenerate.IntentName)]
    public string? Intent { get; set; }

    [JsonPropertyName(ExtensionOptionDefinitions.CliGenerate.CliTypeName)]
    public string? CliType { get; set; }
}
