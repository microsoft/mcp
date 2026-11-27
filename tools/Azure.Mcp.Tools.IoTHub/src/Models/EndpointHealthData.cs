// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

// Health data for a single routing endpoint from the routingEndpointsHealth REST API.
public class EndpointHealthData
{
    public string? EndpointId { get; set; }
    public string? HealthStatus { get; set; }
    public string? LastKnownError { get; set; }
    public string? LastKnownErrorTime { get; set; }
    public string? LastSuccessfulSendAttemptTime { get; set; }
    public string? LastSendAttemptTime { get; set; }
}
