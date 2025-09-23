// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Grafana.Commands;
using Azure.Mcp.Tools.Grafana.Commands.Workspace;
using Azure.Mcp.Tools.Grafana.Models.Workspace;
using Azure.Mcp.Tools.Grafana.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Grafana.UnitTests;

public sealed class WorkspaceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IGrafanaService _grafana;
    private readonly ILogger<WorkspaceListCommand> _logger;

    public WorkspaceListCommandTests()
    {
        _grafana = Substitute.For<IGrafanaService>();
        _logger = Substitute.For<ILogger<WorkspaceListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_grafana);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_Should_Initialize_Command_Properly()
    {
        // Arrange & Act
        var command = new WorkspaceListCommand(_logger);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("list", command.Name);
        Assert.Equal("List Grafana Workspaces", command.Title);
        Assert.Contains("List all Grafana workspace resources", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsWorkspaces_WhenWorkspacesExist()
    {
        // Arrange
        var expectedWorkspaces = new List<Workspace>
        {
            new()
            {
                Name = "grafana-workspace-1",
                ResourceGroupName = "rg-test",
                SubscriptionId = "sub123",
                Location = "East US",
                ProvisioningState = "Succeeded",
                Endpoint = "https://grafana1.grafana.azure.com"
            },
            new()
            {
                Name = "grafana-workspace-2",
                ResourceGroupName = "rg-test2",
                SubscriptionId = "sub123",
                Location = "West US",
                ProvisioningState = "Succeeded",
                Endpoint = "https://grafana2.grafana.azure.com"
            }
        };

        _grafana.ListWorkspacesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedWorkspaces);

        var command = new WorkspaceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);

        Assert.Contains("grafana-workspace-1", json);
        Assert.Contains("grafana-workspace-2", json);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoWorkspacesExist()
    {
        // Arrange
        _grafana.ListWorkspacesAsync("sub123", null, Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var command = new WorkspaceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, GrafanaJsonContext.Default.WorkspaceListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Workspaces);
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesTenantToService()
    {
        // Arrange
        var expectedWorkspaces = new List<Workspace>
        {
            new()
            {
                Name = "grafana-workspace",
                ResourceGroupName = "rg-test",
                SubscriptionId = "sub123"
            }
        };

        _grafana.ListWorkspacesAsync("sub123", "tenant456", Arg.Any<RetryPolicyOptions>())
            .Returns(expectedWorkspaces);

        var command = new WorkspaceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--tenant", "tenant456"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsException()
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        var subscriptionId = "sub123";

        _grafana.ListWorkspacesAsync(subscriptionId, null, Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<IEnumerable<Workspace>>(new Exception("Test error")));

        var command = new WorkspaceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }
}
