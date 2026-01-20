// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.UnitTests.PlatformLandingZone;

public class GetModificationGuidanceCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GetModificationGuidanceCommand> _logger;
    private readonly IPlatformLandingZoneGuidanceService _guidanceService;
    private readonly GetModificationGuidanceCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GetModificationGuidanceCommandTests()
    {
        _logger = Substitute.For<ILogger<GetModificationGuidanceCommand>>();
        _guidanceService = Substitute.For<IPlatformLandingZoneGuidanceService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_guidanceService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("getmodificationguidance", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--topic bastion")]
    [InlineData("--topic \"turn off ddos\"")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args)
    {
        // Arrange
        _guidanceService.GetModificationGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Sample guidance response");

        // Act
        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsScenarioCatalog_WhenEmptyQuestion()
    {
        // Arrange
        _guidanceService.GetModificationGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Available Azure Landing Zone modification scenarios: ...");

        var parseResult = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, Commands.AzureMigrateJsonContext.Default.GetModificationGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.Contains("Available Azure Landing Zone modification scenarios", result.Guidance);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _guidanceService.GetModificationGuidanceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("Bastion guidance: To enable Bastion, configure...");

        var parseResult = _commandDefinition.Parse(["--topic", "bastion"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, Commands.AzureMigrateJsonContext.Default.GetModificationGuidanceCommandResult);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Guidance);
    }

}
