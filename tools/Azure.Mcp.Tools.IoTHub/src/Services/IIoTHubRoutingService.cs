// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Services;

public interface IIoTHubRoutingService
{
    Task<List<RoutingEndpointHealthSnapshot>> GetRoutingEndpointHealth(
        string hubName,
        string resourceGroup,
        string subscription,
        string? endpointName = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<RoutingEndpointDiagnostics> GetRoutingEndpointDiagnostics(
        string hubName,
        string resourceGroup,
        string subscription,
        string? endpointName = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        string? interval = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
