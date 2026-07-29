// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

// Focused unit tests for AdvisorService.ListRecommendationTypesAsync. The Advisor
// metadata endpoint is hit via IHttpClientFactory so we stub the handler with a canned
// ARM payload; the ARM token path is short-circuited by stubbing
// ITenantService.GetTokenCredentialAsync.
//
// The service only consumes the `recommendationType` entity (the one whose supportedValues
// carry per-type linkage to category/impact/resourceType/subCategory). Other metadata
// entities in the payload are intentionally ignored — they belong in a future
// `metadata list` command.
public class AdvisorServiceTests
{
    // Canned ARM metadata response mirroring the live shape documented at
    // https://learn.microsoft.com/rest/api/advisor/recommendation-metadata/list?view=rest-advisor-2025-01-01
    // The recommendationType entry includes the rich linkage fields we now project.
    // Other entities are present to confirm we ignore them.
    private const string SampleMetadataPayload = """
        {
          "value": [
            {
              "id": "providers/Microsoft.Advisor/metadata/recommendationType",
              "name": "recommendationType",
              "type": "Microsoft.Advisor/metadata",
              "properties": {
                "displayName": "Recommendation Type",
                "supportedValues": [
                  {
                    "id": "vm-rightsize",
                    "displayName": "Right-size or shutdown underutilized virtual machines",
                    "recommendationCategory": "Cost",
                    "recommendationImpact": "High",
                    "supportedResourceType": "microsoft.compute/virtualmachines",
                    "recommendationSubCategory": "UsageOptimization"
                  },
                  {
                    "id": "vm-backup",
                    "displayName": "Enable backups on virtual machines",
                    "recommendationCategory": "HighAvailability",
                    "recommendationImpact": "Medium",
                    "supportedResourceType": "microsoft.compute/virtualmachines",
                    "recommendationSubCategory": null
                  },
                  {
                    "id": "sql-tde",
                    "displayName": "Enable transparent data encryption",
                    "recommendationCategory": "Security",
                    "recommendationImpact": "High",
                    "supportedResourceType": "microsoft.sql/servers/databases",
                    "recommendationSubCategory": null
                  },
                  {
                    "id": "storage-soft-delete",
                    "displayName": "Enable soft delete on storage accounts",
                    "recommendationCategory": "OperationalExcellence",
                    "recommendationImpact": "Low",
                    "supportedResourceType": "microsoft.storage/storageaccounts",
                    "recommendationSubCategory": null
                  }
                ]
              }
            },
            {
              "id": "providers/Microsoft.Advisor/metadata/recommendationCategory",
              "name": "recommendationCategory",
              "type": "Microsoft.Advisor/metadata",
              "properties": {
                "displayName": "Category",
                "supportedValues": [
                  { "id": "Cost", "displayName": "Cost" },
                  { "id": "Performance", "displayName": "Performance" }
                ]
              }
            },
            {
              "id": "providers/Microsoft.Advisor/metadata/recommendationImpact",
              "name": "recommendationImpact",
              "type": "Microsoft.Advisor/metadata",
              "properties": {
                "displayName": "Impact",
                "supportedValues": [
                  { "id": "High", "displayName": "High" }
                ]
              }
            }
          ]
        }
        """;

    [Fact]
    public async Task ListRecommendationTypesAsync_NoFilters_ReturnsOnlyRecommendationTypeEntries()
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null, resourceType: null, impact: null, category: null,
            TestContext.Current.CancellationToken);

        // Only the 4 recommendationType supportedValues should be returned; entries from the
        // recommendationCategory and recommendationImpact entities are intentionally ignored.
        Assert.Equal(4, result.Count);
        Assert.All(result, r => Assert.NotNull(r.Category));
        Assert.All(result, r => Assert.NotNull(r.Impact));
        Assert.All(result, r => Assert.NotNull(r.ResourceType));
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_NoFilters_SortsHighMediumLow()
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null, resourceType: null, impact: null, category: null,
            TestContext.Current.CancellationToken);

        // High impacts come first, then Medium, then Low. Two Highs tie so secondary
        // sort by displayName (ascending) determines their order.
        Assert.Equal(["High", "High", "Medium", "Low"], result.Select(r => r.Impact));
        Assert.Equal("Enable transparent data encryption", result[0].DisplayName);
        Assert.Equal("Right-size or shutdown underutilized virtual machines", result[1].DisplayName);
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_ResourceTypeFilter_MatchesCaseInsensitively()
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null,
            resourceType: "MICROSOFT.COMPUTE/VIRTUALMACHINES",
            impact: null,
            category: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal("microsoft.compute/virtualmachines", r.ResourceType));
    }

    [Theory]
    [InlineData("High", 2)]
    [InlineData("high", 2)]
    [InlineData("Medium", 1)]
    [InlineData("Low", 1)]
    public async Task ListRecommendationTypesAsync_ImpactFilter_MatchesCaseInsensitively(string impact, int expectedCount)
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null, resourceType: null, impact, category: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedCount, result.Count);
    }

    [Theory]
    [InlineData("Cost", 1)]
    [InlineData("security", 1)]
    [InlineData("HighAvailability", 1)]
    [InlineData("nonexistent", 0)]
    public async Task ListRecommendationTypesAsync_CategoryFilter_MatchesCaseInsensitively(string category, int expectedCount)
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null, resourceType: null, impact: null, category,
            TestContext.Current.CancellationToken);

        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_CombinedFilters_AreAndedTogether()
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null,
            resourceType: "microsoft.compute/virtualmachines",
            impact: "High",
            category: "Cost",
            TestContext.Current.CancellationToken);

        Assert.Single(result);
        Assert.Equal("vm-rightsize", result[0].Id);
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_BrownfieldOnboarding_ReturnsTypesForResourceTypeSortedByImpact()
    {
        // The meeting outcome: when a customer onboards a new resource type, calling with
        // --resource-type returns all matching recommendations sorted by impact.
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null,
            resourceType: "microsoft.compute/virtualmachines",
            impact: null,
            category: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);
        Assert.Equal("High", result[0].Impact);
        Assert.Equal("Medium", result[1].Impact);
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_WhitespaceFilters_AreTreatedAsNoFilter()
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(
            tenant: null,
            resourceType: "   ",
            impact: "",
            category: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_NonSuccessResponse_ThrowsHttpRequestExceptionWithoutBody()
    {
        const string sensitiveBody = "{\"error\":{\"code\":\"InvalidAuthenticationToken\",\"message\":\"do not leak me\"}}";
        var service = CreateService(sensitiveBody, HttpStatusCode.Unauthorized, "Unauthorized");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ListRecommendationTypesAsync(
                tenant: null, resourceType: null, impact: null, category: null,
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.DoesNotContain("do not leak me", ex.Message);
        Assert.Contains("401", ex.Message);
    }

    private static AdvisorService CreateService(
        string responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? reasonPhrase = null)
    {
        var subscriptionService = Substitute.For<ISubscriptionService>();
        var tenantService = Substitute.For<ITenantService>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var logger = Substitute.For<ILogger<AdvisorService>>();

        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        tenantService.CloudConfiguration.Returns(cloudConfig);

        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1)));
        tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));

        var handler = new StubHttpHandler(responseBody, statusCode, reasonPhrase);
        // AdvisorService calls _httpClientFactory.CreateClient() (parameterless), which is an
        // extension method that delegates to CreateClient(Options.DefaultName) == "". Substitute
        // the interface member it ultimately dispatches to.
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler));

        return new AdvisorService(subscriptionService, tenantService, httpClientFactory, logger);
    }

    private sealed class StubHttpHandler(string body, HttpStatusCode statusCode, string? reasonPhrase) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
            if (reasonPhrase != null)
            {
                response.ReasonPhrase = reasonPhrase;
            }
            return Task.FromResult(response);
        }
    }
}
