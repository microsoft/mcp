// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

// One routing endpoint's non-secret configuration.
public record RoutingEndpointDetails(
    string Name,
    string EndpointType,
    string? EndpointResourceName,
    string? SubscriptionId,
    string? ResourceGroup,
    string? EndpointUri,
    string? EntityPath,
    string? ContainerName,
    string? DatabaseName,
    string? AuthenticationType,
    int? BatchFrequencyInSeconds = null,
    string? EndpointId = null);
