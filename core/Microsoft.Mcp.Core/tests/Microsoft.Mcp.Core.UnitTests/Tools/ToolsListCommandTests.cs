// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Tools.Commands;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Core.UnitTests.Server.Helpers;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Tools;

public class ToolsListCommandTests
{
    private const int SuccessStatusCode = 200;
    private const int ErrorStatusCode = 500;
    private const int MinimumExpectedCommands = 3;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolsListCommand> _logger;
    private readonly CommandContext _context;
    private readonly ToolsListCommand _command;
    private readonly Command _commandDefinition;
    private readonly MockCommandFactory _commandFactory;

    public ToolsListCommandTests()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();

        _commandFactory = new MockCommandFactory();
        collection.AddSingleton<ICommandFactory>(_commandFactory);

        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<ToolsListCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    /// <summary>
    /// Helper method to deserialize response results to CommandInfo list
    /// </summary>
    private static List<CommandInfo> DeserializeResults(object results)
    {
        var json = JsonSerializer.Serialize(results);
        return JsonSerializer.Deserialize<List<CommandInfo>>(json) ?? new List<CommandInfo>();
    }

    /// <summary>
    /// Verifies that the command returns a valid list of CommandInfo objects
    /// when executed with a properly configured context.
    /// </summary>

    [Fact]
    public async Task ExecuteAsync_WithValidContext_ReturnsCommandInfoList()
    {
        // Arrange
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Number of hidden commands in MockCommandFactory is 1.
        var expected = _commandFactory.AllCommands.Count - 1;
        Assert.Equal(expected, result.Count);

        foreach (var command in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(command.Name), "Command name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(command.Description), "Command description should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(command.Command), "Command path should not be empty");

            if (command.Options != null && command.Options.Count > 0)
            {
                foreach (var option in command.Options)
                {
                    Assert.False(string.IsNullOrWhiteSpace(option.Name), "Option name should not be empty");
                    Assert.False(string.IsNullOrWhiteSpace(option.Description), "Option description should not be empty");
                }
            }
        }
    }

    /// <summary>
    /// Verifies that JSON serialization and deserialization works correctly
    /// and preserves data integrity during round-trip operations.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_JsonSerializationStressTest_HandlesLargeResults()
    {
        // Arrange
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);

        var result = DeserializeResults(response.Results);
        Assert.NotNull(result);

        // Verify JSON round-trip preserves all data
        var serializedJson = JsonSerializer.Serialize(result);
        Assert.Equal(json, serializedJson);
    }

    /// <summary>
    /// Verifies that the command properly filters out hidden commands
    /// and only returns visible commands in the results.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidContext_FiltersHiddenCommands()
    {
        // Arrange
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);

        Assert.NotNull(result);

        Assert.DoesNotContain(result, cmd => cmd.Name.Equals(_commandFactory.HiddenCommand.Name));

        Assert.Contains(result, cmd => !string.IsNullOrEmpty(cmd.Name));
    }

    /// <summary>
    /// Verifies that commands include their options with proper validation
    /// and that option properties are correctly populated.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidContext_IncludesOptionsForCommands()
    {
        // Arrange
        var args = _commandDefinition.Parse([]);

        var commandName = "command_with_options";
        var optionName = "option1";
        var mockCommand = new MockCommand(commandName);

        mockCommand.AddOption(optionName);
        _commandFactory.AddCommand(commandName, mockCommand);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);

        Assert.NotNull(result);

        var commandWithOptions = result.Where(cmd => cmd.Options?.Count > 0).ToList();

        Assert.Single(commandWithOptions);

        var first = commandWithOptions.First();

        Assert.NotNull(first);
        Assert.NotNull(first.Options);
        Assert.NotEmpty(first.Options);

        var option = first.Options.First();
        Assert.NotNull(option.Name);
        Assert.NotNull(option.Description);
    }

    /// <summary>
    /// Verifies that the command handles null service provider gracefully
    /// and returns appropriate error response.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNullServiceProvider_HandlesGracefully()
    {
        // Arrange
        var faultyContext = new CommandContext(null!);
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(faultyContext, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(ErrorStatusCode, response.Status);
        Assert.Contains("cannot be null", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that the command handles corrupted command factory gracefully
    /// and returns appropriate error response with error details.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithCorruptedCommandFactory_HandlesGracefully()
    {
        // Arrange
        var faultyServiceProvider = Substitute.For<IServiceProvider>();
        faultyServiceProvider.GetService(typeof(ICommandFactory))
            .Returns(x => throw new InvalidOperationException("Corrupted command factory"));

        var faultyContext = new CommandContext(faultyServiceProvider);
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(faultyContext, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(ErrorStatusCode, response.Status);
        Assert.Contains("Corrupted command factory", response.Message);
    }

    /// <summary>
    /// Verifies that the command returns specific known commands from different areas
    /// and validates the structure and content of returned commands.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ReturnsSpecificKnownCommands()
    {
        // Arrange
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);

        Assert.NotNull(result);

        // Commmand "D" is a hidden command.
        Assert.Equal(3, result.Count);

        var names = result.Select(x => x.Name).ToArray();
        Assert.Contains("A", names);
        Assert.Contains("B", names);
        Assert.Contains("C", names);

        // Verify that each command has proper structure
        foreach (var cmd in result)
        {
            Assert.NotEmpty(cmd.Name);
            Assert.NotEmpty(cmd.Description);
            Assert.NotEmpty(cmd.Command);
        }
    }

    /// <summary>
    /// Verifies that command paths are properly formatted without extra spaces
    /// and follow consistent formatting conventions.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_CommandPathFormattingIsCorrect()
    {
        // Arrange
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);

        Assert.NotNull(result);

        foreach (var command in result)
        {
            // Command paths should not start or end with spaces
            Assert.False(command.Command.StartsWith(' '), $"Command '{command.Command}' should not start with space");
            Assert.False(command.Command.EndsWith(' '), $"Command '{command.Command}' should not end with space");

            // Command paths should not have double spaces
            Assert.DoesNotContain("  ", command.Command);
        }
    }

    /// <summary>
    /// Verifies that the command handles empty command factory gracefully
    /// and returns empty results when no commands are available.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithEmptyCommandFactory_ReturnsEmptyResults()
    {
        // Arrange
        var emptyCollection = new ServiceCollection();
        emptyCollection.AddLogging();

        // Create empty command factory with minimal dependencies
        var tempServiceProvider = emptyCollection.BuildServiceProvider();
        var logger = tempServiceProvider.GetRequiredService<ILogger<ICommandFactory>>();
        var telemetryService = Substitute.For<ITelemetryService>();
        var emptyAreaSetups = Array.Empty<IAreaSetup>();

        // Create a NEW service collection just for the empty command factory
        var finalCollection = new ServiceCollection();
        finalCollection.AddLogging();

        var emptyCommandFactory = Substitute.For<ICommandFactory>();
        finalCollection.AddSingleton(emptyCommandFactory);

        var emptyServiceProvider = finalCollection.BuildServiceProvider();
        var emptyContext = new CommandContext(emptyServiceProvider);
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(emptyContext, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(SuccessStatusCode, response.Status);

        var result = DeserializeResults(response.Results!);

        Assert.NotNull(result);
        Assert.Empty(result); // Should be empty when no commands are available
    }

    /// <summary>
    /// Verifies that the command metadata indicates it is non-destructive and read-only.
    /// </summary>
    [Fact]
    public void Metadata_IndicatesNonDestructiveAndReadOnly()
    {
        // Act
        var metadata = _command.Metadata;

        // Assert
        Assert.NotNull(metadata);
        Assert.False(metadata.Destructive, "Tool list command should not be destructive");
        Assert.True(metadata.ReadOnly, "Tool list command should be read-only");
    }

}
