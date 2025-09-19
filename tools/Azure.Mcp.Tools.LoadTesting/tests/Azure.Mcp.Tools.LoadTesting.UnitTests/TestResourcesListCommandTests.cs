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

public class TestResourceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoadTestingService _service;
    private readonly ILogger<TestResourceListCommand> _logger;
    private readonly TestResourceListCommand _command;

    public TestResourceListCommandTests()
    {
        _service = Substitute.For<ILoadTestingService>();
        _logger = Substitute.For<ILogger<TestResourceListCommand>>();

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
    public async Task ExecuteAsync_ReturnsLoadTests_FromResourceGroup()
    {
        var expectedLoadTests = new List<TestResource> { new() { Id = "Id1", Name = "loadTest1" }, new() { Id = "Id2", Name = "loadTest2" } };
        _service.GetLoadTestResourcesAsync(Arg.Is("sub123"), Arg.Is("resourceGroup123"), Arg.Is((string?)null), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedLoadTests);

        var command = new TestResourceListCommand(_logger);
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
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedLoadTests.Count, result.LoadTest.Count);
        Assert.Collection(result.LoadTest,
            item => Assert.Equal("Id1", item.Id),
            item => Assert.Equal("loadTest2", item.Name));
    }


    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTests_FromTestResource()
    {
        var expectedLoadTests = new List<TestResource> { new() { Id = "Id1", Name = "loadTest1" } };
        _service.GetLoadTestResourcesAsync(Arg.Is("sub123"), Arg.Is("resourceGroup123"), Arg.Is("testResourceName"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedLoadTests);

        var command = new TestResourceListCommand(_logger);
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
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedLoadTests.Count, result.LoadTest.Count);
        Assert.Collection(result.LoadTest,
            item => Assert.Equal("Id1", item.Id));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsLoadTests_WhenLoadTestsNotExist()
    {
        _service.GetLoadTestResourcesAsync(Arg.Is("sub123"), Arg.Is("resourceGroup123"), Arg.Is("loadTestName"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
             .Returns([]);

        var command = new TestResourceListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "resourceGroup123",
            "--test-resource-name", "loadTestName",
            "--tenant", "tenant123"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, LoadTestJsonContext.Default.TestResourceListCommandResult);

        Assert.Empty(result!.LoadTest);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.GetLoadTestResourcesAsync(Arg.Is("sub123"), Arg.Is("resourceGroup123"), Arg.Is("loadTestName"), Arg.Is("tenant123"), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<TestResource>>(new Exception("Test error")));

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
