// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.ResourceGroup;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Group.UnitTests;

public class GroupListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMcpServer _mcpServer;
    private readonly ILogger<GroupListCommand> _logger;
    private readonly IResourceGroupService _resourceGroupService;
    private readonly GroupListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GroupListCommandTests()
    {
        _mcpServer = Substitute.For<IMcpServer>();
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
            .GetResourceGroups(Arg.Is<string>(x => x == subscriptionId), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedGroups);

        var args = _commandDefinition.Parse($"--subscription {subscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var groupsArray = jsonDoc.RootElement.GetProperty("groups");

        Assert.Equal(2, groupsArray.GetArrayLength());

        var first = groupsArray[0];
        var second = groupsArray[1];

        Assert.Equal("rg1", first.GetProperty("name").GetString());
        Assert.Equal("/subscriptions/test-subs-id/resourceGroups/rg1", first.GetProperty("id").GetString());
        Assert.Equal("East US", first.GetProperty("location").GetString());

        Assert.Equal("rg2", second.GetProperty("name").GetString());
        Assert.Equal("/subscriptions/test-subs-id/resourceGroups/rg2", second.GetProperty("id").GetString());
        Assert.Equal("West US", second.GetProperty("location").GetString());

        await _resourceGroupService.Received(1).GetResourceGroups(
            Arg.Is<string>(x => x == subscriptionId),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
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
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedGroups);

        var args = _commandDefinition.Parse($"--subscription {subscriptionId} --tenant {tenantId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        await _resourceGroupService.Received(1).GetResourceGroups(
            Arg.Is<string>(x => x == subscriptionId),
            Arg.Is<string>(x => x == tenantId),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResourceGroupList_ReturnsNullResults()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        _resourceGroupService
            .GetResourceGroups(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse($"--subscription {subscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.Null(result.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var expectedError = "Test error message";
        _resourceGroupService
            .GetResourceGroups(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<ResourceGroupInfo>>(new Exception(expectedError)));

        var args = _commandDefinition.Parse($"--subscription {subscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.Status);
        Assert.Contains(expectedError, result.Message);
    }
}
