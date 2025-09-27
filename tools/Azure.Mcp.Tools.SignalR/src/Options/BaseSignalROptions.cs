// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SignalR.Options;

/// <summary>
/// Base options for Azure SignalR commands.
/// </summary>
public class BaseSignalROptions : SubscriptionOptions
{
    [JsonPropertyName(SignalROptionDefinitions.SignalRName)]
    public string? SignalR { get; set; }
}
