// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.LoadTesting.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoadTestingService _service;
    private readonly ILogger<TestGetCommand> _logger;
    private readonly TestGetCommand _command;

    public TestGetCommandTests()
    {
        _service = Substitute.For<ILoadTestingService>();
        _logger = Substitute.For<ILogger<TestGetCommand>>();

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
    public async Task ExecuteAsync_ReturnsLoadTest_WhenExists()
    {
        var expected = new Test { TestId = "testId1", DisplayName = "TestDisplayName", Description = "TestDescription" };
        _service.GetTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expected.TestId, result.Test.TestId);
        Assert.Equal(expected.DisplayName, result.Test.DisplayName);
        Assert.Equal(expected.Description, result.Test.Description);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        var expected = new Test();
        _service.GetTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestGetCommand(_logger);
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
        _service.GetTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Test>(new Exception("Test error")));

        var command = new TestGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
