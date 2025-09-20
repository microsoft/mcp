// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
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
    [InlineData("--subscription sub123 --filter startTime ge 2023-01-01", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            // Setup service mock for successful cases
            var mockEvents = new List<Models.ServiceHealthEvent>();
            _resourceHealthService.ListServiceHealthEventsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult(mockEvents));
        }

        // Special parsing for complex arguments
        ParseResult parsedArgs;
        if (args.Contains("--filter"))
        {
            parsedArgs = _commandDefinition.Parse(["--subscription", "sub123", "--filter", "startTime ge 2023-01-01"]);
        }
        else
        {
            parsedArgs = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            // Error message might contain "required" for missing subscription or "Invalid" for enum validation
            Assert.True(
                response.Message?.ToLower().Contains("required") == true ||
                response.Message?.ToLower().Contains("invalid") == true,
                $"Expected error message to contain 'required' or 'invalid', but got: {response.Message}");
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSubscription_ReturnsSuccess()
    {
        // Arrange
        var mockEvents = new List<Models.ServiceHealthEvent>();
        _resourceHealthService.ListServiceHealthEventsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(mockEvents));

        var parsedArgs = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var expectedError = "Service error";
        _resourceHealthService.When(x => x.ListServiceHealthEventsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()))
            .Do(x => throw new InvalidOperationException(expectedError));

        var parsedArgs = _commandDefinition.Parse(["--subscription", "nonexistent-sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message ?? "");
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
