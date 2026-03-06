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

    /// <summary>
    /// Selects a candidate template from the manifest for the given language.
    /// Prefers templates sorted by priority (lower = better).
    /// </summary>
    /// <param name="manifest">The template manifest.</param>
    /// <param name="language">The language to find a template for.</param>
    /// <returns>The best matching template entry or null if not found.</returns>
    TemplateManifestEntry? SelectCandidateTemplate(TemplateManifest manifest, string language);
}
