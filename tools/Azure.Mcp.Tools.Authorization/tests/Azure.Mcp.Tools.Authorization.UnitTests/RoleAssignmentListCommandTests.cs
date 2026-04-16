// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Commands;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.UnitTests;

public class RoleAssignmentListCommandTests : CommandUnitTestsBase<RoleAssignmentListCommand, IAuthorizationService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsRoleAssignments_WhenRoleAssignmentsExist()
    {
        // Arrange
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";
        var id1 = "00000000-0000-0000-0000-000000000001";
        var id2 = "00000000-0000-0000-0000-000000000002";
        var expectedRoleAssignments = new ResourceQueryResults<RoleAssignment>(
        [
            new() {
                Id = $"/subscriptions/{subscriptionId}/resourcegroups/azure-mcp/providers/Microsoft.Authorization/roleAssignments/{id1}",
                Name = "Test role definition 1",
                PrincipalId = new Guid(id1),
                PrincipalType = "User",
                RoleDefinitionId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{id1}",
                Scope = scope,
                Description = "Role assignment for azmcp test 1",
                DelegatedManagedIdentityResourceId = string.Empty,
                Condition = string.Empty
            },
            new() {
                Id = $"/subscriptions/{subscriptionId}/resourcegroups/azure-mcp/providers/Microsoft.Authorization/roleAssignments/{id2}",
                Name = "Test role definition 2",
                PrincipalId = new Guid(id2),
                PrincipalType = "User",
                RoleDefinitionId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{id2}",
                Scope = scope,
                Description = "Role assignment for azmcp test 2",
                DelegatedManagedIdentityResourceId = string.Empty,
                Condition = "ActionMatches{'Microsoft.Authorization/roleAssignments/write'}"
            }
        ], false);
        Service.ListRoleAssignmentsAsync(
            Arg.Is(subscriptionId),
            Arg.Is(scope),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedRoleAssignments);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedRoleAssignments.Results, result.Assignments);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoRoleAssignments()
    {
        // Arrange
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";
        Service.ListRoleAssignmentsAsync(subscriptionId, scope, null, null, TestContext.Current.CancellationToken)
            .Returns(new ResourceQueryResults<RoleAssignment>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Assignments);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";

        Service.ListRoleAssignmentsAsync(subscriptionId, scope, null, null, TestContext.Current.CancellationToken)
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
