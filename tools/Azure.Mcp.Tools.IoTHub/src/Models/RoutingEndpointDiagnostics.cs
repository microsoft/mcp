// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public record RoutingEndpointDiagnostics(
    RoutingObservationWindow ObservationWindow,
    List<RoutingEndpointDiagnostic> Endpoints);
