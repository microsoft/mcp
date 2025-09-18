// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class ClusterGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKustoService _kusto;
    private readonly ILogger<ClusterGetCommand> _logger;

    public ClusterGetCommandTests()
    {
        _kusto = Substitute.For<IKustoService>();
        _logger = Substitute.For<ILogger<ClusterGetCommand>>();
        var collection = new ServiceCollection();
        collection.AddSingleton(_kusto);
        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCluster_WhenClusterExists()
    {
        var expectedCluster = new KustoClusterResourceProxy
        {
            ClusterUri = "https://clusterA.kusto.windows.net",
            ClusterName = "clusterA",
            Location = "eastus",
            ResourceGroupName = "rg1",
            SubscriptionId = "sub123",
            Sku = "Standard_D13_v2",
            Zones = "",
            Identity = "SystemAssigned",
            ETag = "etag123",
            State = "Running",
            ProvisioningState = "Succeeded",
            DataIngestionUri = "https://ingest-clusterA.kusto.windows.net",
            StateReason = "",
            IsStreamingIngestEnabled = false,
            EngineType = "V3",
            IsAutoStopEnabled = false
        };

        _kusto.GetCluster(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCluster);
        var command = new ClusterGetCommand(_logger);

        var args = command.GetCommand().Parse("--subscription sub123 --cluster clusterA");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.ClusterGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Cluster);
        Assert.Equal("clusterA", result.Cluster.ClusterName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenClusterDoesNotExist()
    {
        _kusto.GetCluster(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult<KustoClusterResourceProxy?>(null));
        var command = new ClusterGetCommand(_logger);

        var args = command.GetCommand().Parse("--subscription sub123 --cluster clusterA");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _kusto.GetCluster(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));
        var command = new ClusterGetCommand(_logger);

        var args = command.GetCommand().Parse("--subscription sub123 --cluster clusterA");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }
}
