// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.IoTHub;

public class IoTHubQueryCompileOptions : GlobalOptions
{
    public string? Filters { get; set; }
    public string? From { get; set; }
    public int? Top { get; set; }
    public string? LogicalOperator { get; set; }
    public string? DiscoveredFields { get; set; }
}
