// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public record WafAnalysisResult(
    string Pillar,
    List<string> Strengths,
    List<string> Gaps,
    List<WafRecommendation> Recommendations,
    List<string> Suggestions);
