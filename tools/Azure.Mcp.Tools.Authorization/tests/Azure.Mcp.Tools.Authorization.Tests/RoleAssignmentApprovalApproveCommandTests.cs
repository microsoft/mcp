// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Authorization.Commands;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;

public class RoleAssignmentApprovalApproveCommandTests : CommandUnitTestsBase<RoleAssignmentApprovalApproveCommand, IAuthorizationService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsApprovedStage()
    {
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";
        var approval = "approval1";
        var stage = "stage1";
        var justification = "Approved for DRI response.";
        var expectedStage = new RoleAssignmentApprovalStage
        {
            Id = $"{scope}/providers/Microsoft.Authorization/roleAssignmentApprovals/{approval}/stages/{stage}",
            DisplayName = stage,
            AssignedToMe = true,
            Status = "Completed",
            ReviewResult = "Approve",
            Justification = justification
        };

        Service.ApproveRoleAssignmentApprovalAsync(
            Arg.Is(subscriptionId),
            Arg.Is(scope),
            Arg.Is(approval),
            Arg.Is(stage),
            Arg.Is(justification),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedStage);

        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--scope", scope,
            "--approval", approval,
            "--stage", stage,
            "--justification", justification);

        var result = ValidateAndDeserializeResponse(response, AuthorizationJsonContext.Default.RoleAssignmentApprovalApproveCommandResult);
        Assert.Equal(expectedStage.Id, result.Stage.Id);
        Assert.Equal(expectedStage.DisplayName, result.Stage.DisplayName);
        Assert.Equal(expectedStage.Status, result.Stage.Status);
        Assert.Equal(expectedStage.ReviewResult, result.Stage.ReviewResult);
        Assert.Equal(expectedStage.Justification, result.Stage.Justification);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}";
        var approval = "approval1";
        var stage = "stage1";
        var justification = "Approved for DRI response.";

        Service.ApproveRoleAssignmentApprovalAsync(subscriptionId, scope, approval, stage, justification, null, null, TestContext.Current.CancellationToken)
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--scope", scope,
            "--approval", approval,
            "--stage", stage,
            "--justification", justification);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
