// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

// Paged result of EndpointHealthData from the routingEndpointsHealth REST API.
public class EndpointHealthDataListResult
{
    public List<EndpointHealthData>? Value { get; set; }
    public string? NextLink { get; set; }
}
