// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureIsv.Commands.Datadog;
using Azure.Mcp.Tools.AzureIsv.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureIsv.UnitTests.Datadog;

public class MonitoredResourcesListCommandTests : CommandUnitTestsBase<MonitoredResourcesListCommand, IDatadogService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsResources_WhenResourcesExist()
    {
        var expectedResources = new List<string>
        {
            "/subscriptions/1234/resourceGroups/rg-demo/providers/Microsoft.Datadog/monitors/app-demo-1",
            "/subscriptions/1234/resourceGroups/rg-demo/providers/Microsoft.Datadog/monitors/vm-demo-2"
        };
        Service.ListMonitoredResources(Arg.Is("rg1"), Arg.Is("sub123"), Arg.Is("datadog1"), Arg.Any<CancellationToken>())
            .Returns(expectedResources);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--datadog-resource", "datadog1");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResources()
    {
        // Arrange
        Service.ListMonitoredResources("rg1", "sub123", "datadog1", Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--datadog-resource", "datadog1");

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Missing required arguments: datadog-resource";
        Service.ListMonitoredResources("rg1", "sub123", "datadog1", Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--datadog-resource", "datadog1");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
