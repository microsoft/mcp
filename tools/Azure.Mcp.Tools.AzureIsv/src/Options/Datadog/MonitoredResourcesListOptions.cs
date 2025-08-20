// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureIsv.Options.Datadog;

public class MonitoredResourcesListOptions : SubscriptionOptions
{
    [JsonPropertyName(DatadogOptionDefinitions.DatadogResourceParam)]
    public string? DatadogResource { get; set; }
}
