// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ServiceFabric.Commands;
using Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Models;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ServiceFabric.UnitTests.ManagedCluster;

public class ManagedClusterNodeTypeRestartCommandTests : CommandUnitTestsBase<ManagedClusterNodeTypeRestartCommand, IServiceFabricService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("restart", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0", true)]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0 --nodes Worker_1", true)]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0 --update-type ByUpgradeDomain", true)]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --nodes Worker_0", false)] // Missing node-type
    [InlineData("--subscription sub1 --resource-group rg1 --node-type Worker --nodes Worker_0", false)] // Missing cluster
    [InlineData("--subscription sub1 --cluster cluster1 --node-type Worker --nodes Worker_0", false)] // Missing resource-group
    [InlineData("--resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0", false)] // Missing subscription
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker", false)] // Missing nodes
    [InlineData("", false)] // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.RestartManagedClusterNodes(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(new RestartNodeResponse { StatusCode = 202 });
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
    public async Task ExecuteAsync_ReturnsRestartResponse()
    {
        // Arrange
        var expectedResponse = new RestartNodeResponse
        {
            StatusCode = 202,
            AsyncOperationUrl = "https://management.azure.com/subscriptions/sub1/providers/Microsoft.ServiceFabric/locations/eastus/managedClusterOperationResults/op-id",
            Location = "https://management.azure.com/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.ServiceFabric/managedClusters/cluster1/nodeTypes/Worker/operationResults/op-id"
        };

        Service.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1",
            "--node-type", "Worker",
            "--nodes", "Worker_0",
            "--nodes", "Worker_1");

        // Assert
        await Service.Received(1).RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, ServiceFabricJsonContext.Default.ManagedClusterNodeTypeRestartCommandResult);

        Assert.Equal(202, result.Response.StatusCode);
        Assert.Equal(expectedResponse.AsyncOperationUrl, result.Response.AsyncOperationUrl);
        Assert.Equal(expectedResponse.Location, result.Response.Location);
    }

    [Fact]
    public async Task ExecuteAsync_PassesUpdateTypeToService()
    {
        // Arrange
        Service.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Is("ByUpgradeDomain"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new RestartNodeResponse { StatusCode = 202 });

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1",
            "--node-type", "Worker",
            "--nodes", "Worker_0",
            "--update-type", "ByUpgradeDomain");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Is("ByUpgradeDomain"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new RestartNodeResponse { StatusCode = 202 });

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1",
            "--node-type", "Worker",
            "--nodes", "Worker_0");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ServiceFabricJsonContext.Default.ManagedClusterNodeTypeRestartCommandResult);

        Assert.Equal(202, result.Response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1",
            "--node-type", "Worker",
            "--nodes", "Worker_0");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
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
            "--node-type", "Worker",
            "--nodes", "Worker_0");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        Service.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new RestartNodeResponse { StatusCode = 202 });

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--cluster", "cluster1",
            "--node-type", "Worker",
            "--nodes", "Worker_0",
            "--nodes", "Worker_1",
            "--update-type", "ByUpgradeDomain");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).RestartManagedClusterNodes(
            Arg.Is("sub1"),
            Arg.Is("rg1"),
            Arg.Is("cluster1"),
            Arg.Is("Worker"),
            Arg.Is<string[]>(n => n.Length == 2 && n[0] == "Worker_0" && n[1] == "Worker_1"),
            Arg.Is("ByUpgradeDomain"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
