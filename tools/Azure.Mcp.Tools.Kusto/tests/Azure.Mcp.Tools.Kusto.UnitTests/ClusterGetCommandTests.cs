// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Models;
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
        var expectedCluster = new KustoClusterModel
        (
            ClusterName: "clusterA",
            ClusterUri: "https://clusterA.kusto.windows.net",
            Location: "eastus",
            ResourceGroupName: "rg1",
            SubscriptionId: "sub123",
            Sku: "Standard_D13_v2",
            Zones: "",
            Identity: "SystemAssigned",
            ETag: "etag123",
            State: "Running",
            ProvisioningState: "Succeeded",
            DataIngestionUri: "https://ingest-clusterA.kusto.windows.net",
            StateReason: "",
            IsStreamingIngestEnabled: false,
            EngineType: "V3",
            IsAutoStopEnabled: false
        );

        _kusto.GetClusterAsync(
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
    public async Task ExecuteAsync_Returns404_WhenClusterDoesNotExist()
    {
        _kusto.GetClusterAsync(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<KustoClusterModel>(new KeyNotFoundException("Kusto cluster 'clusterA' not found for subscription 'sub123'.")));
        var command = new ClusterGetCommand(_logger);

        var args = command.GetCommand().Parse("--subscription sub123 --cluster clusterA");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _kusto.GetClusterAsync(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));
        var command = new ClusterGetCommand(_logger);

        var args = command.GetCommand().Parse("--subscription sub123 --cluster clusterA");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }
}
