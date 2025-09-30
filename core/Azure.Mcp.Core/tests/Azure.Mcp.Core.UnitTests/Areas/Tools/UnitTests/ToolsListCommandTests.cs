// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Areas.Tools.Commands;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Core.UnitTests.Areas.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Tools.UnitTests;

public class ToolsListCommandTests
{
    private const int MinimumExpectedCommands = 3;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolsListCommand> _logger;
    private readonly CommandContext _context;
    private readonly ToolsListCommand _command;
    private readonly Command _commandDefinition;

    public ToolsListCommandTests()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();

        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        collection.AddSingleton(commandFactory);

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

        foreach (var command in result)
        {
            Assert.False(string.IsNullOrWhiteSpace(command.Name), "Command name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(command.Description), "Command description should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(command.Command), "Command path should not be empty");

            Assert.StartsWith("azmcp ", command.Command);

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

        Assert.DoesNotContain(result, cmd => cmd.Name == "list" && cmd.Command.Contains("tool"));

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

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);

        Assert.NotNull(result);

        var commandWithOptions = result.FirstOrDefault(cmd => cmd.Options?.Count > 0);
        Assert.NotNull(commandWithOptions);
        Assert.NotNull(commandWithOptions.Options);
        Assert.NotEmpty(commandWithOptions.Options);

        var option = commandWithOptions.Options.First();
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
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
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
        faultyServiceProvider.GetService(typeof(CommandFactory))
            .Returns(x => throw new InvalidOperationException("Corrupted command factory"));

        var faultyContext = new CommandContext(faultyServiceProvider);
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(faultyContext, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
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
        Assert.NotEmpty(result);

        Assert.True(result.Count >= MinimumExpectedCommands, $"Expected at least {MinimumExpectedCommands} commands, got {result.Count}");

        var allCommands = result.Select(cmd => cmd.Command).ToList();

        // Should have subscription commands (commands include 'azmcp' prefix)
        var subscriptionCommands = result.Where(cmd => cmd.Command.Contains("subscription")).ToList();
        Assert.True(subscriptionCommands.Count > 0, $"Expected subscription commands. All commands: {string.Join(", ", allCommands)}");

        // Should have keyvault commands
        var keyVaultCommands = result.Where(cmd => cmd.Command.Contains("keyvault")).ToList();
        Assert.True(keyVaultCommands.Count > 0, $"Expected keyvault commands. All commands: {string.Join(", ", allCommands)}");

        // Should have storage commands
        var storageCommands = result.Where(cmd => cmd.Command.Contains("storage")).ToList();
        Assert.True(storageCommands.Count > 0, $"Expected storage commands. All commands: {string.Join(", ", allCommands)}");

        // Should have appconfig commands
        var appConfigCommands = result.Where(cmd => cmd.Command.Contains("appconfig")).ToList();
        Assert.True(appConfigCommands.Count > 0, $"Expected appconfig commands. All commands: {string.Join(", ", allCommands)}");

        // Verify specific known commands exist
        Assert.Contains(result, cmd => cmd.Command == "azmcp subscription list");
        Assert.Contains(result, cmd => cmd.Command == "azmcp keyvault key list");
        Assert.Contains(result, cmd => cmd.Command == "azmcp storage account get");
        Assert.Contains(result, cmd => cmd.Command == "azmcp appconfig account list");

        // Verify that each command has proper structure
        foreach (var cmd in result.Take(4))
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
    /// Verifies that the --namespaces switch returns only distinct top-level namespaces.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNamespaceSwitch_ReturnsNamespacesOnly()
    {
        // Arrange
        var args = _commandDefinition.Parse(new[] { "--namespaces" });

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        // Serialize then deserialize as list of CommandInfo
        var json = JsonSerializer.Serialize(response.Results);
        var namespaces = JsonSerializer.Deserialize<List<CommandInfo>>(json);

        Assert.NotNull(namespaces);
        Assert.NotEmpty(namespaces);

        // Should include some well-known namespaces (matching Name property)
        Assert.Contains(namespaces, ci => ci.Name.Equals("subscription", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(namespaces, ci => ci.Name.Equals("storage", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(namespaces, ci => ci.Name.Equals("keyvault", StringComparison.OrdinalIgnoreCase));

        foreach (var ns in namespaces!)
        {
            Assert.False(string.IsNullOrWhiteSpace(ns.Name));
            Assert.False(string.IsNullOrWhiteSpace(ns.Command));
            Assert.StartsWith("azmcp ", ns.Command, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(ns.Name, ns.Name.Trim());
            Assert.DoesNotContain(" ", ns.Name);
            // Namespace should not itself have options
            Assert.Null(ns.Options);
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
        var logger = tempServiceProvider.GetRequiredService<ILogger<CommandFactory>>();
        var telemetryService = Substitute.For<ITelemetryService>();
        var emptyAreaSetups = Array.Empty<IAreaSetup>();

        // Create a NEW service collection just for the empty command factory
        var finalCollection = new ServiceCollection();
        finalCollection.AddLogging();

        var emptyCommandFactory = new CommandFactory(tempServiceProvider, emptyAreaSetups, telemetryService, logger);
        finalCollection.AddSingleton(emptyCommandFactory);

        var emptyServiceProvider = finalCollection.BuildServiceProvider();
        var emptyContext = new CommandContext(emptyServiceProvider);
        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(emptyContext, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

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
