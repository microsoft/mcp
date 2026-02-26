// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public sealed class WafAnalyzeResponse
{
    public AnalysisContext AnalysisContext { get; set; } = new();
    public WafGuidance WafGuidance { get; set; } = new();
}
