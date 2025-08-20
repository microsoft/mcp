// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.ResourceGroup;

namespace Azure.Mcp.Tests.Helpers;

/// <summary>
/// Helper methods for creating test resource group data.
/// </summary>
public static class ResourceGroupTestHelpers
{
    /// <summary>
    /// Creates a ResourceGroupInfo with the specified properties for testing.
    /// </summary>
    /// <param name="name">The name of the resource group</param>
    /// <param name="subscriptionId">The subscription ID (used to construct the full resource ID)</param>
    /// <param name="location">The Azure region/location</param>
    /// <returns>A ResourceGroupInfo instance for testing</returns>
    public static ResourceGroupInfo CreateResourceGroupInfo(string name, string subscriptionId, string location)
    {
        var id = $"/subscriptions/{subscriptionId}/resourceGroups/{name}";
        return new ResourceGroupInfo(name, id, location);
    }


}
