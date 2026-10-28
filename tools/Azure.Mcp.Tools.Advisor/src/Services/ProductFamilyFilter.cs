// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services;

internal static class ProductFamilyFilter
{
    internal static string? Normalize(string? productFamily) =>
        string.IsNullOrWhiteSpace(productFamily) ? null : productFamily.Trim();

    internal static string BuildPredicate(string productFamily) =>
        $"(tolower(tostring(metadataServiceFamily)) =~ '{RecommendationQueryBuilder.SanitizeForKql(productFamily.ToLowerInvariant())}')";
}
