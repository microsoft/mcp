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
using Azure.Mcp.Tools.Advisor.Validation;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public sealed class AdvisorServiceListTransportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ServiceGroupDisplayName_IsResolvedBeforeListOrSummary(bool summary)
    {
        const string displayName = "Trey Research Shared Services";
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            return ++posts == 1
                ? CreateResourceGraphResponse(new
                {
                    id = "/providers/Microsoft.Management/serviceGroups/resolved-group",
                    name = displayName,
                })
                : summary
                    ? CreateResourceGraphResponse(new { key = "Cost", label = "Cost", count_ = 1 })
                    : CreateResourceGraphResponse(CreateRecommendation("rec-a"));
        }, resolveServiceGroupIds: false);
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        if (summary)
        {
            var result = await new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
                displayName, "category", cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(1, result.TotalRecommendations);
        }
        else
        {
            var result = await new AdvisorService(azureService).ListRecommendationsAsync(
                subscription: null, resourceGroup: null, new RecommendationFilters(ServiceGroup: displayName),
                cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("rec-a", Assert.Single(result.Results).Name);
        }

        Assert.Equal(2, posts);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.Contains("/serviceGroups/resolved-group", query);
        Assert.DoesNotContain(displayName, query);
    }

    [Theory]
    [InlineData("Trey Research Shared Services")]
    [InlineData("Finance")]
    [InlineData("Owner's Shared Services")]
    [InlineData("service-group-a")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/service-group-a")]
    public async Task ResolveServiceGroupScopeAsync_UsesTenantRootNameOrIdLookup(string serviceGroup)
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method == HttpMethod.Get
                ? CreateTenantListResponse(tenantId)
                : CreateResourceGraphResponse(new
                {
                    id = "/providers/Microsoft.Management/serviceGroups/service-group-a",
                    name = "Finance Shared Services",
                }), resolveServiceGroupIds: false);
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        azureService.ResolveTenantIdAsync("selected-tenant", Arg.Any<CancellationToken>()).Returns(tenantId.ToString());

        var result = await new AdvisorResourceGraphQueryExecutor(azureService).ResolveServiceGroupScopeAsync(
            serviceGroup, "selected-tenant", TestContext.Current.CancellationToken);

        Assert.Equal("service-group-a", result.Scope.ServiceGroup);
        Assert.Same(tenant, result.TenantResource);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.Contains("resourcecontainers", query);
        Assert.Contains("type =~ 'microsoft.management/servicegroups'", query);
        var literal = RecommendationQueryBuilder.EscapeKqlString(serviceGroup);
        Assert.Contains("serviceGroupId = tostring(id)", query);
        Assert.Contains("serviceGroupName = coalesce(tostring(properties.displayName), tostring(name))", query);
        Assert.Contains("parentServiceGroupId = tostring(properties.parent.resourceId)", query);
        Assert.Contains("strcat('/providers/Microsoft.Management/serviceGroups/', tostring(tenantId))", query);
        Assert.Contains("| where isnotempty(serviceGroupId)", query);
        Assert.Contains("| where parentServiceGroupId =~ tenantRootServiceGroupId", query);
        Assert.Contains($"serviceGroupId =~ '{literal}'", query);
        Assert.Contains($"name =~ '{literal}'", query);
        Assert.Contains($"serviceGroupName =~ '{literal}'", query);
        Assert.Contains("| project id = serviceGroupId, name = serviceGroupName", query);
        Assert.EndsWith("| order by name asc", query.TrimEnd());
        using var body = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.False(body.RootElement.TryGetProperty("subscriptions", out _));
        await azureService.DidNotReceive().GetSubscription(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Owner's Shared Services", "Owners Shared Services")]
    [InlineData("Owner''s Shared Services", "Owners Shared Services")]
    [InlineData("Finance | Operations", "Finance  Operations")]
    [InlineData("abc-def", "abc def")]
    [InlineData("abc def", "abc-def")]
    [InlineData("Finance' | where true | project name = 'Other", "Finance")]
    public async Task ResolveServiceGroupScopeAsync_PreservesPunctuationAndDoesNotSelectAnotherGroup(
        string displayName, string otherDisplayName)
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            var query = GetResourceGraphQuery(
                request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken).GetAwaiter().GetResult());
            var literal = System.Text.RegularExpressions.Regex.Match(
                query, @"\| where serviceGroupId =~ '((?:\\.|[^'\\])*)' or name =~");
            Assert.True(literal.Success);
            var requestedName = System.Text.RegularExpressions.Regex.Unescape(literal.Groups[1].Value);
            Assert.Equal(displayName, requestedName);
            var groups = new[]
            {
                new { id = "/providers/Microsoft.Management/serviceGroups/expected", name = "expected", displayName },
                new { id = "/providers/Microsoft.Management/serviceGroups/other", name = "other", displayName = otherDisplayName },
            };
            return CreateResourceGraphResponse(groups
                .Where(group => group.displayName.Equals(requestedName, StringComparison.OrdinalIgnoreCase))
                .Select(static group => new { group.id, name = group.displayName })
                .ToArray());
        }, resolveServiceGroupIds: false);
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorResourceGraphQueryExecutor(azureService).ResolveServiceGroupScopeAsync(
            displayName, null, TestContext.Current.CancellationToken);

        Assert.Equal("expected", result.Scope.ServiceGroup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResolveServiceGroupScopeAsync_CompletesPagingBeforeDecidingUniqueness(bool duplicateId)
    {
        var tenantId = Guid.NewGuid();
        var pages = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            pages++;
            return pages == 1
                ? CreateResourceGraphResponseWithSkipToken("scope-page-2", resultTruncated: true,
                    new { id = "/providers/Microsoft.Management/serviceGroups/group-a", name = "group-a" })
                : CreateResourceGraphResponse(new
                {
                    id = $"/providers/Microsoft.Management/serviceGroups/{(duplicateId ? "group-a" : "group-b")}",
                    name = duplicateId ? "group-a" : "group-b",
                });
        }, resolveServiceGroupIds: false);
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        var executor = new AdvisorResourceGraphQueryExecutor(azureService);

        if (duplicateId)
        {
            var result = await executor.ResolveServiceGroupScopeAsync(
                "Shared Services", null, TestContext.Current.CancellationToken);
            Assert.Equal("group-a", result.Scope.ServiceGroup);
        }
        else
        {
            var error = await Assert.ThrowsAsync<ArgumentException>(() =>
                executor.ResolveServiceGroupScopeAsync("Shared Services", null, TestContext.Current.CancellationToken));
            Assert.Contains("ambiguous", error.Message);
            Assert.Contains("/serviceGroups/group-a", error.Message);
            Assert.Contains("/serviceGroups/group-b", error.Message);
        }
        Assert.Equal(2, pages);
        Assert.Contains("scope-page-2", handler.LastRequestBody);
    }

    [Theory]
    [InlineData("empty", typeof(KeyNotFoundException))]
    [InlineData("truncated", typeof(InvalidOperationException))]
    [InlineData("invalid-id", typeof(JsonException))]
    [InlineData("invalid-id-suffix", typeof(JsonException))]
    [InlineData("missing-name", typeof(JsonException))]
    [InlineData("denied", typeof(RequestFailedException))]
    public async Task ResolveServiceGroupScopeAsync_RejectsUnresolvedOrIncompleteScope(string failure, Type expectedException)
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method == HttpMethod.Get
                ? CreateTenantListResponse(tenantId)
                : failure switch
                {
                    "empty" => CreateResourceGraphResponse(),
                    "truncated" => CreateResourceGraphResponse(true,
                        new { id = "/providers/Microsoft.Management/serviceGroups/group-a", name = "group-a" }),
                    "invalid-id" => CreateResourceGraphResponse(new { id = "/subscriptions/not-a-service-group", name = "group-a" }),
                    "invalid-id-suffix" => CreateResourceGraphResponse(new
                    {
                        id = "/providers/Microsoft.Management/serviceGroups/group-a/nested",
                        name = "Shared Services",
                    }),
                    "missing-name" => CreateResourceGraphResponse(new { id = "/providers/Microsoft.Management/serviceGroups/group-a" }),
                    "denied" => new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        Content = new StringContent("""{"error":{"code":"AuthorizationFailed","message":"Access denied."}}""",
                            Encoding.UTF8, "application/json"),
                    },
                    _ => throw new InvalidOperationException("Unexpected test failure case."),
                }, resolveServiceGroupIds: false);
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        await Assert.ThrowsAsync(expectedException, async () =>
        {
            await new AdvisorResourceGraphQueryExecutor(azureService).ResolveServiceGroupScopeAsync(
                "Shared Services", null, TestContext.Current.CancellationToken);
        });
    }

    [Theory]
    [InlineData("group-a")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/group-a")]
    public async Task ResolveServiceGroupScopeAsync_ExplicitIdWinsOverMatchingDisplayName(string serviceGroup)
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method == HttpMethod.Get
                ? CreateTenantListResponse(tenantId)
                : CreateResourceGraphResponse(
                    new { id = "/providers/Microsoft.Management/serviceGroups/group-a", name = "Finance Shared Services" },
                    new { id = "/providers/Microsoft.Management/serviceGroups/group-b", name = "group-a" }),
            resolveServiceGroupIds: false);
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorResourceGraphQueryExecutor(azureService).ResolveServiceGroupScopeAsync(
            serviceGroup, null, TestContext.Current.CancellationToken);

        Assert.Equal("group-a", result.Scope.ServiceGroup);
    }

    [Fact]
    public async Task ResolveServiceGroupScopeAsync_PropagatesCancellation()
    {
        var tenantId = Guid.NewGuid();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        }, resolveServiceGroupIds: false);
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new AdvisorResourceGraphQueryExecutor(azureService).ResolveServiceGroupScopeAsync(
                "Shared Services", null, cancellation.Token));
    }

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
            RecommendationTypeId: "type-a",
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
            RecommendationTypeId: "type-a",
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
            Assert.Contains("isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ '/providers/Microsoft.Management/serviceGroups/commerce'", query);
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
                filters: new RecommendationFilters(
                    Category: "Cost",
                    RecommendationTypeId: "type-a",
                    Prioritized: prioritized),
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
            filters: new RecommendationFilters(
                ServiceGroup: "commerce",
                RecommendationTypeId: "type-a",
                Prioritized: true,
                Status: status),
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
            "isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ '/providers/Microsoft.Management/serviceGroups/commerce'",
            query);
        Assert.Contains(
            "extend properties = iff(keepInstanceProperties, properties, bag_merge(metadataOverrides, properties))",
            query);
        Assert.Contains("metadataRetirementDate", query);
        Assert.DoesNotContain("| limit", query);
        Assert.EndsWith("| take 2 | project id, name, type, properties", query);
    }

    [Fact]
    public async Task ResolveSubscriptionsScopeAsync_BoundsConcurrencyAndPreservesNormalizedOrder()
    {
        var tenantId = Guid.NewGuid();
        var ids = Enumerable.Range(0, 9).Select(static _ => Guid.NewGuid().ToString()).ToArray();
        var handler = new CapturingHttpMessageHandler(request =>
            CreateSubscriptionResponse(tenantId, request.RequestUri!.AbsolutePath.Split('/').Last()));
        var credential = CreateCredential();
        var armClient = CreateArmClient(credential, handler);
        var subscriptions = new Dictionary<string, SubscriptionResource>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in ids)
        {
            subscriptions[id] = (await armClient
                .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(id))
                .GetAsync(TestContext.Current.CancellationToken)).Value;
        }
        var tenant = await CreateTenantResourceAsync(tenantId, credential);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        var firstWaveStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseOthers = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var othersCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var active = 0;
        var completed = 0;
        azureService.GetSubscription(Arg.Any<string>(), "selected-tenant", Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                var id = call.ArgAt<string>(0);
                var token = call.ArgAt<CancellationToken>(2);
                var count = Interlocked.Increment(ref active);
                try
                {
                    Assert.InRange(count, 1, 4);
                    if (count == 4)
                    {
                        firstWaveStarted.TrySetResult();
                    }
                    await (id == ids[0] ? releaseFirst.Task : releaseOthers.Task).WaitAsync(token);
                    if (id != ids[0] && Interlocked.Increment(ref completed) == ids.Length - 1)
                    {
                        othersCompleted.TrySetResult();
                    }
                    return subscriptions[id];
                }
                finally
                {
                    Interlocked.Decrement(ref active);
                }
            });

        var input = new[] { $" {ids[0]} ", ids[0].ToUpperInvariant(), " " }.Concat(ids.Skip(1)).ToArray();
        var resolution = new AdvisorResourceGraphQueryExecutor(azureService).ResolveSubscriptionsScopeAsync(
            input, null, "selected-tenant", TestContext.Current.CancellationToken);
        try
        {
            await firstWaveStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            Assert.Equal(4, Volatile.Read(ref active));
            releaseOthers.SetResult();
            await othersCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        }
        finally
        {
            releaseOthers.TrySetResult();
            releaseFirst.SetResult();
            await resolution;
        }

        var result = await resolution;
        Assert.Equal(ids, result.Scope.ScopedSubscriptionIds);
        Assert.Equal(ids.Select(id => subscriptions[id]), result.Subscriptions);
        Assert.Same(tenant, result.TenantResource);
        await azureService.Received(ids.Length).GetSubscription(
            Arg.Any<string>(), "selected-tenant", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveSubscriptionsScopeAsync_RejectsMixedTenants()
    {
        var ids = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        var tenantIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var handler = new CapturingHttpMessageHandler(request =>
        {
            var id = request.RequestUri!.AbsolutePath.Split('/').Last();
            return CreateSubscriptionResponse(tenantIds[Array.IndexOf(ids, id)], id);
        });
        var armClient = CreateArmClient(CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        foreach (var id in ids)
        {
            var subscription = (await armClient
                .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(id))
                .GetAsync(TestContext.Current.CancellationToken)).Value;
            azureService.GetSubscription(id, null, Arg.Any<CancellationToken>()).Returns(subscription);
        }

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AdvisorResourceGraphQueryExecutor(azureService).ResolveSubscriptionsScopeAsync(
                ids, null, null, TestContext.Current.CancellationToken));

        Assert.Contains("same Azure tenant", exception.Message);
        await azureService.DidNotReceive().GetTenants(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveSubscriptionsScopeAsync_PropagatesResolutionFailure()
    {
        var azureService = Substitute.For<IAzureService>();
        var failure = new UnauthorizedAccessException("Subscription access denied.");
        azureService.GetSubscription(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SubscriptionResource>(failure));

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new AdvisorResourceGraphQueryExecutor(azureService).ResolveSubscriptionsScopeAsync(
                ["subscription-a", "subscription-b"], null, null, TestContext.Current.CancellationToken));

        Assert.Same(failure, exception);
        await azureService.DidNotReceive().GetTenants(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveSubscriptionsScopeAsync_CancelsInFlightResolution()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var azureService = Substitute.For<IAzureService>();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        azureService.GetSubscription(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                started.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, call.ArgAt<CancellationToken>(2));
                throw new InvalidOperationException("Subscription resolution should have been canceled.");
            });

        var resolution = new AdvisorResourceGraphQueryExecutor(azureService).ResolveSubscriptionsScopeAsync(
            ["subscription-a", "subscription-b"], null, null, cancellation.Token);
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        }
        finally
        {
            await cancellation.CancelAsync();
        }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => resolution);
        await azureService.DidNotReceive().GetTenants(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListRecommendationsAsync_MultipleSubscriptionsUsesOnePagedQuery()
    {
        var tenantId = Guid.NewGuid();
        var subscriptionIds = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
        var postCount = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return request.RequestUri!.AbsolutePath.Contains("/tenants", StringComparison.OrdinalIgnoreCase)
                    ? CreateTenantListResponse(tenantId)
                    : CreateSubscriptionResponse(
                        tenantId,
                        request.RequestUri.AbsolutePath.Split('/').Last());
            }

            Interlocked.Increment(ref postCount);
            return CreateResourceGraphResponse(
                CreateRecommendation("subscription-one"),
                CreateRecommendation("subscription-two"));
        });
        var credential = CreateCredential();
        var armClient = CreateArmClient(credential, handler);
        var subscriptions = new List<SubscriptionResource>();
        foreach (var subscriptionId in subscriptionIds)
        {
            subscriptions.Add((await armClient
                .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId))
                .GetAsync(TestContext.Current.CancellationToken)).Value);
        }

        var tenant = await CreateTenantResourceAsync(tenantId, credential, handler);
        handler.Reset();
        var azureService = Substitute.For<IAzureService>();
        azureService.GetSubscription(subscriptionIds[0], null, Arg.Any<CancellationToken>())
            .Returns(subscriptions[0]);
        azureService.GetSubscription(subscriptionIds[1], null, Arg.Any<CancellationToken>())
            .Returns(subscriptions[1]);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorService(azureService).ListRecommendationsAsync(
            subscriptionIds,
            resourceGroup: null,
            filters: new RecommendationFilters(RecommendationTypeId: "type-a"),
            top: 10,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["subscription-one", "subscription-two"], result.Results.Select(row => row.Name));
        Assert.False(result.AreResultsTruncated);
        Assert.Equal(1, postCount);
        Assert.Contains(
            "subscriptionId in~ ('" + string.Join("', '", subscriptionIds) + "')",
            GetResourceGraphQuery(handler.LastRequestBody));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SubscriptionNameAndIdAliases_QueryTheCanonicalSubscriptionOnce(bool summary)
    {
        var tenantId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid().ToString();
        const string subscriptionName = "Shared Services";
        var postCount = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return request.RequestUri!.AbsolutePath.Contains("/tenants", StringComparison.OrdinalIgnoreCase)
                    ? CreateTenantListResponse(tenantId)
                    : CreateSubscriptionResponse(tenantId, subscriptionId);
            }
            postCount++;
            return summary
                ? CreateResourceGraphResponse(new { key = "Cost", label = "Cost", count_ = 1 })
                : CreateResourceGraphResponse(CreateRecommendation("rec-a"));
        });
        var credential = CreateCredential();
        var subscription = (await CreateArmClient(credential, handler)
            .GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscriptionId))
            .GetAsync(TestContext.Current.CancellationToken)).Value;
        var tenant = await CreateTenantResourceAsync(tenantId, credential, handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetSubscription(subscriptionName, null, Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetSubscription(subscriptionId, null, Arg.Any<CancellationToken>()).Returns(subscription);
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        string[] inputs = [subscriptionName, subscriptionId];

        if (summary)
        {
            var result = await new RecommendationSummaryService(azureService).SummarizeRecommendationsAsync(
                inputs, null, "category", cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(1, result.TotalRecommendations);
        }
        else
        {
            var result = await new AdvisorService(azureService).ListRecommendationsAsync(
                inputs, null, cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("rec-a", Assert.Single(result.Results).Name);
        }

        Assert.Equal(1, postCount);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.Contains($"subscriptionId =~ '{subscriptionId}'", query);
        Assert.DoesNotContain("subscriptionId in~", query);
        Assert.DoesNotContain(subscriptionName, query);
    }

    [Fact]
    public async Task ListRecommendationsAsync_SingleServiceGroupUsesOnePagedRecommendationQuery()
    {
        var tenantId = Guid.NewGuid();
        var postCount = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }

            Interlocked.Increment(ref postCount);
            return CreateResourceGraphResponse(
                CreateRecommendation("service-group-one"),
                CreateRecommendation("service-group-two"));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        handler.Reset();
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorService(azureService).ListRecommendationsAsync(
            subscription: null,
            resourceGroup: null,
            filters: new RecommendationFilters(
                ServiceGroup: "service-group-a",
                RecommendationTypeId: "type-a"),
            top: 10,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["service-group-one", "service-group-two"], result.Results.Select(row => row.Name));
        Assert.False(result.AreResultsTruncated);
        Assert.Equal(1, postCount);
        Assert.Equal(1, handler.ServiceGroupLookupCount);
        Assert.Contains(
            "isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ '/providers/Microsoft.Management/serviceGroups/service-group-a'",
            GetResourceGraphQuery(handler.LastRequestBody));
        Assert.DoesNotContain("tostring(properties.serviceGroupId) in~", GetResourceGraphQuery(handler.LastRequestBody));
    }

    [Fact]
    public async Task ListRecommendationsAsync_FollowsSkipTokenAcrossPages()
    {
        var tenantId = Guid.NewGuid();
        var page = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }

            page++;
            return page == 1
                ? CreateResourceGraphResponseWithSkipToken(
                    "next-page",
                    resultTruncated: true,
                    CreateRecommendation("first-page"))
                : CreateResourceGraphResponse(CreateRecommendation("second-page"));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        handler.Reset();
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new AdvisorService(azureService).ListRecommendationsAsync(
            subscription: null,
            resourceGroup: null,
            filters: new RecommendationFilters(ServiceGroup: "service-group-a", RecommendationTypeId: "type-a"),
            top: 10,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["first-page", "second-page"], result.Results.Select(row => row.Name));
        Assert.False(result.AreResultsTruncated);
        Assert.Equal(2, page);
    }

    [Fact]
    public async Task LargeSubscriptionScope_UsesOneGloballyRankedQuery()
    {
        var tenantId = Guid.NewGuid();
        var postCount = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }

            Interlocked.Increment(ref postCount);
            return CreateResourceGraphResponse(CreateRecommendation($"batch-{postCount}"));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var ids = Enumerable.Range(0, 51)
            .Select(static index => $"subscription-{index}")
            .ToArray();
        var scope = RecommendationQueryScope.ForSubscriptions(ids, null);

        var query = RecommendationSummaryService.BuildSummaryQuery(
            scope,
            RecommendationSummaryService.GroupByRecommendationType,
            new RecommendationFilters(Prioritized: true));
        var result = await AdvisorResourceGraphQueryExecutor.ExecutePagedAsync(
            tenant,
            query,
            TestContext.Current.CancellationToken);

        Assert.Equal(1, postCount);
        Assert.Single(result.Rows);
        Assert.All(ids, id => Assert.Contains(id, GetResourceGraphQuery(handler.LastRequestBody)));
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(query, @"\| order by "));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SingleServiceGroupScope_PreservesRankedResults(bool summary)
    {
        var tenantId = Guid.NewGuid();
        var postCount = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            Interlocked.Increment(ref postCount);
            return summary
                ? CreateResourceGraphResponse(new { key = "type-a", label = "A", count_ = 51, annualSavings = 510 })
                : CreateResourceGraphResponse(CreateRecommendation("highest-from-last-group"), CreateRecommendation("extra"));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        const string serviceGroup = "service-group-test";
        var filters = new RecommendationFilters(ServiceGroup: serviceGroup, RecommendationTypeId: "type-a", Prioritized: true);

        if (summary)
        {
            var result = await new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
                serviceGroup, "recommendation-type", filters, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(51, result.TotalRecommendations);
            Assert.Equal(510, Assert.Single(result.Groups).PrioritizationSignals!.AnnualSavings);
        }
        else
        {
            var result = await new AdvisorService(azureService).ListRecommendationsAsync(
                subscription: null, resourceGroup: null, filters, top: 1,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal("highest-from-last-group", Assert.Single(result.Results).Name);
            Assert.True(result.AreResultsTruncated);
        }

        Assert.Equal(1, postCount);
        Assert.Contains(
            $"/providers/Microsoft.Management/serviceGroups/{serviceGroup}'", GetResourceGraphQuery(handler.LastRequestBody));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task CostSummary_SubscriptionsOrSingleServiceGroupPagesTheFullReport(bool prioritized, bool subscriptionScope)
    {
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return request.RequestUri!.AbsolutePath.Contains("/tenants", StringComparison.OrdinalIgnoreCase)
                    ? CreateTenantListResponse(tenantId)
                    : CreateSubscriptionResponse(tenantId, request.RequestUri.AbsolutePath.Split('/')[2]);
            }
            posts++;
            return posts switch
            {
                1 => CreateResourceGraphResponseWithSkipToken(
                    "cost-page-2", resultTruncated: true,
                    CreateSummaryCostRow("Completed", 51, potential: null, realized: 510, prioritized: prioritized)),
                2 => CreateResourceGraphResponse(
                    CreateSummaryCostRow("Completed", 1, potential: null, realized: 30, "type-b", prioritized)),
                _ => throw new InvalidOperationException("Unexpected extra Resource Graph request."),
            };
        });
        var credential = CreateCredential();
        var tenant = await CreateTenantResourceAsync(tenantId, credential, handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        var ids = Enumerable.Range(0, subscriptionScope ? 51 : 1).Select(static _ => Guid.NewGuid().ToString()).ToArray();
        if (subscriptionScope)
        {
            var armClient = CreateArmClient(credential, handler);
            foreach (var id in ids)
            {
                var subscription = (await armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(id))
                    .GetAsync(TestContext.Current.CancellationToken)).Value;
                azureService.GetSubscription(id, null, Arg.Any<CancellationToken>()).Returns(subscription);
            }
        }
        var from = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero);
        var filters = new RecommendationFilters(
            Category: "Cost", SubCategory: "IdleResources", Prioritized: prioritized,
            SavingsAsOf: to, SavingsFrom: from, SavingsTo: to);
        var service = new RecommendationSummaryService(azureService);
        var result = subscriptionScope
            ? await service.SummarizeRecommendationsAsync(
                ids, null, "recommendation-type", filters, cancellationToken: TestContext.Current.CancellationToken)
            : await service.SummarizeServiceGroupRecommendationsAsync(
                ids[0], "recommendation-type", filters, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(52, result.TotalRecommendations);
        Assert.Equal(2, result.Groups.Count);
        var cost = Assert.IsType<RecommendationCostSummary>(result.Cost);
        Assert.Equal(52, cost.RecommendationCount);
        Assert.Equal(0, cost.BillReducingOpportunityCount);
        Assert.Equal(0, cost.ZeroSavingsCount);
        Assert.Equal(0, cost.UnavailablePricingCount);
        var totals = Assert.Single(cost.CurrencyTotals);
        Assert.Equal("USD", totals.Currency);
        Assert.Null(totals.PotentialSavings);
        Assert.Equal(540m, totals.RealizedSavings);
        Assert.Equal(52, totals.RecommendationCount);
        Assert.Equal(52, Assert.Single(cost.StatusTotals!).RecommendationCount);
        Assert.Equal("Completed", Assert.Single(cost.StatusTotals!).Status);
        Assert.Equal(to, cost.AsOfUtc);
        Assert.Equal(from, cost.FromUtc);
        Assert.Equal(to, cost.ToUtc);
        Assert.Equal(2, posts);
        Assert.Contains("cost-page-2", handler.LastRequestBody);
        var query = GetResourceGraphQuery(handler.LastRequestBody);
        Assert.All(ids, id => Assert.Contains(id, query));
        Assert.Contains(RecommendationQueryBuilder.BuildSavingsValues(filters), query);
        Assert.Contains("tostring(properties.recommendationStatus) =~ 'Completed'", query);
        Assert.DoesNotContain(RecommendationQueryBuilder.ActiveRecommendationClause, query);
        Assert.DoesNotContain("| take ", query);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Security")]
    [InlineData("Cost")]
    public async Task NonCostSummary_DoesNotRunACostQuery(string? category)
    {
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            posts++;
            return CreateResourceGraphResponse(new { key = "Security", label = "Security", count_ = 1 });
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var result = await new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
            "group-a", "category", new RecommendationFilters(Category: category),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(result.Cost);
        Assert.Equal(1, posts);
        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(result, AdvisorJsonContext.Default.RecommendationSummary));
        Assert.False(payload.RootElement.TryGetProperty("cost", out _));
    }

    [Fact]
    public async Task EmptyCostSummary_ReturnsZeroTotalsAndTheCalculationWindowWithoutAnExtraQuery()
    {
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            posts++;
            return CreateResourceGraphResponse();
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);
        var asOf = new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.Zero);

        var result = await new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
            "group-a", "category", new RecommendationFilters(Category: "Cost", SubCategory: "IdleResources", SavingsAsOf: asOf),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(result.Groups);
        var cost = Assert.IsType<RecommendationCostSummary>(result.Cost);
        Assert.Equal(0, cost.RecommendationCount);
        Assert.Empty(cost.CurrencyTotals);
        Assert.Equal(asOf, cost.AsOfUtc);
        Assert.Equal(1, posts);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CostSummary_TruncationFailsInsteadOfReturningIncompleteTotals(bool prioritized)
    {
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            posts++;
            return CreateResourceGraphResponse(
                resultTruncated: true, CreateSummaryCostRow("New", 1, 100, null, prioritized: prioritized));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
                "group-a", prioritized ? "recommendation-type" : "category",
                new RecommendationFilters(Category: "Cost", SubCategory: "IdleResources", Prioritized: prioritized),
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("truncated summary", exception.Message);
        Assert.Equal(1, posts);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    public async Task CostSummary_InvalidPageDataFailsInsteadOfReturningZeroTotals(string payload)
    {
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            posts++;
            return CreateJsonResponse(new
            {
                totalRecords = 1,
                count = 1,
                resultTruncated = "false",
                data = JsonSerializer.Deserialize<JsonElement>(payload),
            });
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        await Assert.ThrowsAsync<JsonException>(() =>
            new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
                "group-a", "category", new RecommendationFilters(Category: "Cost", SubCategory: "IdleResources"),
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(1, posts);
    }

    [Fact]
    public async Task CostSummary_MissingPrioritizedReportRowsFailsInsteadOfReturningZeroTotals()
    {
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            posts++;
            return CreateResourceGraphResponse(new
            {
                key = "type-a",
                label = "A",
                count_ = 1,
                costSummaryRows = Array.Empty<object>(),
            });
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var exception = await Assert.ThrowsAsync<JsonException>(() =>
            new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
                "group-a", "recommendation-type",
                new RecommendationFilters(Category: "Cost", SubCategory: "IdleResources", Prioritized: true),
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("inconsistent", exception.Message);
        Assert.Equal(1, posts);
    }

    [Theory]
    [InlineData(RecommendationStatus.New)]
    [InlineData(RecommendationStatus.Postponed)]
    [InlineData(RecommendationStatus.Dismissed)]
    [InlineData(RecommendationStatus.Completed)]
    [InlineData(null)]
    public async Task IdleResourceSummary_PreservesNormalCountOrderWithLifecycleCostTotals(RecommendationStatus? status)
    {
        var resolvedStatus = status ?? RecommendationStatus.Completed;
        var completed = resolvedStatus == RecommendationStatus.Completed;
        var tenantId = Guid.NewGuid();
        var posts = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }
            posts++;
            return CreateResourceGraphResponse(
                CreateSummaryCostRow(resolvedStatus.ToString(), 20, completed ? null : 10, completed ? 10 : null, "type-low"),
                CreateSummaryCostRow(resolvedStatus.ToString(), 10, null, null, "type-missing"),
                CreateSummaryCostRow(resolvedStatus.ToString(), 2, completed ? null : 0, completed ? 0 : null, "type-zero"),
                CreateSummaryCostRow(resolvedStatus.ToString(), 1, completed ? null : 100, completed ? 100 : null, "type-high"));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);
        var azureService = Substitute.For<IAzureService>();
        azureService.GetTenants(Arg.Any<CancellationToken>()).Returns([tenant]);

        var summary = await new RecommendationSummaryService(azureService).SummarizeServiceGroupRecommendationsAsync(
            "group-a", "recommendation-type",
            new RecommendationFilters(
                Category: "Cost", SubCategory: "IdleResources", Status: status,
                SavingsAsOf: status is null ? new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.Zero) : null),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["type-low", "type-missing", "type-zero", "type-high"],
            summary.Groups.Select(static group => group.Key));
        Assert.Equal([20, 10, 2, 1], summary.Groups.Select(static group => group.Count));
        Assert.Equal(33, summary.TotalRecommendations);
        Assert.Equal(4, summary.TotalRecommendationGroupCount);
        Assert.Equal(1, posts);
        Assert.Equal(33, summary.Cost!.RecommendationCount);
        Assert.Equal(["type-high", "type-low", "type-zero", "type-missing"],
            summary.Cost!.TypeTotals!.Select(static item => item.RecommendationTypeId));
    }

    private static Dictionary<string, object?> CreateSummaryCostRow(
        string status, int count, double? potential, double? realized, string typeId = "type-a", bool prioritized = false)
    {
        var costRow = CreateCostAggregateRow(status, count, potential, realized, typeId);
        var row = prioritized ? new Dictionary<string, object?> { ["costSummaryRows"] = new[] { costRow } } : costRow;
        row["key"] = typeId;
        row["label"] = typeId;
        row["count_"] = count;
        return row;
    }

    private static Dictionary<string, object?> CreateCostAggregateRow(string status, int count, double? potential, double? realized,
        string typeId = "type-a") => new()
        {
            ["recommendationStatus"] = status,
            ["recommendationTypeId"] = typeId,
            ["recommendationName"] = typeId,
            ["potentialSavings"] = potential,
            ["accumulatedWaste"] = null,
            ["realizedSavings"] = realized,
            ["recommendationCount"] = count,
            ["potentialCount"] = potential is null ? 0 : count,
            ["costCount"] = 0,
            ["realizedCount"] = realized is null ? 0 : count,
            ["billReducingOpportunityCount"] = potential > 0 ? count : 0,
            ["zeroSavingsCount"] = potential <= 0 ? count : 0,
            ["unavailablePricingCount"] = potential is null && realized is null ? count : 0,
        };

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
                ServiceGroup: "commerce",
                RecommendationTypeId: "type-a",
                Prioritized: true,
                Category: metadataFilters ? "Cost" : null,
                ShowPrioritizationSignals: showSignals),
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("first-from-arg", Assert.Single(result.Results).Name);
        if (showSignals)
        {
            var signalBreakdown = result.Results[0].Properties.SignalBreakdown;
            Assert.Equal(
                0.025,
                signalBreakdown!.Value.GetProperty("healthAlertCoverage").GetProperty("weightedScore").GetDouble());
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
            filters: new RecommendationFilters(
                ServiceGroup: "commerce",
                RecommendationTypeId: "type-a",
                Prioritized: true),
            top: 1,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("first", Assert.Single(result.Results).Name);
        Assert.True(result.AreResultsTruncated);
    }

    [Fact]
    public async Task ExecutePagedAsync_RepeatedSkipToken_ThrowsInsteadOfLooping()
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
            request.Method == HttpMethod.Get
                ? CreateTenantListResponse(tenantId)
                : CreateResourceGraphResponseWithSkipToken(
                    "repeated-token",
                    recommendations: [CreateRecommendation("first")]));
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AdvisorResourceGraphQueryExecutor.ExecutePagedAsync(
                tenant,
                "advisorresources",
                TestContext.Current.CancellationToken));

        Assert.Contains("repeated skip token", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecutePagedAsync_UsesTerminalPageTruncationState()
    {
        var tenantId = Guid.NewGuid();
        var page = 0;
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }

            page++;
            return page == 1
                ? CreateResourceGraphResponseWithSkipToken(
                    "next-page",
                    resultTruncated: true,
                    CreateRecommendation("first"))
                : CreateResourceGraphResponse(CreateRecommendation("last"));
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);

        var result = await AdvisorResourceGraphQueryExecutor.ExecutePagedAsync(
            tenant,
            "advisorresources",
            TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Rows.Count);
        Assert.False(result.IsTruncated);
    }

    [Fact]
    public async Task ExecutePagedAsync_TenantSubscriptionLimitHit_ThrowsInsteadOfReturningPartialResults()
    {
        var tenantId = Guid.NewGuid();
        var handler = new CapturingHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return CreateTenantListResponse(tenantId);
            }

            var response = CreateResourceGraphResponse(CreateRecommendation("first"));
            response.Headers.Add(
                AdvisorResourceGraphQueryExecutor.TenantSubscriptionLimitHitHeader,
                "true");
            return response;
        });
        var tenant = await CreateTenantResourceAsync(tenantId, CreateCredential(), handler);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AdvisorResourceGraphQueryExecutor.ExecutePagedAsync(
                tenant,
                "advisorresources",
                TestContext.Current.CancellationToken));

        Assert.Contains("10,000 accessible subscriptions", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ListRecommendationsAsync_PrioritizedWithoutRecommendationTypeThrowsBeforeAzureAccess(
        bool serviceGroupScope)
    {
        var azureService = Substitute.For<IAzureService>();
        var service = new AdvisorService(azureService);
        var filters = new RecommendationFilters(
            ServiceGroup: serviceGroupScope ? "commerce" : null,
            Prioritized: true);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ListRecommendationsAsync(
            serviceGroupScope ? null : "sub123",
            null,
            filters,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("filters", exception.ParamName);
        Assert.Contains("--prioritized true requires --recommendation-type-id", exception.Message);
        Assert.Empty(azureService.ReceivedCalls());
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

    private static HttpResponseMessage CreateResourceGraphResponseWithSkipToken(
        string skipToken,
        bool resultTruncated = false,
        params object[] recommendations) =>
        CreateJsonResponse(new Dictionary<string, object?>
        {
            ["totalRecords"] = recommendations.Length,
            ["count"] = recommendations.Length,
            ["resultTruncated"] = resultTruncated ? "true" : "false",
            ["$skipToken"] = skipToken,
            ["data"] = recommendations,
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
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory,
        bool resolveServiceGroupIds = true) : HttpMessageHandler
    {
        public string? LastRequestBody { get; private set; }
        public int ServiceGroupLookupCount { get; private set; }

        public void Reset() => LastRequestBody = null;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            if (resolveServiceGroupIds && LastRequestBody is not null)
            {
                var query = GetResourceGraphQuery(LastRequestBody);
                if (query.StartsWith("resourcecontainers", StringComparison.Ordinal))
                {
                    ServiceGroupLookupCount++;
                    var match = System.Text.RegularExpressions.Regex.Match(query, @"\| where serviceGroupId =~ '([^']+)'");
                    Assert.True(match.Success);
                    var name = match.Groups[1].Value;
                    RecommendationScopeValidator.ThrowIfInvalidServiceGroupId(name);
                    return CreateResourceGraphResponse(new
                    {
                        id = RecommendationQueryBuilder.ServiceGroupResourceIdPrefix + name,
                        name,
                    });
                }
            }
            return responseFactory(request);
        }
    }
}
