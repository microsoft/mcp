// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide;

public class ServiceGuideGetOptions : GlobalOptions
{
    [JsonPropertyName(WellArchitectedFrameworkOptionDefinitions.ServiceName)]
    public string? Service { get; set; }
}
