// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Tools.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Areas.Tools.Commands;

[HiddenCommand]
[CommandMetadata(
    Id = "63de05a7-047d-4f8a-86ea-cebd64527e2b",
    Name = "list",
    Title = "List Available Tools",
    Description = """
        List all available commands and their tools in a hierarchical structure. This command returns detailed information
        about each command, including its name, description, full command path, available subcommands, and all supported
        arguments. Use --name-only to return only tool names, and --namespace to filter by specific namespaces.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class ToolsListCommand(ILogger<ToolsListCommand> logger)
    : BaseCommand<ToolsListOptions, ToolsListCommand.ToolsListResult>
{
    private static readonly HashSet<string> s_ignored = new(StringComparer.OrdinalIgnoreCase) { "server", "tools" };
    private static readonly HashSet<string> s_surfaced = new(StringComparer.OrdinalIgnoreCase) { "extension" };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ToolsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var factory = context.GetService<ICommandFactory>();

            // If the --namespace-mode flag is set, return distinct top‑level namespaces (e.g. child groups beneath root 'azmcp').
            if (options.NamespaceMode)
            {
                var rootGroup = factory.RootGroup; // azmcp
                var hasNamespaceFiltering = options.Namespace != null && options.Namespace.Length > 0;

                var namespaceCommands = rootGroup.SubGroup
                    .Where(g => !s_ignored.Contains(g.Name) && !s_surfaced.Contains(g.Name))
                    // Apply namespace filtering if specified
                    .Where(g => !hasNamespaceFiltering || options.Namespace.Contains(g.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(g => new CommandInfo
                    {
                        Name = g.Name,
                        Description = g.Description ?? string.Empty,
                        Command = $"{g.Name}",
                        // We deliberately omit populating Subcommands for the lightweight namespace view.
                    })
                    .OrderBy(ci => ci.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Add the commands to be surfaced directly to the list.
                // For commands in the surfaced list, each command is exposed as a separate tool in the namespace mode.
                foreach (var name in s_surfaced)
                {
                    // Apply namespace filtering for surfaced commands too
                    if (hasNamespaceFiltering && !options.Namespace.Contains(name, StringComparer.OrdinalIgnoreCase))
                        continue;

                    var subgroup = rootGroup.SubGroup.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (subgroup is not null)
                    {
                        List<CommandInfo> foundCommands = [];
                        SearchCommandInCommandGroup("", subgroup, foundCommands);
                        namespaceCommands.AddRange(foundCommands);
                    }
                }

                // If --name-only is also specified, return only the names
                if (options.NameOnly)
                {
                    var namespaceNames = namespaceCommands.Select(nc => nc.Command).ToList();
                    context.Response.Results = ResponseResult.Create(new(null, namespaceNames), ModelsJsonContext.Default.ToolsListResult);
                    return context.Response;
                }

                context.Response.Results = ResponseResult.Create(new(namespaceCommands, null), ModelsJsonContext.Default.ToolsListResult);
                return context.Response;
            }

            // If the --name-only flag is set (without namespace mode), return only tool names
            if (options.NameOnly)
            {
                // Get all visible commands and extract their tokenized names (full command paths)
                var allToolNames = CommandFactory.GetVisibleCommands(factory.AllCommands)
                    .Select(kvp => kvp.Key) // Use the tokenized key instead of just the command name
                    .Where(name => !string.IsNullOrEmpty(name));

                // Apply namespace filtering if specified (using underscore separator for tokenized names)
                allToolNames = ApplyNamespaceFilterToNames(allToolNames, options.Namespace, CommandFactory.Separator);

                var toolNames = allToolNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();

                context.Response.Results = ResponseResult.Create(new(null, toolNames), ModelsJsonContext.Default.ToolsListResult);
                return context.Response;
            }

            // Get all tools with full details
            var allTools = CommandFactory.GetVisibleCommands(factory.AllCommands)
                .Select(kvp => CreateCommand(kvp.Key, kvp.Value));

            // Apply namespace filtering if specified
            var filteredToolNames = ApplyNamespaceFilterToNames(allTools.Select(t => t.Command), options.Namespace, ' ');
            var filteredToolNamesSet = filteredToolNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            allTools = allTools.Where(tool => filteredToolNamesSet.Contains(tool.Command));

            var tools = allTools.ToList();

            context.Response.Results = ResponseResult.Create(new(tools, null), ModelsJsonContext.Default.ToolsListResult);
            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while processing tool listing.");
            HandleException(context, ex);

            return context.Response;
        }
    }

    private static IEnumerable<string> ApplyNamespaceFilterToNames(IEnumerable<string> names, string[]? namespaces, char separator)
    {
        if (namespaces == null || namespaces.Length == 0)
        {
            return names;
        }

        var namespacePrefixes = namespaces.Select(@namespace => $"{@namespace}{separator}").ToList();

        return names.Where(name =>
            namespacePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
    }

    private static CommandInfo CreateCommand(string tokenizedName, IBaseCommand command)
    {
        var commandDetails = command.GetCommand();

        var optionInfos = commandDetails.Options?
            .Select(arg => new OptionInfo(
                name: arg.Name,
                description: arg.Description!,
                required: arg.Required))
            .ToList();

        var fullCommand = tokenizedName.Replace(CommandFactory.Separator, ' ');

        return new CommandInfo
        {
            Id = command.Id,
            Name = commandDetails.Name,
            Description = commandDetails.Description ?? string.Empty,
            Command = fullCommand,
            Options = optionInfos,
            Metadata = command.Metadata
        };
    }

    [JsonConverter(typeof(ToolsListResultConverter))]
    public sealed record ToolsListResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<CommandInfo>? Commands,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<string>? Names);

    public sealed class ToolsListResultConverter : JsonConverter<ToolsListResult>
    {
        public override ToolsListResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                List<string>? names = null;
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "names")
                    {
                        reader.Read(); // Move to the value of "names"
                        names = JsonSerializer.Deserialize(ref reader, ModelsJsonContext.Default.ListString);
                    }
                }
                return new(null, names);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                var commands = JsonSerializer.Deserialize(ref reader, ModelsJsonContext.Default.ListCommandInfo);
                return new(commands, null);
            }

            throw new JsonException("Invalid JSON format for ToolsListResult.");
        }

        public override void Write(Utf8JsonWriter writer, ToolsListResult? value, JsonSerializerOptions options)
        {
            if (value is not null)
            {
                if (value.Commands is not null)
                {
                    JsonSerializer.Serialize(writer, value.Commands, ModelsJsonContext.Default.ListCommandInfo);
                }
                else if (value.Names is not null)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("names");
                    JsonSerializer.Serialize(writer, value.Names, ModelsJsonContext.Default.ListString);
                    writer.WriteEndObject();
                }
            }
        }
    }

    private static void SearchCommandInCommandGroup(string commandPrefix, CommandGroup searchedGroup, List<CommandInfo> foundCommands)
    {
        var commands = CommandFactory.GetVisibleCommands(searchedGroup.Commands).Select(kvp =>
        {
            var command = kvp.Value.GetCommand();
            return new CommandInfo
            {
                Name = $"{commandPrefix.Replace(" ", "_")}{searchedGroup.Name}_{command.Name}",
                Description = command.Description ?? string.Empty,
                Command = $"{(!string.IsNullOrEmpty(commandPrefix) ? commandPrefix : "")}{searchedGroup.Name} {command.Name}"
                // Omit Options and Subcommands for surfaced commands as well.
            };
        });
        foundCommands.AddRange(commands);
        foreach (CommandGroup nextLevelSubGroup in searchedGroup.SubGroup)
        {
            SearchCommandInCommandGroup($"{commandPrefix}{searchedGroup.Name} ", nextLevelSubGroup, foundCommands);
        }
    }
}
