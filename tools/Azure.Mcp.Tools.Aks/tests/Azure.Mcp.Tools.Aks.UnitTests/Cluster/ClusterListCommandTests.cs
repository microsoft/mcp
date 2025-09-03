// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Aks.Commands;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Aks.UnitTests.Cluster;

public sealed class ClusterListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAksService _aksService;
    private readonly ILogger<ClusterListCommand> _logger;
    private readonly ClusterListCommand _command;

    public ClusterListCommandTests()
    {
        _aksService = Substitute.For<IAksService>();
        _logger = Substitute.For<ILogger<ClusterListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_aksService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --tenant tenant123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testClusters = new List<Models.Cluster>
            {
                new() { Name = "cluster1", Location = "eastus" },
                new() { Name = "cluster2", Location = "westus" }
            };
            _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(testClusters);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
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
    public async Task ExecuteAsync_ReturnsClustersList()
    {
        // Arrange
        var expectedClusters = new List<Models.Cluster>
        {
            new() { Name = "cluster1", Location = "eastus", KubernetesVersion = "1.28.0" },
            new() { Name = "cluster2", Location = "westus", KubernetesVersion = "1.29.0" },
            new() { Name = "cluster3", Location = "centralus", KubernetesVersion = "1.28.5" }
        };
        _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedClusters);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _aksService.Received(1).ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        // Debug: Output the actual JSON to understand the structure
        Console.WriteLine($"Actual JSON: {json}");

        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.ClusterListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedClusters.Count, result.Clusters.Count);
        Assert.Equal(expectedClusters[0].Name, result.Clusters[0].Name);
        Assert.Equal(expectedClusters[0].Location, result.Clusters[0].Location);
        Assert.Equal(expectedClusters[0].KubernetesVersion, result.Clusters[0].KubernetesVersion);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenNoClusters()
    {
        // Arrange
        _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(new List<Models.Cluster>());

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.Cluster>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
