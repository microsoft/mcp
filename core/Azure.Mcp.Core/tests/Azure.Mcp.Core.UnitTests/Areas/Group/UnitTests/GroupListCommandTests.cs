// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Models.ResourceGroup;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Group.UnitTests;

public class GroupListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly McpServer _mcpServer;
    private readonly ILogger<GroupListCommand> _logger;
    private readonly IResourceGroupService _resourceGroupService;
    private readonly GroupListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GroupListCommandTests()
    {
        _mcpServer = Substitute.For<McpServer>();
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _logger = Substitute.For<ILogger<GroupListCommand>>();
        var collection = new ServiceCollection()
            .AddSingleton(_mcpServer)
            .AddSingleton(_resourceGroupService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSubscription_ReturnsResourceGroups()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var expectedGroups = new List<ResourceGroupInfo>
        {
            ResourceGroupTestHelpers.CreateResourceGroupInfo("rg1", subscriptionId, "East US"),
            ResourceGroupTestHelpers.CreateResourceGroupInfo("rg2", subscriptionId, "West US")
        };

        _resourceGroupService
            .GetResourceGroups(Arg.Is<string>(x => x == subscriptionId), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedGroups);

        var args = _commandDefinition.Parse($"--subscription {subscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var resultGroups = JsonSerializer.Deserialize(JsonSerializer.Serialize(result.Results), GroupJsonContext.Default.Result);
        Assert.NotNull(resultGroups);
        Assert.Equal(2, resultGroups.Groups.Count);

        var first = resultGroups.Groups[0];
        var second = resultGroups.Groups[1];

        Assert.Equal("rg1", first.Name);
        Assert.Equal("/subscriptions/test-subs-id/resourceGroups/rg1", first.Id);
        Assert.Equal("East US", first.Location);

        Assert.Equal("rg2", second.Name);
        Assert.Equal("/subscriptions/test-subs-id/resourceGroups/rg2", second.Id);
        Assert.Equal("West US", second.Location);

        await _resourceGroupService.Received(1).GetResourceGroups(
            Arg.Is<string>(x => x == subscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesTenantToService()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var tenantId = "test-tenant-id";
        var expectedGroups = new List<ResourceGroupInfo>
        {
            ResourceGroupTestHelpers.CreateResourceGroupInfo("rg1", subscriptionId, "East US")
        };

        _resourceGroupService
            .GetResourceGroups(
                Arg.Is<string>(x => x == subscriptionId),
                Arg.Is<string>(x => x == tenantId),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedGroups);

        var args = _commandDefinition.Parse($"--subscription {subscriptionId} --tenant {tenantId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await _resourceGroupService.Received(1).GetResourceGroups(
            Arg.Is<string>(x => x == subscriptionId),
            Arg.Is<string>(x => x == tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResourceGroupList_ReturnsEmptyResults()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        _resourceGroupService
            .GetResourceGroups(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var args = _commandDefinition.Parse($"--subscription {subscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var resultGroups = JsonSerializer.Deserialize(JsonSerializer.Serialize(result.Results), GroupJsonContext.Default.Result);
        Assert.NotNull(resultGroups);
        Assert.Empty(resultGroups.Groups);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var expectedError = "Test error message";
        _resourceGroupService
            .GetResourceGroups(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<ResourceGroupInfo>>(new Exception(expectedError)));

        var args = _commandDefinition.Parse($"--subscription {subscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);
    }
}
