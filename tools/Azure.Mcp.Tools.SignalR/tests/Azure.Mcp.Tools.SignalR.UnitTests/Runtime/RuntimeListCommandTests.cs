// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.UnitTests.Runtime;

public class RuntimeListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<RuntimeListCommand> _logger;
    private readonly RuntimeListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RuntimeListCommandTests()
    {
        _signalRService = Substitute.For<ISignalRService>();
        _logger = Substitute.For<ILogger<RuntimeListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_signalRService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1", true)]
    [InlineData("--subscription sub1 --tenant tenant1", true)] // Missing all required options
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testRuntimes = new List<Models.Runtime>
            {
                new() { Name = "test-signalr", ResourceGroupName = "test-rg", Location = "East US" }
            };

            _signalRService.ListRuntimesAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<AuthMethod?>(),
                    Arg.Any<RetryPolicyOptions>())
                .Returns(testRuntimes);
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
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
    public async Task ExecuteAsync_ReturnsRuntimesWhenFound()
    {
        // Arrange
        var expectedRuntimes = new List<Models.Runtime>
        {
            new()
            {
                Name = "test-signalr-1",
                ResourceGroupName = "test-rg",
                Location = "East US",
                SkuName = "Standard_S1"
            },
            new()
            {
                Name = "test-signalr-2",
                ResourceGroupName = "test-rg",
                Location = "West US",
                SkuName = "Free_F1"
            }
        };

        _signalRService.ListRuntimesAsync(
                "test-subscription",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedRuntimes);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        var json = JsonSerializer.Serialize(response.Results);

        var result =
            JsonSerializer.Deserialize<RuntimeListCommand.RuntimeListCommandResult>(json,
                Commands.SignalRJsonContext.Default.RuntimeListCommandResult);
        Assert.NotNull(result);
        var runtimeList = result.Runtimes.ToList();
        Assert.Equal(2, runtimeList.Count);
        Assert.Contains(runtimeList, r => r.Name == "test-signalr-1" && r.SkuName == "Standard_S1");
        Assert.Contains(runtimeList, r => r.Name == "test-signalr-2" && r.Location == "West US");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoRuntimesFound()
    {
        // Arrange
        _signalRService.ListRuntimesAsync(
                "test-subscription",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(new List<Models.Runtime>());

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _signalRService.ListRuntimesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<IEnumerable<Models.Runtime>>(new Exception("Service unavailable")));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "invalid-subscription"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service unavailable", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesServiceMethodCalled()
    {
        // Arrange
        var expectedRuntimes = new List<Models.Runtime> { new() { Name = "test-signalr" } };

        _signalRService.ListRuntimesAsync(
                "test-subscription",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedRuntimes);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription"
        ]);

        // Act
        await _command.ExecuteAsync(_context, parseResult);

        // Assert
        await _signalRService.Received(1).ListRuntimesAsync(
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<RetryPolicyOptions>());
    }
}
