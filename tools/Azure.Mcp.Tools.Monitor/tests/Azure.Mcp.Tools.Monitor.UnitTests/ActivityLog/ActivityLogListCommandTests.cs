// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.ActivityLog;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.ActivityLog;

[Trait("Area", "Monitor")]
public sealed class ActivityLogListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorService _monitorService;
    private readonly ILogger<ActivityLogListCommand> _logger;
    private readonly ActivityLogListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string _knownSubscription = "knownSubscription";
    private const string _knownResourceName = "myResource";

    public ActivityLogListCommandTests()
    {
        _monitorService = Substitute.For<IMonitorService>();
        _logger = Substitute.For<ILogger<ActivityLogListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_monitorService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

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
                    Properties = new Dictionary<string, object>()
                }
            };
            _monitorService.ListActivityLogs(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<double>(),
                Arg.Any<ActivityLogEventLevel?>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(testActivityLogs);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args));

        // Assert
        Assert.Equal(shouldSucceed ? (HttpStatusCode)200 : (HttpStatusCode)400, response.Status);
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
                Properties = new Dictionary<string, object>()
            },
            new()
            {
                Description = "Storage account updated",
                ResourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/test1",
                OperationName = new() { LocalizedValue = "Update Storage Account", Value = "Microsoft.Storage/storageAccounts/write" },
                Level = ActivityLogEventLevel.Warning,
                EventTimestamp = "2023-01-01T01:00:00Z",
                Properties = new Dictionary<string, object>()
            }
        };

        _monitorService.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedActivityLogs);

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-name {_knownResourceName}"));

        // Assert
        Assert.Equal((HttpStatusCode)200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _monitorService.Received(1).ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.ActivityLogListCommandResult);

        Assert.NotNull(result);
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
        _monitorService.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(new List<ActivityLogEventData>());

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-name {_knownResourceName}"));

        // Assert
        Assert.Equal((HttpStatusCode)200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.ActivityLogListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.ActivityLogs);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _monitorService.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<ActivityLogEventData>>(new Exception("Test error")));

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse($"--subscription {_knownSubscription} --resource-name {_knownResourceName}"));

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

        _monitorService.ListActivityLogs(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<double>(),
            Arg.Any<ActivityLogEventLevel?>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(new List<ActivityLogEventData>
            {
                new()
                {
                    Description = "Test activity log",
                    ResourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/test",
                    OperationName = new() { LocalizedValue = "Create Storage Account", Value = "Microsoft.Storage/storageAccounts/write" },
                    Level = ActivityLogEventLevel.Informational,
                    EventTimestamp = "2023-01-01T00:00:00Z",
                    Properties = new Dictionary<string, object>()
                }
            });

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args));

        // Assert
        Assert.Equal(shouldSucceed ? (HttpStatusCode)200 : (HttpStatusCode)400, response.Status);
    }
}
