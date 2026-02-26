// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public sealed class RelevantRecommendation
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pillar { get; set; } = string.Empty;
    public string RelevanceReason { get; set; } = string.Empty;
}
