// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Commands;

public class CommandFactory
{
    private readonly IAreaSetup[] _serviceAreas;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandFactory> _logger;
    private readonly RootCommand _rootCommand;
    private readonly CommandGroup _rootGroup;
    private readonly ModelsJsonContext _srcGenWithOptions;

    private const string RootCommandGroupName = "azmcp";
    public const char Separator = '_';

    /// <summary>
    /// Mapping of tokenized command names to their <see cref="IBaseCommand" />
    /// </summary>
    private readonly Dictionary<string, IBaseCommand> _commandMap;
    private readonly Dictionary<string, IAreaSetup> _commandNamesToArea = new(StringComparer.OrdinalIgnoreCase);
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
        _rootGroup = new CommandGroup(RootCommandGroupName, "Azure MCP Server");
        _rootCommand = CreateRootCommand();
        _commandMap = CreateCommandDictionary(_rootGroup, string.Empty);
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
                    var commandsInGroup = CreateCommandDictionary(group, string.Empty);
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
        var existingAreaNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var area in _serviceAreas)
        {
            if (string.IsNullOrEmpty(area.Name))
            {
                var error = new ArgumentException("IAreaSetup cannot have an empty or null name. Type "
                    + area.GetType());
                _logger.LogError(error, "Invalid IAreaSetup encountered. Type: {Type}", area.GetType());

                throw error;
            }

            if (!existingAreaNames.Add(area.Name))
            {
                var matchingAreaTypes = _serviceAreas
                    .Where(x => string.Equals(x.Name, area.Name, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.GetType().FullName);

                var error = new ArgumentException("Cannot have multiple IAreaSetup with the same Name.");
                _logger.LogError(error,
                    "Duplicate {AreaName}. Areas with same name: {AllAreaTypes}",
                    area.Name,
                    string.Join(", ", matchingAreaTypes));

                throw error;
            }

            // Get the commands for the IAreaSetup and register it to the root node.
            var commandTree = area.RegisterCommands(_serviceProvider);
            _rootGroup.AddSubGroup(commandTree);

            // Create a temporary root node to register all the area's subgroups and commands to.
            // Use this to create the mapping of all commands to that area.
            var tempRoot = new CommandGroup(RootCommandGroupName, string.Empty);
            tempRoot.AddSubGroup(commandTree);

            var commandDictionary = CreateCommandDictionary(tempRoot, string.Empty);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Registered commands for area '{AreaName}' are: {AllCommands}.",
                    area.Name, string.Join(", ", commandDictionary.Keys));
            }

            foreach (var item in commandDictionary)
            {
                _commandNamesToArea.Add(item.Key, area);
            }
        }
    }

    private void ConfigureCommands(CommandGroup group)
    {
        // Configure direct commands in this group
        foreach (var command in group.Commands.Values)
        {
            var cmd = command.GetCommand();

            ConfigureCommandHandler(cmd, command);

            group.Command.Subcommands.Add(cmd);
        }

        // Recursively configure subgroup commands
        foreach (var subGroup in group.SubGroup)
        {
            ConfigureCommands(subGroup);
        }
    }

    private void ConfigureCommandHandler(Command command, IBaseCommand implementation)
    {
        command.SetAction(async (ParseResult parseResult, CancellationToken ct) =>
        {
            _logger.LogTrace("Executing '{Command}'.", command.Name);

            using var activity = await _telemetryService.StartActivity(ActivityName.CommandExecuted);

            var cmdContext = new CommandContext(_serviceProvider, activity);
            var startTime = DateTime.UtcNow;
            try
            {
                // Centralized preflight validation before executing the command
                var validation = implementation.Validate(parseResult.CommandResult, cmdContext.Response);
                if (!validation.IsValid)
                {
                    Console.WriteLine(JsonSerializer.Serialize(cmdContext.Response, _srcGenWithOptions.CommandResponse));
                    return (int)cmdContext.Response.Status;
                }

                var response = await implementation.ExecuteAsync(cmdContext, parseResult);

                // Calculate execution time
                var endTime = DateTime.UtcNow;
                response.Duration = (long)(endTime - startTime).TotalMilliseconds;

                if (response.Status == HttpStatusCode.OK && response.Results == null)
                {
                    response.Results = ResponseResult.Create(new List<string>(), JsonSourceGenerationContext.Default.ListString);
                }

                var isServiceStartCommand = implementation is Areas.Server.Commands.ServiceStartCommand;
                if (!isServiceStartCommand)
                {
                    Console.WriteLine(JsonSerializer.Serialize(response, _srcGenWithOptions.CommandResponse));
                }

                if (response.Status < HttpStatusCode.OK || response.Status >= HttpStatusCode.Ambiguous)
                {
                    activity?.SetStatus(ActivityStatusCode.Error).AddTag(TagName.ErrorDetails, response.Message);
                }

                return (int)response.Status;
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while executing '{Command}'. Exception: {Exception}",
                    command.Name, ex);
                activity?.SetStatus(ActivityStatusCode.Error)?.AddTag(TagName.ErrorDetails, ex.Message);
                return 1;
            }
            finally
            {
                _logger.LogTrace("Finished running '{Command}'.", command.Name);
            }
        });
    }

    private RootCommand CreateRootCommand()
    {
        // RootCommand title/description comes from the root group
        var root = new RootCommand(_rootGroup.Description);

        // Register area groups and their commands
        RegisterCommandGroup();

        // Attach subgroups to the root and configure their commands/actions
        foreach (var subGroup in _rootGroup.SubGroup)
        {
            ConfigureCommands(subGroup);
            root.Subcommands.Add(subGroup.Command);
        }

        return root;
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
    /// <param name="fullCommandName">Name of the command with prefixes.</param>
    /// <returns></returns>
    public IBaseCommand? FindCommandByName(string fullCommandName)
    {
        return _commandMap.GetValueOrDefault(fullCommandName);
    }

    /// <summary>
    /// Gets the service area given the full command name (i.e. 'storage_account_list' or 'azmcp_storage_account_list' would return 'storage').
    /// </summary>
    /// <param name="fullCommandName">Name of the command.</param>
    public string? GetServiceArea(string fullCommandName)
    {
        if (string.IsNullOrEmpty(fullCommandName))
        {
            return null;
        }

        if (_commandNamesToArea.TryGetValue(fullCommandName, out var area))
        {
            return area.Name;
        }

        // If it starts with azmcp, then it is already the full command name.
        if (fullCommandName.StartsWith(RootCommandGroupName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Else, it means that the command could be from namespace mode where the IAreaSetup.Name 
        // is the root of the command tree.
        var rootPrefixAppended = string.Join(Separator, RootCommandGroupName, fullCommandName);
        return _commandNamesToArea.TryGetValue(rootPrefixAppended, out var area2)
            ? area2.Name
            : null;
    }

    internal static Dictionary<string, IBaseCommand> CreateCommandDictionary(CommandGroup node, string prefix)
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
            var subcommandsDictionary = CreateCommandDictionary(command, updatedPrefix);
            foreach (var item in subcommandsDictionary)
            {
                aggregated.Add(item.Key, item.Value);
            }
        }

        return aggregated;
    }

    internal static string GetPrefix(string currentPrefix, string additional) => string.IsNullOrEmpty(currentPrefix)
        ? additional
        : currentPrefix + Separator + additional;

    public static IEnumerable<KeyValuePair<string, IBaseCommand>> GetVisibleCommands(IEnumerable<KeyValuePair<string, IBaseCommand>> commands)
    {
        return commands
            .Where(kvp => kvp.Value.GetType().GetCustomAttribute<HiddenCommandAttribute>() == null)
            .OrderBy(kvp => kvp.Key);
    }
}
