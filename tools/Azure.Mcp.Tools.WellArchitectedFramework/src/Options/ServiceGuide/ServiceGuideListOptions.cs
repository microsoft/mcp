// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide;

public class ServiceGuideListOptions : GlobalOptions
{
    [JsonPropertyName(WellArchitectedFrameworkOptionDefinitions.ServicesName)]
    public string[]? Services { get; set; }
}
