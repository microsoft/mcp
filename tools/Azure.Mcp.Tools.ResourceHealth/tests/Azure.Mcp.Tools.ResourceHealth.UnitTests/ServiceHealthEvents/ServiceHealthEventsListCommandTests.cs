// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResourceHealth.Commands.ServiceHealthEvents;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResourceHealth.UnitTests.ServiceHealthEvents;

public class ServiceHealthEventsListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IResourceHealthService _resourceHealthService;
    private readonly ILogger<ServiceHealthEventsListCommand> _logger;
    private readonly ServiceHealthEventsListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServiceHealthEventsListCommandTests()
    {
        _resourceHealthService = Substitute.For<IResourceHealthService>();
        _logger = Substitute.For<ILogger<ServiceHealthEventsListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_resourceHealthService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --event-type ServiceIssue", true)]
    [InlineData("--subscription sub123 --event-type InvalidType", false)]
    [InlineData("--subscription sub123 --status Active", true)]
    [InlineData("--subscription sub123 --status InvalidStatus", false)]
    [InlineData("--subscription sub123 --tracking-id TRACK123", true)]
    [InlineData("--subscription sub123 --filter \"startTime ge 2023-01-01\"", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parsedArgs = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act & Assert
        if (shouldSucceed)
        {
            var response = await _command.ExecuteAsync(_context, parsedArgs);
            Assert.NotNull(response);
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _command.ExecuteAsync(_context, parsedArgs));
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSubscription_ReturnsSuccess()
    {
        // Arrange
        var parsedArgs = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        _resourceHealthService.When(x => x.ListServiceHealthEventsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()))
            .Do(x => throw new InvalidOperationException("Service error"));

        var parsedArgs = _commandDefinition.Parse(["--subscription", "nonexistent-sub"]);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _command.ExecuteAsync(_context, parsedArgs));
    }

    [Theory]
    [InlineData("--subscription", "sub123")]
    [InlineData("--subscription", "sub123", "--event-type", "ServiceIssue", "--status", "Active")]
    public async Task ExecuteAsync_ReturnsValidJsonStructure(params string[] args)
    {
        // Arrange
        var parsedArgs = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert - Should have proper structure even if empty results
        Assert.NotNull(response);
    }
}
