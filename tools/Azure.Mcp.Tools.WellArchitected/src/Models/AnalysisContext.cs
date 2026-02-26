// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public sealed class AnalysisContext
{
    public string Intent { get; set; } = string.Empty;
    public List<string> DetectedResourceTypes { get; set; } = [];
    public List<string> DetectedServices { get; set; } = [];
    public int ResourceCount { get; set; }
    public PropertySignals PropertySignals { get; set; } = new();
}
