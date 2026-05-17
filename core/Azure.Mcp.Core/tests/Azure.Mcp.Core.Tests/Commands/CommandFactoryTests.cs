// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Commands;

public class CommandFactoryTests
{
    private const string FullCommandName1 = "root_directCommand";
    private const string FullCommandName2 = "root_subgroup1_directCommand2";
    private const string FullCommandName3 = "root_subgroup1_directCommand3";
    private const string FullCommandName4 = "root_subgroup2_directCommand4";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandFactory> _logger;
    private readonly ITelemetryService _telemetryService;
    private readonly McpServerConfiguration _serverConfiguration;
    private readonly IOptions<McpServerConfiguration> _configurationOptions;

    public CommandFactoryTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        _serverConfiguration = new McpServerConfiguration
        {
            Name = "Test Server",
            Version = "Test Version",
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        };

        _serviceProvider = services.BuildServiceProvider();
        _logger = Substitute.For<ILogger<CommandFactory>>();
        _telemetryService = Substitute.For<ITelemetryService>();
        _configurationOptions = Microsoft.Extensions.Options.Options.Create(_serverConfiguration);
    }

    [Fact]
    public void Separator_Should_Be_Underscore()
    {
        // This test verifies our fix for supporting dashes in command names
        // by ensuring the separator is underscore instead of dash

        // Arrange & Act
        var separator = CommandFactory.Separator;

        // Assert
        Assert.Equal('_', separator);
    }

    [Theory]
    [InlineData("subscription", "list", "subscription_list")]
    [InlineData("storage", "account_list", "storage_account_list")]
    [InlineData("role", "assignment_list", "role_assignment_list")]
    [InlineData("azmcp", "subscription_list", "azmcp_subscription_list")]
    public void GetPrefix_Should_Use_Underscore_Separator(string currentPrefix, string additional, string expected)
    {
        // This test verifies that command hierarchies are joined with underscores
        // which allows commands to use dashes naturally without conflicting with separators

        // Arrange & Act
        var result = CallGetPrefix(currentPrefix, additional);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetPrefix_Should_Handle_Empty_CurrentPrefix()
    {
        // Arrange & Act
        var result = CallGetPrefix(string.Empty, "subscription");

        // Assert
        Assert.Equal("subscription", result);
    }

    [Fact]
    public void GetPrefix_Should_Handle_Null_CurrentPrefix()
    {
        // Arrange & Act
        var result = CallGetPrefix(null!, "subscription");

        // Assert
        Assert.Equal("subscription", result);
    }

    [Theory]
    [InlineData("list-roles")]
    [InlineData("get-resource-group")]
    [InlineData("create-storage-account")]
    public void Command_Names_With_Dashes_Should_Not_Conflict_With_Separator(string commandNameWithDash)
    {
        // This test verifies that command names containing dashes don't conflict
        // with our underscore separator, which was the core issue we're solving

        // Arrange
        var prefix = "azmcp_role";

        // Act
        var result = CallGetPrefix(prefix, commandNameWithDash);

        // Assert
        Assert.Contains('_', result); // Should contain our separator
        Assert.Contains('-', result); // Should preserve dashes in command names
        Assert.Equal($"{prefix}_{commandNameWithDash}", result);

        // Verify the dash in the command name doesn't interfere with parsing
        var parts = result.Split('_');
        Assert.True(parts.Length >= 2);
        Assert.Equal("azmcp", parts[0]);
        Assert.Equal("role", parts[1]);
        Assert.Equal(commandNameWithDash, parts[2]);
    }

    [Fact]
    public void Constructor_Throws_AreaSetups_Duplicate()
    {
        // Arrange
        var duplicate = "Duplicate Name";
        var area = CreateIAreaSetup(duplicate);
        var area1 = CreateIAreaSetup("name1");
        var area2 = CreateIAreaSetup(duplicate);

        var serviceAreas = new List<IAreaSetup> { area, area1, area2 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger));
    }

    [Fact]
    public void Constructor_Throws_AreaSetups_EmptyName()
    {
        // Arrange
        var area = CreateIAreaSetup("Name");
        var area1 = CreateIAreaSetup("Name1");
        var area2 = CreateIAreaSetup(string.Empty);

        var serviceAreas = new List<IAreaSetup> { area, area1, area2 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger));
    }

    [Theory]
    [InlineData("name3_directCommand", "name3")]
    [InlineData("name2_subgroup1_directCommand2", "name2")]
    [InlineData("name3_subgroup2_directCommand4", "name3")]
    public void GetServiceArea_Existing_SetupArea(string commandName, string expected)
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var area2 = CreateIAreaSetup("name2");
        var area3 = CreateIAreaSetup("name3");

        var serviceAreas = new List<IAreaSetup> { area1, area3, area2 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act
        // Try in the case that the root prefix is not used.  This is in the case that the tool
        // is created using the IAreaSetup name as root.
        var actual2 = factory.GetServiceArea(commandName);

        // Assert
        Assert.Equal(expected, actual2);
    }

    [Fact]
    public void GetServiceArea_DoesNotExist()
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var area2 = CreateIAreaSetup("name2");
        var area3 = CreateIAreaSetup("name3");

        var serviceAreas = new List<IAreaSetup> { area1, area2, area3 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // All commands created in command factory are prefixed with the root command group, "azmcp".
        var commandNameToTry = "azmcp" + CommandFactory.Separator + "name0_subgroup2_directCommand4";

        // Act
        var actual = factory.GetServiceArea(commandNameToTry);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void CommandDictionaryCreated_WithPrefix()
    {
        // Arrange
        var prefix = "abc";
        var commandGroup = CreateCommandGroup();

        // Act
        var commandDictionary = CommandFactory.CreateCommandDictionaryInner(commandGroup, prefix);

        // Assert
        Assert.NotNull(commandDictionary);
        Assert.NotEmpty(commandDictionary);

        // Expected 4 commands to be created.
        Assert.Equal(4, commandDictionary.Count);

        Assert.Contains($"abc_{FullCommandName1}", commandDictionary.Keys);
        Assert.Contains($"abc_{FullCommandName2}", commandDictionary.Keys);
        Assert.Contains($"abc_{FullCommandName3}", commandDictionary.Keys);
        Assert.Contains($"abc_{FullCommandName4}", commandDictionary.Keys);
    }

    [Fact]
    public void CommandDictionaryCreated_EmptyPrefix()
    {
        // Arrange
        var commandGroup = CreateCommandGroup();

        // Act
        var commandDictionary = CommandFactory.CreateCommandDictionaryInner(commandGroup, string.Empty);

        // Assert
        Assert.NotNull(commandDictionary);
        Assert.NotEmpty(commandDictionary);

        // Expected 4 commands to be created.
        Assert.Equal(4, commandDictionary.Count);

        Assert.Contains(FullCommandName1, commandDictionary.Keys);
        Assert.Contains(FullCommandName2, commandDictionary.Keys);
        Assert.Contains(FullCommandName3, commandDictionary.Keys);
        Assert.Contains(FullCommandName4, commandDictionary.Keys);
    }

    /// <summary>
    /// Helper method to access the private GetPrefix method via reflection
    /// </summary>
    private static string CallGetPrefix(string currentPrefix, string additional)
    {
        return CommandFactory.GetPrefix(currentPrefix, additional);
    }

    [Fact]
    public void LearnOption_ExistsOnGroupCommand_AfterFactoryCreation()
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var area2 = CreateIAreaSetup("name2");
        var serviceAreas = new List<IAreaSetup> { area1, area2 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act
        var name1Group = factory.RootGroup.SubGroup.First(g => g.Name == "name1");
        var learnOption = name1Group.Command.Options.FirstOrDefault(CommandFactory.IsLearnOption);

        // Assert
        Assert.NotNull(learnOption);
        Assert.IsType<Option<bool>>(learnOption);
    }

    [Fact]
    public void LearnOption_ExistsOnLeafCommand_AfterFactoryCreation()
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var serviceAreas = new List<IAreaSetup> { area1 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act
        // Find a leaf command's System.CommandLine Command object via the subgroup structure
        var name1Group = factory.RootGroup.SubGroup.First(g => g.Name == "name1");
        // The leaf command is in the subgroup Commands collection
        var leafCommand = name1Group.Commands.Values.FirstOrDefault();
        var leafSystemCommand = leafCommand?.GetCommand();
        var learnOption = leafSystemCommand?.Options.FirstOrDefault(CommandFactory.IsLearnOption);

        // Assert
        Assert.NotNull(learnOption);
        Assert.IsType<Option<bool>>(learnOption);
    }

    [Fact]
    public void LearnOption_ExistsOnNestedGroupCommand()
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var serviceAreas = new List<IAreaSetup> { area1 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act - verify nested subgroup (subgroup1) also has --learn
        var name1Group = factory.RootGroup.SubGroup.First(g => g.Name == "name1");
        var subgroup1 = name1Group.SubGroup.FirstOrDefault(g => g.Name == "subgroup1");
        var learnOption = subgroup1?.Command.Options.FirstOrDefault(CommandFactory.IsLearnOption);

        // Assert
        Assert.NotNull(subgroup1);
        Assert.NotNull(learnOption);
        Assert.IsType<Option<bool>>(learnOption);
    }

    [Fact]
    public void LearnOption_InvokedAtRootLevel_OutputsAllCommandsAsJson()
    {
        // Arrange — two areas, each with 4 commands = 8 total visible commands
        var area1 = CreateIAreaSetup("name1");
        var area2 = CreateIAreaSetup("name2");
        var serviceAreas = new List<IAreaSetup> { area1, area2 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act — no command prefix, only the --learn flag (the root-level code path sets prefix = "")
        var output = factory.GetLearnResponse(["--learn"]);

        // Assert
        Assert.False(string.IsNullOrEmpty(output));

        using var doc = JsonDocument.Parse(output);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal(200, status.GetInt32());

        Assert.True(root.TryGetProperty("results", out var results));
        Assert.Equal(JsonValueKind.Array, results.ValueKind);

        // All 8 commands (4 per area × 2 areas) should be returned
        Assert.Equal(8, results.GetArrayLength());

        // Every entry must have the required shape fields
        foreach (var entry in results.EnumerateArray())
        {
            Assert.True(entry.TryGetProperty("name", out _));
            Assert.True(entry.TryGetProperty("description", out _));
            Assert.True(entry.TryGetProperty("command", out _));
        }

        // Results are ordered — verify both area prefixes are present
        var commandPaths = results.EnumerateArray()
            .Select(e => e.GetProperty("command").GetString())
            .ToList();
        Assert.Contains(commandPaths, p => p!.StartsWith("name1", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(commandPaths, p => p!.StartsWith("name2", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LearnOption_InvokedOnGroup_OutputsCommandListAsJson()
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var serviceAreas = new List<IAreaSetup> { area1 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act - call GetLearnResponse with group-level args (simulates Program.cs intercept)
        var output = factory.GetLearnResponse(["name1", "--learn"]);

        // Assert - output should be valid JSON matching CommandResponse structure
        Assert.False(string.IsNullOrEmpty(output));

        using var doc = JsonDocument.Parse(output);
        var root = doc.RootElement;

        // Should have a 'status' field indicating OK (200)
        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal(200, status.GetInt32());

        // Should have a 'results' field with a list of commands
        Assert.True(root.TryGetProperty("results", out var results));
        Assert.Equal(JsonValueKind.Array, results.ValueKind);
        Assert.True(results.GetArrayLength() > 0);

        // Each entry should have 'name', 'description', and 'command' fields
        var firstCommand = results[0];
        Assert.True(firstCommand.TryGetProperty("name", out _));
        Assert.True(firstCommand.TryGetProperty("description", out _));
        Assert.True(firstCommand.TryGetProperty("command", out var commandPath));

        // The command path should start with the group name
        Assert.StartsWith("name1", commandPath.GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LearnOption_InvokedOnLeafCommand_OutputsCommandInfoAsJson()
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var serviceAreas = new List<IAreaSetup> { area1 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act - call GetLearnResponse with leaf command args (simulates Program.cs intercept)
        var output = factory.GetLearnResponse(["name1", "directCommand", "--learn"]);

        // Assert - output should be valid JSON
        Assert.False(string.IsNullOrEmpty(output));

        using var doc = JsonDocument.Parse(output);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal(200, status.GetInt32());

        Assert.True(root.TryGetProperty("results", out var results));
        Assert.Equal(JsonValueKind.Array, results.ValueKind);
        Assert.Equal(1, results.GetArrayLength()); // Single command for leaf

        var commandEntry = results[0];
        Assert.True(commandEntry.TryGetProperty("name", out var name));
        Assert.Equal("directCommand", name.GetString());
    }

    [Fact]
    public void LearnOption_InvokedOnNonexistentCommand_Returns404WithMessage()
    {
        // Arrange
        var area1 = CreateIAreaSetup("name1");
        var serviceAreas = new List<IAreaSetup> { area1 };
        var factory = new CommandFactory(_serviceProvider, serviceAreas, _telemetryService, _configurationOptions, _logger);

        // Act
        var output = factory.GetLearnResponse(["nonexistent", "--learn"]);

        // Assert
        Assert.False(string.IsNullOrEmpty(output));

        using var doc = JsonDocument.Parse(output);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal(404, status.GetInt32());

        Assert.True(root.TryGetProperty("message", out var message));
        var messageText = message.GetString();
        Assert.False(string.IsNullOrEmpty(messageText));
        Assert.Contains("nonexistent", messageText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_Throws_When_Command_Uses_Reserved_Learn_Option(bool useAlias)
    {
        // Arrange
        var area = Substitute.For<IAreaSetup>();
        area.Name.Returns("name1");
        area.RegisterCommands(Arg.Any<IServiceProvider>()).Returns(_ => CreateCommandGroupWithReservedLearnOption("name1", useAlias));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new CommandFactory(_serviceProvider, [area], _telemetryService, _configurationOptions, _logger));

        Assert.Contains(ICommandFactory.LearnOptionName, ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name1 directCommand", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static IAreaSetup CreateIAreaSetup(string areaName)
    {
        var area = Substitute.For<IAreaSetup>();

        area.Name.Returns(areaName);
        area.RegisterCommands(Arg.Any<IServiceProvider>()).Returns(caller =>
        {
            var newCommandGroup = CreateCommandGroup(areaName);
            return newCommandGroup;
        });

        return area;
    }

    /// <summary>
    /// Creates a "root" command group that has:
    /// - 1 direct command
    /// - 2 subgroups
    /// - subgroup1: has 2 command
    /// - subgroup2 has 1 command
    /// </summary>
    /// <returns></returns>
    private static CommandGroup CreateCommandGroup(string rootName = "root")
    {
        var group = new CommandGroup(rootName, "Test root");
        var subGroup = new CommandGroup("subgroup1", "Test subgroup");
        var subGroup2 = new CommandGroup("subgroup2", "Test subgroup");

        var directCommand = Substitute.For<IBaseCommand>();
        directCommand.Name.Returns(nameof(directCommand));
        directCommand.GetCommand().Returns(new Command(nameof(directCommand)));

        var directCommand2 = Substitute.For<IBaseCommand>();
        directCommand2.Name.Returns(nameof(directCommand2));
        directCommand2.GetCommand().Returns(new Command(nameof(directCommand2)));


        var directCommand3 = Substitute.For<IBaseCommand>();
        directCommand3.Name.Returns(nameof(directCommand3));
        directCommand3.GetCommand().Returns(new Command(nameof(directCommand3)));

        var directCommand4 = Substitute.For<IBaseCommand>();
        directCommand4.Name.Returns(nameof(directCommand4));
        directCommand4.GetCommand().Returns(new Command(nameof(directCommand4)));

        // Add commands to each group
        group.Commands.Add(nameof(directCommand), directCommand);

        subGroup.Commands.Add(nameof(directCommand2), directCommand2);
        subGroup.Commands.Add(nameof(directCommand3), directCommand3);

        subGroup2.Commands.Add(nameof(directCommand4), directCommand4);

        // Make subgroups
        group.SubGroup.Add(subGroup);
        group.SubGroup.Add(subGroup2);

        return group;
    }

    private static CommandGroup CreateCommandGroupWithReservedLearnOption(string rootName, bool useAlias)
    {
        var group = new CommandGroup(rootName, "Test root");
        var command = new Command("directCommand");

        if (useAlias)
        {
            var option = new Option<bool>("--custom");
            option.Aliases.Add(ICommandFactory.LearnOptionName);
            command.Options.Add(option);
        }
        else
        {
            command.Options.Add(new Option<bool>(ICommandFactory.LearnOptionName));
        }

        var baseCommand = Substitute.For<IBaseCommand>();
        baseCommand.Name.Returns("directCommand");
        baseCommand.GetCommand().Returns(command);

        group.Commands.Add("directCommand", baseCommand);
        return group;
    }
}
