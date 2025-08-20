
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;

public class BaseHostPoolOptions : SubscriptionOptions
{
    [JsonPropertyName(VirtualDesktopOptionDefinitions.HostPoolName)]
    public string? HostPoolName { get; set; }

    [JsonPropertyName(VirtualDesktopOptionDefinitions.HostPoolResourceId)]
    public string? HostPoolResourceId { get; set; }
}
