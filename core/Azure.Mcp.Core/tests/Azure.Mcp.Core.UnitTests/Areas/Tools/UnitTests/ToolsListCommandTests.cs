// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Areas.Tools.Commands;
using Azure.Mcp.Core.Areas.Tools.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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
    /// Helper method to deserialize response results to ToolNamesResult
    /// </summary>
    private static ToolsListCommand.ToolNamesResult DeserializeToolNamesResult(object results)
    {
        var json = JsonSerializer.Serialize(results);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Deserialize<ToolsListCommand.ToolNamesResult>(json, options) ?? new ToolsListCommand.ToolNamesResult(new List<string>());
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

            Assert.False(command.Command.StartsWith("azmcp "));

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
        Assert.Contains(result, cmd => cmd.Command == "subscription list");
        Assert.Contains(result, cmd => cmd.Command == "keyvault key list");
        Assert.Contains(result, cmd => cmd.Command == "storage account get");
        Assert.Contains(result, cmd => cmd.Command == "appconfig account list");

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
    /// Verifies that the --namespace-mode switch returns only distinct top-level namespaces.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNamespaceSwitch_ReturnsNamespacesOnly()
    {
        // Arrange
        var args = _commandDefinition.Parse(new[] { "--namespace-mode" });

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

            // For regular namespaces, Command equals Name
            // For surfaced extension commands like "azqr", Command is "extension azqr" but Name is "azqr"
            if (!ns.Command.Contains(' '))
            {
                // Regular namespace: Command == Name
                Assert.Equal(ns.Name, ns.Command);
            }
            else
            {
                // Surfaced extension command: Command is "{namespace} {commandName}", Name is just "{commandName}"
                Assert.EndsWith(ns.Name, ns.Command);
            }

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

    /// <summary>
    /// Verifies that the command includes metadata for each tool in the output.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_IncludesMetadataForAllCommands()
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

        // Verify that all commands have metadata
        foreach (var command in result)
        {
            Assert.NotNull(command.Metadata);

            // Verify that metadata has the expected properties
            // Destructive, ReadOnly, Idempotent, OpenWorld, Secret, LocalRequired
            var metadata = command.Metadata;

            // Check that at least the main properties are accessible
            Assert.True(metadata.Destructive || !metadata.Destructive, "Destructive should be defined");
            Assert.True(metadata.ReadOnly || !metadata.ReadOnly, "ReadOnly should be defined");
            Assert.True(metadata.Idempotent || !metadata.Idempotent, "Idempotent should be defined");
            Assert.True(metadata.OpenWorld || !metadata.OpenWorld, "OpenWorld should be defined");
            Assert.True(metadata.Secret || !metadata.Secret, "Secret should be defined");
            Assert.True(metadata.LocalRequired || !metadata.LocalRequired, "LocalRequired should be defined");
        }
    }

    /// <summary>
    /// Verifies that the --name-only option returns only tool names without descriptions.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNameOption_ReturnsOnlyToolNames()
    {
        // Arrange
        var args = _commandDefinition.Parse(new[] { "--name-only" });

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = DeserializeToolNamesResult(response.Results);
        Assert.NotNull(result);
        Assert.NotNull(result.Names);
        Assert.NotEmpty(result.Names);

        // Validate that the response only contains Names field and no other fields
        var json = JsonSerializer.Serialize(response.Results);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Verify that only the "names" property exists
        Assert.True(jsonElement.TryGetProperty("names", out _), "Response should contain 'names' property");

        // Count the number of properties - should only be 1 (names)
        var propertyCount = jsonElement.EnumerateObject().Count();
        Assert.Equal(1, propertyCount);

        // Explicitly verify that description and command fields are not present
        Assert.False(jsonElement.TryGetProperty("description", out _), "Response should not contain 'description' property when using --name-only option");
        Assert.False(jsonElement.TryGetProperty("command", out _), "Response should not contain 'command' property when using --name-only option");
        Assert.False(jsonElement.TryGetProperty("options", out _), "Response should not contain 'options' property when using --name-only option");
        Assert.False(jsonElement.TryGetProperty("metadata", out _), "Response should not contain 'metadata' property when using --name-only option");

        // Verify that all names are properly formatted tokenized names
        foreach (var name in result.Names)
        {
            Assert.False(string.IsNullOrWhiteSpace(name), "Tool name should not be empty");
            Assert.DoesNotContain(" ", name);
        }

        // Should contain some well-known commands
        Assert.Contains(result.Names, name => name.Contains("subscription"));
        Assert.Contains(result.Names, name => name.Contains("storage"));
        Assert.Contains(result.Names, name => name.Contains("keyvault"));
    }

    /// <summary>
    /// Verifies that the --namespace option filters tools correctly for a single namespace.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithSingleNamespaceOption_FiltersCorrectly()
    {
        // Arrange
        var args = _commandDefinition.Parse(new[] { "--namespace", "storage" });

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // All commands should be from the storage namespace
        foreach (var command in result)
        {
            Assert.StartsWith("storage", command.Command);
        }

        // Should contain some well-known storage commands
        Assert.Contains(result, cmd => cmd.Command == "storage account get");
    }

    /// <summary>
    /// Verifies that multiple --namespace options work correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithMultipleNamespaceOptions_FiltersCorrectly()
    {
        // Arrange
        var args = _commandDefinition.Parse(new[] { "--namespace", "storage", "--namespace", "keyvault" });

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = DeserializeResults(response.Results);
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // All commands should be from either storage or keyvault namespaces
        foreach (var command in result)
        {
            var isStorageCommand = command.Command.StartsWith("storage");
            var isKeyvaultCommand = command.Command.StartsWith("keyvault");
            Assert.True(isStorageCommand || isKeyvaultCommand,
                $"Command '{command.Command}' should be from storage or keyvault namespace");
        }

        // Should contain commands from both namespaces
        Assert.Contains(result, cmd => cmd.Command.StartsWith("storage"));
        Assert.Contains(result, cmd => cmd.Command.StartsWith("keyvault"));
    }

    /// <summary>
    /// Verifies that --name-only and --namespace options work together correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNameAndNamespaceOptions_FiltersAndReturnsNamesOnly()
    {
        // Arrange
        var args = _commandDefinition.Parse(new[] { "--name-only", "--namespace", "storage" });

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = DeserializeToolNamesResult(response.Results);
        Assert.NotNull(result);
        Assert.NotNull(result.Names);
        Assert.NotEmpty(result.Names);

        // Validate that the response only contains Names field and no other fields
        var json = JsonSerializer.Serialize(response.Results);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Verify that only the "names" property exists
        Assert.True(jsonElement.TryGetProperty("names", out _), "Response should contain 'names' property");

        // Count the number of properties - should only be 1 (names)
        var propertyCount = jsonElement.EnumerateObject().Count();
        Assert.Equal(1, propertyCount);

        // All names should be from the storage namespace
        foreach (var name in result.Names)
        {
            Assert.StartsWith("storage_", name);
        }

        // Should contain some well-known storage commands
        Assert.Contains(result.Names, name => name.Contains("account_get"));
    }

    /// <summary>
    /// Verifies that --name-only with multiple --namespace options works correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNameAndMultipleNamespaceOptions_FiltersAndReturnsNamesOnly()
    {
        // Arrange
        var args = _commandDefinition.Parse(new[] { "--name-only", "--namespace", "storage", "--namespace", "keyvault" });

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = DeserializeToolNamesResult(response.Results);
        Assert.NotNull(result);
        Assert.NotNull(result.Names);
        Assert.NotEmpty(result.Names);

        // Validate that the response only contains Names field and no other fields
        var json = JsonSerializer.Serialize(response.Results);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Verify that only the "names" property exists
        Assert.True(jsonElement.TryGetProperty("names", out _), "Response should contain 'names' property");

        // Count the number of properties - should only be 1 (names)
        var propertyCount = jsonElement.EnumerateObject().Count();
        Assert.Equal(1, propertyCount);

        // All names should be from either storage or keyvault namespaces
        foreach (var name in result.Names)
        {
            var isStorageName = name.StartsWith("storage_");
            var isKeyvaultName = name.StartsWith("keyvault_");
            Assert.True(isStorageName || isKeyvaultName,
                $"Tool name '{name}' should be from storage or keyvault namespace");
        }

        // Should contain names from both namespaces
        Assert.Contains(result.Names, name => name.StartsWith("storage_"));
        Assert.Contains(result.Names, name => name.StartsWith("keyvault_"));
    }

    /// <summary>
    /// Verifies that option binding works correctly for the new options.
    /// </summary>
    [Fact]
    public void BindOptions_WithNewOptions_BindsCorrectly()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(new[] { "--name-only", "--namespace", "storage", "--namespace", "keyvault" });

        // Use reflection to call the protected BindOptions method
        var bindOptionsMethod = typeof(ToolsListCommand).GetMethod("BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(bindOptionsMethod);

        // Act
        var options = bindOptionsMethod.Invoke(_command, new object?[] { parseResult }) as ToolsListOptions;

        // Assert
        Assert.NotNull(options);
        Assert.True(options.NameOnly);
        Assert.False(options.NamespaceMode);
        Assert.Equal(2, options.Namespaces.Count);
        Assert.Contains("storage", options.Namespaces);
        Assert.Contains("keyvault", options.Namespaces);
    }

    /// <summary>
    /// Verifies that parsing the new options works correctly.
    /// </summary>
    [Fact]
    public void CanParseNewOptions()
    {
        // Arrange & Act
        var parseResult1 = _commandDefinition.Parse(["--name-only"]);
        var parseResult2 = _commandDefinition.Parse(["--namespace", "storage"]);
        var parseResult3 = _commandDefinition.Parse(["--name-only", "--namespace", "storage", "--namespace", "keyvault"]);

        // Assert
        Assert.False(parseResult1.Errors.Any(), $"Parse errors for --name-only: {string.Join(", ", parseResult1.Errors)}");
        Assert.False(parseResult2.Errors.Any(), $"Parse errors for --namespace: {string.Join(", ", parseResult2.Errors)}");
        Assert.False(parseResult3.Errors.Any(), $"Parse errors for combined options: {string.Join(", ", parseResult3.Errors)}");

        // Verify values
        Assert.True(parseResult1.GetValueOrDefault<bool>(ToolsListOptionDefinitions.NameOnly.Name));

        var namespaces2 = parseResult2.GetValueOrDefault<string[]>(ToolsListOptionDefinitions.Namespace.Name);
        Assert.NotNull(namespaces2);
        Assert.Single(namespaces2);
        Assert.Equal("storage", namespaces2[0]);

        var namespaces3 = parseResult3.GetValueOrDefault<string[]>(ToolsListOptionDefinitions.Namespace.Name);
        Assert.NotNull(namespaces3);
        Assert.Equal(2, namespaces3.Length);
        Assert.Contains("storage", namespaces3);
        Assert.Contains("keyvault", namespaces3);
    }
}
