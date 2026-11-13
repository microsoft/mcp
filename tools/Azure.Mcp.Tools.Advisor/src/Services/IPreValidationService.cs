// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IPreValidationService
{
    Task<PreValidationResult> ValidateAsync(
        string recommendationTypeId,
        IReadOnlyList<Recommendation> candidates,
        string? tenant,
        bool sourceResultsTruncated,
        IReadOnlyList<string> unmatchedResources,
        int minimumUnusedDays,
        CancellationToken cancellationToken = default);
}
