// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Services.Telemetry;

namespace Microsoft.Mcp.Core.Commands;

public class CommandFactory : ICommandFactory
{
    internal const char Separator = '_';
    private readonly IAreaSetup[] _serviceAreas;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandFactory> _logger;
    private readonly RootCommand _rootCommand;
    private readonly CommandGroup _rootGroup;

    private const string LearnOptionDescription =
        "Discover available sub-commands and their parameters without executing any Azure operation. " +
        "Use on a command group (e.g. 'azmcp storage --learn') to list all commands in that group, " +
        "or on a specific command (e.g. 'azmcp storage account list --learn') to see its options.";

    /// <summary>
    /// Creates a fresh <see cref="Option{T}"/> for <c>--learn</c> each time it is called.
    /// A new instance is required per command because System.CommandLine v2 tracks option
    /// ownership by object identity; sharing a single static instance causes
    /// <see cref="ParseResult.GetValue{T}"/> to return the default value on every command
    /// except the last one the option was added to.
    /// </summary>
    private static Option<bool> CreateLearnOption()
    {
        var opt = new Option<bool>(ICommandFactory.LearnOptionName)
        {
            Description = LearnOptionDescription,
            Required = false
        };
        _factoryInjectedLearnOptions.Add(opt, null);
        return opt;
    }

    /// <summary>
    /// Pre-computed normalized form of <see cref="ICommandFactory.LearnOptionName"/> (i.e. <c>"learn"</c>).
    /// Cached to avoid recomputing on every option comparison.
    /// </summary>
    private static readonly string _normalizedLearnName =
        NameNormalization.NormalizeOptionName(ICommandFactory.LearnOptionName);

    /// <summary>
    /// Weak-keyed set of <see cref="Option"/> instances created and injected by <see cref="CommandFactory"/>.
    /// Allows any factory instance to distinguish its own injected <c>--learn</c> options from
    /// options added by command authors — without requiring the same factory instance to be reused.
    /// </summary>
    private static readonly ConditionalWeakTable<Option, object?> _factoryInjectedLearnOptions = new();

    private static bool HasFactoryInjectedLearnOption(Command command)
        => command.Options.Any(o => _factoryInjectedLearnOptions.TryGetValue(o, out _));

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="option"/> is the reserved <c>--learn</c>
    /// discovery option, matched by primary name or any alias, ignoring prefix and case.
    /// </summary>
    internal static bool IsLearnOption(Option option)
        => IsReservedLearnOptionIdentifier(option.Name) || option.Aliases.Any(IsReservedLearnOptionIdentifier);

    private static bool IsReservedLearnOptionIdentifier(string? identifier)
        => string.Equals(NameNormalization.NormalizeOptionName(identifier), _normalizedLearnName, StringComparison.OrdinalIgnoreCase);

    private void ValidateNoReservedLearnOption(Command command, string commandPath)
    {
        foreach (var option in command.Options)
        {
            if (IsLearnOption(option))
            {
                var message = $"Command '{commandPath}' defines the reserved option '{ICommandFactory.LearnOptionName}'. This option name is reserved for discovery and cannot be used by commands.";
                var error = new ArgumentException(message);
                _logger.LogError(error,
                    "Command {CommandPath} defines the reserved discovery option {OptionName}.",
                    commandPath,
                    ICommandFactory.LearnOptionName);
                throw error;
            }
        }
    }
    private readonly ModelsJsonContext _srcGenWithOptions;

    /// <summary>
    /// Mapping of tokenized command names to their <see cref="IBaseCommand" />
    /// </summary>
    private readonly Dictionary<string, IBaseCommand> _commandMap;
    private readonly Dictionary<string, IAreaSetup> _commandNamesToArea = new(StringComparer.OrdinalIgnoreCase);
    private readonly ITelemetryService _telemetryService;
    private readonly IOptions<McpServerConfiguration> _configurationOptions;

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


    public CommandFactory(IServiceProvider serviceProvider,
        IEnumerable<IAreaSetup> serviceAreas,
        ITelemetryService telemetryService,
        IOptions<McpServerConfiguration> configurationOptions,
        ILogger<CommandFactory> logger)
    {
        _serviceAreas = serviceAreas?.ToArray() ?? throw new ArgumentNullException(nameof(serviceAreas));
        _serviceProvider = serviceProvider;
        _logger = logger;
        _telemetryService = telemetryService;
        _configurationOptions = configurationOptions;
        _rootGroup = new(_configurationOptions.Value.RootCommandGroupName, _configurationOptions.Value.DisplayName);
        _rootCommand = CreateRootCommand();
        _commandMap = CreateCommandDictionary(_rootGroup);
        _srcGenWithOptions = new(new()
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
        Dictionary<string, IBaseCommand> commandsFromGroups = [];
        foreach (string groupName in groupNames)
        {
            foreach (CommandGroup group in _rootGroup.SubGroup)
            {
                if (string.Equals(group.Name, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    var commandsInGroup = CreateCommandDictionaryInner(group, string.Empty);
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
            var tempRoot = new CommandGroup(_rootGroup.Name, string.Empty);
            tempRoot.AddSubGroup(commandTree);

            var commandDictionary = CreateCommandDictionary(tempRoot);

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

    private void ConfigureCommands(CommandGroup group, string parentTokenizedPrefix = "")
    {
        var groupTokenizedPath = GetPrefix(parentTokenizedPrefix, group.Name);

        // Guard against the same Command object being processed by a second CommandFactory instance
        // (e.g. ConsolidatedToolDiscoveryStrategy reuses the same IAreaSetup/CommandGroup objects).
        // HasFactoryInjectedLearnOption checks whether the --learn on this command was created by
        // us (via _factoryInjectedLearnOptions) rather than by a command author, so we skip
        // re-validation and re-injection only when we own the existing option.
        if (!HasFactoryInjectedLearnOption(group.Command))
        {
            ValidateNoReservedLearnOption(group.Command, groupTokenizedPath.Replace(Separator, ' '));
            group.Command.Options.Add(CreateLearnOption());
        }

        // Configure direct commands in this group
        foreach (var command in group.Commands.Values)
        {
            var cmd = command.GetCommand();

            // Build the full tokenized key for this leaf command (e.g. "storage_account_list").
            var fullTokenizedKey = GetPrefix(groupTokenizedPath, command.Name);
            if (!HasFactoryInjectedLearnOption(cmd))
            {
                ValidateNoReservedLearnOption(cmd, fullTokenizedKey.Replace(Separator, ' '));
            }
            ConfigureCommandHandler(cmd, command);

            group.Command.Subcommands.Add(cmd);
        }

        // Recursively configure subgroup commands, passing the accumulated prefix.
        foreach (var subGroup in group.SubGroup)
        {
            ConfigureCommands(subGroup, groupTokenizedPath);
        }
    }

    /// <summary>
    /// Builds a <see cref="CommandInfo"/> suitable for <c>--learn</c> output, converting the
    /// underscore-separated <paramref name="tokenizedName"/> to a space-separated CLI path.
    /// </summary>
    private static CommandInfo BuildLearnCommandInfo(string tokenizedName, IBaseCommand command)
    {
        var commandDetails = command.GetCommand();

        // Command.Options is never null in System.CommandLine — the null-conditional is not needed.
        var optionInfos = commandDetails.Options
            .Where(opt => opt is not HelpOption && !IsLearnOption(opt))
            .Select(opt => new OptionInfo(
                name: opt.Name,
                description: opt.Description ?? string.Empty,
                required: opt.Required))
            .ToList();

        return new CommandInfo
        {
            Id = command.Id,
            Name = commandDetails.Name,
            Description = commandDetails.Description ?? string.Empty,
            Command = tokenizedName.Replace(Separator, ' '),
            Options = optionInfos,
            Metadata = command.Metadata
        };
    }

    private void ConfigureCommandHandler(Command command, IBaseCommand implementation)
    {
        // Add --learn as a recognized option on this command (for autocomplete/help display).
        // The actual --learn response is handled in GetLearnResponse() before InvokeAsync is called,
        // which bypasses System.CommandLine's required-option validation that would otherwise block the action.
        // Guard: skip if already injected by a prior factory run on the same Command instance.
        if (!HasFactoryInjectedLearnOption(command))
        {
            command.Options.Add(CreateLearnOption());
        }

        command.SetAction(async (parseResult, ct) =>
        {
            _logger.LogTrace("Executing '{Command}'.", command.Name);

            using var activity = _telemetryService.StartActivity(ActivityName.ToolExecuted);
            activity?.SetTag(TagName.ToolId, implementation.Id)
                .SetTag(TagName.ServerMode, "cli");
            InjectToolAreaAndName(activity, parseResult);
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

                var response = await implementation.ExecuteAsync(cmdContext, parseResult, ct);

                // Calculate execution time
                var endTime = DateTime.UtcNow;
                response.Duration = (long)(endTime - startTime).TotalMilliseconds;

                if (response.Status == HttpStatusCode.OK && response.Results == null)
                {
                    response.Results = ResponseResult.Create([], JsonSourceGenerationContext.Default.ListString);
                }

                var isServiceStartCommand = implementation is ServiceStartCommand;
                if (!isServiceStartCommand)
                {
                    Console.WriteLine(JsonSerializer.Serialize(response, _srcGenWithOptions.CommandResponse));
                }

                if (response.Status < HttpStatusCode.OK || response.Status >= HttpStatusCode.Ambiguous)
                {
                    activity?.SetStatus(ActivityStatusCode.Error)
                        ?.SetTagIfNotExists(TagName.ExceptionType, "ToolCallError")
                        ?.SetTagIfNotExists(TagName.ExceptionMessage, new JsonObject([new("StatusCode", (int)response.Status)]));
                }

                return (int)response.Status;
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while executing '{Command}'. Exception: {Exception}",
                    command.Name, ex);
                activity?.SetStatus(ActivityStatusCode.Error)
                    ?.SetTagIfNotExists(TagName.ExceptionType, ex.GetType().ToString())
                    ?.SetTagIfNotExists(TagName.ExceptionStackTrace, ex.StackTrace);
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

        CustomizeHelpOption(root);

        // Register area groups and their commands
        RegisterCommandGroup();

        // Attach subgroups to the root and configure their commands/actions
        foreach (var subGroup in _rootGroup.SubGroup)
        {
            ConfigureCommands(subGroup);
            root.Subcommands.Add(subGroup.Command);
            subGroup.Command.Options.Add(new HelpOption());

            CustomizeHelpOption(subGroup.Command);
        }

        return root;
    }

    private void CustomizeHelpOption(Command command)
    {
        for (int i = 0; i < command.Options.Count; i++)
        {
            if (command.Options[i] is HelpOption helpOption && helpOption.Action is HelpAction helpAction)
            {
                helpOption.Action = new CustomHelpAction(_configurationOptions, helpAction, _serviceAreas);
                break;
            }
        }
    }

    /// <summary>
    /// Handles a <c>--learn</c> request by examining the raw CLI <paramref name="args"/> to determine
    /// the intended command path and returns a serialized JSON <see cref="CommandResponse"/> describing
    /// the available commands or the specific command's parameters.
    /// </summary>
    /// <remarks>
    /// This method is intentionally called BEFORE <see cref="ParseResult.InvokeAsync"/> so that
    /// System.CommandLine's required-option validation cannot block the discovery response.
    /// </remarks>
    /// <param name="args">The raw command-line arguments array received by the process.</param>
    /// <returns>A JSON string ready to write to stdout.</returns>
    public string GetLearnResponse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        // Extract the command path: consume non-option tokens from the start.
        // Stop at the first token that begins with '-' (an option flag).
        var commandParts = new List<string>();
        foreach (var token in args)
        {
            if (token.StartsWith('-'))
                break;
            commandParts.Add(token);
        }

        var tokenizedKey = string.Join(Separator, commandParts);

        // Case 1: Exact match in AllCommands → this is a leaf command.
        if (!string.IsNullOrEmpty(tokenizedKey) && _commandMap.TryGetValue(tokenizedKey, out var leafCommand))
        {
            var info = BuildLearnCommandInfo(tokenizedKey, leafCommand);
            var leafResponse = new CommandResponse
            {
                Status = HttpStatusCode.OK,
                Results = ResponseResult.Create([info], ModelsJsonContext.Default.ListCommandInfo),
                Duration = 0
            };
            return JsonSerializer.Serialize(leafResponse, _srcGenWithOptions.CommandResponse);
        }

        // Case 2: No exact match → treat as a group prefix and list all commands under it.
        var prefix = string.IsNullOrEmpty(tokenizedKey) ? "" : tokenizedKey + Separator;
        var commandsInGroup = _commandMap
            .Where(kvp => string.IsNullOrEmpty(prefix) || kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var commandInfos = GetVisibleCommands(commandsInGroup)
            .Select(kvp => BuildLearnCommandInfo(kvp.Key, kvp.Value))
            .ToList();

        var groupResponse = new CommandResponse
        {
            Status = commandInfos.Count > 0 ? HttpStatusCode.OK : HttpStatusCode.NotFound,
            Results = commandInfos.Count > 0
                ? ResponseResult.Create(commandInfos, ModelsJsonContext.Default.ListCommandInfo)
                : null,
            Message = commandInfos.Count == 0
                ? $"No commands found for '{string.Join(" ", commandParts)}'. Run 'azmcp --learn' to list all available commands."
                : string.Empty,
            Duration = 0
        };
        return JsonSerializer.Serialize(groupResponse, _srcGenWithOptions.CommandResponse);
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
    /// Gets the service area given the full command name (i.e. 'storage_account_list' would return 'storage').
    /// </summary>
    /// <param name="fullCommandName">Name of the command.</param>
    public string? GetServiceArea(string fullCommandName)
    {
        if (string.IsNullOrEmpty(fullCommandName))
        {
            return null;
        }

        return _commandNamesToArea.TryGetValue(fullCommandName, out var area) ? area.Name : null;
    }

    /// <summary>
    /// Injects tool area and name tags into the activity based on the command being executed. The full command name
    /// is parsed to determine the tool area and name, which are then added as tags to the activity.
    /// </summary>
    /// <param name="activity">The activity to inject tool area and name tags into.</param>
    /// <param name="parseResult">The parsing result for the command.</param>
    private void InjectToolAreaAndName(Activity? activity, ParseResult parseResult)
    {
        if (activity == null)
        {
            return;
        }

        var commandResult = parseResult.CommandResult;
        var fullCommandName = new List<string>() { commandResult.Command.Name };
        while (commandResult.Parent is CommandResult parent
            && parent.Command.Name != RootCommand.Name)
        {
            fullCommandName.Insert(0, parent.Command.Name);
            commandResult = parent;
        }

        activity.SetTag(TagName.ToolArea, fullCommandName[0])
            .SetTag(TagName.ToolName, string.Join(Separator, fullCommandName));
    }

    /// <summary>
    /// Creates a command dictionary. Each sibling and child of the root node is created without using its name as a prefix.
    ///
    /// Node: RootNode
    /// * Siblings: A11, A12
    /// * Children (Subgroups): B1, B2
    ///
    /// Node: B1
    /// * Siblings: B11
    /// * Children: C1, C2
    ///
    /// The command dictionary would be output:
    /// - A11
    /// - A12
    /// - B1_B11
    /// - B1_C1
    /// - B1_C2
    /// - B2
    /// </summary>
    /// <param name="rootNode">Node to begin traversal.</param>
    private static Dictionary<string, IBaseCommand> CreateCommandDictionary(CommandGroup rootNode)
    {
        const string rootPrefix = "";
        var aggregated = new Dictionary<string, IBaseCommand>();

        // Add any immediate commands from root group.
        foreach (var kvp in rootNode.Commands)
        {
            aggregated.Add(kvp.Key, kvp.Value);
        }

        // Add any sub commands.
        foreach (var command in rootNode.SubGroup)
        {
            var temp = CreateCommandDictionaryInner(command, rootPrefix);

            foreach (var kvp in temp)
            {
                aggregated.Add(kvp.Key, kvp.Value);
            }
        }

        return aggregated;
    }

    /// <summary>
    /// Creates a command dictionary. Each direct node and descendent is created with its parent's name as
    /// its first prefix.  For example, given the tree:
    ///
    /// Node: A1
    /// * Siblings: A11, A12
    /// * Children (Subgroups): B1, B2
    ///
    /// Node: B1
    /// * Siblings: B11
    /// * Children: C1, C2
    ///
    /// The command dictionary would be output:
    /// - A1_A11
    /// - A1_A12
    /// - A1_B1_B11
    /// - A1_B1_C1
    /// - A1_B1_C2
    /// - A1_B2
    /// </summary>
    /// <param name="node">Node to begin traversal.</param>
    /// <param name="prefix">Prefix. If prefix is an empty string, the name of the current node is used.</param>
    internal static Dictionary<string, IBaseCommand> CreateCommandDictionaryInner(CommandGroup node, string prefix)
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
            var subcommandsDictionary = CreateCommandDictionaryInner(command, updatedPrefix);
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

    internal static IEnumerable<KeyValuePair<string, IBaseCommand>> GetVisibleCommands(IEnumerable<KeyValuePair<string, IBaseCommand>> commands)
    {
        return commands
            .Where(kvp => kvp.Value.GetType().GetCustomAttribute<HiddenCommandAttribute>() == null)
            .OrderBy(kvp => kvp.Key);
    }
}
