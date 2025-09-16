// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Speech.Options;

public class BaseSpeechOptions : SubscriptionOptions
{
    [JsonPropertyName(SpeechOptionDefinitions.EndpointName)]
    public string? Endpoint { get; set; }
}
