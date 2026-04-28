// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.ActivityLog;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.ActivityLog;

public sealed class ActivityLogListCommandTests : CommandUnitTestsBase<ActivityLogListCommand, IMonitorService>
{
    private const string _knownSubscription = "knownSubscription";
    private const string _knownResourceName = "myResource";

    [Theory]
    [InlineData($"--subscription {_knownSubscription} --resource-name {_knownResourceName}", true)]
    [InlineData($"--subscription {_knownSubscription} --resource-name {_knownResourceName} --hours 2", true)]
    [InlineData($"--subscription {_knownSubscription} --resource-name {_knownResourceName} --event-level Error", true)]
    [InlineData($"--subscription {_knownSubscription} --resource-name {_knownResourceName} --top 20", true)]
    [InlineData($"--subscription {_knownSubscription}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testActivityLogs = new List<ActivityLogEventData>
            {
                new()
                {
                    Description = "Test activity log",
                    ResourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/test",
                    OperationName = new() { LocalizedValue = "Create Storage Account", Value = "Microsoft.Storage/storageAccounts/write" },
                    Level = ActivityLogEventLevel.Informational,
                    EventTimestamp = "2023-01-01T00:00:00Z",
                    Properties = []
                }
            };
            Service.ListActivityLogs(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<double>(),
                Arg.Any<ActivityLogEventLevel?>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(testActivityLogs);
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
    public async Task ExecuteAsync_ReturnsActivityLogsList()
    {
        // Arrange
        var expectedActivityLogs = new List<ActivityLogEventData>
        {
            new()
            {
                Description = "Storage account created",
                ResourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/test1",
                OperationName = new() { LocalizedValue = "Create Storage Account", Value = "Microsoft.Storage/storageAccounts/write" },
                Level = ActivityLogEventLevel.Informational,
                EventTimestamp = "2023-01-01T00:00:00Z",
                Properties = []
            },
            new()
            {
                Description = "Storage account updated",
                ResourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/test1",
                OperationName = new() { LocalizedValue = "Update Storage Account", Value = "Microsoft.Storage/storageAccounts/write" },
                Level = ActivityLogEventLevel.Warning,
                EventTimestamp = "2023-01-01T01:00:00Z",
                Properties = []
            }
        };

        Service.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedActivityLogs);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-name", _knownResourceName);

        // Assert
        // Verify the mock was called
        await Service.Received(1).ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.ActivityLogListCommandResult);

        Assert.Equal(expectedActivityLogs.Count, result.ActivityLogs.Count);
        Assert.Equal(expectedActivityLogs[0].Description, result.ActivityLogs[0].Description);
        Assert.Equal(expectedActivityLogs[0].Level, result.ActivityLogs[0].Level);
        Assert.Equal(expectedActivityLogs[1].Description, result.ActivityLogs[1].Description);
        Assert.Equal(expectedActivityLogs[1].Level, result.ActivityLogs[1].Level);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoActivityLogs()
    {
        // Arrange
        Service.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-name", _knownResourceName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.ActivityLogListCommandResult);

        Assert.Empty(result.ActivityLogs);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", _knownSubscription,
            "--resource-name", _knownResourceName);

        // Assert
        Assert.Equal((HttpStatusCode)500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Theory]
    [InlineData("--resource-name myResource --hours 24", true)]
    [InlineData("--resource-name myResource --event-level Critical", true)]
    [InlineData("--resource-name myResource --top 5", true)]
    [InlineData("--resource-name myResource --resource-type Microsoft.Storage/storageAccounts", true)]
    public async Task ExecuteAsync_HandlesOptionalParametersCorrectly(string partialArgs, bool shouldSucceed)
    {
        // Arrange
        var args = $"--subscription {_knownSubscription} {partialArgs}";

        Service.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(
            [
                new()
                {
                    Description = "Test activity log",
                    ResourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/test",
                    OperationName = new() { LocalizedValue = "Create Storage Account", Value = "Microsoft.Storage/storageAccounts/write" },
                    Level = ActivityLogEventLevel.Informational,
                    EventTimestamp = "2023-01-01T00:00:00Z",
                    Properties = []
                }
            ]);

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }
}
