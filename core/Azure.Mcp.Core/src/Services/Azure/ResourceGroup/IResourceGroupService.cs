// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.ResourceGroup;
using Azure.Mcp.Core.Options;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.ResourceGroup;

public interface IResourceGroupService
{
    Task<List<ResourceGroupInfo>> GetResourceGroups(string subscriptionId, string? tenant = null, RetryPolicyOptions? retryPolicy = null);
    Task<ResourceGroupInfo?> GetResourceGroup(string subscriptionId, string resourceGroupName, string? tenant = null, RetryPolicyOptions? retryPolicy = null);
    Task<ResourceGroupResource?> GetResourceGroupResource(string subscriptionId, string resourceGroupName, string? tenant = null, RetryPolicyOptions? retryPolicy = null);
}
