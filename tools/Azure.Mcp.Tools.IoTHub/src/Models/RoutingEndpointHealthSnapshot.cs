// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public record RoutingEndpointHealthSnapshot(
    string EndpointId,
    string EndpointName,
    string? HealthStatus,
    string? LastKnownError = null,
    string? LastKnownErrorTime = null,
    string? LastSuccessfulSendAttemptTime = null,
    string? LastSendAttemptTime = null);
