// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Areas.Tools.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Tools.Commands;

[HiddenCommand]
public sealed class ToolsListCommand(ILogger<ToolsListCommand> logger) : BaseCommand<ToolsListOptions>
{
    private const string CommandTitle = "List Available Tools";

    public override string Id => "63de05a7-047d-4f8a-86ea-cebd64527e2b";

    public override string Name => "list";

    public override string Description =>
        """
        List all available commands and their tools in a hierarchical structure. This command returns detailed information
        about each command, including its name, description, full command path, available subcommands, and all supported
        arguments. Use --name-only to return only tool names, and --namespace to filter by specific namespaces.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ToolsListOptionDefinitions.NamespaceMode);
        command.Options.Add(ToolsListOptionDefinitions.Namespace);
        command.Options.Add(ToolsListOptionDefinitions.NameOnly);
    }

    protected override ToolsListOptions BindOptions(ParseResult parseResult)
    {
        var namespaces = parseResult.GetValueOrDefault<string[]>(ToolsListOptionDefinitions.Namespace.Name) ?? [];
        return new ToolsListOptions
        {
            NamespaceMode = parseResult.GetValueOrDefault(ToolsListOptionDefinitions.NamespaceMode),
            Name = parseResult.GetValueOrDefault(ToolsListOptionDefinitions.NameOnly),
            Namespaces = namespaces.ToList()
        };
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var factory = context.GetService<CommandFactory>();
            var options = BindOptions(parseResult);

            // If the --name-only flag is set, return only tool names
            if (options.Name)
            {
                // Get all visible commands and extract their tokenized names (full command paths)
                var allToolNames = CommandFactory.GetVisibleCommands(factory.AllCommands)
                    .Select(kvp => kvp.Key) // Use the tokenized key instead of just the command name
                    .Where(name => !string.IsNullOrEmpty(name));

                // Apply namespace filtering if specified (using underscore separator for tokenized names)
                allToolNames = ApplyNamespaceFilterToNames(allToolNames, options.Namespaces, '_');

                var toolNames = await Task.Run(() => allToolNames
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList());

                var result = new ToolNamesResult(toolNames);
                context.Response.Results = ResponseResult.Create(result, ModelsJsonContext.Default.ToolNamesResult);
                return context.Response;
            }

            // If the --namespace-mode flag is set, return distinct top‑level namespaces (child groups beneath root 'azmcp').
            if (options.NamespaceMode)
            {
                var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "server", "tools" };
                var surfaced = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "extension" };
                var rootGroup = factory.RootGroup; // azmcp

                var namespaceCommands = rootGroup.SubGroup
                    .Where(g => !ignored.Contains(g.Name) && !surfaced.Contains(g.Name))
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
                foreach (var name in surfaced)
                {
                    var subgroup = rootGroup.SubGroup.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (subgroup is not null)
                    {
                        var commands = CommandFactory.GetVisibleCommands(subgroup.Commands).Select(kvp =>
                            {
                                var command = kvp.Value.GetCommand();
                                return new CommandInfo
                                {
                                    Name = command.Name,
                                    Description = command.Description ?? string.Empty,
                                    Command = $"{subgroup.Name} {command.Name}"
                                    // Omit Options and Subcommands for surfaced commands as well.
                                };
                            });
                        namespaceCommands.AddRange(commands);
                    }
                }

                context.Response.Results = ResponseResult.Create(namespaceCommands, ModelsJsonContext.Default.ListCommandInfo);
                return context.Response;
            }

            // Get all tools with full details
            var allTools = CommandFactory.GetVisibleCommands(factory.AllCommands)
                .Select(kvp => CreateCommand(kvp.Key, kvp.Value));

            // Apply namespace filtering if specified
            var filteredToolNames = ApplyNamespaceFilterToNames(allTools.Select(t => t.Command), options.Namespaces, ' ');
            var filteredToolNamesSet = filteredToolNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            allTools = allTools.Where(tool => filteredToolNamesSet.Contains(tool.Command));

            var tools = await Task.Run(() => allTools.ToList());

            context.Response.Results = ResponseResult.Create(tools, ModelsJsonContext.Default.ListCommandInfo);
            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while processing tool listing.");
            HandleException(context, ex);

            return context.Response;
        }
    }

    private static IEnumerable<string> ApplyNamespaceFilterToNames(IEnumerable<string> names, List<string> namespaces, char separator)
    {
        if (namespaces.Count == 0)
        {
            return names;
        }

        // For tokenized names (using underscore), include "azmcp_" prefix
        // For command strings (using space), don't include "azmcp " prefix
        var namespacePrefixes = separator == '_' 
            ? namespaces.Select(ns => $"{ns}{separator}").ToList()
            : namespaces.Select(ns => $"{ns}{separator}").ToList();
            
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
        // Strip the "azmcp " prefix from the command
        var commandWithoutPrefix = fullCommand.StartsWith("azmcp ", StringComparison.OrdinalIgnoreCase) 
            ? fullCommand.Substring(6) // Remove "azmcp "
            : fullCommand;

        return new CommandInfo
        {
            Id = command.Id,
            Name = commandDetails.Name,
            Description = commandDetails.Description ?? string.Empty,
            Command = commandWithoutPrefix,
            Options = optionInfos,
            Metadata = command.Metadata
        };
    }

    public record ToolNamesResult(List<string> Names);
}
