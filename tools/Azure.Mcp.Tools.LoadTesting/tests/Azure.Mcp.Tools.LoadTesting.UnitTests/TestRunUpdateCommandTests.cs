// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

public class TestRunUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoadTestingService _service;
    private readonly ILogger<TestRunUpdateCommand> _logger;
    private readonly TestRunUpdateCommand _command;

    public TestRunUpdateCommandTests()
    {
        _service = Substitute.For<ILoadTestingService>();
        _logger = Substitute.For<ILogger<TestRunUpdateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateLoadTestRun_TestNotExisting()
    {
        var expected = new TestRun { TestId = "testId1", TestRunId = "testRunId1", DisplayName = "displayName" };
        _service.CreateOrUpdateLoadTestRunAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("run1"), Arg.Is((string?)null), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Is("displayName"), Arg.Is((string?)null), Arg.Is(false), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestRunUpdateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--testrun-id", "run1",
            "--tenant", "tenant123",
            "--test-id", "testId1",
            "--display-name", "displayName"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        var expected = new TestRun();
        _service.CreateOrUpdateLoadTestRunAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("run1"), Arg.Is((string?)null), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Is((string?)null), Arg.Is((string?)null), Arg.Is(false), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestRunUpdateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--tenant", "tenant123",
            "--testrun-id", "run1"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(400, response.Status);
    }

    private class TestRunUpdateCommandResult
    {
        public TestRun TestRun { get; set; } = new();
    }
}
