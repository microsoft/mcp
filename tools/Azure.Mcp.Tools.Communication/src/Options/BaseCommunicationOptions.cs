// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Communication.Options;

public class BaseCommunicationOptions : GlobalOptions
{
    [JsonPropertyName(CommunicationOptionDefinitions.EndpointName)]
    public string? Endpoint { get; set; }
}
