// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public static class WellArchitectedTelemetryTags
{
    private static string AddPrefix(string tagName) => $"wellarchitected/{tagName}";

    public static readonly string Pillar = AddPrefix("Pillar");
    public static readonly string Service = AddPrefix("Service");
    public static readonly string RecommendationId = AddPrefix("RecommendationId");
    public static readonly string Intent = AddPrefix("Intent");
}
