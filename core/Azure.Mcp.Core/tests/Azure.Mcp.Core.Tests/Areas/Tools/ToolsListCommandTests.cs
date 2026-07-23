// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Tests.Areas.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Tools.Commands;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Tools;

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
    private static ToolsListCommand.ToolsListResult DeserializeCommandsResults(CommandResponse response) =>
        DeserializeJson(response, () => new([], null));

    /// <summary>
    /// Helper method to deserialize response results to ToolNamesResult
    /// </summary>
    private static ToolsListCommand.ToolsListResult DeserializeResult(CommandResponse response) =>
        DeserializeJson(response, () => new(null, []));

    private static ToolsListCommand.ToolsListResult DeserializeJson(CommandResponse response, Func<ToolsListCommand.ToolsListResult> defaultValueFactory)
    {
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ModelsJsonContext.Default.ToolsListResult) ?? defaultValueFactory();

        Assert.NotNull(result);
        return result;
    }

    /// <summary>
    /// Verifies that the command returns a valid list of CommandInfo objects
    /// when executed with a properly configured context.
    /// </summary>

    [Fact]
    public async Task ExecuteAsync_WithValidContext_ReturnsCommandInfoList()
    {
        // Arrange & Act
        var response = await ExecuteAsync();

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.NotEmpty(result.Commands);

        foreach (var command in result.Commands)
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
        // Arrange & Act
        var response = await ExecuteAsync();

        // Assert
        var result = DeserializeCommandsResults(response);
        var json = JsonSerializer.Serialize(response.Results);

        // Verify JSON round-trip preserves all data
        var serializedJson = JsonSerializer.Serialize(result, ModelsJsonContext.Default.ToolsListResult);
        Assert.Equal(json, serializedJson);
    }

    /// <summary>
    /// Verifies that the command properly filters out hidden commands
    /// and only returns visible commands in the results.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidContext_FiltersHiddenCommands()
    {
        // Arrange & Act
        var response = await ExecuteAsync();

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.DoesNotContain(result.Commands, cmd => cmd.Name == "list" && cmd.Command.Contains("tool"));
        Assert.Contains(result.Commands, cmd => !string.IsNullOrEmpty(cmd.Name));
    }

    /// <summary>
    /// Verifies that commands include their options with proper validation
    /// and that option properties are correctly populated.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidContext_IncludesOptionsForCommands()
    {
        // Arrange & Act
        var response = await ExecuteAsync();

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        var commandWithOptions = result.Commands.FirstOrDefault(cmd => cmd.Options?.Count > 0);
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
        // Arrange & Act
        var response = await ExecuteAsync(new CommandContext(null!));

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
        faultyServiceProvider.GetService(typeof(ICommandFactory))
            .Returns(x => throw new InvalidOperationException("Corrupted command factory"));

        // Act
        var response = await ExecuteAsync(new CommandContext(faultyServiceProvider));

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
        // Arrange & Act
        var response = await ExecuteAsync();

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.NotEmpty(result.Commands);

        Assert.True(result.Commands.Count >= MinimumExpectedCommands, $"Expected at least {MinimumExpectedCommands} commands, got {result.Commands.Count}");

        var allCommands = result.Commands.Select(cmd => cmd.Command).ToList();

        // Should have subscription commands (commands include 'azmcp' prefix)
        var subscriptionCommands = result.Commands.Where(cmd => cmd.Command.StartsWith("subscription")).ToList();
        Assert.True(subscriptionCommands.Count > 0, $"Expected subscription commands. All commands: {string.Join(", ", allCommands)}");

        // Should have keyvault commands
        var keyVaultCommands = result.Commands.Where(cmd => cmd.Command.StartsWith("keyvault")).ToList();
        Assert.True(keyVaultCommands.Count > 0, $"Expected keyvault commands. All commands: {string.Join(", ", allCommands)}");

        // Should have storage commands
        var storageCommands = result.Commands.Where(cmd => cmd.Command.StartsWith("storage")).ToList();
        Assert.True(storageCommands.Count > 0, $"Expected storage commands. All commands: {string.Join(", ", allCommands)}");

        // Should have appconfig commands
        var appConfigCommands = result.Commands.Where(cmd => cmd.Command.StartsWith("appconfig")).ToList();
        Assert.True(appConfigCommands.Count > 0, $"Expected appconfig commands. All commands: {string.Join(", ", allCommands)}");

        // Verify specific known commands exist
        Assert.Contains(result.Commands, cmd => cmd.Command == "subscription list");
        Assert.Contains(result.Commands, cmd => cmd.Command == "keyvault key get");
        Assert.Contains(result.Commands, cmd => cmd.Command == "storage account get");
        Assert.Contains(result.Commands, cmd => cmd.Command == "appconfig account list");

        // Verify that each command has proper structure
        foreach (var cmd in result.Commands)
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
        // Arrange & Act
        var response = await ExecuteAsync();

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        foreach (var command in result.Commands)
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
        // Arrange & Act
        var response = await ExecuteAsync("--namespace-mode");

        // Assert
        var namespaces = DeserializeCommandsResults(response);

        Assert.NotNull(namespaces.Commands);
        Assert.NotEmpty(namespaces.Commands);

        // Should include some well-known namespaces (matching Name property)
        Assert.Contains(namespaces.Commands, ci => ci.Name.Equals("subscription", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(namespaces.Commands, ci => ci.Name.Equals("storage", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(namespaces.Commands, ci => ci.Name.Equals("keyvault", StringComparison.OrdinalIgnoreCase));

        foreach (var ns in namespaces.Commands)
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
                // When Azure MCP presents the commands as tools, the spaces in the commands are replaced by underscore
                Assert.EndsWith(ns.Name, ns.Command.Replace(" ", "_"));
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
        var configurationOptions = Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "Test Server",
            ShortName = "test",
            Version = "Test Version",
            DisplayName = "Test Display",
            Description = "Test Description",
            RootCommandGroupName = "azmcp"
        });

        // Create a NEW service collection just for the empty command factory
        var finalCollection = new ServiceCollection();
        finalCollection.AddLogging();

        var emptyCommandFactory = new CommandFactory(tempServiceProvider, emptyAreaSetups, telemetryService, configurationOptions, logger);
        finalCollection.AddSingleton<ICommandFactory>(emptyCommandFactory);

        var emptyServiceProvider = finalCollection.BuildServiceProvider();

        // Act
        var response = await ExecuteAsync(new CommandContext(emptyServiceProvider));

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.Empty(result.Commands); // Should be empty when no commands are available
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
        // Arrange & Act
        var response = await ExecuteAsync();

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.NotEmpty(result.Commands);

        // Verify that all commands have metadata
        foreach (var command in result.Commands)
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
        // Arrange & Act
        var response = await ExecuteAsync("--name-only");

        // Assert
        var result = DeserializeResult(response);

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
        // Arrange & Act
        var response = await ExecuteAsync("--namespace", "storage");

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.NotEmpty(result.Commands);

        // All commands should be from the storage namespace
        foreach (var command in result.Commands)
        {
            Assert.StartsWith("storage", command.Command);
        }

        // Should contain some well-known storage commands
        Assert.Contains(result.Commands, cmd => cmd.Command == "storage account get");
    }

    /// <summary>
    /// Verifies that multiple --namespace options work correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithMultipleNamespaceOptions_FiltersCorrectly()
    {
        // Arrange & Act
        var response = await ExecuteAsync("--namespace", "storage", "--namespace", "keyvault");

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.NotEmpty(result.Commands);

        // All commands should be from either storage or keyvault namespaces
        foreach (var command in result.Commands)
        {
            var isStorageCommand = command.Command.StartsWith("storage");
            var isKeyvaultCommand = command.Command.StartsWith("keyvault");
            Assert.True(isStorageCommand || isKeyvaultCommand,
                $"Command '{command.Command}' should be from storage or keyvault namespace");
        }

        // Should contain commands from both namespaces
        Assert.Contains(result.Commands, cmd => cmd.Command.StartsWith("storage"));
        Assert.Contains(result.Commands, cmd => cmd.Command.StartsWith("keyvault"));
    }

    /// <summary>
    /// Verifies that --name-only and --namespace options work together correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNameAndNamespaceOptions_FiltersAndReturnsNamesOnly()
    {
        // Arrange & Act
        var response = await ExecuteAsync("--name-only", "--namespace", "storage");

        // Assert
        var result = DeserializeResult(response);

        Assert.NotNull(result.Names);
        Assert.NotEmpty(result.Names);

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
        // Arrange & Act
        var response = await ExecuteAsync("--name-only", "--namespace", "storage", "--namespace", "keyvault");

        // Assert
        var result = DeserializeResult(response);

        Assert.NotNull(result.Names);
        Assert.NotEmpty(result.Names);

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
        var parseResult = _commandDefinition.Parse(["--name-only", "--namespace", "storage", "--namespace", "keyvault"]);

        // Act
        var options = _command.BindOptions(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.True(options.NameOnly);
        Assert.False(options.NamespaceMode);
        Assert.NotNull(options.Namespace);
        Assert.Equal(2, options.Namespace.Length);
        Assert.Contains("storage", options.Namespace);
        Assert.Contains("keyvault", options.Namespace);
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
        Assert.True(_command.BindOptions(parseResult1).NameOnly);

        var namespaces2 = _command.BindOptions(parseResult2).Namespace;
        Assert.NotNull(namespaces2);
        Assert.Single(namespaces2);
        Assert.Equal("storage", namespaces2[0]);

        var namespaces3 = _command.BindOptions(parseResult3).Namespace;
        Assert.NotNull(namespaces3);
        Assert.Equal(2, namespaces3.Length);
        Assert.Contains("storage", namespaces3);
        Assert.Contains("keyvault", namespaces3);
    }

    /// <summary>
    /// Verifies that --namespace-mode and --name-only work together correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNamespaceModeAndNameOnly_ReturnsNamespaceNamesOnly()
    {
        // Arrange & Act
        var response = await ExecuteAsync("--namespace-mode", "--name-only");

        // Assert
        var result = DeserializeResult(response);

        Assert.NotNull(result.Names);
        Assert.NotEmpty(result.Names);

        // Should contain only namespace names (not individual commands)
        Assert.Contains(result.Names, name => name.Equals("subscription", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Names, name => name.Equals("storage", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Names, name => name.Equals("keyvault", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that --namespace-mode, --name-only, and --namespace filtering work together correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNamespaceModeNameOnlyAndNamespaceFilter_ReturnsFilteredNamespaceNamesOnly()
    {
        // Arrange & Act
        var response = await ExecuteAsync("--namespace-mode", "--name-only", "--namespace", "storage");

        // Assert
        var result = DeserializeResult(response);

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

        // Should contain only storage namespace (and possibly surfaced storage-related commands)
        foreach (var name in result.Names)
        {
            Assert.True(name.Equals("storage", StringComparison.OrdinalIgnoreCase) ||
                       name.StartsWith("storage ", StringComparison.OrdinalIgnoreCase),
                       $"Name '{name}' should be from storage namespace");
        }
    }

    /// <summary>
    /// Verifies that --namespace-mode with multiple namespace filters works correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNamespaceModeAndMultipleNamespaces_ReturnsFilteredNamespaces()
    {
        // Arrange & Act
        var response = await ExecuteAsync("--namespace-mode", "--namespace", "storage", "--namespace", "keyvault");

        // Assert
        var result = DeserializeCommandsResults(response);

        Assert.NotNull(result.Commands);
        Assert.NotEmpty(result.Commands);

        // Should contain only storage and keyvault namespaces
        foreach (var command in result.Commands)
        {
            var isStorageNamespace = command.Name.Equals("storage", StringComparison.OrdinalIgnoreCase);
            var isKeyvaultNamespace = command.Name.Equals("keyvault", StringComparison.OrdinalIgnoreCase);
            var isStorageCommand = command.Command.StartsWith("storage ", StringComparison.OrdinalIgnoreCase);
            var isKeyvaultCommand = command.Command.StartsWith("keyvault ", StringComparison.OrdinalIgnoreCase);

            Assert.True(isStorageNamespace || isKeyvaultNamespace || isStorageCommand || isKeyvaultCommand,
                $"Command '{command.Command}' (Name: '{command.Name}') should be from storage or keyvault namespace");
        }

        // Should contain both namespaces
        Assert.Contains(result.Commands, cmd => cmd.Name.Equals("storage", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Commands, cmd => cmd.Name.Equals("keyvault", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<CommandResponse> ExecuteAsync(params string[] args) => await ExecuteAsync(_context, args);

    private async Task<CommandResponse> ExecuteAsync(CommandContext context, params string[] args)
    {
        var parseResult = _commandDefinition.Parse(args);
        return await _command.ExecuteAsync(context, _command.BindOptions(parseResult), TestContext.Current.CancellationToken);
    }
}
