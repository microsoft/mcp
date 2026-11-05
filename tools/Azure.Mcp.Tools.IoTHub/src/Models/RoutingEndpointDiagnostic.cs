// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public record RoutingEndpointDiagnostic(
    string EndpointId,
    string Name,
    string EndpointType,
    string? AuthenticationType,
    string? EndpointUri,
    string? SubscriptionId,
    string? ResourceGroup,
    string? EndpointResourceName,
    string? EntityPath,
    string? ContainerName,
    string? DatabaseName,
    int? BatchFrequencyInSeconds,
    RoutingTargetInfo Target,
    RoutingHubEmitted HubEmitted,
    RoutingTargetEmitted TargetEmitted);
