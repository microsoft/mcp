// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Authorization.Commands;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;

public class RoleAssignmentListCommandTests : SubscriptionCommandUnitTestsBase<RoleAssignmentListCommand, IAuthorizationService>
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
            Arg.Any<CancellationToken>())
            .Returns(expectedRoleAssignments);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

        Assert.Equal(expectedRoleAssignments.Results, result.Assignments);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoRoleAssignments()
    {
        // Arrange
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";
        Service.ListRoleAssignmentsAsync(subscriptionId, scope, null, TestContext.Current.CancellationToken)
            .Returns(new ResourceQueryResults<RoleAssignment>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

        Assert.Empty(result.Assignments);
    }

    [Fact]
    public async Task ExecuteAsync_QueriesManagementGroupScope_WithoutSubscription()
    {
        // Arrange
        var scope = "/providers/Microsoft.Management/managementGroups/mg-contoso";
        var assignmentId = "00000000-0000-0000-0000-000000000003";
        var expected = new ResourceQueryResults<RoleAssignment>(
        [
            new() {
                Id = $"{scope}/providers/Microsoft.Authorization/roleAssignments/{assignmentId}",
                Name = "Reader",
                PrincipalId = new Guid(assignmentId),
                PrincipalType = "ServicePrincipal",
                Scope = scope
            }
        ], false);

        Service.ListRoleAssignmentsAsync(null, scope, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act: a management group scope is outside every subscription, so --subscription is not supplied.
        var response = await ExecuteCommandAsync("--scope", scope);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

        Assert.Equal(expected.Results, result.Assignments);
        await Service.Received(1).ListRoleAssignmentsAsync(null, scope, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EchoesQueriedScope()
    {
        // Arrange
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";
        Service.ListRoleAssignmentsAsync(subscriptionId, scope, null, TestContext.Current.CancellationToken)
            .Returns(new ResourceQueryResults<RoleAssignment>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        // Assert: an empty result must still say which scope produced it.
        var result = ValidateAndDeserializeResponse(response, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

        Assert.Equal(scope, result.Scope);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresSubscription_ForNonManagementGroupScope()
    {
        // Arrange
        var scope = "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1";

        // Act
        var response = await ExecuteCommandAsync("--scope", scope);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--subscription", response.Message);
    }

    [Theory]
    [InlineData("/providers/Microsoft.Management/managementGroups/mg-contoso", true, "mg-contoso")]
    [InlineData("/PROVIDERS/Microsoft.Management/ManagementGroups/mg-contoso", true, "mg-contoso")]
    [InlineData("/providers/Microsoft.Management/managementGroups/mg-contoso/", true, "mg-contoso")]
    [InlineData("/subscriptions/00000000-0000-0000-0000-000000000001", false, "")]
    [InlineData("/providers/Microsoft.Management/managementGroups/", false, "")]
    // A resource nested under a management group is a resource scope, not a management group scope.
    [InlineData("/providers/Microsoft.Management/managementGroups/mg-contoso/providers/Microsoft.Authorization/roleAssignments/abc", false, "")]
    public void TryParse_RecognizesManagementGroupScopes(string scope, bool expectedResult, string expectedManagementGroup)
    {
        var actualResult = ManagementGroupScope.TryParse(scope, out var actualManagementGroup);

        Assert.Equal(expectedResult, actualResult);
        Assert.Equal(expectedManagementGroup, actualManagementGroup);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";

        Service.ListRoleAssignmentsAsync(subscriptionId, scope, null, TestContext.Current.CancellationToken)
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--scope", scope);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
