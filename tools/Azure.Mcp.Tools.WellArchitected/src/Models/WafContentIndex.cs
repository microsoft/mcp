// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public sealed class WafContentIndex
{
    public Dictionary<string, WafRecommendation> Recommendations { get; set; } = new();
    public Dictionary<string, WafChecklist> Checklists { get; set; } = new();
    public Dictionary<string, WafServiceGuide> ServiceGuides { get; set; } = new();
}
