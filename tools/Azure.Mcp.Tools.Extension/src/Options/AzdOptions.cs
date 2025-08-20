// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Extension.Options;

public class AzdOptions : GlobalOptions
{
    [JsonPropertyName(ExtensionOptionDefinitions.Azd.CommandName)]
    public string? Command { get; set; }

    [JsonPropertyName(ExtensionOptionDefinitions.Azd.CwdName)]
    public string? Cwd { get; set; }

    [JsonPropertyName(ExtensionOptionDefinitions.Azd.EnvironmentName)]
    public string? Environment { get; set; }

    [JsonPropertyName(ExtensionOptionDefinitions.Azd.LearnName)]
    public bool Learn { get; set; }
}
