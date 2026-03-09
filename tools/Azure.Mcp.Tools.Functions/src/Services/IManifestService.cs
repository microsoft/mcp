// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Service for fetching and caching the Azure Functions template manifest from CDN.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Fetches the template manifest from CDN, using cache when available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The template manifest.</returns>
    Task<TemplateManifest> FetchManifestAsync(CancellationToken cancellationToken);
}
