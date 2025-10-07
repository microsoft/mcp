// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.SignalR.Commands;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;
using Azure.Mcp.Tools.SignalR.Models;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.UnitTests.Runtime;

public class RuntimeGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<RuntimeGetCommand> _logger;
    private readonly RuntimeGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RuntimeGetCommandTests()
    {
        _signalRService = Substitute.For<ISignalRService>();
        _logger = Substitute.For<ILogger<RuntimeGetCommand>>();

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
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1", true)]
    [InlineData("--subscription sub1 --resource-group rg1", true)] // signalr is optional
    [InlineData("--subscription sub1 --resource-group rg1 --signalr s1", true)] // all provided
    [InlineData("--resource-group rg1 --signalr s1", false)] // subscription is required
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var runtimes = new List<Models.Runtime>
            {
                new()
                {
                    Name = "s1",
                    Location = "loc",
                    Identity = new Models.Identity
                    {
                        Type = "SystemAssigned"
                    },
                    Properties = new RuntimeProperties
                    {
                        ProvisioningState = "Succeeded",
                        HostName = "host",
                        NetworkAcls = new NetworkAcls
                        {
                            DefaultAction = "Allow",
                            PublicNetwork = new PublicNetwork
                            {
                                Allow = ["ClientConnection", "ServerConnection"]
                            }
                        },
                        UpstreamTemplates =
                        [
                            new()
                            {
                                Auth = new AuthSettings
                                {
                                    Type = "ManagedIdentity",
                                    Resource = "resource"
                                },
                                CategoryPattern = "category",
                                EventPattern = "event",
                                HubPattern = "hub",
                                UrlTemplate = "https://example.com/{userId}",
                            }
                        ]
                    }
                }
            };
            _signalRService.GetRuntimeAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions?>())
                .Returns(runtimes);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
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
    public async Task ExecuteAsync_ReturnsEmptyWhenNoRuntimes()
    {
        // Arrange
        _signalRService.GetRuntimeAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SignalRJsonContext.Default.RuntimeGetCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.Runtimes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _signalRService.GetRuntimeAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404NotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException(404, "Not Found");
        _signalRService.GetRuntimeAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(notFoundException);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        // Arrange
        var forbiddenException = new RequestFailedException(403, "Forbidden");
        _signalRService.GetRuntimeAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(forbiddenException);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }
}
