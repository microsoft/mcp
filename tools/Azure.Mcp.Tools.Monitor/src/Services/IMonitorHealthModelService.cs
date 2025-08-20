// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Services;

public interface IMonitorHealthModelService
{
    /// <summary>
    /// Gets the health of an entity from a health model
    /// </summary>
    /// <param name="entity">The entity to get health for</param>
    /// <param name="healthModelName">The health model name</param>
    /// <param name="resourceGroupName">The resource group name containing the health model</param>
    /// <param name="subscription">Subscription ID or name</param>
    /// <param name="authMethod">Optional authentication method</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>Entity health information</returns>
    /// <exception cref="AuthenticationFailedException">When authentication fails</exception>
    /// <exception cref="RequestFailedException">When the service request fails</exception>
    Task<JsonNode> GetEntityHealth(
        string entity,
        string healthModelName,
        string resourceGroup,
        string subscription,
        AuthMethod? authMethod = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);
}
