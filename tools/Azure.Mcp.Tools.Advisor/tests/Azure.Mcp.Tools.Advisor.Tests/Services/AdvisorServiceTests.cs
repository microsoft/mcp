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

// Focused unit tests for the new filter-mapping + projection logic added in
// AdvisorService.ListRecommendationTypesAsync. The Advisor metadata endpoint is hit via
// IHttpClientFactory so we stub the handler with a canned ARM payload; the ARM token
// path is short-circuited by stubbing ITenantService.GetTokenCredentialAsync.
public class AdvisorServiceTests
{
    // Canned ARM metadata response containing one entity per dimension. Mirrors the shape
    // documented at https://learn.microsoft.com/rest/api/advisor/recommendation-metadata/list?view=rest-advisor-2025-01-01
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
                  { "id": "rt-1", "displayName": "Right-size VMs" },
                  { "id": "rt-2", "displayName": "Use reserved instances" }
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
                  { "id": "High", "displayName": "High" },
                  { "id": "Medium", "displayName": "Medium" },
                  { "id": "Low", "displayName": "Low" }
                ]
              }
            },
            {
              "id": "providers/Microsoft.Advisor/metadata/supportedResourceType",
              "name": "supportedResourceType",
              "type": "Microsoft.Advisor/metadata",
              "properties": {
                "displayName": "Resource Type",
                "supportedValues": [
                  { "id": "microsoft.compute/virtualmachines", "displayName": "Virtual machines" }
                ]
              }
            }
          ]
        }
        """;

    [Fact]
    public async Task ListRecommendationTypesAsync_NoFilter_ReturnsAllSupportedValuesAcrossDimensions()
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(tenant: null, filter: null, TestContext.Current.CancellationToken);

        // 2 (type) + 2 (category) + 3 (impact) + 1 (resourceType) = 8 entries
        Assert.Equal(8, result.Count);
    }

    [Theory]
    [InlineData("category", "Cost", "Performance")]
    [InlineData("impact", "High", "Medium", "Low")]
    [InlineData("recommendationType", "rt-1", "rt-2")]
    [InlineData("resourceType", "microsoft.compute/virtualmachines")]
    public async Task ListRecommendationTypesAsync_FriendlyFilter_TranslatesToArmEntityName(string filter, params string[] expectedIds)
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(tenant: null, filter, TestContext.Current.CancellationToken);

        Assert.Equal(expectedIds.Length, result.Count);
        Assert.Equal(expectedIds.OrderBy(x => x), result.Select(r => r.Id).OrderBy(x => x));
    }

    [Theory]
    [InlineData("CATEGORY")]
    [InlineData("Category")]
    [InlineData("cAtEgOrY")]
    public async Task ListRecommendationTypesAsync_FriendlyFilter_IsCaseInsensitive(string filter)
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(tenant: null, filter, TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);
    }

    [Theory]
    [InlineData("recommendationCategory")]
    [InlineData("RECOMMENDATIONCATEGORY")]
    public async Task ListRecommendationTypesAsync_RawArmEntityName_AlsoMatches(string filter)
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(tenant: null, filter, TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_UnknownFilter_ReturnsEmpty()
    {
        var service = CreateService(SampleMetadataPayload);

        var result = await service.ListRecommendationTypesAsync(tenant: null, filter: "bogus", TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListRecommendationTypesAsync_NonSuccessResponse_ThrowsHttpRequestExceptionWithoutBody()
    {
        const string sensitiveBody = "{\"error\":{\"code\":\"InvalidAuthenticationToken\",\"message\":\"do not leak me\"}}";
        var service = CreateService(sensitiveBody, HttpStatusCode.Unauthorized, "Unauthorized");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ListRecommendationTypesAsync(tenant: null, filter: null, TestContext.Current.CancellationToken));

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
