// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Communication.Options;

/// <summary>
/// Base options class for Communication Services commands.
/// </summary>
public class BaseCommunicationOptions : SubscriptionOptions
{
    [JsonPropertyName(CommunicationOptionDefinitions.EndpointName)]
    public string? Endpoint { get; set; }
}
