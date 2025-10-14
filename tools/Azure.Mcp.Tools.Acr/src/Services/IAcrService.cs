// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Acr.Models;
namespace Azure.Mcp.Tools.Acr.Services;

public interface IAcrService
{
    Task<List<AcrRegistryInfo>> ListRegistries(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<Dictionary<string, List<string>>> ListRegistryRepositories(
        string subscription,
        string? resourceGroup = null,
        string? registry = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
