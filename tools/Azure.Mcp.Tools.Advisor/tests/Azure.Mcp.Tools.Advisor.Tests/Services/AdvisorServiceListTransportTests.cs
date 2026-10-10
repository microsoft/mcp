// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
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
    [Theory]
    [InlineData(false, false, null)]
    [InlineData(false, false, false)]
    [InlineData(false, true, null)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, null)]
    [InlineData(true, false, false)]
    [InlineData(true, true, null)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public async Task ListRecommendationsAsync_SignalsAreOptInWithoutChangingResults(
        bool serviceGroupScope, bool prioritized, bool? showSignals)
    {
        using var signals = JsonDocument.Parse("""
            {
                "healthAlertCoverage": { "weightedScore": 0.025 },
                "serviceHealthEvents": { "weightedScore": 0 },
                "sgHaGoal": { "weightedScore": 0 }
            }
            """);
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method != HttpMethod.Get
                ? CreateResourceGraphResponse(
                    CreateRecommendation("with-signals", score: 0.55, tracked: true, signalBreakdown: signals.RootElement),
                    CreateRecommendation("without-signals", score: 0.9))
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
        azureService.GetSubscription(subscriptionId, null, Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        var service = new AdvisorService(azureService);
        var filters = new RecommendationFilters(
            ServiceGroup: serviceGroupScope ? "commerce" : null,
            Prioritized: prioritized,
            Category: "HighAvailability",
            Status: RecommendationStatus.Dismissed,
            ShowPrioritizationSignals: showSignals);

        var result = await service.ListRecommendationsAsync(
            serviceGroupScope ? null : subscriptionId, null, filters, top: 2,
            cancellationToken: TestContext.Current.CancellationToken);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        var baseline = await service.ListRecommendationsAsync(
            serviceGroupScope ? null : subscriptionId, null, filters with { ShowPrioritizationSignals = false }, top: 2,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(query, GetResourceGraphQuery(handler.LastRequestBody));
        Assert.Equal(baseline.Results.Select(row => row.Name), result.Results.Select(row => row.Name));
        Assert.Equal(["with-signals", "without-signals"], result.Results.Select(row => row.Name));
        Assert.Equal(baseline.AreResultsTruncated, result.AreResultsTruncated);
        Assert.False(result.AreResultsTruncated);
        Assert.All(baseline.Results, row => Assert.Null(row.Properties.SignalBreakdown));
        var payload = JsonSerializer.Serialize(
            new RecommendationListCommand.RecommendationListResult(result.Results, result.AreResultsTruncated),
            AdvisorJsonContext.Default.RecommendationListResult);
        using var response = JsonDocument.Parse(payload);
        foreach (var row in response.RootElement.GetProperty("recommendations").EnumerateArray())
        {
            var shouldInclude = showSignals == true && row.GetProperty("name").GetString() == "with-signals";
            Assert.Equal(shouldInclude, row.GetProperty("properties").TryGetProperty("signalBreakdown", out var actual));
            if (shouldInclude)
            {
                Assert.True(JsonElement.DeepEquals(signals.RootElement, actual));
            }
        }
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, false)]
    [InlineData(true, null)]
    [InlineData(true, false)]
    public async Task ListRecommendationsAsync_SignalsWithoutPrioritizationThrowsBeforeAzureAccess(
        bool serviceGroupScope, bool? prioritized)
    {
        var azureService = Substitute.For<IAzureService>();
        var service = new AdvisorService(azureService);
        var filters = new RecommendationFilters(
            ServiceGroup: serviceGroupScope ? "commerce" : null,
            Prioritized: prioritized,
            ShowPrioritizationSignals: true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ListRecommendationsAsync(
            serviceGroupScope ? null : "sub123", null, filters,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("filters", exception.ParamName);
        Assert.Contains("--show-prioritization-signals true requires --prioritized true", exception.Message);
        Assert.Empty(azureService.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, false, null)]
    [InlineData(false, true, null)]
    [InlineData(true, false, null)]
    [InlineData(true, true, null)]
    [InlineData(false, false, "Cost")]
    [InlineData(false, true, "Cost")]
    [InlineData(true, false, "Cost")]
    [InlineData(true, true, "Cost")]
    [InlineData(false, false, "Security")]
    [InlineData(false, true, "Security")]
    [InlineData(true, false, "Security")]
    [InlineData(true, true, "Security")]
    public async Task ListRecommendationsAsync_PreservesArgOrderWithoutClientSorting(
        bool serviceGroupScope, bool prioritized, string? category)
    {
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method != HttpMethod.Get
                ? CreateResourceGraphResponse(
                    CreateRecommendation("no-score", score: null, savings: 100, category: category ?? "Cost", tracked: true),
                    CreateRecommendation("low-score", score: 1, savings: 10, category: category ?? "Security", tracked: true),
                    CreateRecommendation("high-score", score: 2, savings: null, category: category ?? "HighAvailability"))
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
        azureService.GetSubscription(subscriptionId, null, Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorService(azureService).ListRecommendationsAsync(
            serviceGroupScope ? null : subscriptionId,
            resourceGroup: null,
            filters: new RecommendationFilters(
                ServiceGroup: serviceGroupScope ? "commerce" : null,
                Prioritized: prioritized,
                Category: category,
                RecommendationTypeId: "type-a"),
            top: 10,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["no-score", "low-score", "high-score"], result.Results.Select(row => row.Name));
        Assert.False(result.AreResultsTruncated);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.DoesNotContain("properties.tracked", query);
        Assert.DoesNotContain("!~ 'Security'", query);
        Assert.DoesNotContain("isnotnull(properties.criticalityScore)", query);
        Assert.DoesNotContain("metadataPriorityScore", query);
        Assert.Contains("tostring(properties.language) =~ 'en'", query);
        Assert.Contains("tostring(properties.recommendationTypeId) =~ 'type-a'", query);
        Assert.EndsWith(prioritized ? "| take 11 | project id, name, type, properties" : "| limit 11", query);
        Assert.Equal(prioritized, query.Contains("| order by", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public async Task ListRecommendationsAsync_MetadataFiltersUseOneJoinedQuery(
        bool serviceGroupScope,
        bool prioritized,
        bool emptyResults)
    {
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        var queryCount = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return request.RequestUri!.AbsolutePath.Contains("/tenants", StringComparison.OrdinalIgnoreCase)
                    ? CreateTenantListResponse(tenantId)
                    : CreateSubscriptionResponse(tenantId, subscriptionId);
            }

            queryCount++;
            return emptyResults ? CreateResourceGraphResponse() : CreateResourceGraphResponse(CreateRecommendation("first"));
        });
        var credential = CreateCredential();
        var armClient = CreateArmClient(credential, handler);
        var subscription = (await armClient
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId))
            .GetAsync(TestContext.Current.CancellationToken)).Value;
        var tenant = await CreateTenantResourceAsync(tenantId, credential, handler);
        handler.Reset();
        var azureService = Substitute.For<IAzureService>();
        azureService.GetSubscription(subscriptionId, null, Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorService(azureService).ListRecommendationsAsync(
            serviceGroupScope ? null : subscriptionId,
            resourceGroup: null,
            filters: new RecommendationFilters(
                ServiceGroup: serviceGroupScope ? "commerce" : null,
                Prioritized: prioritized,
                Category: "Cost",
                Impact: "High",
                ResourceType: "Microsoft.Storage/storageAccounts",
                SubCategory: "ServiceUpgradeAndRetirement",
                TrackingIds: ["QNY1-HB8", "9G0V-_G8"],
                RetirementDateOperator: "le",
                RetirementDate: new DateOnly(2026, 3, 31),
                Status: RecommendationStatus.Completed,
                Resource: "storage",
                Search: "encrypt",
                RecommendationTypeId: "type-a"),
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(emptyResults ? 0 : 1, result.Results.Count);
        Assert.False(result.AreResultsTruncated);
        Assert.Equal(1, queryCount);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.StartsWith("advisorresources | where type =~ 'Microsoft.Advisor/recommendations'", query);
        Assert.Contains("strlen(name) == 64", query);
        Assert.Contains("| join kind=inner (", query);
        Assert.Contains("tostring(properties.recommendationCategory) =~ 'Cost'", query);
        Assert.Contains("tostring(properties.recommendationImpact) =~ 'High'", query);
        Assert.DoesNotContain("tostring(properties.category) =~ 'Cost'", query);
        Assert.DoesNotContain("tostring(properties.impact) =~ 'High'", query);
        Assert.DoesNotContain("tostring(properties.recommendationTypeId) in~", query);
        Assert.Contains("tostring(properties.recommendationStatus) =~ 'Completed'", query);
        Assert.Contains("tostring(properties.recommendationTypeId) =~ 'type-a'", query);
        Assert.Contains("tostring(properties.resourceMetadata.resourceId) contains 'Microsoft.Storage/storageAccounts'", query);
        Assert.Contains("tostring(properties.resourceMetadata.resourceId) contains 'storage'", query);
        Assert.Contains("tostring(properties.shortDescription.problem) contains 'encrypt'", query);
        Assert.Contains("| distinct *", query);
        if (serviceGroupScope)
        {
            Assert.Contains("tostring(properties.serviceGroupId) =~ '/providers/Microsoft.Management/serviceGroups/commerce'", query);
            Assert.DoesNotContain("tostring(properties.serviceGroupId) in~", query);
            Assert.DoesNotContain("subscriptionId =~", query);
            await azureService.DidNotReceive().GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }
        else
        {
            Assert.Contains($"subscriptionId =~ '{subscriptionId}'", query);
            Assert.Contains("isempty(properties.serviceGroupId)", query);
        }

        Assert.EndsWith(prioritized ? "| take 2 | project id, name, type, properties" : "| limit 2", query);
    }

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
        Assert.EndsWith("| limit 2", query);
        await azureService.Received(1).GetTenants(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ListRecommendationsAsync_EmptyMetadataJoinStillValidatesResourceGroup(bool prioritized)
    {
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        var resourceGroupChecks = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("/resourcegroups/", StringComparison.OrdinalIgnoreCase))
            {
                resourceGroupChecks++;
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return request.Method != HttpMethod.Get
                ? CreateResourceGraphResponse()
                : request.RequestUri.AbsolutePath.Contains("/tenants", StringComparison.OrdinalIgnoreCase)
                    ? CreateTenantListResponse(tenantId)
                    : CreateSubscriptionResponse(tenantId, subscriptionId);
        });
        var credential = CreateCredential();
        var armClient = CreateArmClient(credential, handler);
        var subscription = (await armClient
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId))
            .GetAsync(TestContext.Current.CancellationToken)).Value;
        var tenant = await CreateTenantResourceAsync(tenantId, credential, handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetSubscription(subscriptionId, null, Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new AdvisorService(azureService).ListRecommendationsAsync(
                subscriptionId,
                resourceGroup: "missing-group",
                filters: new RecommendationFilters(Category: "Cost", Prioritized: prioritized),
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("Resource group 'missing-group' does not exist", exception.Message);
        Assert.Equal(1, resourceGroupChecks);
    }

    [Fact]
    public async Task ListRecommendationsAsync_SubscriptionScope_ExtraRowIsTrimmedAndMarkedTruncated()
    {
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method != HttpMethod.Get
                ? CreateResourceGraphResponse(CreateRecommendation("first"), CreateRecommendation("second"))
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
        Assert.True(result.AreResultsTruncated);
        Assert.EndsWith("| limit 2", GetResourceGraphQuery(handler.LastRequestBody));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(RecommendationStatus.New)]
    [InlineData(RecommendationStatus.Postponed)]
    [InlineData(RecommendationStatus.Dismissed)]
    [InlineData(RecommendationStatus.Completed)]
    public async Task ListRecommendationsAsync_ServiceGroupScope_PrioritizedUsesRequestedStatus(RecommendationStatus? status)
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method == HttpMethod.Get
                ? CreateTenantListResponse(tenantId)
                : CreateResourceGraphResponse(
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
            filters: new RecommendationFilters(ServiceGroup: "commerce", Prioritized: true, Status: status),
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.AreResultsTruncated);
        Assert.Equal("first", Assert.Single(result.Results).Name);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.Contains($"tostring(properties.recommendationStatus) =~ '{status ?? RecommendationStatus.New}'", query);
        Assert.Contains("strlen(name) == 64", query);
        Assert.DoesNotContain("properties.recommendationStatus == 'New'", query);
        Assert.DoesNotContain("suppressionIds", query);
        Assert.DoesNotContain("properties.tracked", query);
        Assert.DoesNotContain("!~ 'Security'", query);
        Assert.Contains("tostring(properties.language) =~ 'en'", query);
        if (status is not null and not RecommendationStatus.New)
        {
            Assert.DoesNotContain("'New'", query);
        }

        Assert.Contains(
            "tostring(properties.serviceGroupId) =~ '/providers/Microsoft.Management/serviceGroups/commerce'",
            query);
        Assert.Contains(
            "extend properties = iff(keepInstanceProperties, properties, bag_merge(metadataOverrides, properties))",
            query);
        Assert.Contains("metadataRetirementDate", query);
        Assert.DoesNotContain("| limit", query);
        Assert.EndsWith("| take 2 | project id, name, type, properties", query);
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

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ListRecommendationsAsync_PrioritizedRequestsTopPlusOneInOneQuery(bool metadataFilters, bool showSignals)
    {
        using var signals = JsonDocument.Parse("""{"healthAlertCoverage":{"weightedScore":0.025}}""");
        var tenantId = Guid.NewGuid();
        var pageCount = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }

            pageCount++;
            return CreateResourceGraphResponse(
                CreateRecommendation("first-from-arg", "a", 60, savings: 200, signalBreakdown: signals.RootElement),
                CreateRecommendation("extra-record", "a", 160, savings: 30, signalBreakdown: signals.RootElement));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        handler.Reset();
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorService(azureService).ListRecommendationsAsync(
            subscription: null,
            resourceGroup: null,
            filters: new RecommendationFilters(
                ServiceGroup: "commerce", Prioritized: true, Category: metadataFilters ? "Cost" : null,
                ShowPrioritizationSignals: showSignals),
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("first-from-arg", Assert.Single(result.Results).Name);
        if (showSignals)
        {
            Assert.True(JsonElement.DeepEquals(signals.RootElement, result.Results[0].Properties.SignalBreakdown!.Value));
        }
        else
        {
            Assert.Null(result.Results[0].Properties.SignalBreakdown);
        }
        Assert.True(result.AreResultsTruncated);
        Assert.Equal(1, pageCount);
        Assert.DoesNotContain("$skipToken", handler.LastRequestBody);
        Assert.DoesNotContain("| limit", GetResourceGraphQuery(handler.LastRequestBody));
        Assert.EndsWith("| take 2 | project id, name, type, properties", GetResourceGraphQuery(handler.LastRequestBody));
        Assert.Contains("strlen(name) == 64", GetResourceGraphQuery(handler.LastRequestBody));
        Assert.Contains(metadataFilters ? "| join kind=inner (" : "| join kind=leftouter (", GetResourceGraphQuery(handler.LastRequestBody));
    }

    [Fact]
    public async Task ListRecommendationsAsync_PrioritizedReportsArgTruncation()
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request => request.Method == HttpMethod.Get
            ? CreateTenantListResponse(tenantId)
            : CreateResourceGraphResponse(resultTruncated: true, CreateRecommendation("first")));
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorService(azureService).ListRecommendationsAsync(
            subscription: null,
            resourceGroup: null,
            filters: new RecommendationFilters(ServiceGroup: "commerce", Prioritized: true),
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("first", Assert.Single(result.Results).Name);
        Assert.True(result.AreResultsTruncated);
    }

    private static string GetResourceGraphQuery(string? requestBody)
    {
        using var request = JsonDocument.Parse(Assert.IsType<string>(requestBody));
        return request.RootElement.GetProperty("query").GetString()!;
    }

    private static object CreateRecommendation(
        string name,
        string typeId = "type-a",
        double? score = 0.9,
        double? savings = null,
        string category = "HighAvailability",
        bool tracked = false,
        JsonElement? signalBreakdown = null) => new
    {
        id = $"/providers/Microsoft.Advisor/recommendations/{name}",
        name,
        type = "Microsoft.Advisor/recommendations",
        properties = new
        {
            category,
            tracked,
            signalBreakdown,
            savings = new { retail = new { dailyPotentialSavings = savings } },
            impact = "High",
            recommendationTypeId = typeId,
            criticality = "Critical",
            criticalityScore = score,
            resourceMetadata = new { resourceId = $"/resources/{name}" },
            impactedValue = name,
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
