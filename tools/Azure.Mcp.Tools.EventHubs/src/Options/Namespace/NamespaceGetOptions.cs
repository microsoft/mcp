// Copyright (c) Microsoft Corporation.  
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Options.Namespace;

public class NamespaceGetOptions : BaseEventHubsOptions
{
    [JsonPropertyName(EventHubsOptionDefinitions.Namespace)]
    public string? NamespaceName { get; set; }
}
