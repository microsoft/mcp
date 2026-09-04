// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceFilterBuilderTests
{
    // This default clause is used when no explicit status filter is provided.
    private const string StatusClause = "tostring(properties.recommendationStatus) =~ 'New'";

    [Fact]
    public void BuildAdditionalFilter_NullFilters_ReturnsStatusClauseOnly()
    {
        Assert.Equal(StatusClause, AdvisorService.BuildAdditionalFilter(null));
    }

    [Fact]
    public void BuildAdditionalFilter_AllFieldsNull_ReturnsStatusClauseOnly()
    {
        Assert.Equal(StatusClause, AdvisorService.BuildAdditionalFilter(new RecommendationFilters()));
    }

    [Theory]
    [InlineData(RecommendationStatus.New, "New")]
    [InlineData(RecommendationStatus.Postponed, "Postponed")]
    [InlineData(RecommendationStatus.Dismissed, "Dismissed")]
    [InlineData(RecommendationStatus.Completed, "Completed")]
    public void BuildAdditionalFilter_Status_UsesRequestedStatus(
        RecommendationStatus status,
        string expectedStatus)
    {
        var result = AdvisorService.BuildAdditionalFilter(new RecommendationFilters(Status: status));

        Assert.Equal($"tostring(properties.recommendationStatus) =~ '{expectedStatus}'", result);
    }

    [Fact]
    public void BuildAdditionalFilter_WhitespaceFields_ReturnsStatusClauseOnly()
    {
        var filters = new RecommendationFilters(
            Category: "  ",
            Impact: "",
            ResourceType: "\t",
            Resource: " ",
            Search: "");

        Assert.Equal(StatusClause, AdvisorService.BuildAdditionalFilter(filters));
    }

    [Fact]
    public void BuildAdditionalFilter_Category_UsesCaseInsensitiveEquality()
    {
        var result = AdvisorService.BuildAdditionalFilter(new RecommendationFilters(Category: "Security"));

        Assert.Equal($"{StatusClause} and tostring(properties.category) =~ 'Security'", result);
    }

    [Fact]
    public void BuildAdditionalFilter_Impact_UsesCaseInsensitiveEquality()
    {
        var result = AdvisorService.BuildAdditionalFilter(new RecommendationFilters(Impact: "High"));

        Assert.Equal($"{StatusClause} and tostring(properties.impact) =~ 'High'", result);
    }

    [Fact]
    public void BuildAdditionalFilter_RecommendationTypeId_UsesCaseInsensitiveEquality()
    {
        var result = AdvisorService.BuildAdditionalFilter(
            new RecommendationFilters(RecommendationTypeId: "1d70919c-1a4a-4f79-8300-bb576c291e9d"));

        Assert.Equal(
            $"{StatusClause} and tostring(properties.recommendationTypeId) =~ '1d70919c-1a4a-4f79-8300-bb576c291e9d'",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_ResourceType_UsesCaseInsensitiveContainsOnResourceId()
    {
        var result = AdvisorService.BuildAdditionalFilter(
            new RecommendationFilters(ResourceType: "Microsoft.Storage/storageAccounts"));

        Assert.Equal(
            $"{StatusClause} and "
            + "tostring(properties.resourceMetadata.resourceId) contains 'Microsoft.Storage/storageAccounts'",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_Resource_UsesCaseInsensitiveContainsOnResourceId()
    {
        var result = AdvisorService.BuildAdditionalFilter(new RecommendationFilters(Resource: "mystorage"));

        Assert.Equal(
            $"{StatusClause} and "
            + "tostring(properties.resourceMetadata.resourceId) contains 'mystorage'",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_Search_UsesCaseInsensitiveContainsOnProblemText()
    {
        var result = AdvisorService.BuildAdditionalFilter(new RecommendationFilters(Search: "encryption"));

        Assert.Equal(
            $"{StatusClause} and "
            + "tostring(properties.shortDescription.problem) contains 'encryption'",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_MultipleFields_JoinedWithAnd()
    {
        var filters = new RecommendationFilters(
            Category: "Security",
            Impact: "High",
            Search: "tls");

        var result = AdvisorService.BuildAdditionalFilter(filters);

        Assert.Equal(
            $"{StatusClause} and "
            + "tostring(properties.category) =~ 'Security' and "
            + "tostring(properties.impact) =~ 'High' and "
            + "tostring(properties.shortDescription.problem) contains 'tls'",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_NeverContainsPipe()
    {
        // BaseAzureResourceService rejects additionalFilter strings containing '|'
        // as a KQL-injection guard. Verify our builder never emits one.
        var filters = new RecommendationFilters(
            Category: "Sec|urity",
            Impact: "Hi|gh",
            RecommendationTypeId: "Type|Id",
            ResourceType: "Microsoft.Storage|fake",
            Resource: "my|storage",
            Search: "tls|injection");

        var result = AdvisorService.BuildAdditionalFilter(filters);

        Assert.NotNull(result);
        Assert.DoesNotContain('|', result!);
    }

    [Fact]
    public void BuildAdditionalFilter_SingleQuoteIsEscaped()
    {
        // EscapeKqlString doubles single quotes so user input cannot break out
        // of the KQL string literal.
        var result = AdvisorService.BuildAdditionalFilter(new RecommendationFilters(Search: "it's broken"));

        Assert.NotNull(result);
        Assert.Contains("'it''s broken'", result);
    }

    [Fact]
    public void BuildAdditionalFilter_MetadataOnlyFilters_ProduceNoInstanceClauses()
    {
        // SubCategory, TrackingIds and RetirementDate do not exist on recommendation instances;
        // they are resolved to recommendation type IDs before the query is built.
        var filters = new RecommendationFilters(
            SubCategory: "ServiceUpgradeAndRetirement",
            TrackingIds: ["QNY1-HB8", "9G0V-_G8"],
            RetirementDateOperator: "ge",
            RetirementDate: new DateOnly(2026, 3, 31));

        Assert.Equal(StatusClause, AdvisorService.BuildAdditionalFilter(filters));
    }

    [Fact]
    public void BuildAdditionalFilter_RecommendationTypeIds_AddsInClause()
    {
        var result = AdvisorService.BuildAdditionalFilter(null, ["Type-A", "Type-B"]);

        Assert.Equal(
            $"{StatusClause} and tostring(properties.recommendationTypeId) in~ ('Type-A', 'Type-B')",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_EmptyRecommendationTypeIds_AddsNoInClause()
    {
        Assert.Equal(StatusClause, AdvisorService.BuildAdditionalFilter(null, []));
    }

    [Fact]
    public void BuildAdditionalFilter_RecommendationTypeIds_AreSanitized()
    {
        var result = AdvisorService.BuildAdditionalFilter(null, ["Type|A", "it's"]);

        Assert.NotNull(result);
        Assert.DoesNotContain('|', result!);
        Assert.Contains("'TypeA', 'it''s'", result);
    }

    [Fact]
    public void BuildAdditionalFilter_RecommendationTypeIds_SkipsMetadataInstanceClauses()
    {
        var result = AdvisorService.BuildAdditionalFilter(
            new RecommendationFilters(Category: "Security"),
            ["Type-A"]);

        Assert.Equal(
            $"{StatusClause} and tostring(properties.recommendationTypeId) in~ ('Type-A')",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_RecommendationTypeId_IntersectsMetadataMatches()
    {
        var result = AdvisorService.BuildAdditionalFilter(
            new RecommendationFilters(RecommendationTypeId: "1d70919c-1a4a-4f79-8300-bb576c291e9d"),
            ["Type-A", "1d70919c-1a4a-4f79-8300-bb576c291e9d"]);

        Assert.Equal(
            $"{StatusClause} and " +
            "tostring(properties.recommendationTypeId) =~ '1d70919c-1a4a-4f79-8300-bb576c291e9d' and " +
            "tostring(properties.recommendationTypeId) in~ ('Type-A', '1d70919c-1a4a-4f79-8300-bb576c291e9d')",
            result);
    }

    [Fact]
    public void BuildAdditionalFilter_RecommendationTypeId_IsSanitizedForInternalCallers()
    {
        var result = AdvisorService.BuildAdditionalFilter(
            new RecommendationFilters(RecommendationTypeId: "bad'|id"));

        Assert.NotNull(result);
        Assert.DoesNotContain('|', result!);
        Assert.Contains("'bad''id'", result);
    }
}
