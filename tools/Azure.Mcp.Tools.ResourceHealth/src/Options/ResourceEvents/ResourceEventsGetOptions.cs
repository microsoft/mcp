// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResourceHealth.Options;

namespace Azure.Mcp.Tools.ResourceHealth.Options.ResourceEvents;

public class ResourceEventsGetOptions : BaseResourceHealthOptions
{
    public string? ResourceId { get; set; }
    public string? QueryStartTime { get; set; }
    public string? QueryEndTime { get; set; }
    public string? Filter { get; set; }
}
