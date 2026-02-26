// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.WellArchitected.Models;

namespace Azure.Mcp.Tools.WellArchitected.Services;

public interface IWellArchitectedService
{
    Task<WafAnalyzeResponse> AnalyzeAsync(string infrastructureConfig, string intent, CancellationToken cancellationToken);
    Task<List<WafRecommendation>> ListRecommendationsAsync(string? pillar, string? service, CancellationToken cancellationToken);
    Task<WafRecommendation?> GetRecommendationAsync(string id, CancellationToken cancellationToken);
    Task<WafChecklist?> GetChecklistAsync(string pillar, CancellationToken cancellationToken);
    Task<WafServiceGuide?> GetServiceGuideAsync(string service, CancellationToken cancellationToken);
}
