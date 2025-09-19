// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
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

public class RegistryListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAcrService _service;
    private readonly ILogger<RegistryListCommand> _logger;
    private readonly RegistryListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RegistryListCommandTests()
    {
        _service = Substitute.For<IAcrService>();
        _logger = Substitute.For<ILogger<RegistryListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
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
    [InlineData("--subscription sub", true)]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Ensure environment variable fallback does not interfere with validation tests
        EnvironmentHelpers.SetAzureSubscriptionId(null);
        // Arrange
        if (shouldSucceed)
        {
            _service.ListRegistries(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(
                [
                    new("registry1", "eastus", "registry1.azurecr.io", "Basic", "Basic"),
                    new("registry2", "eastus2", "registry2.azurecr.io", "Standard", "Standard")
                ]);
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
        _service.ListRegistries(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.AcrRegistryInfo>>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersById_ReturnsFilteredRegistries()
    {
        // Arrange
        var expectedRegistries = new List<Models.AcrRegistryInfo> { new("registry1", null, null, null, null) };
        _service.ListRegistries("sub", "rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedRegistries);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        await _service.Received(1).ListRegistries("sub", "rg", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        _service.ListRegistries("sub", null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AcrJsonContext.Default.RegistryListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Registries);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedRegistryProperties()
    {
        // Arrange
        var registry = new Models.AcrRegistryInfo("myregistry", "eastus", "myregistry.azurecr.io", "Basic", "Basic");
        _service.ListRegistries("sub", null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([registry]);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }
}
