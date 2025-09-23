// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Acr.Commands;
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
                    ["myacr"] = ["repo1", "repo2"]
                });
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
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
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Empty_ReturnsEmptyResults()
    {
        // Arrange
        _service.ListRegistryRepositories(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AcrJsonContext.Default.RegistryRepositoryListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.RepositoriesByRegistry);
    }
}
