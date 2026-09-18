// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public interface IFabricCoreService
{
    Task<FabricItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves generic metadata for one existing Fabric item without reading its data or definition.
    /// </summary>
    /// <param name="workspaceId">The containing workspace ID as a nonempty UUID string.</param>
    /// <param name="itemId">The item ID as a nonempty UUID string.</param>
    /// <param name="cancellationToken">A token that cancels authentication and the HTTP request.</param>
    /// <returns>The item's ID, display name, type, workspace ID, and description when available.</returns>
    /// <remarks>
    /// Valid IDs are normalized before use. The configured identity must have read permission for the item.
    /// Names are not resolved and requests are not retried automatically. For throttled responses, a valid
    /// <c>Retry-After</c> header is preserved as guidance in the sanitized exception message.
    /// </remarks>
    /// <exception cref="ArgumentException">Either ID is not a nonempty UUID.</exception>
    /// <exception cref="Azure.Identity.AuthenticationFailedException">The credential cannot acquire a Fabric access token.</exception>
    /// <exception cref="HttpRequestException">The HTTP request fails or returns a status other than 200 (OK).</exception>
    /// <exception cref="System.Text.Json.JsonException">The response is not valid metadata for the requested workspace and item.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled or times out.</exception>
    Task<FabricItemMetadata> GetItemAsync(string workspaceId, string itemId, CancellationToken cancellationToken = default);

    Task<CatalogSearchResponse> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);
}
