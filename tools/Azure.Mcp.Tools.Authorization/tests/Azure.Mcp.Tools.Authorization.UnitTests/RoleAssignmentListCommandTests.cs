// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Commands;
using Azure.Mcp.Tools.Authorization.Models;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.UnitTests;

public class RoleAssignmentListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<RoleAssignmentListCommand> _logger;
    private readonly RoleAssignmentListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RoleAssignmentListCommandTests()
    {
        _authorizationService = Substitute.For<IAuthorizationService>();
        _logger = Substitute.For<ILogger<RoleAssignmentListCommand>>();

        _command = new(_logger, _authorizationService);
        _commandDefinition = _command.GetCommand();
        // CommandContext requires an IServiceProvider, but these tests do not resolve any services,
        // so an empty ServiceProvider is sufficient and intentionally used here.
        _serviceProvider = new ServiceCollection()
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

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
        _authorizationService.ListRoleAssignmentsAsync(
                Arg.Is(subscriptionId),
                Arg.Is(scope),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedRoleAssignments);
        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--scope", scope,
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedRoleAssignments.Results, result.Assignments);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoRoleAssignments()
    {
        // Arrange
        var subscriptionId = "00000000-0000-0000-0000-000000000001";
        var scope = $"/subscriptions/{subscriptionId}/resourceGroups/rg1";
        _authorizationService.ListRoleAssignmentsAsync(subscriptionId, scope, null, null, TestContext.Current.CancellationToken)
            .Returns(new ResourceQueryResults<RoleAssignment>([], false));

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--scope", scope
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AuthorizationJsonContext.Default.RoleAssignmentListCommandResult);

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

        _authorizationService.ListRoleAssignmentsAsync(subscriptionId, scope, null, null, TestContext.Current.CancellationToken)
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--scope", scope
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
