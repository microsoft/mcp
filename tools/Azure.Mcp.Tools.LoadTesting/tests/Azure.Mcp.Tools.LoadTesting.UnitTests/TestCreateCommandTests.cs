// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoadTestingService _service;
    private readonly ILogger<TestCreateCommand> _logger;
    private readonly TestCreateCommand _command;
    public TestCreateCommandTests()
    {
        _service = Substitute.For<ILoadTestingService>();
        _logger = Substitute.For<ILogger<TestCreateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_CreateLoadTest_WhenExists()
    {
        var expected = new Test { TestId = "testId1", DisplayName = "TestDisplayName", Description = "TestDescription" };
        _service.CreateTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"),
            Arg.Is("TestDisplayName"), Arg.Is("TestDescription"),
            Arg.Is((int?)20), Arg.Is((int?)50), Arg.Is((int?)1), Arg.Is("https://example.com/api/test"),
            Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123",
            "--display-name", "TestDisplayName",
            "--description", "TestDescription",
            "--duration", "20",
            "--virtual-users", "50",
            "--ramp-up-time", "1",
            "--endpoint", "https://example.com/api/test"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TestCreateCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expected.TestId, result.Test.TestId);
        Assert.Equal(expected.DisplayName, result.Test.DisplayName);
        Assert.Equal(expected.Description, result.Test.Description);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestErrors()
    {
        var expected = new Test();
        _service.CreateTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"),
            Arg.Is("TestDisplayName"), Arg.Is("TestDescription"),
            Arg.Is((int?)20), Arg.Is((int?)50), Arg.Is((int?)1), Arg.Is((string?)null),
            Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expected);

        var command = new TestCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(400, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.CreateTestAsync(
            Arg.Is("sub123"), Arg.Is("testResourceName"), Arg.Is("testId1"), Arg.Is("resourceGroup123"),
            Arg.Is("TestDisplayName"), Arg.Is("TestDescription"),
            Arg.Is((int?)20), Arg.Is((int?)50), Arg.Is((int?)1), Arg.Is("https://example.com/api/test"),
            Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Test>(new Exception("Test error")));

        var command = new TestCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--test-id", "testId1",
            "--tenant", "tenant123",
            "--display-name", "TestDisplayName",
            "--description", "TestDescription",
            "--duration", "20",
            "--virtual-users", "50",
            "--ramp-up-time", "1",
            "--endpoint", "https://example.com/api/test"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    private class TestCreateCommandResult
    {
        public Test Test { get; set; } = new();
    }
}
