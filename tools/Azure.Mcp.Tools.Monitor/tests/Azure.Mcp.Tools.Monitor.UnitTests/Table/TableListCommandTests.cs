// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Table;

public sealed class TableListCommandTests : CommandUnitTestsBase<TableListCommand, IMonitorService>
{
    private const string _knownSubscription = "knownSubscription";
    private const string _knownWorkspace = "knownWorkspace";
    private const string _knownResourceGroupName = "knownResourceGroup";
    private const string _knownTableType = "CustomLog";

    [Theory]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --table-type {_knownTableType} --resource-group {_knownResourceGroupName}", true)]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroupName}", true)]
    [InlineData($"--subscription {_knownSubscription}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testTables = new List<string>
            {
                "AppEvents",
                "AppRequests",
                "AppDependencies"
            };
            Service.ListTables(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(testTables);
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
    public async Task ExecuteAsync_ReturnsTablesList()
    {
        // Arrange
        var expectedTables = new List<string>
        {
            "AppEvents",
            "AppRequests",
            "AppDependencies",
            "AppMetrics"
        };
        Service.ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedTables);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName);

        // Assert
        // Verify the mock was called
        await Service.Received(1).ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TableListCommandResult);

        Assert.Equal(expectedTables.Count, result.Tables.Count);
        Assert.Equal(expectedTables[0], result.Tables[0]);
        Assert.Equal(expectedTables[1], result.Tables[1]);
        Assert.Equal(expectedTables[2], result.Tables[2]);
        Assert.Equal(expectedTables[3], result.Tables[3]);
    }

    [Fact]
    public async Task ExecuteAsync_WithTableTypeParameter_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedTables = new List<string> { "CustomTable1", "CustomTable2" };
        Service.ListTables(
            _knownSubscription,
            _knownResourceGroupName,
            _knownWorkspace,
            _knownTableType,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedTables);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName,
            "--table-type", _knownTableType);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListTables(
            _knownSubscription,
            _knownResourceGroupName,
            _knownWorkspace,
            _knownTableType,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoTables()
    {
        // Arrange
        Service.ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TableListCommandResult);

        Assert.Empty(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListTables(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroupName);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
