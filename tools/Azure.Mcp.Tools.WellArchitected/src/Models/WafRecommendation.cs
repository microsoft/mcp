// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public record WafRecommendation(
    string Id,
    string Title,
    string Description,
    string Pillar,
    string Content,
    string? Service = null);
