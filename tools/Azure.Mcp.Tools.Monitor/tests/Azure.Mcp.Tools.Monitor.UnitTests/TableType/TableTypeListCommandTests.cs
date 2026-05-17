// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.TableType;

public sealed class TableTypeListCommandTests : CommandUnitTestsBase<TableTypeListCommand, IMonitorService>
{
    private const string _knownSubscription = "knownSubscription";
    private const string _knownWorkspace = "knownWorkspace";
    private const string _knownResourceGroup = "knownResourceGroup";

    [Theory]
    [InlineData($"--subscription {_knownSubscription} --workspace {_knownWorkspace} --resource-group {_knownResourceGroup}", true)]
    [InlineData($"--subscription {_knownSubscription}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testTableTypes = new List<string>
            {
                "CustomLog",
                "AzureMetrics",
                "SystemEvents"
            };
            Service.ListTableTypes(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(testTableTypes);
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
    public async Task ExecuteAsync_ReturnsTableTypesList()
    {
        // Arrange
        var expectedTableTypes = new List<string>
        {
            "CustomLog",
            "AzureMetrics",
            "SystemEvents",
            "ApplicationEvents"
        };
        Service.ListTableTypes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedTableTypes);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroup);

        // Assert
        // Verify the mock was called
        await Service.Received(1).ListTableTypes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TableTypeListCommandResult);

        Assert.Equal(expectedTableTypes.Count, result.TableTypes.Count);
        Assert.Equal(expectedTableTypes[0], result.TableTypes[0]);
        Assert.Equal(expectedTableTypes[1], result.TableTypes[1]);
        Assert.Equal(expectedTableTypes[2], result.TableTypes[2]);
        Assert.Equal(expectedTableTypes[3], result.TableTypes[3]);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var expectedTableTypes = new List<string> { "CustomLog", "AzureMetrics" };
        Service.ListTableTypes(
            _knownSubscription,
            _knownResourceGroup,
            _knownWorkspace,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedTableTypes);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--workspace", _knownWorkspace,
            "--resource-group", _knownResourceGroup);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListTableTypes(
            _knownSubscription,
            _knownResourceGroup,
            _knownWorkspace,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoTableTypes()
    {
        // Arrange
        Service.ListTableTypes(
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
            "--resource-group", _knownResourceGroup);

        // Assert
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TableTypeListCommandResult);
        Assert.Empty(result.TableTypes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListTableTypes(
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
            "--resource-group", _knownResourceGroup);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
