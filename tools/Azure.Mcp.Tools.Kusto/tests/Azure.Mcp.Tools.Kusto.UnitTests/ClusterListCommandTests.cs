// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class ClusterListCommandTests : CommandUnitTestsBase<ClusterListCommand, IKustoService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsClusters_WhenClustersExist()
    {
        // Arrange
        var expectedClusters = new ResourceQueryResults<string>(["clusterA", "clusterB"], false);
        Service.ListClustersAsync(
            "sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedClusters);

        // Act
        var response = await ExecuteCommandAsync("--subscription sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.ClusterListCommandResult);

        Assert.Equal(expectedClusters.Results, result.Clusters);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoClustersExist()
    {
        // Arrange
        Service.ListClustersAsync("sub123", null, null, Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<string>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.ClusterListCommandResult);

        Assert.Empty(result.Clusters);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsException()
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        var subscriptionId = "sub123";

        // Arrange
        Service.ListClustersAsync(subscriptionId, null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription sub123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }
}
