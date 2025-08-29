// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Startups.Commands.Guidance;
using Azure.Mcp.Tools.Startups.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Startups.UnitTests;

[Trait("Area", "Startups")]
[Trait("Category", "Unit")]
public sealed class StartupsGuidanceCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStartupsService _startupsService;
    private readonly ILogger<StartupsGuidanceCommand> _logger;
    private readonly StartupsGuidanceCommand _command;

    public StartupsGuidanceCommandTests()
    {
        _startupsService = Substitute.For<IStartupsService>();
        _logger = Substitute.For<ILogger<StartupsGuidanceCommand>>();

        var collection = new ServiceCollection().AddSingleton(_startupsService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Act
        var command = _command.GetCommand();

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("guidance", command.Description.ToLowerInvariant());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResponseSuccessfully()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        // Verify the guidance content
        var guidanceJson = JsonSerializer.Serialize(response.Results);
        Assert.Contains("Microsoft for Startups", guidanceJson);
        Assert.Contains("https://startups.microsoft.com/", guidanceJson);
    }

    [Fact]
    public void GetCommand_HasCorrectOptions()
    {
        // Act
        var command = _command.GetCommand();

        // Assert
        Assert.NotNull(command);
        Assert.Equal("get", command.Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCorrectResponseStructure()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<CommandResponse>(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ContainsSamplePrompts()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        var guidanceJson = JsonSerializer.Serialize(response.Results);
        Assert.Contains("Quick Deployment", guidanceJson);
        Assert.Contains("React Development", guidanceJson);
        Assert.Contains("Deploy existing static website", guidanceJson);
    }

    [Fact]
    public async Task ExecuteAsync_ContainsValidationRules()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        var guidanceJson = JsonSerializer.Serialize(response.Results);
        Assert.Contains("storage-account", guidanceJson);
        Assert.Contains("source-path", guidanceJson);
        Assert.Contains("globally unique", guidanceJson);
    }
}
