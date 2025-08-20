// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;

namespace Azure.Mcp.Tools.VirtualDesktop.Options.SessionHost;

public class SessionHostUserSessionListOptions : BaseHostPoolOptions
{
    [JsonPropertyName(VirtualDesktopOptionDefinitions.SessionHostName)]
    public string? SessionHostName { get; set; }
}
