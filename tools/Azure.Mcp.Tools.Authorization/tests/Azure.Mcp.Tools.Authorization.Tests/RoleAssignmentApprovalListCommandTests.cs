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

public class RoleAssignmentApprovalListCommandTests : CommandUnitTestsBase<RoleAssignmentApprovalListCommand, IAuthorizationService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsPendingApprovals()
    {
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";
        var expectedApprovals = new List<RoleAssignmentApproval>
        {
            new()
            {
                Id = $"{scope}/providers/Microsoft.Authorization/roleAssignmentApprovals/approval1",
                Name = "approval1",
                PrincipalId = "00000000-0000-0000-0000-000000000002",
                RoleDefinitionId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/role1",
                RequestorId = "00000000-0000-0000-0000-000000000003",
                Scope = scope,
                Status = "InProgress",
                Stages =
                [
                    new()
                    {
                        Id = $"{scope}/providers/Microsoft.Authorization/roleAssignmentApprovals/approval1/stages/stage1",
                        DisplayName = "stage1",
                        AssignedToMe = true,
                        Status = "InProgress"
                    }
                ]
            }
        };

        Service.ListPendingRoleAssignmentApprovalsAsync(
            Arg.Is(subscriptionId),
            Arg.Is(scope),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedApprovals);

        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        var result = ValidateAndDeserializeResponse(response, AuthorizationJsonContext.Default.RoleAssignmentApprovalListCommandResult);
        var approvalResult = Assert.Single(result.Approvals);
        Assert.Equal(expectedApprovals[0].Id, approvalResult.Id);
        Assert.Equal(expectedApprovals[0].Name, approvalResult.Name);
        Assert.Equal(expectedApprovals[0].PrincipalId, approvalResult.PrincipalId);
        Assert.Equal(expectedApprovals[0].Status, approvalResult.Status);
        var stageResult = Assert.Single(approvalResult.Stages);
        Assert.Equal(expectedApprovals[0].Stages[0].Id, stageResult.Id);
        Assert.True(stageResult.AssignedToMe);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoPendingApprovals()
    {
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}";
        Service.ListPendingRoleAssignmentApprovalsAsync(subscriptionId, scope, null, null, TestContext.Current.CancellationToken)
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        var result = ValidateAndDeserializeResponse(response, AuthorizationJsonContext.Default.RoleAssignmentApprovalListCommandResult);
        Assert.Empty(result.Approvals);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}";

        Service.ListPendingRoleAssignmentApprovalsAsync(subscriptionId, scope, null, null, TestContext.Current.CancellationToken)
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
