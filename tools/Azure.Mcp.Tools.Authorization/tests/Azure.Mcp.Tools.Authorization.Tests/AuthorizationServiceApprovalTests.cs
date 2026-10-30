// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore notreviewed skiptoken

using System.Net;
using System.Security;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services;
using Azure.ResourceManager;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;

public sealed class AuthorizationServiceApprovalTests
{
    private const string Subscription = "00000000-0000-0000-0000-000000000001";
    private const string Tenant = "00000000-0000-0000-0000-000000000002";
    private const string Scope = $"/subscriptions/{Subscription}/resourceGroups/pim-test";
    private const string ApprovalPath = $"{Scope}/providers/Microsoft.Authorization/roleAssignmentApprovals/approval1";
    private const string StagePath = $"{ApprovalPath}/stages/stage1";

    [Theory]
    [InlineData("InProgress", "NotReviewed", true)]
    [InlineData("inprogress", "notreviewed", true)]
    [InlineData("Pending", "", true)]
    [InlineData("NotStarted", "", true)]
    [InlineData("Completed", "NotReviewed", false)]
    [InlineData("InProgress", "Approve", false)]
    [InlineData("InProgress", "Deny", false)]
    public async Task ListPendingApprovals_FiltersApiStageResults(string status, string reviewResult, bool expected)
    {
        using var handler = new ApprovalHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("management.azure.com", request.RequestUri!.Host);
            Assert.Equal($"{Scope}/providers/Microsoft.Authorization/roleAssignmentApprovals", request.RequestUri.AbsolutePath);
            Assert.Contains("api-version=2021-01-01-preview", request.RequestUri.Query, StringComparison.Ordinal);
            Assert.Contains("%24filter=asApprover()", request.RequestUri.Query, StringComparison.Ordinal);
            Assert.Contains("%24top=100", request.RequestUri.Query, StringComparison.Ordinal);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-arm-token", request.Headers.Authorization?.Parameter);
            return Task.FromResult(JsonResponse($$"""
                {
                  "value": [{
                    "id": "{{ApprovalPath}}",
                    "name": "approval1",
                    "properties": {
                      "stages": [{
                        "id": "{{StagePath}}",
                        "type": "Microsoft.Authorization/roleAssignmentApprovals/stages",
                        "name": "stage1",
                        "properties": {
                          "displayName": "Initial approval",
                          "assignedToMe": true,
                          "status": "{{status}}",
                          "reviewedBy": null,
                          "reviewedDateTime": null,
                          "reviewResult": "{{reviewResult}}",
                          "justification": ""
                        }
                      }]
                    }
                  }]
                }
                """));
        });
        IAzureService azureService = CreateAzureService(handler);
        var service = new AuthorizationService(azureService);

        List<RoleAssignmentApproval> approvals = await service.ListPendingRoleAssignmentApprovalsAsync(
            Subscription, Scope, Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(expected ? 1 : 0, approvals.Count);
        if (expected)
        {
            RoleAssignmentApproval approval = Assert.Single(approvals);
            Assert.Equal(ApprovalPath, approval.Id);
            RoleAssignmentApprovalStage stage = Assert.Single(approval.Stages);
            Assert.Equal(StagePath, stage.Id);
            Assert.True(stage.AssignedToMe);
            Assert.Equal(status, stage.Status);
            Assert.Equal(reviewResult, stage.ReviewResult);
        }
    }

    [Theory]
    [InlineData("Public")]
    [InlineData("China")]
    [InlineData("Government")]
    public async Task ListPendingApprovals_FollowsAllPagesInConfiguredCloud(string cloud)
    {
        ArmEnvironment environment = GetEnvironment(cloud);
        string nextLink = $"{environment.Endpoint.AbsoluteUri.TrimEnd('/')}{Scope}/providers/Microsoft.Authorization/roleAssignmentApprovals?api-version=2021-01-01-preview&$skiptoken=page2";
        string lastLink = nextLink.Replace("page2", "page3", StringComparison.Ordinal);
        using var handler = new ApprovalHttpMessageHandler((request, _) =>
        {
            Assert.Equal(environment.Endpoint.Host, request.RequestUri!.Host);
            Assert.Equal("test-arm-token", request.Headers.Authorization?.Parameter);
            string response = request.RequestUri.AbsoluteUri == lastLink
                ? """
                  {"value":[{"name":"pending2","properties":{"stages":[{"status":"Pending"}]}}],"nextLink":null}
                  """
                : request.RequestUri.AbsoluteUri == nextLink
                    ? $$$"""
                      {"value":[{"name":"pending1","properties":{"stages":[{"status":"InProgress","reviewResult":"NotReviewed"}]}},
                                {"name":"completed","properties":{"stages":[{"status":"Completed","reviewResult":"Approve"}]}}],
                       "nextLink":"{{{lastLink}}}"}
                      """
                    : $$"""
                      {"value":[],"nextLink":"{{nextLink}}"}
                      """;
            return Task.FromResult(JsonResponse(response));
        });
        var service = new AuthorizationService(CreateAzureService(handler, environment));

        List<RoleAssignmentApproval> approvals = await service.ListPendingRoleAssignmentApprovalsAsync(
            Subscription, Scope, Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(3, handler.CallCount);
        Assert.Equal(["pending1", "pending2"], approvals.Select(approval => approval.Name));
    }

    [Fact]
    public async Task ListPendingApprovals_StopsAtInternalAccessReviewsContinuation()
    {
        const string nextLink =
            "https://api.accessreviews.identitygovernance.azure.com/api/arm/roleAssignmentApprovals?$skiptoken=page2";
        using var handler = new ApprovalHttpMessageHandler((_, _) => Task.FromResult(JsonResponse($$$"""
            {"value":[{"name":"pending","properties":{"stages":[{"status":"InProgress","reviewResult":"NotReviewed"}]}}],
             "nextLink":"{{{nextLink}}}"}
            """)));
        var service = new AuthorizationService(CreateAzureService(handler));

        List<RoleAssignmentApproval> approvals = await service.ListPendingRoleAssignmentApprovalsAsync(
            Subscription, Scope, Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
        Assert.Equal("pending", Assert.Single(approvals).Name);
    }

    [Theory]
    [InlineData("https://evil.example/approvals")]
    [InlineData("https://management.azure.com.evil.example/approvals")]
    [InlineData("https://management.azure.com@evil.example/approvals")]
    [InlineData("https://evil.example/#management.azure.com")]
    [InlineData("http://management.azure.com/approvals")]
    [InlineData("https://management.chinacloudapi.cn/approvals")]
    [InlineData("https://management.usgovcloudapi.net/approvals")]
    [InlineData("https://api.accessreviews.identitygovernance.azure.com.evil.example/approvals")]
    public async Task ListPendingApprovals_RejectsUnsafeContinuationBeforeNextClientOrToken(string nextLink)
    {
        using var handler = new ApprovalHttpMessageHandler((_, _) =>
            Task.FromResult(JsonResponse($$"""{"value":[],"nextLink":"{{nextLink}}"}""")));
        IAzureService azureService = CreateAzureService(handler);
        var service = new AuthorizationService(azureService);

        await Assert.ThrowsAsync<SecurityException>(() => service.ListPendingRoleAssignmentApprovalsAsync(
            Subscription, Scope, Tenant, TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
        azureService.Received(1).GetClient();
        await azureService.Received(1).GetTokenCredentialAsync(Tenant, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Public", false)]
    [InlineData("China", true)]
    [InlineData("Government", true)]
    public async Task ApproveRoleAssignmentApproval_SendsPatchAndMapsStage(string cloud, bool fullResourceIds)
    {
        const string justification = "Approve \"test\" access\nfor validation.";
        ArmEnvironment environment = GetEnvironment(cloud);
        using var handler = new ApprovalHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal(environment.Endpoint.Host, request.RequestUri!.Host);
            Assert.Equal(StagePath, request.RequestUri.AbsolutePath);
            Assert.Equal("?api-version=2021-01-01-preview", request.RequestUri.Query);
            Assert.Equal("application/json", request.Content!.Headers.ContentType?.MediaType);
            Assert.Equal("test-arm-token", request.Headers.Authorization?.Parameter);
            using JsonDocument body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            JsonElement properties = body.RootElement.GetProperty("properties");
            Assert.Equal("Approve", properties.GetProperty("reviewResult").GetString());
            Assert.Equal(justification, properties.GetProperty("justification").GetString());
            return JsonResponse($$"""
                {
                  "id": "{{StagePath}}",
                  "properties": {
                    "displayName": "Initial approval",
                    "assignedToMe": true,
                    "status": "Completed",
                    "reviewResult": "Approve",
                    "reviewedBy": "{{Tenant}}",
                    "reviewedDateTime": "2026-10-27T00:00:00Z",
                    "justification": "Approve \"test\" access\nfor validation."
                  }
                }
                """);
        });
        var service = new AuthorizationService(CreateAzureService(handler, environment));

        RoleAssignmentApprovalStage stage = await service.ApproveRoleAssignmentApprovalAsync(
            Subscription, Scope, fullResourceIds ? ApprovalPath : "approval1",
            fullResourceIds ? StagePath : "stage1", justification, Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
        Assert.Equal(StagePath, stage.Id);
        Assert.Equal("Completed", stage.Status);
        Assert.Equal("Approve", stage.ReviewResult);
        Assert.Equal(justification, stage.Justification);
        Assert.Equal(Tenant, stage.ReviewedBy);
        Assert.Equal("2026-10-27T00:00:00Z", stage.ReviewedDateTime);
    }

    [Fact]
    public async Task ApproveRoleAssignmentApproval_FetchesStageAfterEmptyPatchResponse()
    {
        using var handler = new ApprovalHttpMessageHandler((request, _) => Task.FromResult(
            request.Method == HttpMethod.Patch
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : JsonResponse($$"""
                  {
                    "id": "{{StagePath}}",
                    "properties": {
                      "status": "Completed",
                      "reviewResult": "Approve",
                      "justification": "test"
                    }
                  }
                  """)));
        var service = new AuthorizationService(CreateAzureService(handler));

        RoleAssignmentApprovalStage stage = await service.ApproveRoleAssignmentApprovalAsync(
            Subscription, Scope, "approval1", "stage1", "test", Tenant,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.CallCount);
        Assert.Equal(StagePath, stage.Id);
        Assert.Equal("Completed", stage.Status);
        Assert.Equal("Approve", stage.ReviewResult);
    }

    [Fact]
    public async Task ApproveRoleAssignmentApproval_RejectsAuthorityReplacementBeforeClientOrToken()
    {
        using var handler = new ApprovalHttpMessageHandler((_, _) =>
            throw new InvalidOperationException("No network call expected."));
        IAzureService azureService = CreateAzureService(handler);
        var service = new AuthorizationService(azureService);

        // Backslashes can act as authority separators when .NET resolves a relative URI.
        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.ApproveRoleAssignmentApprovalAsync(
            Subscription, Scope, "approval1", @"/\\evil.example/stage", "test",
            Tenant, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        azureService.DidNotReceive().GetClient();
        await azureService.DidNotReceive().GetTokenCredentialAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveRoleAssignmentApproval_PropagatesHttpFailure()
    {
        using var handler = new ApprovalHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)));
        var service = new AuthorizationService(CreateAzureService(handler));

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ApproveRoleAssignmentApprovalAsync(
                Subscription, Scope, "approval1", "stage1", "test", Tenant, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    private static IAzureService CreateAzureService(HttpMessageHandler handler, ArmEnvironment? environment = null)
    {
        IAzureService azureService = AzureServiceTestHelpers.CreateAzureService(armEnvironment: environment);
        TokenCredential credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-arm-token", DateTimeOffset.UtcNow.AddHours(1))));
        azureService.ResolveTenantIdAsync(Tenant, Arg.Any<CancellationToken>()).Returns(Tenant);
        azureService.GetTokenCredentialAsync(Tenant, Arg.Any<CancellationToken>()).Returns(credential);
        azureService.GetClient().Returns(_ => new HttpClient(handler, disposeHandler: false));
        return azureService;
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private static ArmEnvironment GetEnvironment(string cloud) => cloud switch
    {
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        _ => ArmEnvironment.AzurePublicCloud
    };
}
