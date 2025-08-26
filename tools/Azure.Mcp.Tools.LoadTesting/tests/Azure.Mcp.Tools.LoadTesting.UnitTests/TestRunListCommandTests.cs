// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestRunListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoadTestingService _service;
    private readonly ILogger<TestRunListCommand> _logger;
    private readonly TestRunListCommand _command;

    public TestRunListCommandTests()
    {
        _service = Substitute.For<ILoadTestingService>();
        _logger = Substitute.For<ILogger<TestRunListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTestRuns_WhenExists()
    {
        var expected = new List<TestRun>
        {
            new TestRun { TestId = "testId1", TestRunId = "testRunId1" },
            new TestRun { TestId = "testId2", TestRunId = "testRunId2" }
        };
        _service.GetLoadTestRunsFromTestIdAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestRunListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TestRunListCommandResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.TestRun);
        Assert.True(result.TestRun.Count > 0, "TestRuns collection should not be empty");
        Assert.Equal(expected.First().TestId, result.TestRun.First().TestId);
        Assert.Equal(expected.First().TestRunId, result.TestRun.First().TestRunId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        var expected = new List<TestRun>();
        _service.GetLoadTestRunsFromTestIdAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestRunListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--load-test-name", "loadTestName",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(400, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.GetLoadTestRunsFromTestIdAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<TestRun>>(new Exception("Test error")));

        var command = new TestRunListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    private class TestRunListCommandResult
    {
        public List<TestRun> TestRun { get; set; } = new();
    }
}
