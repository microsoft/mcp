// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Grafana.Commands;
using Azure.Mcp.Tools.Grafana.Commands.Workspace;
using Azure.Mcp.Tools.Grafana.Models;
using Azure.Mcp.Tools.Grafana.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Grafana.UnitTests;

public sealed class WorkspaceListCommandTests : CommandUnitTestsBase<WorkspaceListCommand, IGrafanaService>
{
    [Fact]
    public void Constructor_Should_Initialize_Command_Properly()
    {
        Assert.Equal("list", Command.Name);
        Assert.Equal("List Grafana Workspaces", Command.Title);
        Assert.False(string.IsNullOrEmpty(Command.Description));
        Assert.True(Command.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsWorkspaces_WhenWorkspacesExist()
    {
        // Arrange
        var expectedWorkspaces = new ResourceQueryResults<GrafanaWorkspace>(
        [
            new(
                Name : "grafana-workspace-1",
                ResourceGroupName : "rg-test",
                SubscriptionId : "sub123",
                Location : "East US",
                Sku: "Standard",
                ProvisioningState : "Succeeded",
                Endpoint : "https://grafana1.grafana.azure.com",
                ZoneRedundancy : "Disabled",
                PublicNetworkAccess : "Enabled",
                GrafanaVersion : "8.0",
                Identity : null,
                Tags : null
            ),
            new(
                Name : "grafana-workspace-2",
                ResourceGroupName : "rg-test2",
                SubscriptionId : "sub123",
                Location : "West US",
                Sku: "Standard",
                ProvisioningState : "Succeeded",
                Endpoint : "https://grafana2.grafana.azure.com",
                ZoneRedundancy : "Disabled",
                PublicNetworkAccess : "Enabled",
                GrafanaVersion : "8.0",
                Identity : null,
                Tags : null
            )
        ], false);

        Service.ListWorkspacesAsync("sub123", Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedWorkspaces);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, GrafanaJsonContext.Default.WorkspaceListCommandResult);

        Assert.NotNull(result.Workspaces);
        Assert.Contains(result.Workspaces, w => w.Name == "grafana-workspace-1");
        Assert.Contains(result.Workspaces, w => w.Name == "grafana-workspace-2");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoWorkspacesExist()
    {
        // Arrange
        Service.ListWorkspacesAsync("sub123", Arg.Any<string?>(), null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<GrafanaWorkspace>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, GrafanaJsonContext.Default.WorkspaceListCommandResult);
        Assert.Empty(result.Workspaces);
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesTenantToService()
    {
        // Arrange
        var expectedWorkspaces = new ResourceQueryResults<GrafanaWorkspace>(
        [
            new(
                Name : "grafana-workspace",
                ResourceGroupName : "rg-test",
                SubscriptionId : "sub123",
                Location : "West US",
                Sku: "Standard",
                ProvisioningState : "Succeeded",
                Endpoint : "https://grafana2.grafana.azure.com",
                ZoneRedundancy : "Disabled",
                PublicNetworkAccess : "Enabled",
                GrafanaVersion : "8.0",
                Identity : null,
                Tags : null
            )
        ], false);

        Service.ListWorkspacesAsync("sub123", Arg.Any<string?>(), "tenant456", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedWorkspaces);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--tenant", "tenant456");

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

        Service.ListWorkspacesAsync(subscriptionId, Arg.Any<string?>(), null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroup_ForwardsResourceGroupToService()
    {
        // Arrange
        const string resourceGroup = "test-rg";
        Service.ListWorkspacesAsync(Arg.Any<string>(), Arg.Is(resourceGroup), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<GrafanaWorkspace>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", resourceGroup);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListWorkspacesAsync(Arg.Any<string>(), Arg.Is(resourceGroup), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
