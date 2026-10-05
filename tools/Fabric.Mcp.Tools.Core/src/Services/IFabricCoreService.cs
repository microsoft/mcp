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

    /// <summary>Retrieves metadata for one existing workspace without reading its items or data.</summary>
    /// <param name="workspaceId">The workspace ID as a nonempty UUID.</param>
    /// <param name="preferWorkspaceSpecificEndpoints">The endpoint preference, or null to preserve the service default.</param>
    /// <param name="cancellationToken">The token that cancels authentication and the request.</param>
    /// <returns>The validated workspace metadata.</returns>
    /// <exception cref="ArgumentException">The workspace ID is not a nonempty UUID.</exception>
    /// <exception cref="HttpRequestException">Fabric returns a failure or an unexpected success status.</exception>
    /// <exception cref="System.Text.Json.JsonException">Fabric returns invalid workspace metadata.</exception>
    Task<FabricWorkspaceMetadata> GetWorkspaceAsync(
        string workspaceId,
        bool? preferWorkspaceSpecificEndpoints = null,
        CancellationToken cancellationToken = default);

    Task<CatalogSearchResponse> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);
}
