// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public record RoutingEndpointHealthResult(
    List<RoutingEndpointHealthSnapshot> Endpoints);
