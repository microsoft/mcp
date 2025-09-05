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
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("run1"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
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
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TestRunGetCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expected.TestId, result.TestRun.TestId);
        Assert.Equal(expected.TestRunId, result.TestRun.TestRunId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {

        var expected = new TestRun();
        _service.GetLoadTestRunAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("run1"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestRunGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(400, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {

        _service.GetLoadTestRunAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("run1"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
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
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    private class TestRunGetCommandResult
    {
        public TestRun TestRun { get; set; } = new();
    }
}
