// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ServiceFabric.Commands;
using Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ServiceFabric.UnitTests.ManagedCluster;

public class ManagedClusterNodeGetCommandTests : CommandUnitTestsBase<ManagedClusterNodeGetCommand, IServiceFabricService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1", true)]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node primary_0", true)]
    [InlineData("--subscription sub1 --cluster cluster1", false)]  // Missing resource-group
    [InlineData("--subscription sub1 --resource-group rg1", false)] // Missing cluster
    [InlineData("--resource-group rg1 --cluster cluster1", false)] // Missing subscription
    [InlineData("", false)]                                         // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.ListManagedClusterNodes(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns([]);

            Service.GetManagedClusterNode(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(new Models.ManagedClusterNode());
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNodesList()
    {
        // Arrange
        var expectedNodes = new List<Models.ManagedClusterNode>
        {
            new()
            {
                Id = "/subscriptions/sub1/resourcegroups/rg1/providers/Microsoft.ServiceFabric/managedClusters/cluster1/Nodes/primary_0",
                Properties = new()
                {
                    Name = "primary_0",
                    Type = "primary",
                    NodeStatus = 1,
                    IpAddressOrFQDN = "10.0.0.4",
                    FaultDomain = "fd:/0",
                    UpgradeDomain = "0",
                    IsSeedNode = true
                }
            },
            new()
            {
                Id = "/subscriptions/sub1/resourcegroups/rg1/providers/Microsoft.ServiceFabric/managedClusters/cluster1/Nodes/Worker_1",
                Properties = new()
                {
                    Name = "Worker_1",
                    Type = "Worker",
                    NodeStatus = 1,
                    IpAddressOrFQDN = "10.0.0.5",
                    FaultDomain = "fd:/az1/0",
                    UpgradeDomain = "1",
                    IsSeedNode = false
                }
            }
        };

        Service.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedNodes);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1");

        // Assert
        await Service.Received(1).ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);

        Assert.Equal(expectedNodes.Count, result.Nodes.Count);
        Assert.Equal(expectedNodes[0].Id, result.Nodes[0].Id);

        var node0Props = result.Nodes[0].Properties!;
        var node1Props = result.Nodes[1].Properties!;
        Assert.Equal("primary_0", node0Props.Name);
        Assert.Equal("primary", node0Props.Type);
        Assert.Equal(1, node0Props.NodeStatus);
        Assert.Equal("10.0.0.4", node0Props.IpAddressOrFQDN);
        Assert.Equal("Worker_1", node1Props.Name);
        Assert.Equal("fd:/az1/0", node1Props.FaultDomain);
        Assert.False(node1Props.IsSeedNode);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSingleNodeWhenNodeNameProvided()
    {
        // Arrange
        var expectedNode = new Models.ManagedClusterNode
        {
            Id = "/subscriptions/sub1/resourcegroups/rg1/providers/Microsoft.ServiceFabric/managedClusters/cluster1/Nodes/primary_0",
            Properties = new()
            {
                Name = "primary_0",
                Type = "primary",
                NodeStatus = 1,
                IpAddressOrFQDN = "10.0.0.4",
                FaultDomain = "fd:/0",
                UpgradeDomain = "0",
                IsSeedNode = true
            }
        };

        Service.GetManagedClusterNode(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedNode);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1",
            "--node", "primary_0");

        // Assert
        await Service.Received(1).GetManagedClusterNode(
            "sub1", "rg1", "cluster1", "primary_0",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        await Service.DidNotReceive().ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);

        Assert.Single(result.Nodes);
        Assert.Equal(expectedNode.Id, result.Nodes[0].Id);
        Assert.Equal("primary_0", result.Nodes[0].Properties!.Name);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);

        Assert.Empty(result.Nodes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrorsForSingleNode()
    {
        // Arrange
        Service.GetManagedClusterNode(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Not found", null, HttpStatusCode.NotFound));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1",
            "--node", "nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoNodes()
    {
        // Arrange
        Service.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);

        Assert.Empty(result.Nodes);
    }

    [Fact]
    public void BindOptions_BindsNodeNameCorrectly()
    {
        var parseResult = CommandDefinition.Parse("--subscription sub1 --resource-group rg1 --cluster cluster1 --node primary_0");
        Assert.Empty(parseResult.Errors);
    }
}
