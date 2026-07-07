// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AzureMigrate.Models;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service interface for platform landing zone operations.
/// </summary>
public interface IPlatformLandingZoneService
{
    /// <summary>
    /// Updates the cached parameters for a platform landing zone.
    /// </summary>
    /// <param name="context">The landing zone context.</param>
    /// <param name="regionType">The region type (single or multi).</param>
    /// <param name="fireWallType">The firewall type (azurefirewall, nva, or none).</param>
    /// <param name="networkArchitecture">The network architecture (hubspoke or vwan).</param>
    /// <param name="identitySubscriptionId">The identity subscription ID.</param>
    /// <param name="managementSubscriptionId">The management subscription ID.</param>
    /// <param name="connectivitySubscriptionId">The connectivity subscription ID.</param>
    /// <param name="regions">The regions.</param>
    /// <param name="environmentName">The environment name.</param>
    /// <param name="versionControlSystem">The version control system.</param>
    /// <param name="organizationName">The organization name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated parameters.</returns>
    Task<PlatformLandingZoneParameters> UpdateParametersAsync(
        PlatformLandingZoneContext context,
        string? regionType = null,
        string? fireWallType = null,
        string? networkArchitecture = null,
        string? identitySubscriptionId = null,
        string? managementSubscriptionId = null,
        string? connectivitySubscriptionId = null,
        string? regions = null,
        string? environmentName = null,
        string? versionControlSystem = null,
        string? organizationName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a platform landing zone already exists for the given context.
    /// </summary>
    /// <param name="context">The landing zone context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if platform landing zone exists, false otherwise.</returns>
    Task<bool> CheckExistingAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a platform landing zone.
    /// </summary>
    /// <param name="context">The landing zone context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The download URL if successful, null otherwise.</returns>
    Task<string?> GenerateAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the generated platform landing zone.
    /// </summary>
    /// <param name="context">The platform landing zone context.</param>
    /// <param name="outputPath">The output path for the downloaded file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The path to the downloaded file.</returns>
    Task<string> DownloadAsync(
        PlatformLandingZoneContext context,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of cached parameters.
    /// </summary>
    /// <param name="context">The platform landing zone context.</param>
    /// <returns>A message describing the parameter status.</returns>
    string GetParameterStatus(PlatformLandingZoneContext context);

    /// <summary>
    /// Gets a list of missing required parameters.
    /// </summary>
    /// <param name="context">The platform landing zone context.</param>
    /// <returns>A list of missing parameter names.</returns>
    List<string> GetMissingParameters(PlatformLandingZoneContext context);
}
