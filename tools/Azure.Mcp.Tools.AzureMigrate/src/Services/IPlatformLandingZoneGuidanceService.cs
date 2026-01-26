// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using static Azure.Mcp.Tools.AzureMigrate.Services.PlatformLandingZoneGuidanceService;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service interface for platform landing zone modification guidance.
/// </summary>
public interface IPlatformLandingZoneGuidanceService
{
    /// <summary>
    /// Fetches platform landing zone modification guidance for a specific scenario.
    /// </summary>
    /// <param name="scenario">The scenario key (e.g., 'bastion', 'ddos', 'policy-assignment').</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The official documentation for the scenario.</returns>
    Task<string> GetGuidanceAsync(string scenario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all policies organized by archetype.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Dictionary of archetype name to list of policy names.</returns>
    Task<Dictionary<string, List<string>>> GetAllPoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for policies matching a search term across all archetypes.
    /// </summary>
    /// <param name="searchTerm">Partial or full policy name to search for (case-insensitive).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of matching policies with their archetype locations.</returns>
    Task<List<PolicyLocationResult>> SearchPoliciesAsync(string searchTerm, CancellationToken cancellationToken = default);
}
