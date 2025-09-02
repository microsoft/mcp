// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Commands;

public class CommandFactory : ICommandFactory
{
    private readonly IAreaSetup[] _serviceAreas;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandFactory> _logger;
    private readonly RootCommand _rootCommand;
    private readonly CommandGroup _rootGroup;
    private readonly ModelsJsonContext _srcGenWithOptions;

    /// <summary>
    /// Mapping of tokenized command names to their <see cref="IBaseCommand" />
    /// </summary>
    private readonly Dictionary<string, IBaseCommand> _commandMap;
    private readonly HashSet<string> _serviceAreaNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly ITelemetryService _telemetryService;

    // Add this new class inside CommandFactory
    private class StringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() ?? string.Empty;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            var cleanValue = value?.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            writer.WriteStringValue(cleanValue);
        }
    }

    public CommandFactory(IServiceProvider serviceProvider, IEnumerable<IAreaSetup> serviceAreas, ITelemetryService telemetryService, ILogger<CommandFactory> logger)
    {
        _serviceAreas = serviceAreas?.ToArray() ?? throw new ArgumentNullException(nameof(serviceAreas));
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rootGroup = new CommandGroup("azmcp", "Azure MCP Server");
        _rootCommand = CreateRootCommand();
        _commandMap = CreateCommmandDictionary(_rootGroup, string.Empty);
        _telemetryService = telemetryService;
        _srcGenWithOptions = new ModelsJsonContext(new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public RootCommand RootCommand => _rootCommand;

    public CommandGroup RootGroup => _rootGroup;

    public IReadOnlyDictionary<string, IBaseCommand> AllCommands => _commandMap;

    public char Separator => '_';

    public IReadOnlyDictionary<string, IBaseCommand> GroupCommands(string[] groupNames)
    {
        if (groupNames is null)
        {
            throw new ArgumentException("groupNames cannot be null.");
        }
        Dictionary<string, IBaseCommand> commandsFromGroups = new();
        foreach (string groupName in groupNames)
        {
            foreach (CommandGroup group in _rootGroup.SubGroup)
            {
                if (string.Equals(group.Name, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    Dictionary<string, IBaseCommand> commandsInGroup = CreateCommmandDictionary(group, string.Empty);
                    foreach (var (key, value) in commandsInGroup)
                    {
                        commandsFromGroups[key] = value;
                    }
                    break;
                }
            }
        }

        return commandsFromGroups;
    }

    private void RegisterCommandGroup()
    {
        // Register area command groups
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        foreach (var area in _serviceAreas)
        {
            if (string.IsNullOrEmpty(area.Name))
            {
                var error = new ArgumentException("IAreaSetup cannot have an empty or null name. Type "
                    + area.GetType());
                _logger.LogError(error, "Invalid IAreaSetup encountered. Type: {Type}", area.GetType());

                throw error;
            }

            if (!_serviceAreaNames.Add(area.Name))
            {
                var matchingAreaTypes = _serviceAreas
                    .Where(x => x.Name == area.Name)
                    .Select(a => a.GetType().FullName);

                var error = new ArgumentException("Cannot have multiple IAreaSetup with the same Name.");
                _logger.LogError(error,
                    "Duplicate {AreaName}. Areas with same name: {AllAreaTypes}",
                    area.Name,
                    string.Join(", ", matchingAreaTypes));

                throw error;
            }

            area.RegisterCommands(_rootGroup, loggerFactory);
        }
    }

    private void ConfigureCommands(CommandGroup group)
    {
        // Configure direct commands in this group
        foreach (var command in group.Commands.Values)
        {
            var cmd = command.GetCommand();

            if (cmd.Handler == null)
            {
                ConfigureCommandHandler(cmd, command);
            }

            group.Command.Add(cmd);
        }

        // Recursively configure subgroup commands
        foreach (var subGroup in group.SubGroup)
        {
            ConfigureCommands(subGroup);
        }
    }

    private RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("Azure MCP Server - A Model Context Protocol (MCP) server that enables AI agents to interact with Azure services through standardized communication patterns.");

        RegisterCommandGroup();

        // Copy the root group's subcommands to the RootCommand
        foreach (var subGroup in _rootGroup.SubGroup)
        {
            rootCommand.Add(subGroup.Command);
        }

        // Configure all commands in the hierarchy
        ConfigureCommands(_rootGroup);

        return rootCommand;
    }

    private void ConfigureCommandHandler(Command command, IBaseCommand implementation)
    {
        command.SetHandler(async context =>
        {
            _logger.LogTrace("Executing '{Command}'.", command.Name);

            using var activity = await _telemetryService.StartActivity(ActivityName.CommandExecuted);

            var cmdContext = new CommandContext(_serviceProvider, activity);
            var startTime = DateTime.UtcNow;
            try
            {
                var response = await implementation.ExecuteAsync(cmdContext, context.ParseResult);

                // Calculate execution time
                var endTime = DateTime.UtcNow;
                response.Duration = (long)(endTime - startTime).TotalMilliseconds;

                if (response.Status == 200 && response.Results == null)
                {
                    response.Results = ResponseResult.Create(new List<string>(), JsonSourceGenerationContext.Default.ListString);
                }

                var isServiceStartCommand = implementation is Areas.Server.Commands.ServiceStartCommand;
                if (!isServiceStartCommand)
                {
                    Console.WriteLine(JsonSerializer.Serialize(response, _srcGenWithOptions.CommandResponse));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while executing '{Command}'. Exception: {Exception}",
                    command.Name, ex);
                activity?.SetStatus(ActivityStatusCode.Error)?.AddTag(TagName.ErrorDetails, ex.Message);
            }
            finally
            {
                _logger.LogTrace("Finished running '{Command}'.", command.Name);
            }
        });
    }

    private static IBaseCommand? FindCommandInGroup(CommandGroup group, Queue<string> nameParts)
    {
        // If we've processed all parts and this group has a matching command, return it
        if (nameParts.Count == 1)
        {
            var commandName = nameParts.Dequeue();
            return group.Commands.GetValueOrDefault(commandName);
        }

        // Find the next subgroup
        var groupName = nameParts.Dequeue();
        var nextGroup = group.SubGroup.FirstOrDefault(g => g.Name == groupName);

        return nextGroup != null ? FindCommandInGroup(nextGroup, nameParts) : null;
    }

    /// <summary>
    /// Finds the BaseCommand given its full command name (i.e. storage_account_list).
    /// </summary>
    /// <param name="tokenizedName">Name of the command with prefixes.</param>
    /// <returns></returns>
    public IBaseCommand? FindCommandByName(string tokenizedName)
    {
        return _commandMap.GetValueOrDefault(tokenizedName);
    }

    /// <summary>
    /// Gets the service area given the full command name (i.e. 'storage_account_list' would return 'storage').
    /// </summary>
    /// <param name="tokenizedName">Name of the command with prefixes.</param>
    public string? GetServiceArea(string tokenizedName)
    {
        if (string.IsNullOrEmpty(tokenizedName))
        {
            return null;
        }

        var split = tokenizedName.Split(Separator, 2);

        return _serviceAreaNames.Contains(split[0]) ? split[0] : null;
    }

    private Dictionary<string, IBaseCommand> CreateCommmandDictionary(CommandGroup node, string prefix)
    {
        var aggregated = new Dictionary<string, IBaseCommand>();
        var updatedPrefix = GetPrefix(prefix, node.Name);

        if (node.Commands != null)
        {
            foreach (var kvp in node.Commands)
            {
                var key = GetPrefix(updatedPrefix, kvp.Key);
                aggregated.Add(key, kvp.Value);
            }
        }

        if (node.SubGroup == null)
        {
            return aggregated;
        }

        foreach (var command in node.SubGroup)
        {
            var subcommandsDictionary = CreateCommmandDictionary(command, updatedPrefix);
            foreach (var item in subcommandsDictionary)
            {
                aggregated.Add(item.Key, item.Value);
            }
        }

        return aggregated;
    }

    private string GetPrefix(string currentPrefix, string additional) => string.IsNullOrEmpty(currentPrefix)
        ? additional
        : currentPrefix + Separator + additional;

    public static IEnumerable<KeyValuePair<string, IBaseCommand>> GetVisibleCommands(IEnumerable<KeyValuePair<string, IBaseCommand>> commands)
    {
        return commands
            .Where(kvp => kvp.Value.GetType().GetCustomAttribute<HiddenCommandAttribute>() == null)
            .OrderBy(kvp => kvp.Key);
    }

    internal static IEnumerable<KeyValuePair<string, IBaseCommand>> GetVisibleCommands(IReadOnlyDictionary<string, IBaseCommand> toolCommands)
    {
        return GetVisibleCommands(toolCommands.AsEnumerable());
    }
}
