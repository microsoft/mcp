// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.LoadTesting.Commands;
using Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.UnitTests;

public class TestResourceCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoadTestingService _service;
    private readonly ILogger<TestResourceCreateCommand> _logger;
    private readonly TestResourceCreateCommand _command;

    public TestResourceCreateCommandTests()
    {
        _service = Substitute.For<ILoadTestingService>();
        _logger = Substitute.For<ILogger<TestResourceCreateCommand>>();

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
    public async Task ExecuteAsync_CreateLoadTests()
    {
        var expectedLoadTests = new TestResource { Id = "Id1", Name = "loadTest1" };
        _service.CreateOrUpdateLoadTestingResourceAsync(Arg.Is("sub123"), Arg.Is("resourceGroup123"), Arg.Is("testResourceName"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedLoadTests);

        var command = new TestResourceCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "testResourceName",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Id1", result.LoadTest.Id);
        Assert.Equal("loadTest1", result.LoadTest.Name);
    }


    [Fact]
    public async Task ExecuteAsync_CreateLoadTests_FromDefaultResource()
    {
        var expectedLoadTests = new TestResource { Id = "Id1", Name = "loadTest1" };
        _service.CreateOrUpdateLoadTestingResourceAsync(Arg.Is("sub123"), Arg.Is("resourceGroup123"), Arg.Is((string?)null), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedLoadTests);

        var command = new TestResourceCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Id1", result.LoadTest.Id);
        Assert.Equal("loadTest1", result.LoadTest.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.CreateOrUpdateLoadTestingResourceAsync(Arg.Is("sub123"), Arg.Is("resourceGroup123"), Arg.Is("loadTestName"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<TestResource>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "loadTestName",
            "--tenant", "tenant123"
        ]);
        var response = await _command.ExecuteAsync(context, parseResult);
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
