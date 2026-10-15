// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceMetadataJoinTests
{
    private static RecommendationMetadata CreateMetadata(
        string recommendationTypeId,
        string? displayName = "Metadata display name",
        string? category = "Security",
        string? subCategory = "ZoneResiliency",
        string? impact = "High",
        string? potentialBenefits = "Improved resiliency",
        string? learnMoreLink = "https://aka.ms/advisor",
        RecommendationServiceRetirement? serviceRetirement = null,
        string? detailedDescription = "Detailed description") =>
        new(
            RecommendationTypeId: recommendationTypeId,
            DisplayName: displayName,
            Label: "Reliability",
            Category: category,
            SubCategory: subCategory,
            Impact: impact,
            PriorityScore: 42.5,
            PotentialBenefits: potentialBenefits,
            DetailedDescription: detailedDescription,
            LearnMoreLink: learnMoreLink,
            SupportedResourceType: "microsoft.storage/storageaccounts",
            Scope: "Resource",
            DataSourceQuery: "resources | take 1",
            ResourceSingularName: "storage account",
            ResourcePluralName: "storage accounts",
            Actions: [new RecommendationMetadataAction("Fix", "Enable it", "https://aka.ms/doc", "BladeName")],
            Language: "en",
            LastRefreshed: "2026-08-01",
            ServiceRetirement: serviceRetirement);

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
    public void SecurityCategoryRequiresMetadataLookup()
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

    [Fact]
    public void BuildMetadataLookup_IsCaseInsensitiveAndSkipsBlankIds()
    {
        var lookup = AdvisorService.BuildMetadataLookup(
        [
            CreateMetadata("Type-A"),
            CreateMetadata("   "),
        ]);

        Assert.Single(lookup);
        Assert.True(lookup.ContainsKey("type-a"));
    }

    [Fact]
    public void BuildMetadataLookup_DuplicateIds_KeepsLastEntry()
    {
        var lookup = AdvisorService.BuildMetadataLookup(
        [
            CreateMetadata("Type-A", displayName: "first"),
            CreateMetadata("type-a", displayName: "second"),
        ]);

        Assert.Single(lookup);
        Assert.Equal("second", lookup["Type-A"].DisplayName);
    }

    [Fact]
    public async Task CollectMetadataPagesAsync_FollowsContinuationTokens()
    {
        var requestedTokens = new List<string?>();
        var callCount = 0;

        var results = await AdvisorService.CollectMetadataPagesAsync(
            (skipToken, _) =>
            {
                requestedTokens.Add(skipToken);
                callCount++;
                return Task.FromResult(callCount == 1
                    ? (new List<RecommendationMetadata> { CreateMetadata("Type-A") }, "next-page", false)
                    : (new List<RecommendationMetadata> { CreateMetadata("Type-B") }, (string?)null, false));
            },
            CancellationToken.None);

        Assert.Equal([null, "next-page"], requestedTokens);
        Assert.Equal(["Type-A", "Type-B"], results.Select(result => result.RecommendationTypeId));
    }

    [Fact]
    public async Task CollectMetadataPagesAsync_TruncationWithoutContinuationTokenThrows()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AdvisorService.CollectMetadataPagesAsync(
                (_, _) => Task.FromResult((new List<RecommendationMetadata>(), (string?)null, true)),
                CancellationToken.None));

        Assert.Contains("without returning a continuation token", exception.Message);
    }

    [Fact]
    public async Task CollectMetadataPagesAsync_RepeatedContinuationTokenThrows()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AdvisorService.CollectMetadataPagesAsync(
                (_, _) => Task.FromResult((new List<RecommendationMetadata>(), (string?)"repeated", false)),
                CancellationToken.None));

        Assert.Contains("repeated continuation token", exception.Message);
    }

    [Fact]
    public async Task CollectMetadataPagesAsync_ObservesCancellationBeforeRequest()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            AdvisorService.CollectMetadataPagesAsync(
                (_, _) => throw new InvalidOperationException("The page request should not run."),
                cancellationTokenSource.Token));
    }
}
