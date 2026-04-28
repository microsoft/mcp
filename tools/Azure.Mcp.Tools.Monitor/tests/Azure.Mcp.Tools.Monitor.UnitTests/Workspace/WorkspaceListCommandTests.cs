// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Workspace;

public sealed class WorkspaceListCommandTests : CommandUnitTestsBase<WorkspaceListCommand, IMonitorService>
{
    private const string _knownSubscription = "knownSubscription";

    [Theory]
    [InlineData($"--subscription {_knownSubscription}", true)]
    [InlineData($"--subscription {_knownSubscription} --tenant tenant123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testWorkspaces = new List<WorkspaceInfo>
            {
                new() { Name = "workspace1", CustomerId = "guid1" },
                new() { Name = "workspace2", CustomerId = "guid2" }
            };
            Service.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(testWorkspaces);
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
    public async Task ExecuteAsync_ReturnsWorkspacesList()
    {
        // Arrange
        var expectedWorkspaces = new List<WorkspaceInfo>
        {
            new() { Name = "workspace1", CustomerId = "guid1" },
            new() { Name = "workspace2", CustomerId = "guid2" },
            new() { Name = "workspace3", CustomerId = "guid3" }
        };
        Service.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedWorkspaces);

        // Act
        var response = await ExecuteCommandAsync($"--subscription {_knownSubscription}");

        // Assert
        // Verify the mock was called
        await Service.Received(1).ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.WorkspaceListCommandResult);

        Assert.Equal(expectedWorkspaces.Count, result.Workspaces.Count);
        Assert.Equal(expectedWorkspaces[0].Name, result.Workspaces[0].Name);
        Assert.Equal(expectedWorkspaces[0].CustomerId, result.Workspaces[0].CustomerId);
        Assert.Equal(expectedWorkspaces[1].Name, result.Workspaces[1].Name);
        Assert.Equal(expectedWorkspaces[1].CustomerId, result.Workspaces[1].CustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoWorkspaces()
    {
        // Arrange
        Service.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync($"--subscription {_knownSubscription}");

        // Assert
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.WorkspaceListCommandResult);

        Assert.Empty(result.Workspaces);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync($"--subscription {_knownSubscription}");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
