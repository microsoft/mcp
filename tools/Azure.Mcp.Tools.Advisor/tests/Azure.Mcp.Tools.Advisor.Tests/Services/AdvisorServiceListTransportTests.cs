// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public sealed class AdvisorServiceListTransportTests
{
    [Fact]
    public async Task ListRecommendationsAsync_SubscriptionScope_UsesSubscriptionTenantAndExactLimitIsNotTruncated()
    {
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method != HttpMethod.Get
                ? CreateResourceGraphResponse(CreateRecommendation("first"))
                : request.RequestUri!.AbsolutePath.Contains("/tenants", StringComparison.OrdinalIgnoreCase)
                    ? CreateTenantListResponse(tenantId)
                    : CreateSubscriptionResponse(tenantId, subscriptionId));
        var credential = CreateCredential();
        var armClient = CreateArmClient(credential, handler);
        var subscription = (await armClient
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId))
            .GetAsync(TestContext.Current.CancellationToken)).Value;
        var tenant = await CreateTenantResourceAsync(tenantId, credential, handler);
        handler.Reset();

        var azureService = Substitute.For<IAzureService>();
        azureService.GetSubscription(
            subscriptionId,
            null,
            Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var service = new AdvisorService(azureService);
        var result = await service.ListRecommendationsAsync(
            subscriptionId,
            resourceGroup: null,
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("first", Assert.Single(result.Results).Name);
        Assert.False(result.AreResultsTruncated);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.Contains($"subscriptionId =~ '{subscriptionId}'", query);
        Assert.Contains("join kind=leftouter", query);
        Assert.EndsWith("| limit 1", query);
        await azureService.Received(1).GetTenants(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListRecommendationsAsync_ServiceGroupScope_UsesTenantQueryAndPropagatesTruncation()
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method == HttpMethod.Get
                ? CreateTenantListResponse(tenantId)
                : CreateResourceGraphResponse(
                    resultTruncated: true,
                    CreateRecommendation("first")));
        var credential = CreateCredential();
        var tenant = await CreateTenantResourceAsync(tenantId, credential, handler);
        handler.Reset();

        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var service = new AdvisorService(azureService);
        var result = await service.ListRecommendationsAsync(
            subscription: null,
            resourceGroup: null,
            filters: new RecommendationFilters(ServiceGroup: "commerce", Prioritized: true),
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.AreResultsTruncated);
        Assert.Equal("first", Assert.Single(result.Results).Name);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.Contains(
            "tostring(properties.serviceGroupId) =~ '/providers/Microsoft.Management/serviceGroups/commerce'",
            query);
        Assert.Contains("extend properties = bag_merge(metadataOverrides, properties)", query);
        Assert.Contains(
            "order by metadataPriorityScore desc, metadataImpactRank asc, joinTypeId asc, todouble(properties.criticalityScore) desc, id asc",
            query);
        Assert.EndsWith("| limit 1", query);
    }

    private static async Task<TenantResource> CreateTenantResourceAsync(
        Guid tenantId,
        TokenCredential credential,
        CapturingHttpMessageHandler? handler = null)
    {
        handler ??= new CapturingHttpMessageHandler(_ => CreateTenantListResponse(tenantId));
        var armClient = CreateArmClient(credential, handler);
        await foreach (var tenant in armClient.GetTenants().GetAllAsync(TestContext.Current.CancellationToken))
        {
            return tenant;
        }

        throw new InvalidOperationException("The fake ARM response did not return a tenant.");
    }

    private static string GetResourceGraphQuery(string? requestBody)
    {
        using var request = JsonDocument.Parse(Assert.IsType<string>(requestBody));
        return request.RootElement.GetProperty("query").GetString()!;
    }

    private static object CreateRecommendation(string name) => new
    {
        id = $"/providers/Microsoft.Advisor/recommendations/{name}",
        name,
        type = "Microsoft.Advisor/recommendations",
        properties = new
        {
            category = "Cost",
            impact = "High",
            recommendationTypeId = "type-a",
            criticality = "Critical",
            criticalityScore = 0.9,
        },
    };

    private static HttpResponseMessage CreateTenantListResponse(Guid tenantId) =>
        CreateJsonResponse(new
        {
            value = new[]
            {
                new { id = $"/tenants/{tenantId}", tenantId = tenantId.ToString() },
            },
        });

    private static HttpResponseMessage CreateSubscriptionResponse(Guid tenantId, string subscriptionId) =>
        CreateJsonResponse(new
        {
            id = $"/subscriptions/{subscriptionId}",
            subscriptionId,
            displayName = "Test Subscription",
            tenantId,
            state = "Enabled",
        });

    private static HttpResponseMessage CreateResourceGraphResponse(params object[] recommendations) =>
        CreateResourceGraphResponse(resultTruncated: false, recommendations);

    private static HttpResponseMessage CreateResourceGraphResponse(
        bool resultTruncated,
        params object[] recommendations) =>
        CreateJsonResponse(new
        {
            totalRecords = recommendations.Length,
            count = recommendations.Length,
            resultTruncated = resultTruncated ? "true" : "false",
            data = recommendations,
        });

    private static HttpResponseMessage CreateJsonResponse(object payload) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
    };

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(
                new AccessToken("fake-arm-token", DateTimeOffset.UtcNow.AddHours(1))));
        return credential;
    }

    private static ArmClient CreateArmClient(TokenCredential credential, HttpMessageHandler handler)
    {
        var options = new ArmClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(handler, disposeHandler: false)),
        };
        options.Retry.MaxRetries = 0;
        return new ArmClient(credential, defaultSubscriptionId: null, options);
    }

    private sealed class CapturingHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public string? LastRequestBody { get; private set; }

        public void Reset() => LastRequestBody = null;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return responseFactory(request);
        }
    }
}
