// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceMetadataJoinTests
{
    [Fact]
    public void HasMetadataOnlyFilters_NullFilters_ReturnsFalse()
    {
        Assert.False(AdvisorService.HasMetadataOnlyFilters(null));
    }

    [Fact]
    public void HasMetadataOnlyFilters_InstanceOnlyFilters_ReturnsFalse()
    {
        var filters = new RecommendationFilters(
            Category: "Security",
            Impact: "High",
            ResourceType: "Microsoft.Storage/storageAccounts",
            Resource: "mystorage",
            Search: "encryption");

        Assert.False(AdvisorService.HasMetadataOnlyFilters(filters));
    }

    [Fact]
    public void HasMetadataFilters_InstanceOnlyFilters_ReturnsFalse()
    {
        Assert.False(AdvisorService.HasMetadataFilters(
            new RecommendationFilters(Resource: "mystorage", Search: "encryption")));
    }

    [Fact]
    public void HasMetadataFilters_RecommendationTypeIdOnly_ReturnsFalse()
    {
        Assert.False(AdvisorService.HasMetadataFilters(
            new RecommendationFilters(
                RecommendationTypeId: "1d70919c-1a4a-4f79-8300-bb576c291e9d")));
    }

    [Fact]
    public void SecurityQueryWithMetadataOnlyFilters_UsesMetadata()
    {
        var filters = new RecommendationFilters(
            Category: "Security",
            SubCategory: "ZoneResiliency");

        Assert.True(AdvisorService.HasMetadataFilters(filters));
    }

    [Theory]
    [InlineData("Security", null, null, true)]
    [InlineData(null, "High", null, true)]
    [InlineData(null, null, "Microsoft.Storage/storageAccounts", true)]
    public void HasMetadataFilters_MetadataBackedFilters_UsesMetadataForAllCategories(
        string? category,
        string? impact,
        string? resourceType,
        bool expected)
    {
        Assert.Equal(expected, AdvisorService.HasMetadataFilters(
            new RecommendationFilters(
                Category: category,
                Impact: impact,
                ResourceType: resourceType)));
    }

    [Fact]
    public void SecurityCategoryRequiresMetadataFilter()
    {
        Assert.True(AdvisorService.HasMetadataFilters(
            new RecommendationFilters(Category: "Security", Impact: "High")));
    }
    [Fact]
    public void HasMetadataFilters_CombinedFilters_ReturnsTrue()
    {
        Assert.True(AdvisorService.HasMetadataFilters(
            new RecommendationFilters(
                SubCategory: "ServiceUpgradeAndRetirement",
                Resource: "mystorage",
                Search: "encryption")));
    }

    [Theory]
    [InlineData("ZoneResiliency", null, false)]
    [InlineData(null, "QNY1-HB8", false)]
    [InlineData(null, null, true)]
    [InlineData("  ", "  ", true)]
    public void HasMetadataOnlyFilters_MetadataFilters_ReturnsTrue(
        string? subCategory,
        string? trackingId,
        bool withRetirementDate)
    {
        var filters = new RecommendationFilters(
            SubCategory: subCategory,
            TrackingIds: trackingId is null ? null : [trackingId],
            RetirementDateOperator: withRetirementDate ? "ge" : null,
            RetirementDate: withRetirementDate ? new DateOnly(2026, 3, 31) : null);

        Assert.True(AdvisorService.HasMetadataOnlyFilters(filters));
    }

    [Fact]
    public void HasMetadataOnlyFilters_MultipleTrackingIds_ReturnsTrue()
    {
        var filters = new RecommendationFilters(TrackingIds: ["QNY1-HB8", "9G0V-_G8"]);

        Assert.True(AdvisorService.HasMetadataOnlyFilters(filters));
    }

    [Fact]
    public void HasMetadataOnlyFilters_BlankTrackingIdsOnly_ReturnsFalse()
    {
        var filters = new RecommendationFilters(TrackingIds: ["  ", ""]);

        Assert.False(AdvisorService.HasMetadataOnlyFilters(filters));
    }
}
