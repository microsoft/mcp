// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Models;

namespace Azure.Mcp.Tools.Compute.Services;

public interface IComputePlacementService
{
    Task<SpotPlacementMetadataInfo> GetSpotPlacementMetadataAsync(
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<PlacementScoreInfo>> GetSpotPlacementScoresAsync(
        string location,
        string subscription,
        string[] desiredLocations,
        string[] desiredSizes,
        int desiredCount = 1,
        bool availabilityZones = true,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
