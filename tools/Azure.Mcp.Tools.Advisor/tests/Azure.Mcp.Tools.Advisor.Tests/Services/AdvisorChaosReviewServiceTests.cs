// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;
using AdvisorRecommendation = Azure.Mcp.Tools.Advisor.Models.Recommendation;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorChaosReviewServiceTests
{
    private static readonly Guid RecommendationTypeId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string SubscriptionId =
        "22222222-2222-2222-2222-222222222222";
    private const string Resource =
        "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Compute/virtualMachineScaleSets/vmss";
    private const string Workspace =
        "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Chaos/workspaces/advisor-chaos";
    private const string Scenario = Workspace + "/scenarios/ComputeZoneDown";
    private const string Configuration = Scenario + "/configurations/compute-zone-down";
    private const string PrincipalId =
        "33333333-3333-3333-3333-333333333333";
    private const string RunId =
        "44444444-4444-4444-4444-444444444444";

    [Fact]
    public async Task ReviewChaosRemediationAsync_ReturnsReadyReview()
    {
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            WorkspaceListResponse(WorkspaceDefinition()),
            WorkspaceResponse(WorkspaceDefinition()),
            ScenarioListResponse(),
            RunListResponse(),
            ConfigurationListResponse(),
            ValidationResponse("Succeeded"));
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Ready", result.Status);
        Assert.True(result.Ready);
        Assert.False(result.MutationPerformed);
        Assert.Equal(Resource, result.Target.ResourceId);
        Assert.Equal(Workspace, result.Workspace?.Id);
        Assert.Equal(Scenario, result.Scenario?.Id);
        Assert.Equal(Configuration, result.Configuration?.Id);
        Assert.Equal("Succeeded", result.Validation?.Status);
        Assert.Equal(7, handler.Requests.Count);
        Assert.All(handler.Requests, request => Assert.Equal(HttpMethod.Get, request.Method));
        Assert.All(
            handler.Requests,
            request => Assert.Equal("Bearer", request.Authorization?.Scheme));
        Assert.Contains(
            handler.Requests,
            request => request.Uri.AbsolutePath.EndsWith(
                "/validations/latest",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_VerifiesExactActiveAdvisorRecommendation()
    {
        RecommendationFilters? capturedFilters = null;
        var advisorService = CreateAdvisorService();
        advisorService.ListRecommendationsAsync(
                SubscriptionId,
                "rg",
                Arg.Do<RecommendationFilters?>(filters => capturedFilters = filters),
                100,
                null,
                Arg.Any<CancellationToken>())
            .Returns(MatchingRecommendationResults());
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            WorkspaceListResponse(WorkspaceDefinition()),
            WorkspaceResponse(WorkspaceDefinition()),
            ScenarioListResponse(),
            RunListResponse(),
            ConfigurationListResponse(),
            ValidationResponse("Succeeded"));
        var service = CreateService(handler, advisorService: advisorService);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Ready", result.Status);
        Assert.NotNull(capturedFilters);
        Assert.Equal(RecommendationStatus.New, capturedFilters!.Status);
        Assert.Equal(
            RecommendationTypeId.ToString("D"),
            capturedFilters.RecommendationTypeId);
        Assert.Equal(Resource, capturedFilters.Resource);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_BlocksWhenExactAdvisorRecommendationIsMissing()
    {
        var advisorService = CreateAdvisorService(new([], false));
        var handler = new QueueHttpMessageHandler();
        var service = CreateService(handler, advisorService: advisorService);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.Status);
        Assert.Equal("AdvisorRecommendationNotFound", result.ReasonCode);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_FailsClosedWhenAdvisorResultsAreTruncated()
    {
        var advisorService = CreateAdvisorService(new([], true));
        var handler = new QueueHttpMessageHandler();
        var service = CreateService(handler, advisorService: advisorService);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.Status);
        Assert.Equal(
            "AdvisorRecommendationVerificationIncomplete",
            result.ReasonCode);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_ReturnsSetupRequiredWhenNoWorkspaceCoversTarget()
    {
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            JsonResponse(new { value = Array.Empty<object>() }));
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("SetupRequired", result.Status);
        Assert.Equal("CoveringWorkspaceNotFound", result.ReasonCode);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_ReturnsSortedWorkspaceCandidatesWhenSelectionIsAmbiguous()
    {
        const string secondWorkspace =
            "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/shared/providers/Microsoft.Chaos/workspaces/advisor-chaos-shared";
        var second = WorkspaceDefinition(
            secondWorkspace,
            "advisor-chaos-shared");
        var first = WorkspaceDefinition();
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            WorkspaceListResponse(second, first),
            WorkspaceResponse(second),
            WorkspaceResponse(first));
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("SelectionRequired", result.Status);
        Assert.Equal("WorkspaceSelectionRequired", result.ReasonCode);
        Assert.Equal(2, result.WorkspaceCandidates.Count);
        Assert.Equal(
            result.WorkspaceCandidates.OrderBy(
                candidate => candidate.Id,
                StringComparer.OrdinalIgnoreCase),
            result.WorkspaceCandidates);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_ReturnsPermissionsRequiredFromValidation()
    {
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            WorkspaceListResponse(WorkspaceDefinition()),
            WorkspaceResponse(WorkspaceDefinition()),
            ScenarioListResponse(),
            RunListResponse(),
            ConfigurationListResponse(),
            ValidationResponse(
                "RequiresAttention",
                permissionErrors: 1,
                resourceErrors: 0));
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("PermissionsRequired", result.Status);
        Assert.Equal("ScenarioPermissionsRequired", result.ReasonCode);
        Assert.Equal(1, result.Validation?.PermissionErrorCount);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_ReturnsRunningWhenActiveRunExists()
    {
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            WorkspaceListResponse(WorkspaceDefinition()),
            WorkspaceResponse(WorkspaceDefinition()),
            ScenarioListResponse(),
            RunListResponse(RunDefinition("Running", Resource)),
            ConfigurationListResponse());
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.Equal("ActiveRunExists", result.ReasonCode);
        Assert.Single(result.Runs);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_FailsClosedWhenActiveRunEscapesTarget()
    {
        const string otherResource =
            "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/other/providers/Microsoft.Compute/virtualMachineScaleSets/other";
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            WorkspaceListResponse(WorkspaceDefinition()),
            WorkspaceResponse(WorkspaceDefinition()),
            ScenarioListResponse(),
            RunListResponse(RunDefinition("Running", otherResource)),
            ConfigurationListResponse());
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Blocked", result.Status);
        Assert.Equal("RunScopeMismatch", result.ReasonCode);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_RejectsPaginationOutsideArmCollection()
    {
        var handler = new QueueHttpMessageHandler(
            VmssResponse(),
            JsonResponse(new
            {
                value = new[] { WorkspaceDefinition() },
                nextLink = "https://example.com/subscriptions/steal",
            }));
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.Status);
        Assert.Equal("InvalidWorkspaceResponse", result.ReasonCode);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_ReturnsSubscriptionMismatchWithoutAzureReads()
    {
        var handler = new QueueHttpMessageHandler();
        var service = CreateService(
            handler,
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var result = await service.ReviewChaosRemediationAsync(
            "other-subscription",
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.Status);
        Assert.Equal("SubscriptionMismatch", result.ReasonCode);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ReviewChaosRemediationAsync_ReturnsRequiredPermissionForVmssForbidden()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Forbidden));
        var service = CreateService(handler);

        var result = await service.ReviewChaosRemediationAsync(
            SubscriptionId,
            RecommendationTypeId,
            Resource,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Blocked", result.Status);
        Assert.Equal("CustomerReadAccessDenied", result.ReasonCode);
        Assert.Equal(
            "Microsoft.Compute/virtualMachineScaleSets/read",
            result.RequiredPermission);
    }

    private static AdvisorChaosReviewService CreateService(
        QueueHttpMessageHandler handler,
        string subscriptionId = SubscriptionId,
        IAdvisorService? advisorService = null)
    {
        var azureService = Substitute.For<IAzureService>();
        var cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloud);

        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(
                Arg.Any<TokenRequestContext>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(
                new AccessToken(
                    "test-token",
                    DateTimeOffset.UtcNow.AddHours(1))));
        azureService.GetTokenCredentialAsync(
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));

        var subscription = Substitute.For<SubscriptionResource>();
        subscription.Id.Returns(
            SubscriptionResource.CreateResourceIdentifier(subscriptionId));
        azureService.GetSubscription(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(subscription));
        azureService.GetClient(Arg.Any<string?>())
            .Returns(new HttpClient(handler));

        advisorService ??= CreateAdvisorService();
        return new(
            azureService,
            advisorService,
            NullLogger<AdvisorChaosReviewService>.Instance);
    }

    private static IAdvisorService CreateAdvisorService(
        ResourceQueryResults<AdvisorRecommendation>? recommendations = null)
    {
        var advisorService = Substitute.For<IAdvisorService>();
        advisorService.ListRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RecommendationFilters?>(),
                Arg.Any<int>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(recommendations ?? MatchingRecommendationResults());
        return advisorService;
    }

    private static ResourceQueryResults<AdvisorRecommendation>
        MatchingRecommendationResults() =>
        new(
            [
                new(
                    new(
                        Category: "HighAvailability",
                        RecommendationStatus: RecommendationStatus.New.ToString(),
                        RecommendationTypeId: RecommendationTypeId.ToString("D"),
                        ResourceMetadata: new(Resource)),
                    Id: "/subscriptions/22222222-2222-2222-2222-222222222222/providers/Microsoft.Advisor/recommendations/recommendation"),
            ],
            false);

    private static HttpResponseMessage VmssResponse() =>
        JsonResponse(new
        {
            type = "Microsoft.Compute/virtualMachineScaleSets",
            location = "eastus",
            zones = new[] { "1", "2" },
            sku = new { capacity = 3 },
            properties = new { provisioningState = "Succeeded" },
        });

    private static object WorkspaceDefinition(
        string id = Workspace,
        string name = "advisor-chaos") =>
        new
        {
            id,
            name,
            location = "eastus",
            identity = new
            {
                type = "SystemAssigned",
                principalId = PrincipalId,
            },
            properties = new
            {
                provisioningState = "Succeeded",
                scopes = new[] { Resource },
            },
        };

    private static HttpResponseMessage WorkspaceListResponse(
        params object[] workspaces) =>
        JsonResponse(new { value = workspaces });

    private static HttpResponseMessage WorkspaceResponse(object workspace) =>
        JsonResponse(workspace);

    private static HttpResponseMessage ScenarioListResponse() =>
        JsonResponse(new
        {
            value = new[]
            {
                new
                {
                    id = Scenario,
                    name = "ComputeZoneDown",
                    properties = new
                    {
                        recommendation = new
                        {
                            recommendationStatus = "Recommended",
                        },
                        actions = new[]
                        {
                            new
                            {
                                actionId =
                                    "microsoft-virtualMachineScaleSet-shutdown/1.0",
                            },
                        },
                    },
                },
            },
        });

    private static object RunDefinition(
        string status,
        params string[] resources) =>
        new
        {
            id = $"{Scenario}/runs/{RunId}",
            name = RunId,
            properties = new
            {
                status,
                scenarioConfigurationName = "compute-zone-down",
                startTime = "2026-09-03T10:00:00Z",
                resources = resources.Select(id => new { id }).ToArray(),
            },
        };

    private static HttpResponseMessage RunListResponse(
        params object[] runs) =>
        JsonResponse(new { value = runs });

    private static HttpResponseMessage ConfigurationListResponse() =>
        JsonResponse(new
        {
            value = new[]
            {
                new
                {
                    id = Configuration,
                    name = "compute-zone-down",
                    properties = new
                    {
                        provisioningState = "Succeeded",
                        scenarioId = Scenario,
                        resourceTargeting = new
                        {
                            include = new
                            {
                                locations = new[] { "eastus" },
                                zones = new[] { "1" },
                            },
                            exclude = new
                            {
                                locations = Array.Empty<string>(),
                                zones = Array.Empty<string>(),
                            },
                        },
                        parameters = new object[]
                        {
                            new
                            {
                                key = "duration",
                                value = "PT10M",
                            },
                            new
                            {
                                key = "targetResourceIds",
                                value = JsonSerializer.Serialize(
                                    new[] { Resource }),
                            },
                        },
                    },
                    systemData = new
                    {
                        lastModifiedAt = "2026-09-03T09:00:00Z",
                    },
                },
            },
        });

    private static HttpResponseMessage ValidationResponse(
        string status,
        int permissionErrors = 0,
        int resourceErrors = 0) =>
        JsonResponse(new
        {
            properties = new
            {
                status,
                startTime = "2026-09-03T09:30:00Z",
                endTime = "2026-09-03T09:31:00Z",
                validationErrors = new
                {
                    permission = Enumerable.Range(
                        0,
                        permissionErrors).Select(_ => new { }).ToArray(),
                    resource = Enumerable.Range(
                        0,
                        resourceErrors).Select(_ => new { }).ToArray(),
                },
            },
        });

    private static HttpResponseMessage JsonResponse(object value) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(value),
                Encoding.UTF8,
                "application/json"),
        };

    private sealed class QueueHttpMessageHandler(
        params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses =
            new(responses);

        public List<CapturedRequest> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new(
                request.Method,
                request.RequestUri
                    ?? throw new InvalidOperationException(
                        "The request URI is missing."),
                request.Headers.Authorization));

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException(
                    "No queued HTTP response is available.");
            }

            return Task.FromResult(_responses.Dequeue());
        }
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri Uri,
        AuthenticationHeaderValue? Authorization);
}
