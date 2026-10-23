// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

// Shared deserialization shape for every routing custom-endpoint kind. Kind-specific fields stay null
// when absent. Intentionally omits connection strings / keys / identity so secrets are never deserialized.
public class RoutingEndpointProperties
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? SubscriptionId { get; set; }

    public string? ResourceGroup { get; set; }

    public string? EndpointUri { get; set; }

    public string? EntityPath { get; set; }

    public string? ContainerName { get; set; }

    public string? DatabaseName { get; set; }

    public string? AuthenticationType { get; set; }

    public int? BatchFrequencyInSeconds { get; set; }
}
