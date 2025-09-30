// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Models;

namespace Azure.Mcp.Tools.EventHubs.Services;

public interface IEventHubsService
{
    Task<List<Namespace>> GetNamespacesAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<Namespace> GetNamespaceAsync(
        string namespaceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
