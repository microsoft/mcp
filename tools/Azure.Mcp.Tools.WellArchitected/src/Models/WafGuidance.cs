// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public sealed class WafGuidance
{
    public string AgentInstructions { get; set; } = string.Empty;
    public List<ServiceGuideMatch> MatchedServiceGuides { get; set; } = [];
    public Dictionary<string, PillarRecommendations> RelevantRecommendations { get; set; } = new();
    public Dictionary<string, List<string>> ChecklistItems { get; set; } = new();
}
