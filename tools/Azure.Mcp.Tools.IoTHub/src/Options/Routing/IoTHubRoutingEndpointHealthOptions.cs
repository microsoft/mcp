// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTHub.Options.Routing;

public sealed class IoTHubRoutingEndpointHealthOptions : ISubscriptionOption
{
    [Option(Description = "The name of the IoT Hub.")]
    public required string HubName { get; set; }

    [Option(Description = "The name of a specific routing endpoint to return. If omitted, all configured routing endpoints are returned.")]
    public string? EndpointName { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
