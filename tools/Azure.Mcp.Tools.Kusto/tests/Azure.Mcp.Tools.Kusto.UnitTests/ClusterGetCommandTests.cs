// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Models;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class ClusterGetCommandTests : CommandUnitTestsBase<ClusterGetCommand, IKustoService>
{
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

        Service.GetClusterAsync(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCluster);

        var response = await ExecuteCommandAsync("--subscription sub123 --cluster clusterA");

        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.ClusterGetCommandResult);

        Assert.NotNull(result.Cluster);
        Assert.Equal("clusterA", result.Cluster.ClusterName);
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenClusterDoesNotExist()
    {
        Service.GetClusterAsync(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Kusto cluster 'clusterA' not found for subscription 'sub123'."));

        var response = await ExecuteCommandAsync("--subscription sub123 --cluster clusterA");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        Service.GetClusterAsync(
            "sub123", "clusterA", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription sub123 --cluster clusterA");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }
}
