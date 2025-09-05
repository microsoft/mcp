// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.SignalR.Commands.Identity;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.UnitTests.Identity;

public class IdentityShowCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<IdentityShowCommand> _logger;
    private readonly IdentityShowCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public IdentityShowCommandTests()
    {
        _signalRService = Substitute.For<ISignalRService>();
        _logger = Substitute.For<ILogger<IdentityShowCommand>>();

        var collection = new ServiceCollection().AddSingleton(_signalRService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("show", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("managed identity configuration", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --signalr-name signalr1", true)]
    [InlineData("--subscription sub1 --signalr-name signalr1", false)] // Missing resource-group
    [InlineData("--subscription sub1 --resource-group rg1", false)] // Missing signalr-name
    [InlineData("--resource-group rg1 --signalr-name signalr1", false)] // Missing subscription
    [InlineData("", false)] // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testIdentity = new Models.Identity
            {
                Type = "SystemAssigned",
                PrincipalId = "principal123",
                TenantId = "tenant123"
            };

            _signalRService.GetSignalRIdentityAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<AuthMethod?>(),
                    Arg.Any<RetryPolicyOptions>())
                .Returns(testIdentity);
        }

        var parseResult = _parser.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

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
    public async Task ExecuteAsync_ReturnsIdentityWhenFound()
    {
        // Arrange
        var expectedIdentity = new Models.Identity
        {
            Type = "SystemAssigned",
            PrincipalId = "test-principal-id",
            TenantId = "test-tenant-id"
        };

        _signalRService.GetSignalRIdentityAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedIdentity);

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenIdentityNotFound()
    {
        // Arrange
        _signalRService.GetSignalRIdentityAsync(
                "test-subscription",
                "test-rg",
                "nonexistent-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns((Models.Identity?)null);

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name",
            "nonexistent-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException()
    {
        // Arrange
        var exception = new RequestFailedException(404, "SignalR service not found");
        _signalRService.GetSignalRIdentityAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.Identity?>(exception));

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("SignalR service not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _signalRService.GetSignalRIdentityAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.Identity?>(new Exception("Test error")));

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
