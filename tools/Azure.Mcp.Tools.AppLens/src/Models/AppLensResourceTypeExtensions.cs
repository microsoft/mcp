// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppLens.Models;

internal static class AppLensResourceTypeExtensions
{
    internal static string ToResourceType(this AppLensResourceType resourceType) => resourceType switch
    {
        AppLensResourceType.AppService => "microsoft.web/sites",
        AppLensResourceType.Aks => "microsoft.containerservice/managedclusters",
        AppLensResourceType.ApiManagement => "microsoft.apimanagement/service",
        _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null)
    };
}
