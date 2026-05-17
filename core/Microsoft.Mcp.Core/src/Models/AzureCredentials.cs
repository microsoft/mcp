// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Models;

public record AzureCredentials(
    [property: JsonPropertyName("clientId")] string ClientId,
    [property: JsonPropertyName("clientSecret")] string ClientSecret,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("subscriptionId")] string? SubscriptionId = null
);
