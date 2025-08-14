// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using AzureMcp.Areas.Startups.Commands.Guidance;
using AzureMcp.Areas.Startups.Services;
using AzureMcp.Models.Command;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Areas.Startups.UnitTests;

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

        var collection = new ServiceCollection();
        collection.AddSingleton(_startupsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
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
        var expectedGuidance = new StartupsGuidanceInfo(
            Title: "Microsoft for Startups Guidance",
            Description: "Microsoft for Startups is a global program",
            Link: "https://startups.microsoft.com/"
        );
        
        _startupsService.GetGuidanceAsync()
            .Returns(expectedGuidance);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

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
    public async Task ExecuteAsync_CallsServiceOnce()
    {
        // Arrange
        var expectedGuidance = new StartupsGuidanceInfo(
            Title: "Test Title",
            Description: "Test Description", 
            Link: "https://test.com"
        );
        
        _startupsService.GetGuidanceAsync()
            .Returns(expectedGuidance);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("");

        // Act
        await _command.ExecuteAsync(context, parseResult);

        // Assert
        await _startupsService.Received(1).GetGuidanceAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("--help")]
    public async Task ExecuteAsync_AcceptsValidArguments(string args)
    {
        // Arrange
        var expectedGuidance = new StartupsGuidanceInfo(
            Title: "Test",
            Description: "Test", 
            Link: "https://test.com"
        );
        
        _startupsService.GetGuidanceAsync()
            .Returns(expectedGuidance);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        if (args != "--help")
        {
            Assert.Equal(200, response.Status);
        }
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
        var expectedGuidance = new StartupsGuidanceInfo(
            Title: "Microsoft for Startups Guidance",
            Description: "Test description",
            Link: "https://startups.microsoft.com/"
        );
        
        _startupsService.GetGuidanceAsync()
            .Returns(expectedGuidance);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response);
        Assert.IsType<CommandResponse>(response);
        Assert.NotNull(response.Results);
    }
}