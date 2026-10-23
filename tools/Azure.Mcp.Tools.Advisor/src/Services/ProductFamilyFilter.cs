// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services;

internal static class ProductFamilyFilter
{
    internal static readonly string[] AllowedProductFamilies =
    [
        "Compute",
        "Containers",
        "Databases",
        "Networking",
        "Storage",
        "Web",
    ];

    private static readonly IReadOnlyDictionary<string, string[]> ResourceProviderPrefixes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Compute"] =
            [
                "microsoft.compute/",
                // Azure Ops IQ classifies App Service plans as Compute.
                "microsoft.web/serverfarms/",
            ],
            ["Containers"] =
            [
                "microsoft.app/",
                "microsoft.containerinstance/",
                "microsoft.containerregistry/",
                "microsoft.containerservice/",
            ],
            ["Databases"] =
            [
                "microsoft.cache/",
                "microsoft.cosmosdb/",
                "microsoft.dbformariadb/",
                "microsoft.dbformysql/",
                "microsoft.dbforpostgresql/",
                "microsoft.documentdb/",
                "microsoft.sql/",
            ],
            ["Networking"] = ["microsoft.network/"],
            ["Storage"] =
            [
                "microsoft.compute/disks",
                "microsoft.compute/snapshots",
                "microsoft.storage/",
            ],
            ["Web"] = ["microsoft.web/"],
        };

    internal static string? Normalize(string? productFamily)
    {
        if (string.IsNullOrWhiteSpace(productFamily))
        {
            return null;
        }

        return AllowedProductFamilies.FirstOrDefault(
            family => family.Equals(productFamily.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    internal static string BuildPredicate(
        string productFamily,
        string resourceTypeExpression = "properties.impactedField")
    {
        var metadataFamily = $"tolower(tostring(metadataServiceFamily)) =~ '{productFamily.ToLowerInvariant()}'";
        // impactedField is the resource-type expression. The resource ID is a separate
        // provider-prefix fallback used only when metadataServiceFamily is empty.
        var resourceIdFallbackExpression = resourceTypeExpression == "properties.impactedField"
            ? "properties.resourceMetadata.resourceId"
            : "resourceId";
        return $"({metadataFamily} or (isempty(tostring(metadataServiceFamily)) and " +
            $"{BuildProviderFallbackPredicate(productFamily, resourceTypeExpression, resourceIdFallbackExpression)}))";
    }

    internal static string BuildProviderFallbackPredicate(
        string productFamily,
        string resourceTypeExpression = "properties.impactedField",
        string resourceIdExpression = "properties.resourceMetadata.resourceId")
    {
        if (!ResourceProviderPrefixes.TryGetValue(productFamily, out var prefixes))
        {
            throw new ArgumentException($"Unsupported product family '{productFamily}'.", nameof(productFamily));
        }

        var prefixClauses = prefixes
            .Select(prefix =>
                $"(tolower(tostring({resourceTypeExpression})) startswith '{RecommendationQueryBuilder.EscapeKqlString(prefix)}' or " +
                $"tolower(tostring({resourceIdExpression})) contains '/providers/{RecommendationQueryBuilder.EscapeKqlString(prefix)}')")
            .ToArray();

        return $"({string.Join(" or ", prefixClauses)})";
    }
}
