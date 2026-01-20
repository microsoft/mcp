// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Service interface for platform landing zone modification guidance.
/// </summary>
public interface IPlatformLandingZoneGuidanceService
{
    /// <summary>
    /// Generates landing zone modification guidance based on a user question.
    /// </summary>
    /// <param name="question">The user's question or modification request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The guidance response.</returns>
    Task<string> GetModificationGuidanceAsync(string question, CancellationToken cancellationToken = default);
}
