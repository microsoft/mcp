// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SignalR.Options;

/// <summary>
/// Base options for Azure SignalR commands.
/// </summary>
public class BaseSignalROptions : SubscriptionOptions
{
    public string? SignalRName { get; set; }
}
