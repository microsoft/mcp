// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.LoadTesting.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestRunGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoadTestingService _service;
    private readonly ILogger<TestRunGetCommand> _logger;
    private readonly TestRunGetCommand _command;

    public TestRunGetCommandTests()
    {
        _service = Substitute.For<ILoadTestingService>();
        _logger = Substitute.For<ILogger<TestRunGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTestRun_WhenExists()
    {
        var expected = new TestRun { TestId = "testId1", TestRunId = "testRunId1" };
        _service.GetLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("run1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var command = new TestRunGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestRunGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.TestRuns);
        Assert.Single(result.TestRuns);
        Assert.Equal(expected.TestId, result.TestRuns.First().TestId);
        Assert.Equal(expected.TestRunId, result.TestRuns.First().TestRunId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {

        var expected = new TestRun();
        _service.GetLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("run1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var command = new TestRunGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {

        _service.GetLoadTestRunAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("run1"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<TestRun>(new Exception("Test error")));

        var command = new TestRunGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTestRuns_WhenTestIdProvided()
    {
        var expected = new List<TestRun>
        {
            new() { TestId = "testId1", TestRunId = "testRunId1" },
            new() { TestId = "testId2", TestRunId = "testRunId2" }
        };
        _service.GetLoadTestRunsFromTestIdAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var command = new TestRunGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestRunGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.TestRuns);
        Assert.Equal(2, result.TestRuns.Count);
        Assert.Equal(expected.First().TestId, result.TestRuns.First().TestId);
        Assert.Equal(expected.First().TestRunId, result.TestRuns.First().TestRunId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesListServiceErrors()
    {
        _service.GetLoadTestRunsFromTestIdAsync(
            Arg.Is("sub123"),
            Arg.Is("testResourceName"),
            Arg.Is("testId"),
            Arg.Is("resourceGroup123"),
            Arg.Is("tenant123"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<TestRun>>(new Exception("Test error")));

        var command = new TestRunGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
