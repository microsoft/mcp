// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Acr.Commands.Registry;
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Acr.UnitTests.Registry;

public class RegistryRepositoryListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAcrService _service;
    private readonly ILogger<RegistryRepositoryListCommand> _logger;
    private readonly RegistryRepositoryListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RegistryRepositoryListCommandTests()
    {
        _service = Substitute.For<IAcrService>();
        _logger = Substitute.For<ILogger<RegistryRepositoryListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData("--subscription sub", true)]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("--subscription sub --registry myacr", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _service.ListRegistryRepositories(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
                .Returns(new Dictionary<string, List<string>>
                {
                    ["myacr"] = new List<string> { "repo1", "repo2" }
                });
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.ListRegistryRepositories(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<Dictionary<string, List<string>>>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Empty_ReturnsNullResults()
    {
        // Arrange
        _service.ListRegistryRepositories(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(new Dictionary<string, List<string>>());

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
    }
}
