// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;

public sealed class AuthorizationApprovalCommandTests(
    ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // This grants access. Run explicitly with a controlled request, never an arbitrary pending approval.
    [Fact(Explicit = true)]
    public async Task Should_discover_and_approve_controlled_role_request()
    {
        string scope = RegisterOrRetrieveDeploymentOutputVariable("scope", "PIM_APPROVAL_SCOPE");
        string approvalId = RegisterOrRetrieveDeploymentOutputVariable("approvalId", "PIM_APPROVAL_ID");
        string stageId = RegisterOrRetrieveDeploymentOutputVariable("stageId", "PIM_APPROVAL_STAGE_ID");
        const string justification = "Approve controlled Azure MCP PIM integration test request.";

        JsonElement? listResult = await CallToolAsync(
            "role_approval_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "scope", scope }
            });

        JsonElement approval = Assert.Single(listResult.AssertProperty("approvals").EnumerateArray(),
            item => item.AssertProperty("id").GetString() == approvalId);
        JsonElement pendingStage = Assert.Single(approval.AssertProperty("stages").EnumerateArray(),
            item => item.AssertProperty("id").GetString() == stageId);
        Assert.True(pendingStage.AssertProperty("assignedToMe").GetBoolean());
        Assert.Equal("InProgress", pendingStage.AssertProperty("status").GetString());
        Assert.Equal("NotReviewed", pendingStage.AssertProperty("reviewResult").GetString());

        JsonElement? approveResult = await CallToolAsync(
            "role_approval_approve",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "scope", scope },
                { "approval", approvalId },
                { "stage", stageId },
                { "justification", justification }
            });

        JsonElement reviewedStage = approveResult.AssertProperty("stage");
        Assert.Equal(stageId, reviewedStage.AssertProperty("id").GetString());
        Assert.Equal("Completed", reviewedStage.AssertProperty("status").GetString());
        Assert.Equal("Approve", reviewedStage.AssertProperty("reviewResult").GetString());
        Assert.Equal(justification, reviewedStage.AssertProperty("justification").GetString());
        Assert.False(string.IsNullOrEmpty(reviewedStage.AssertProperty("reviewedDateTime").GetString()));
    }
}
