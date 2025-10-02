// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Identity;

namespace Azure.Mcp.Tools.Grafana.Models;

public record GrafanaWorkspace(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("resourceGroupName")] string? ResourceGroupName,
    [property: JsonPropertyName("subscriptionId")] string? SubscriptionId,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("sku")] string? Sku,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("endpoint")] string? Endpoint,
    [property: JsonPropertyName("zoneRedundancy")] string? ZoneRedundancy,
    [property: JsonPropertyName("publicNetworkAccess")] string? PublicNetworkAccess,
    [property: JsonPropertyName("grafanaVersion")] string? GrafanaVersion,
    [property: JsonPropertyName("identity")] ManagedIdentityInfo? Identity,
    [property: JsonPropertyName("tags")] IDictionary<string, string>? Tags
);
