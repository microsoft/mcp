// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;

public sealed class AuthorizationApprovalCommandTests(
    ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // ARM includes tenant and principal identifiers in this diagnostic response header.
    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        new(new HeaderRegexSanitizerBody("x-ms-operation-identifier")
        {
            Regex = ".*",
            Value = "Sanitized"
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new(new BodyKeySanitizerBody("$..principalName") { Value = "Sanitized" }),
        new(new BodyKeySanitizerBody("$..userPrincipalName") { Value = "Sanitized" })
    ];

    [Fact]
    public async Task Should_list_pending_role_approvals()
    {
        JsonElement? result = await CallToolAsync(
            "role_approval_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "scope", $"/subscriptions/{Settings.SubscriptionId}" }
            });

        JsonElement approvals = result.AssertProperty("approvals");
        Assert.Equal(JsonValueKind.Array, approvals.ValueKind);
    }

    [Fact]
    public async Task Should_reject_missing_role_approval_stage()
    {
        // These nonexistent stage identifiers exercise the ARM error path without granting access.
        CallToolResult result = await Client.CallToolAsync(
            "role_approval_approve",
            new Dictionary<string, object?>
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "scope", $"/subscriptions/{Settings.SubscriptionId}" },
                { "approval", "00000000-0000-0000-0000-000000000001" },
                { "stage", "00000000-0000-0000-0000-000000000002" },
                { "justification", "Verify missing approval stage handling without granting access." }
            },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("status 404", McpTestUtilities.GetFirstText(result.Content), StringComparison.Ordinal);
    }

    // This grants access. Run explicitly with a controlled request, never an arbitrary pending approval.
    [Fact(Explicit = true)]
    public async Task Should_discover_and_approve_controlled_role_request()
    {
        string scope = TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback
            ? $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}"
            : Settings.DeploymentOutputs["PIM_APPROVAL_SCOPE"];
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
