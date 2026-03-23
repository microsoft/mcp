// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Models.Resource;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Group.UnitTests;

public class ResourceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly McpServer _mcpServer;
    private readonly ILogger<ResourceListCommand> _logger;
    private readonly IResourceGroupService _resourceGroupService;
    private readonly ResourceListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ResourceListCommandTests()
    {
        _mcpServer = Substitute.For<McpServer>();
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _logger = Substitute.For<ILogger<ResourceListCommand>>();
        var collection = new ServiceCollection()
            .AddSingleton(_mcpServer)
            .AddSingleton(_resourceGroupService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _resourceGroupService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsResources()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        var expectedResources = new List<GenericResourceInfo>
        {
            new("storageAccount1", "/subscriptions/test-subs-id/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/storageAccount1", "Microsoft.Storage/storageAccounts", "East US"),
            new("vm1", "/subscriptions/test-subs-id/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/vm1", "Microsoft.Compute/virtualMachines", "West US")
        };

        _resourceGroupService
            .GetGenericResources(
                Arg.Is<string>(subscriptionId),
                Arg.Is<string>(resourceGroup),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResources.ToAsyncEnumerable());

        var args = _commandDefinition.Parse($"--subscription {subscriptionId} --resource-group {resourceGroup}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var listResult = JsonSerializer.Deserialize(JsonSerializer.Serialize(result.Results), GroupJsonContext.Default.ResourceListCommandResult);
        Assert.NotNull(listResult);
        Assert.Equal(2, listResult.Resources.Count);

        Assert.Equal("storageAccount1", listResult.Resources[0].Name);
        Assert.Equal("Microsoft.Storage/storageAccounts", listResult.Resources[0].Type);
        Assert.Equal("East US", listResult.Resources[0].Location);

        Assert.Equal("vm1", listResult.Resources[1].Name);
        Assert.Equal("Microsoft.Compute/virtualMachines", listResult.Resources[1].Type);
        Assert.Equal("West US", listResult.Resources[1].Location);

        _resourceGroupService.Received(1).GetGenericResources(
            Arg.Is<string>(x => x == subscriptionId),
            Arg.Is<string>(x => x == resourceGroup),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesTenantToService()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        var tenantId = "test-tenant-id";
        var expectedResources = new List<GenericResourceInfo>
        {
            new("resource1", "/subscriptions/test-subs-id/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/resource1", "Microsoft.Storage/storageAccounts", "East US")
        };

        _resourceGroupService
            .GetGenericResources(
                Arg.Is<string>(x => x == subscriptionId),
                Arg.Is<string>(x => x == resourceGroup),
                Arg.Is<string>(x => x == tenantId),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResources.ToAsyncEnumerable());

        var args = _commandDefinition.Parse($"--subscription {subscriptionId} --resource-group {resourceGroup} --tenant {tenantId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        _resourceGroupService.Received(1).GetGenericResources(
            Arg.Is<string>(x => x == subscriptionId),
            Arg.Is<string>(x => x == resourceGroup),
            Arg.Is<string>(x => x == tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResourceList_ReturnsEmptyResults()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        _resourceGroupService
            .GetGenericResources(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<GenericResourceInfo>());

        var args = _commandDefinition.Parse($"--subscription {subscriptionId} --resource-group {resourceGroup}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        var listResult = JsonSerializer.Deserialize(JsonSerializer.Serialize(result.Results), GroupJsonContext.Default.ResourceListCommandResult);
        Assert.NotNull(listResult);
        Assert.Empty(listResult.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var resourceGroup = "test-rg";
        var expectedError = "Test error message";
        _resourceGroupService
            .GetGenericResources(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Throw(new Exception(expectedError)));

        var args = _commandDefinition.Parse($"--subscription {subscriptionId} --resource-group {resourceGroup}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroup_ReturnsValidationError()
    {
        // Arrange
        var subscriptionId = "test-subs-id";
        var args = _commandDefinition.Parse($"--subscription {subscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(HttpStatusCode.OK, result.Status);
    }

    private static async IAsyncEnumerable<GenericResourceInfo> Throw(Exception ex)
    {
        throw ex;
#pragma warning disable CS0162 // Unreachable code detected - needed to satisfy compiler for IAsyncEnumerable return type
        yield break;
#pragma warning restore CS0162
    }
}
