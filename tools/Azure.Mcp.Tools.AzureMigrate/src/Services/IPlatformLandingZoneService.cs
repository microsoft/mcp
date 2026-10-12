// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Operations against the Platform Landing Zone ARM resource and the artifact it produces.
/// </summary>
public interface IPlatformLandingZoneService
{
    /// <summary>
    /// Creates a landing zone, or updates an existing one by layering the supplied options over its
    /// current configuration.
    /// </summary>
    Task<PlatformLandingZoneView> CreateOrUpdateAsync(
        PlatformLandingZoneContext context,
        RequestOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a landing zone, returning null when it does not exist.
    /// </summary>
    Task<PlatformLandingZoneView?> GetAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the landing zones under the migrate project.
    /// </summary>
    Task<IReadOnlyList<PlatformLandingZoneView>> ListAsync(
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls until generation reaches a terminal state, or the timeout elapses.
    /// </summary>
    Task<PlatformLandingZoneView> WaitForTerminalAsync(
        PlatformLandingZoneContext context,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the generated artifact files into <paramref name="outputDirectory"/>.
    /// </summary>
    Task<IReadOnlyList<string>> DownloadAsync(
        PlatformLandingZoneContext context,
        string outputDirectory,
        bool includeDesignDocument,
        CancellationToken cancellationToken = default);
}
