// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public interface IFabricCoreService
{
    /// <summary>Retrieves one page of capacities where the principal is an administrator or contributor.</summary>
    /// <param name="continuationToken">An unchanged continuation token, or null for the first page.</param>
    /// <param name="cancellationToken">The cancellation token for the request.</param>
    Task<CapacityListResponse> ListCapacitiesAsync(
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    Task<FabricItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets metadata for one existing Fabric capacity.</summary>
    Task<FabricCapacityMetadata> GetCapacityAsync(string capacityId, CancellationToken cancellationToken);

    Task<CatalogSearchResponse> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);
}
