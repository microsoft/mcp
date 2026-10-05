// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public interface IFabricCoreService
{
    Task<FabricItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets metadata for one existing Fabric capacity.</summary>
    Task<FabricCapacityMetadata> GetCapacityAsync(string capacityId, CancellationToken cancellationToken);

    Task<CatalogSearchResponse> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);
}
