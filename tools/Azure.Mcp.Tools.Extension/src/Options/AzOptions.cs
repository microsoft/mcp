// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Extension.Options;

public class AzOptions : GlobalOptions
{
    [JsonPropertyName(ExtensionOptionDefinitions.Az.CommandName)]
    public string? Command { get; set; }
}
