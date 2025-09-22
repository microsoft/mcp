// Copyright (c) Microsoft Corporation.  
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.EventHubs.Options;

namespace Azure.Mcp.Tools.EventHubs.Options.Namespace;

public class NamespaceGetOptions : BaseEventHubsOptions
{
    [JsonPropertyName(EventHubsOptionDefinitions.NamespaceNameName)]
    public string? NamespaceName { get; set; }
}
